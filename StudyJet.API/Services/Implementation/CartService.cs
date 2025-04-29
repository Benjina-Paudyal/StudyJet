using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Cart;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class CartService: ICartService
    {
        private readonly ICartRepo _cartRepo;

        public CartService(ICartRepo cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task AddToCartAsync(string userId, int courseId)
        {
            // Get the course
            var course = await _cartRepo.SelectCourseDetailsAsync(courseId);
            if (course == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            // Check if course is already in the cart
            var existingCartItems = await _cartRepo.SelectCartItemsAsync(userId);
            if (existingCartItems.Any(c => c.CourseID == courseId))
            {
                throw new InvalidOperationException("The course is already in the cart.");
            }

            // Add the course 
            await _cartRepo.InsertCourseToCartAsync(userId, courseId, course.Price);
        }

        public async Task<IEnumerable<CartItemDTO>> GetCartItemsAsync(string userId)
        {
            return await _cartRepo.SelectCartItemsAsync(userId);
        }

        public async Task<Course> GetCourseDetailsAsync(int courseId)
        {
            return await _cartRepo.SelectCourseDetailsAsync(courseId);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _cartRepo.SelectUserByIdAsync(userId);
        }

        public async Task<bool> RemoveCourseFromCartAsync(string userId, int courseId)
        {
            return await _cartRepo.DeleteCourseFromCartAsync(userId, courseId);
        }

        public async Task<bool> IsCourseInCartAsync(string userId, int courseId)
        {
            return await _cartRepo.IsCourseInCartAsync(userId, courseId);
        }
    }
}
