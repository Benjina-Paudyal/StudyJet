using StudyJet.API.DTOs.Wishlist;

namespace StudyJet.API.Services.Interface
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistCourseDTO>> GetWishlistAsync(string userId);
        Task<bool> AddCourseToWishlistAsync(string userId, int courseId);
        Task<bool> RemoveCourseFromWishlistAsync(string userId, int courseId);
        Task<bool> IsCourseInWishlistAsync(string userId, int courseId);
    }
}
