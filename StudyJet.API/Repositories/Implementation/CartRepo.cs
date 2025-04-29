using StudyJet.API.Data.Entities;
using StudyJet.API.Data;
using StudyJet.API.DTOs.Cart;
using StudyJet.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace StudyJet.API.Repositories.Implementation
{
    public class CartRepo : ICartRepo
    {
        private readonly ApplicationDbContext _context;

        public CartRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InsertCourseToCartAsync(string userId, int courseId, decimal price)
        {
            var user = await _context.Users.Include(u => u.Carts)
                                           .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (user.Carts.Any(c => c.CourseID == courseId))
                throw new InvalidOperationException("The course is already in the cart.");


            var newCartItem = new Cart
            {
                UserID = user.Id,
                CourseID = courseId,
                TotalPrice = price
            };

            user.Carts.Add(newCartItem);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CartItemDTO>> SelectCartItemsAsync(string userId)
        {
            return await _context.Carts
                .Where(c => c.User.Id == userId)
                .Include(c => c.Course)
                .Select(c => new CartItemDTO
                {
                    CartID = c.CartID,
                    CourseID = c.Course.CourseID,
                    CourseTitle = c.Course.Title,
                    CourseDescription = c.Course.Description,
                    InstructorName = c.Course.Instructor.FullName,
                    ImageUrl = c.Course.ImageUrl,
                    Price = c.Course.Price
                })
                .ToListAsync();
        }

        public async Task<Course> SelectCourseDetailsAsync(int courseId)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.CourseID == courseId);
        }

        public async Task<User?> SelectUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Carts)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> DeleteCourseFromCartAsync(string userId, int courseId)
        {
            var user = await _context.Users.Include(u => u.Carts)
                                           .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            var cartItem = user.Carts.FirstOrDefault(c => c.CourseID == courseId);
            if (cartItem == null) return false;

            user.Carts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsCourseInCartAsync(string userId, int courseId)
        {
            var user = await _context.Users
                .Include(u => u.Carts)
                .FirstOrDefaultAsync(u => u.Id == userId);  

            return user?.Carts.Any(c => c.CourseID == courseId) ?? false;
        }

    }
}
