using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepo _courseRepo;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;

        public CourseService(ICourseRepo courseRepo, INotificationService notificationService, IFileStorageService fileStorageService)
        {
            _courseRepo = courseRepo;
            _notificationService = notificationService;
            _fileStorageService = fileStorageService;
        }
        public async Task<List<CourseResponseDTO>> GetAllAsync()
        {
            var courseStatuses = new List<CourseStatus>
            {
                CourseStatus.Approved,
                CourseStatus.Pending,
                CourseStatus.Rejected
            };

            // fetch all the courses
            var originalCourses = await _courseRepo.SelectCourseByStatusAsync(courseStatuses);

            // fetch latest approved or pending updates per course
            var courseUpdatesStatus = new List<CourseStatus>
            {
                CourseStatus.Approved,
                CourseStatus.Pending
            };

            var courseUpdates = await _courseRepo.SelectCourseUpdatesByStatusAsync(courseUpdatesStatus);

            var combinedCourses = new List<CourseResponseDTO>();

            foreach (var course in originalCourses)
            {
                var latestUpdate = courseUpdates.FirstOrDefault(cu => cu.CourseID == course.CourseID);

                // Show rejected courses only if they have a pending update
                if (latestUpdate != null || course.Status != CourseStatus.Rejected)
                {
                    combinedCourses.Add(new CourseResponseDTO
                    {
                        CourseID = latestUpdate?.CourseID ?? course.CourseID,
                        Title = latestUpdate?.Title ?? course.Title,
                        Description = latestUpdate?.Description ?? course.Description,
                        ImageUrl = latestUpdate?.ImageUrl ?? course.ImageUrl,
                        Price = latestUpdate?.Price ?? course.Price,
                        InstructorID = course.InstructorID,
                        InstructorName = course.Instructor?.UserName,
                        CategoryID = course.CategoryID,
                        CategoryName = course.Category?.Name,
                        CreationDate = course.CreationDate,
                        LastUpdatedDate = latestUpdate?.SubmittedAt ?? course.LastUpdatedDate,
                        VideoUrl = latestUpdate?.VideoUrl ?? course.VideoUrl,
                        Status = latestUpdate?.Status ?? course.Status,  
                        IsUpdate = latestUpdate != null
                    });
                }
            }

            return combinedCourses
                .OrderByDescending(c => c.Status == CourseStatus.Pending)  
                .ThenByDescending(c => c.LastUpdatedDate)
                .ToList();
        }

        public async Task<List<CourseResponseDTO>> GetByInstructorIdAsync(string instructorId)
        {
            if (string.IsNullOrEmpty(instructorId))
            {
                throw new ArgumentException("InstructorId must be provided.");
            }

            var courses = await _courseRepo.SelectByInstructorIdAsync(instructorId);
            var pendingUpdates = await _courseRepo.SelectPendingUpdatesByInstructorIdAsync(instructorId);

            var result = new List<CourseResponseDTO>();

            foreach (var course in courses)
            {
                result.Add(new CourseResponseDTO
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Description = course.Description,
                    ImageUrl = course.ImageUrl,
                    Price = course.Price,
                    InstructorID = course.InstructorID,
                    InstructorName = course.Instructor?.UserName,
                    CategoryID = course.CategoryID,
                    CategoryName = course.Category?.Name,
                    CreationDate = course.CreationDate,
                    LastUpdatedDate = course.LastUpdatedDate,
                    VideoUrl = course.VideoUrl,
                    Status = course.Status,
                    IsUpdate = false,
                    UpdateId = null,
                });
            }

            foreach (var update in pendingUpdates)
            {
                result.Add(new CourseResponseDTO
                {
                    CourseID = update.CourseID,
                    Title = update.Title ?? update.Course.Title,
                    Description = update.Description ?? update.Course.Description,
                    ImageUrl = update.ImageUrl ?? update.Course.ImageUrl,
                    Price = update.Price ?? update.Course.Price,
                    InstructorID = update.Course.InstructorID,
                    InstructorName = update.Course.Instructor?.UserName,
                    CategoryID = update.Course.CategoryID,
                    CategoryName = update.Course.Category?.Name,
                    CreationDate = update.Course.CreationDate,
                    LastUpdatedDate = update.SubmittedAt,
                    VideoUrl = update.VideoUrl ?? update.Course.VideoUrl,
                    Status = update.Status,
                    IsUpdate = true,
                    UpdateId = update.ID,
                });
            }
            return result
                .OrderByDescending(c => c.Status == CourseStatus.Pending)
                .ThenByDescending(c => c.IsUpdate.HasValue ? c.LastUpdatedDate : c.CreationDate)
                .ToList();
        }

        public async Task<CourseResponseDTO> GetByIdAsync(int courseId)
        {
            var course = await _courseRepo.SelectByIdAsync(courseId);
            if (course == null) return null;

            var latestUpdate = await _courseRepo.SelectLatestCourseUpdateByCourseIdAsync(courseId);

            var result = new CourseResponseDTO
            {
                CourseID = course.CourseID,
                Title = latestUpdate?.Title ?? course.Title,
                Description = latestUpdate?.Description ?? course.Description,
                ImageUrl = latestUpdate?.ImageUrl ?? course.ImageUrl,
                Price = latestUpdate?.Price ?? course.Price,
                InstructorID = course.InstructorID,
                InstructorName = course.Instructor?.FullName,
                CategoryID = course.CategoryID,
                CategoryName = course.Category?.Name,
                CreationDate = course.CreationDate,
                LastUpdatedDate = latestUpdate?.SubmittedAt ?? course.LastUpdatedDate,
                VideoUrl = latestUpdate?.VideoUrl ?? course.VideoUrl,
                Status = latestUpdate?.Status ?? course.Status, // Use the status from the latest update if available
                IsUpdate = latestUpdate != null
            };

            return result;
        }

        public async Task<CourseUpdateDTO> GetCourseForUpdateAsync(int courseId, string instructorId)
        {
            return await _courseRepo.SelectCourseForUpdateAsync(courseId, instructorId);
        }

        public async Task<List<User>> GetEnrolledStudentsByCourseIdAsync(int courseId)
        {
            return await _courseRepo.SelectEnrolledStudentsByCourseIdAsync(courseId);
        }

        public async Task<List<CourseWithStudentsDTO>> GetCoursesWithStudentsForInstructorAsync(string instructorId)
        {
            return await _courseRepo.SelectCoursesWithStudentsForInstructorAsync(instructorId);
        }

        public async Task<List<CourseResponseDTO>> GetPopularCoursesAsync()
        {
            var courses = await _courseRepo.SelectPopularCoursesAsync();
            return courses.Select(c => new CourseResponseDTO
            {
                CourseID = c.CourseID,
                Title = c.Title,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                Price = c.Price,
                InstructorID = c.InstructorID,
                InstructorName = c.Instructor?.FullName,
                CategoryID = c.CategoryID,
                CategoryName = c.Category?.Name ?? "Unkown",
                CreationDate = c.CreationDate,
                LastUpdatedDate = c.LastUpdatedDate,
                VideoUrl = c.VideoUrl,


            }).ToList();
        }

        public async Task<List<CourseResponseDTO>> SearchCoursesAsync(string query)
        {
            return await _courseRepo.SearchCoursesAsync(query);
        }

        public async Task<IEnumerable<Course>> GetPendingCoursesAsync()
        {
            return await _courseRepo.SelectPendingCoursesAsync();
        }

        public async Task<IEnumerable<Course>> GetApprovedCoursesAsync()
        {
            return await _courseRepo.SelectApprovedCoursesAsync();
        }

        public async Task<int> GetTotalCoursesByInstructorIdAsync(string instructorId)
        {
            return await _courseRepo.SelectTotalCoursesByInstructorIdAsync(instructorId);
        }

        public async Task<int> AddAsync(CreateCourseRequestDTO courseDto)
        {

            string imagePath = null;
            if (courseDto.ImageFile != null)
            {
                imagePath = await _fileStorageService.SaveImageAsync(courseDto.ImageFile);
            }

            // Create new course
            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                Price = courseDto.Price,
                InstructorID = courseDto.InstructorID,
                CategoryID = courseDto.CategoryID,
                VideoUrl = courseDto.VideoUrl,
                ImageUrl = imagePath,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                Status = CourseStatus.Pending // Explicitly setting the satus to pending
            };

            // Insert course into the repository
            var result = await _courseRepo.InsertAsync(course);

            return result.CourseID;
        }

        public async Task<int> GetTotalCoursesAsync()
        {
            return await _courseRepo.SelectTotalCoursesAsync();
        }

        public async Task<bool> SubmitCourseUpdateAsync(int courseId, UpdateCourseRequestDTO updateDto)
        {
            if (updateDto == null)
                throw new ArgumentNullException(nameof(updateDto), "Update course request cannot be null.");

            return await _courseRepo.SubmitCourseUpdateAsync(courseId, updateDto, _fileStorageService);
        }

        public async Task<bool> ApproveCourseAsync(int courseId)
        {
            var course = await _courseRepo.SelectByIdAsync(courseId);
            if (course == null) return false;

            course.Status = CourseStatus.Approved;
            course.LastUpdatedDate = DateTime.Now;

            await _courseRepo.UpdateAsync(course);
            return true;
        }

        public async Task<bool> RejectCourseAsync(int courseId)
        {
            var course = await _courseRepo.SelectByIdAsync(courseId);
            if (course == null) return false;

            course.Status = CourseStatus.Rejected;
            course.LastUpdatedDate = DateTime.Now;

            await _courseRepo.UpdateAsync(course);
            return true;
        }

        public async Task<bool> ApprovePendingUpdatesAsync(int courseId)
        {
            var pendingUpdate = await _courseRepo.SelectPendingCourseUpdateAsync(courseId);
            if (pendingUpdate == null)
            {
                throw new InvalidOperationException("No pending updates found for the course.");
            }
            return await _courseRepo.ApprovePendingUpdatesAsync(courseId);
        }

        public async Task<bool> RejectPendingUpdatesAsync(int courseId)
        {
            var pendingUpdate = await _courseRepo.SelectPendingCourseUpdateAsync(courseId);
            if (pendingUpdate == null)
            {
                throw new InvalidOperationException("No pending updates found for the course.");
            }

            bool isRejected = await _courseRepo.RejectPendingUpdatesAsync(courseId);

            if (isRejected)
            {
                await _notificationService.NotifyInstructorOnCourseUpdateRejectionAsync(courseId);
            }

            return isRejected;
        }

        public async Task<bool> ExistsAsync(int CourseId)
        {
            return await _courseRepo.ExistsAsync(CourseId);
        }

        public async Task<bool> UpdateAsync(int id, UpdateCourseRequestDTO dto)
        {
            var existingCourse = await _courseRepo.SelectByIdAsync(id);
            if (existingCourse == null)
                throw new KeyNotFoundException("Course not found.");

            // Update only the provided fields
            if (!string.IsNullOrEmpty(dto.Title))
                existingCourse.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description))
                existingCourse.Description = dto.Description;

            if (dto.Price.HasValue)
                existingCourse.Price = dto.Price.Value;

            if (!string.IsNullOrEmpty(dto.VideoUrl))
                existingCourse.VideoUrl = dto.VideoUrl;

            if (dto.ImageFile != null)
            {
                var newImageUrl = await _fileStorageService.SaveImageAsync(dto.ImageFile);

                if (!string.IsNullOrEmpty(existingCourse.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCourse.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }
                existingCourse.ImageUrl = newImageUrl;
            }
            await _courseRepo.UpdateAsync(existingCourse);

            await _notificationService.NotifyAdminForCourseAdditionOrUpdateAsync(existingCourse.InstructorID, "updated a course");

            return true;

        }


    }
}
