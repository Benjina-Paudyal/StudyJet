using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Repositories.Implementation
{
    public class CourseRepo: ICourseRepo
    {
        private readonly ApplicationDbContext _context;

        public CourseRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> SelectAllAsync()
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .ToListAsync();

        }

        public async Task<List<Course>> SelectCourseByStatusAsync(List<CourseStatus> status)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Where(c => status.Contains(c.Status))
                .ToListAsync();
        }

        public async Task<List<CourseUpdate>> SelectCourseUpdatesByStatusAsync(List<CourseStatus> status)
        {
            return await _context.CourseUpdates
                .Include(cu => cu.Course)
                .ThenInclude(c => c.Instructor)
                .Include(cu => cu.Course)
                .ThenInclude(c => c.Category)
                .Where(cu => status.Contains(cu.Status))
                .GroupBy(cu => cu.CourseID)
                .Select(g => g.OrderByDescending(cu => cu.SubmittedAt).First())
                .ToListAsync();
        }

        public async Task<CourseUpdate?> SelectLatestCourseUpdateByCourseIdAsync(int courseId)
        {
            return await _context.CourseUpdates
                .Where(cu => cu.CourseID == courseId &&
                             (cu.Status == CourseStatus.Pending || cu.Status == CourseStatus.Approved))
                .OrderByDescending(cu => cu.SubmittedAt)
                .FirstOrDefaultAsync();
        }


        public async Task<Course> SelectByIdAsync(int courseId)
        {
            var course = await _context.Courses
                                       .Include(c => c.Instructor)
                                       .Include(c => c.Category)
                                       .FirstOrDefaultAsync(c => c.CourseID == courseId);

            if (course == null) 
                throw new KeyNotFoundException($"Course with ID {courseId} was not found.");

            return course;
        }

        public async Task<List<Course>> SelectByIdsAsync(List<int> courseIds)
        {
            var courses = await _context.Courses
                                        .Include(c => c.Instructor)
                                        .Include(c => c.Category)
                                        .Where(c => courseIds.Contains(c.CourseID)) 
                                        .ToListAsync();
           
            if (courses == null || !courses.Any()) 
                throw new KeyNotFoundException("Courses not found for the provided IDs.");

            return courses;
        }

        public async Task<(int CourseID, string Title)> SelectTitleByIdAsync(int courseId)
        {
            var course = await _context.Courses
                .Where(c => c.CourseID == courseId)
                .Select(c => new { c.CourseID, c.Title })
                .FirstOrDefaultAsync();

            if (course == null)
                throw new KeyNotFoundException("Course not found.");

            return (course.CourseID, course.Title);// Tuple for notification service
        } 


        public async Task<Course> InsertAsync(Course course)
        {
            if (course == null) 
                throw new ArgumentNullException(nameof(course), "The course cannot be null.");

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> UpdateAsync(Course course)
        {
            var existingCourse = await _context.Courses.FindAsync(course.CourseID);
            if (existingCourse == null)
            {
                throw new KeyNotFoundException($"Course with ID {course.CourseID} was not found.");
            }

            _context.Courses.Update(course);
            var updated = await _context.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteAsync(int courseId)
        {
            try
            {
                var course = await SelectByIdAsync(courseId);

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the course with ID {courseId}.", ex);
            }
        }

        public async Task<bool> ExistsAsync(int courseId)
        {
            return await _context.Courses.AnyAsync(c => c.CourseID == courseId);
        }

        public async Task<List<Course>> SelectPopularCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Where(c => c.IsPopular && c.Status == CourseStatus.Approved)
                .ToListAsync();
        }

        public async Task<List<CourseResponseDTO>> SearchCoursesAsync(string query)
        {
            var normalizedQuery = query.ToLower();
           
            var courses = await _context.Courses
                .Include(c => c.Instructor) 
                .Include(c => c.Category)   
                .Where(c => c.Title.ToLower().Contains(normalizedQuery) ||
                            c.Description.ToLower().Contains(normalizedQuery) ||
                            c.Category.Name.ToLower().Contains(normalizedQuery) ||
                            (c.Instructor.FullName != null && c.Instructor.FullName.ToLower().Contains(normalizedQuery)))
                .ToListAsync();

            var courseDTOs = courses.Select(course => new CourseResponseDTO
            {
                CourseID = course.CourseID,
                Title = course.Title,
                Description = course.Description,
                ImageUrl = course.ImageUrl,
                Price = course.Price,
                InstructorName = course.Instructor?.FullName, 
                CategoryName = course.Category?.Name 
            }).ToList();

            return courseDTOs;
        }

        public async Task<int> SelectTotalCoursesAsync()
        {
            return await _context.Courses.CountAsync();
        }

        public async Task<IEnumerable<Course>> SelectPendingCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.Status == CourseStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> SelectApprovedCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.Status == CourseStatus.Approved)
                .ToListAsync();
        }

        public async Task<bool> ApproveCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            course.Status = CourseStatus.Approved; 
            course.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;
            course.Status = CourseStatus.Rejected;
            course.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectPendingUpdatesAsync(int courseId)
        {
            var pendingUpdate = await _context.CourseUpdates
                .FirstOrDefaultAsync(cu => cu.CourseID == courseId && cu.Status == CourseStatus.Pending);
            if (pendingUpdate == null) return false;
            pendingUpdate.Status = CourseStatus.Rejected;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SelectTotalCoursesByInstructorIdAsync(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentException("InstructorId must be provided.");

            var totalCourses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .CountAsync();

            return totalCourses;
        }

        public async Task<List<User>> SelectEnrolledStudentsByCourseIdAsync(int courseId)
        {
            var students = await _context.Users
                .Include(u => u.UserCoursePurchases)
                .Where(u => u.UserCoursePurchases.Any(upc => upc.CourseID == courseId))
                .ToListAsync();

            return students;
        }

        public async Task<List<CourseWithStudentsDTO>> SelectCoursesWithStudentsForInstructorAsync(string instructorId)
        {
            var instructorCourses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .Include(c => c.UserPurchaseCourses) 
                .ToListAsync();

            var coursesWithStudents = new List<CourseWithStudentsDTO>();

            foreach (var course in instructorCourses)
            {
                var courseWithStudents = new CourseWithStudentsDTO
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    ImageUrl = course.ImageUrl, 
                    Students = new List<StudentDTO>()
                };

                foreach (var userPurchaseCourse in course.UserPurchaseCourses)
                {
                    var student = await _context.Users
                        .Where(u => u.Id == userPurchaseCourse.UserID)
                        .Select(u => new StudentDTO
                        {

                            FullName = u.FullName,
                            UserName = u.UserName,
                            ProfilePictureUrl = u.ProfilePictureUrl,
                            Email = u.Email,
                            PurchaseDate = userPurchaseCourse.PurchaseDate
                        })
                        .FirstOrDefaultAsync();

                    if (student != null)
                    {
                        courseWithStudents.Students.Add(student);
                    }
                }

                coursesWithStudents.Add(courseWithStudents);
            }

            return coursesWithStudents;
        }

        public async Task<CourseUpdateDTO> SelectCourseForUpdateAsync(int courseId, string instructorId)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == courseId && c.InstructorID == instructorId);

            if (course == null) return null;

            return new CourseUpdateDTO
            {
                Title = course.Title,
                Description = course.Description,
                ImageUrl = course.ImageUrl,
                Price = course.Price,
                VideoUrl = course.VideoUrl
            };
        }

        public async Task<CourseUpdate> SelectPendingCourseUpdateAsync(int courseId)
        {
            return await _context.CourseUpdates
                .FirstOrDefaultAsync(cu => cu.CourseID == courseId && cu.Status == CourseStatus.Pending);
        }

        public async Task<bool> ApprovePendingUpdatesAsync(int courseId)
        {
            try
            {
                var pendingUpdate = await _context.CourseUpdates
                    .FirstOrDefaultAsync(cu => cu.CourseID == courseId && cu.Status == CourseStatus.Pending);

                if (pendingUpdate == null) return false;

                var course = await _context.Courses.FindAsync(courseId);
                if (course == null) return false;

                // Archive the previously rejected course, if it exists
                var rejectedCourse = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseID == courseId && c.Status == CourseStatus.Rejected);

                if (rejectedCourse != null)
                {
                    rejectedCourse.IsArchived = true; // Mark rejected course as archived
                }

                // Apply changes from the pending update
                course.Title = pendingUpdate.Title ?? course.Title;
                course.Description = pendingUpdate.Description ?? course.Description;
                course.Price = pendingUpdate.Price ?? course.Price;
                course.ImageUrl = pendingUpdate.ImageUrl ?? course.ImageUrl;
                course.VideoUrl = pendingUpdate.VideoUrl ?? course.VideoUrl;
                course.LastUpdatedDate = DateTime.UtcNow;

                // Mark the update as approved and remove it
                course.IsArchived = false;
                course.Status = CourseStatus.Approved;
                pendingUpdate.Status = CourseStatus.Approved;
                _context.CourseUpdates.Remove(pendingUpdate);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while approving the pending update for course ID {courseId}.", ex);
            }
        }

        public async Task<List<Course>> SelectByInstructorIdAsync(string instructorId)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.CourseUpdates)
                .Where(c => c.InstructorID == instructorId)
                .ToListAsync();
        }

        public async Task<List<CourseUpdate>> SelectPendingUpdatesByInstructorIdAsync(string instructorId)
        {
            return await _context.CourseUpdates
                .Include(cu => cu.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(cu => cu.Course)
                    .ThenInclude(c => c.Category)
                .Where(cu => cu.Course.InstructorID == instructorId &&
                             cu.Status == CourseStatus.Pending &&
                             !cu.Course.IsArchived)
                .OrderByDescending(cu => cu.SubmittedAt)
                .ToListAsync();
        }

        public async Task<bool> SubmitCourseUpdateAsync(int courseId, UpdateCourseRequestDTO updateDto, IFileStorageService fileService)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            string imageUrl = course.ImageUrl;

            if (updateDto.ImageFile != null)
            {
                imageUrl = await fileService.SaveImageAsync(updateDto.ImageFile);
            }

            var courseUpdate = new CourseUpdate
            {
                CourseID = courseId,
                Title = updateDto.Title,
                Description = updateDto.Description,
                Price = updateDto.Price,
                VideoUrl = updateDto.VideoUrl,
                ImageUrl = imageUrl,
                SubmittedAt = DateTime.UtcNow,
                Status = CourseStatus.Pending // Resubmitting for approval
            };

            _context.CourseUpdates.Add(courseUpdate);
            await _context.SaveChangesAsync();
            return true;
        }




    }
}
