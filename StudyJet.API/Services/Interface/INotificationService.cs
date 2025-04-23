using StudyJet.API.Data.Entities;

namespace StudyJet.API.Services.Interface
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message);
        Task<Notification> GetNotificationByIdAsync(int notificationId);
        Task UpdateNotificationAsync(Notification notification);
        Task<List<Notification>> GetNotificationByUserIdAsync(string userId);


        Task NotifyAdminForCourseAdditionOrUpdateAsync(string instructorId, string message, int? courseId = null);
        Task NotifyInstructorOnCourseApprovalStatusAsync(int courseId, string status);
        Task NotifyInstructorOnStudentPurchaseAsync(int courseId, string studentId, string studentFullName);
        Task NotifyInstructorOnCourseUpdateRejectionAsync(int courseId);

    }
}
