using StudyJet.API.DTOs.Wishlist;

namespace StudyJet.API.Repositories.Interface
{
    public interface IWishlistRepo
    {
        Task<IEnumerable<WishlistCourseDTO>> SelectWishlistByIdAsync(string userId);
        Task<bool> InsertCourseToWishlistAsync(string userId, int courseId);
        Task<bool> DeleteCourseFromWishlistAsync(string userId, int courseId);
        Task<bool> IsCourseInWishlistAsync(string userId, int courseId);
    }
}
