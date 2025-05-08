using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Repositories.Implementation;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepo _notificationRepo;
        private readonly UserManager<User> _userManager;
        private readonly ICourseRepo _courseRepo;

        public NotificationService(INotificationRepo notificationRepository, UserManager<User> userManager, ICourseRepo courseRepo)
        {
            _notificationRepo = notificationRepository;
            _userManager = userManager;
            _courseRepo = courseRepo;
        }

        public async Task CreateNotificationAsync(string userId, string message)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.");

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty.");

            try
            {
                var notification = new Notification
                {
                    UserID = userId,
                    Message = message,
                    IsRead = false,
                    DateCreated = DateTime.UtcNow
                };

                Console.WriteLine($"Creating notification for UserId: {userId} - Message: {message}");

                await _notificationRepo.CreateAsync(notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating notification: {ex.Message}");
                throw;
            }
        }

        public async Task<Notification> GetNotificationByIdAsync(int notificationId)
        {
            if (notificationId <= 0)
                throw new ArgumentException("Invalid notification ID.");

            return await _notificationRepo.SelectByIdAsync(notificationId);
        }

        public async Task<List<Notification>> GetNotificationByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.");

            Console.WriteLine($"Fetching notifications for userId: {userId}");

            var notifications = await _notificationRepo.SelectByUserIdAsync(userId);
            Console.WriteLine($"Found {notifications.Count} notifications for userId: {userId}");

            return notifications;
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _notificationRepo.UpdateAsync(notification);
        }

        public async Task NotifyAdminForCourseAdditionOrUpdateAsync(string instructorId, string message, int? courseId = null)
        {

            if (string.IsNullOrEmpty(instructorId))
                throw new ArgumentException("Instructor ID cannot be null or empty.");

            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Message cannot be null or empty.");

            try
            {
                var instructor = await _userManager.FindByIdAsync(instructorId);
                if (instructor == null)
                    throw new Exception("Instructor not found");

                string instructorName = instructor.FullName;

                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");


                foreach (var adminUser in adminUsers)
                {
                    if (adminUser.Id != instructorId)
                    {
                        string notificationMessage = $"Instructor {instructorName} {message.TrimEnd('.')}";

                        await _notificationRepo.CreateAsync(new Notification
                        {
                            UserID = adminUser.Id,
                            Message = notificationMessage,
                            IsRead = false,
                            DateCreated = DateTime.UtcNow,
                            CourseID = courseId
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying admins: {ex.Message}");
                throw;
            }
        }

        public async Task NotifyInstructorOnCourseApprovalStatusAsync(int courseId, string status)
        {
            if (courseId <= 0)
                throw new ArgumentException("Invalid course ID.");

            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status cannot be null or empty.");
            
                var (id, title) = await _courseRepo.SelectTitleByIdAsync(courseId);

                var instructor = await _userManager.Users
                    .Where(u => u.CoursesTaught.Any(c => c.CourseID == courseId))
                    .FirstOrDefaultAsync();

                if (instructor != null)
                {
                    string instructorName = instructor.FullName;
                    string message = $"Your course {title} has been {status}.";
                 
                    await _notificationRepo.CreateAsync(new Notification
                    {
                        UserID = instructor.Id,
                        Message = message,
                        IsRead = false,
                        DateCreated = DateTime.UtcNow,
                        CourseID = courseId
                    });
                }
        }

        public async Task NotifyInstructorOnStudentPurchaseAsync(int courseId, string studentId, string studentFullName)
        {
            if (courseId <= 0)
                throw new ArgumentException("Invalid course ID.");

            if (string.IsNullOrEmpty(studentId))
                throw new ArgumentException("Student ID cannot be null or empty.");

            if (string.IsNullOrEmpty(studentFullName))
                throw new ArgumentException("Student full name cannot be null or empty.");

            try
            {
                var courseInfo = await _courseRepo.SelectTitleByIdAsync(courseId);

                var instructor = await _userManager.Users
                    .Where(u => u.CoursesTaught.Any(c => c.CourseID == courseId))
                    .FirstOrDefaultAsync();

                if (instructor != null)
                {
                    string message = $"Student {studentFullName} has purchased your course {courseInfo.Title}.";

                    await _notificationRepo.CreateAsync(new Notification
                    {
                        UserID = instructor.Id,
                        Message = message,
                        IsRead = false,
                        DateCreated = DateTime.UtcNow,
                        CourseID = courseId
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error notifying instructor: {ex.Message}");
                throw;
            }
        }

        public async Task NotifyInstructorOnCourseUpdateRejectionAsync(int courseId)
        {
            if (courseId <= 0)
                throw new ArgumentException("Invalid course ID.");

            try
            {
                var courseInfo = await _courseRepo.SelectTitleByIdAsync(courseId);

                var instructor = await _userManager.Users
                    .Where(u => u.CoursesTaught.Any(c => c.CourseID == courseId))
                    .FirstOrDefaultAsync();

                if (instructor != null)
                {
                    string message = $"Your update request for course '{courseInfo.Title}' has been rejected.";

                    await _notificationRepo.CreateAsync(new Notification
                    {
                        UserID = instructor.Id,
                        Message = message,
                        IsRead = false,
                        DateCreated = DateTime.UtcNow,
                        CourseID = courseId
                    });


                }
            }
            catch (Exception)
            {
                throw; 
            }
        }


    }
}
