using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Cart;

namespace StudyJet.API.Repositories.Interface
{
    public interface ICartRepo
    {
        Task InsertCourseToCartAsync(string userId, int courseId, decimal price);
        Task<IEnumerable<CartItemDTO>> SelectCartItemsAsync(string userId);
        Task<Course> SelectCourseDetailsAsync(int courseId);
        Task<User?> SelectUserByIdAsync(string userId);
        Task<bool> DeleteCourseFromCartAsync(string userId, int courseId);
        Task<bool> IsCourseInCartAsync(string userId, int courseId);


    }
}
