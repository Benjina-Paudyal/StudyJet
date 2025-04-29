using StudyJet.API.DTOs.Course;

namespace StudyJet.API.Services.Interface
{
    public interface IUserPurchaseCourseService
    {
        Task<List<UserPurchaseCourseDTO>> GetPurchasedCoursesAsync(string userId);
        Task<bool> PurchaseCourseAsync(string userId, List<int> courseIds);
        Task<List<CourseResponseDTO>> GetSuggestedCoursesAsync(string userId);
        Task<string> CreateCheckoutSession(string userId, List<int> courseIds);
    }
}
