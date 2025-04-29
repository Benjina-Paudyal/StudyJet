using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Cart;

namespace StudyJet.API.Services.Interface
{
    public interface ICartService
    {
        Task AddToCartAsync(string userId, int courseId);
        Task<IEnumerable<CartItemDTO>> GetCartItemsAsync(string userId);
        Task<Course> GetCourseDetailsAsync(int courseId);
        Task<bool> RemoveCourseFromCartAsync(string userId, int courseId);
        Task<bool> IsCourseInCartAsync(string userId, int courseId);
    }
}
