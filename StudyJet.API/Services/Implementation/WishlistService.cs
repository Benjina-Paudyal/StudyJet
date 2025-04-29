using StudyJet.API.DTOs.Wishlist;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepo _wishlistRepo;

        public WishlistService(IWishlistRepo wishlistRepo)
        {
            _wishlistRepo = wishlistRepo;
        }

        public async Task<IEnumerable<WishlistCourseDTO>> GetWishlistAsync(string userId)
        {
            return await _wishlistRepo.SelectWishlistByIdAsync(userId);
        }

        public async Task<bool> AddCourseToWishlistAsync(string userId, int courseId)
        {
            return await _wishlistRepo.InsertCourseToWishlistAsync(userId, courseId);
        }

        public async Task<bool> RemoveCourseFromWishlistAsync(string userId, int courseId)
        {
            return await _wishlistRepo.DeleteCourseFromWishlistAsync(userId, courseId);
        }

        public async Task<bool> IsCourseInWishlistAsync(string userId, int courseId)
        {
            var wishlistItems = await _wishlistRepo.SelectWishlistByIdAsync(userId);
            return wishlistItems.Any(item => item.CourseID == courseId);
        }

    }
}
