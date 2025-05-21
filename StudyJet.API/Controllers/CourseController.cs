using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Security.Claims;

namespace StudyJet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IFileStorageService _fileService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public CourseController(ICourseService courseService, IFileStorageService fileService, INotificationService notificationService, UserManager<User> userManager)
        {
            _courseService = courseService;
            _fileService = fileService;
            _notificationService = notificationService;
            _userManager = userManager;

        }

        // Get all courses (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<CourseResponseDTO>>> GetAll()
        {
            var courses = await _courseService.GetAllAsync();
            return Ok(courses);
        }

        // Get course details by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseResponseDTO>> GetById(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound(new { message = "Course not found." });
            }
            return Ok(course);
        }


        // Get popular courses
        [HttpGet("popular")]
        public async Task<ActionResult<List<CourseResponseDTO>>> GetPopularCourses()
        {
            var courses = await _courseService.GetPopularCoursesAsync();
            return Ok(courses);
        }


        // Search courses by query string
        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return BadRequest(new { message = "Search query must be at least 2 characters long." });
            }

            var results = await _courseService.SearchCoursesAsync(query);

            if (results == null || results.Count == 0)
            {
                return NotFound( new { message = "No matching courses found." });
            }
            return Ok(results);
        }

        // Get total number of courses (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("GetTotalCourses")]
        public async Task<IActionResult> GetTotalCourses()
        {
            var totalCourses = await _courseService.GetTotalCoursesAsync();
            return Ok(totalCourses);
        }


        // Get courses pending approval (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Course>>> GetPendingCourses()
        {
            var pendingCourses = await _courseService.GetPendingCoursesAsync();
            return Ok(pendingCourses);
        }

        // Get approved courses
        [HttpGet("approved")]
        public async Task<ActionResult<IEnumerable<Course>>> GetApprovedCourses()
        {
            var approvedCourses = await _courseService.GetApprovedCoursesAsync();
            return Ok(approvedCourses);
        }


        // Get all courses created by the logged-in instructor
        [Authorize(Roles = "Instructor")]
        [HttpGet("course-by-instructor")]
        public async Task<IActionResult> GetCoursesByInstructor()
        {
            var instructorId = User.FindFirstValue(CustomClaimTypes.UserId);

            Console.WriteLine($"Extracted Instructor ID: {instructorId}");

            if (string.IsNullOrEmpty(instructorId))
            {
                return BadRequest(new { message = "Instructor is not logged in." });
            }

            try
            {
                var courses = await _courseService.GetByInstructorIdAsync(instructorId);
                return Ok(courses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }


        // Get the total number of courses by the logged-in instructor
        [Authorize(Roles = "Instructor")]
        [HttpGet("total-courses-by-instructor")]
        public async Task<IActionResult> GetTotalCoursesForInstructor()
        {
            var instructorId = User.FindFirstValue(CustomClaimTypes.UserId);

            if (string.IsNullOrEmpty(instructorId))
            {
                return BadRequest(new { message = "Instructor is not logged in."});
            }

            try
            {
                var totalCourses = await _courseService.GetTotalCoursesByInstructorIdAsync(instructorId);
                return Ok(new { TotalCourses = totalCourses });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        // Get list of students enrolled in a specific course (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("{courseId}/students")]
        public async Task<IActionResult> GetEnrolledStudents(int courseId)
        {
            try
            {
                var students = await _courseService.GetEnrolledStudentsByCourseIdAsync(courseId);
                if (students == null || students.Count == 0)
                {
                    return NotFound(new { message = "No students are enrolled in this course." });
                }

                return Ok(students.Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.Email,
                    s.ProfilePictureUrl,
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // Get all courses along with their enrolled students for the logged-in instructor
        [Authorize(Roles = "Instructor")]
        [HttpGet("instructor/courses/students")]
        public async Task<IActionResult> GetCoursesWithStudents()
        {
            var instructorId = User.FindFirstValue(CustomClaimTypes.UserId);

            if (string.IsNullOrEmpty(instructorId))
            {
                return Unauthorized(new { message = "Instructor not found." });
            }

            var coursesWithStudents = await _courseService.GetCoursesWithStudentsForInstructorAsync(instructorId);

            if (coursesWithStudents == null || !coursesWithStudents.Any())
            {
                return Ok(new List<object>());
            }

            return Ok(coursesWithStudents);
        }


        // Create a new course with image upload by the instructor
        [Authorize(Roles = "Instructor")]
        [HttpPost("create")]
        public async Task<ActionResult<int>> Create([FromForm] CreateCourseRequestDTO dto)
        {
            if (dto == null || dto.ImageFile == null || dto.ImageFile.Length == 0)
                return BadRequest(new { message = "Invalid course data or image file." });

            try
            {
                var courseId = await _courseService.AddAsync(dto);

                var instructorId = User.FindFirstValue(CustomClaimTypes.UserId); 
                await _notificationService.NotifyAdminForCourseAdditionOrUpdateAsync(instructorId, "added a new course", courseId);
                await _notificationService.NotifyInstructorOnCourseApprovalStatusAsync(courseId, "pending approval");

                return CreatedAtAction(nameof(GetById), new { id = courseId }, courseId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        // Approve a course (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("approve/{courseId}")]
        public async Task<IActionResult> ApproveCourse(int courseId)
        {
            var isApproved = await _courseService.ApproveCourseAsync(courseId);
            if (!isApproved)
                return NotFound(new { message = "Course not found or already approved." });


            await _notificationService.NotifyInstructorOnCourseApprovalStatusAsync(courseId, "approved!");

            return Ok(new { message = "Course approved successfully" });
        }


        // Reject a course (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("reject/{courseId}")]
        public async Task<IActionResult> RejectCourse(int courseId)
        {
            var isRejected = await _courseService.RejectCourseAsync(courseId);
            if (!isRejected)
                return NotFound(new { message = "Course not found or rejection failed" });

            await _notificationService.NotifyInstructorOnCourseApprovalStatusAsync(courseId, "rejected");

            return Ok(new { message = "Course rejected successfully" });
        }


        // Get course details for update (instructor must own the course)
        [Authorize(Roles = "Instructor")]
        [HttpGet("{courseId}/update")]
        public async Task<IActionResult> GetCourseForUpdate(int courseId)
        {
            var instructorId = User.FindFirstValue(CustomClaimTypes.UserId);

            if (string.IsNullOrEmpty(instructorId))
            {
                return BadRequest(new { message = "Instructor is not logged in." });
            }

            var courseUpdate = await _courseService.GetCourseForUpdateAsync(courseId, instructorId);

            if (courseUpdate == null)
            {
                return NotFound(new { message = "Course not found or no updates available." });
            }

            if (courseUpdate.InstructorID != instructorId)
            {
                return Forbid("You are not authorized to update this course.");
            }



            return Ok(courseUpdate);
        }

        // Submit course update request (instructor must own the course)
        [Authorize(Roles = "Instructor")]
        [HttpPost("{courseId}/submitUpdate")]
        public async Task<IActionResult> SubmitUpdateAsync(int courseId, [FromForm] UpdateCourseRequestDTO updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest(new { message = "Update course request cannot be null." });
            }

            var course = await _courseService.GetByIdAsync(courseId);
            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Ensure the logged-in instructor is the owner of the course
            var instructorId = User.FindFirstValue(CustomClaimTypes.UserId);

            if (course.InstructorID != instructorId)  
            {
                return Forbid("You are not authorized to update this course." );
            }

            var success = await _courseService.SubmitCourseUpdateAsync(courseId, updateDto);

            if (!success)
            {
                return BadRequest(new { message = "Failed to submit course update." });
            }

            await _notificationService.NotifyAdminForCourseAdditionOrUpdateAsync(instructorId, "updated the existing course", courseId);
            await _notificationService.NotifyInstructorOnCourseApprovalStatusAsync(courseId, "pending approval for updation");

            return Ok(new { message = "Course update submitted successfully." });
        }

        // Approve pending course update (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("approve-update/{courseId}")]
        public async Task<IActionResult> ApprovePendingUpdate(int courseId)
        {
            try
            {
                var success = await _courseService.ApprovePendingUpdatesAsync(courseId);

                if (!success)
                {
                    return NotFound(new { message = "No pending updates found or approval failed." });
                }

                return Ok(new { message = "Course update approved successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Reject pending course updates (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost("reject-updates/{courseId}")]
        public async Task<IActionResult> RejectPendingUpdates(int courseId)
        {
            var result = await _courseService.RejectPendingUpdatesAsync(courseId);
            if (!result)
            {
                return NotFound(new { message = "No pending updates found for the course." });
            }

            return Ok( new { message = "Course update rejected successfully."});
        }



    }
}
