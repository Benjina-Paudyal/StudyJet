using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;

namespace StudyJet.API.Repositories.Interface
{
    public interface IUserPurchaseCourseRepo
    {
        Task<List<UserPurchaseCourseDTO>> SelectPurchasedCourseAsync(string userId);
        Task<bool> HasUserPurchasedCourseAsync(string userId, int courseId);
        Task InsertPurchaseAsync(UserPurchaseCourse purchase);
        Task<User> SelectUserByIdAsync(string userId);
        Task<List<CourseResponseDTO>> SelectSuggestedCoursesAsync(string userId, int limit = 3);
        Task<List<int>> SelectPurchasedCourseIdsAsync(string userId);
        Task<int> SaveChangesAsync();

    }
}
