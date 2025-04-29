using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Wishlist;
using StudyJet.API.Repositories.Interface;

namespace StudyJet.API.Repositories.Implementation
{
    public class WishlistRepo : IWishlistRepo
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistCourseDTO>> SelectWishlistByIdAsync(string userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserID == userId)
                .Include(w => w.Course)
                .ThenInclude(c => c.Instructor)
                .Select(w => new WishlistCourseDTO
                {
                    CourseID = w.Course.CourseID,
                    Title = w.Course.Title,
                    Description = w.Course.Description,
                    ImageUrl = w.Course.ImageUrl,
                    Price = w.Course.Price,
                    InstructorName = w.Course.Instructor.FullName,
                    CategoryName = w.Course.Category.Name,
                    CreationDate = w.Course.CreationDate,
                    LastUpdatedDate = w.Course.LastUpdatedDate

                })
                .ToListAsync();   
        }

        public async Task<bool> InsertCourseToWishlistAsync(string userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Wishlists)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Check if the course exists
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseID == courseId);
            if (course == null)
                return false;

            // Ensure the course is not already in the wishlist
            if (user.Wishlists.Any(w => w.CourseID == courseId))
                return false;

            user.Wishlists.Add(new Wishlist { UserID = user.Id, CourseID = courseId });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCourseFromWishlistAsync(string userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Wishlists)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            var wishlistItem = user.Wishlists.FirstOrDefault(w => w.CourseID == courseId);
            if (wishlistItem == null)
                return false;

            user.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsCourseInWishlistAsync(string userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Wishlists) 
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Check if the course is already in the user's wishlist
            return user.Wishlists.Any(w => w.CourseID == courseId);
        }

    }
}
