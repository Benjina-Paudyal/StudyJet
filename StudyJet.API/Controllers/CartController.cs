using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.DTOs.Cart;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Security.Claims;

namespace StudyJet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IWishlistService _wishlistService;
        private readonly ICourseService _courseService;

        public CartController(ICartService cartService, IWishlistService wishlistService, ICourseService courseService)
        {
            _cartService = cartService;
            _wishlistService = wishlistService;
            _courseService = courseService;
        }

        [HttpPost("{courseId}/add")]
        public async Task<IActionResult> AddToCart(int courseId)
        {
            try
            {
                var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                // Check if the course is already wishlist
                var isInWishlist = await _wishlistService.IsCourseInWishlistAsync(userId, courseId);
                if (isInWishlist)
                {
                    await _wishlistService.RemoveCourseFromWishlistAsync(userId, courseId);
                }
               
                await _cartService.AddToCartAsync(userId, courseId);

                return Ok(new { message = "Course added to cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var cartItems = await _cartService.GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                return NotFound(new { message = "No items found in the cart." });
            }

            var cartItemsDTO = cartItems.Select(cartItem => new CartItemDTO
            {
                CartID = cartItem.CartID,
                CourseID = cartItem.CourseID,
                CourseTitle = cartItem.CourseTitle,
                CourseDescription = cartItem.CourseDescription,
                InstructorName = cartItem.InstructorName,
                ImageUrl = cartItem.ImageUrl,
                Price = cartItem.Price
            }).ToList();

            return Ok(cartItemsDTO);
        }


        [HttpDelete("{courseId}/remove")]
        public async Task<IActionResult> RemoveFromCart(int courseId)
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var success = await _cartService.RemoveCourseFromCartAsync(userId, courseId);

            if (!success)
            {
                return NotFound(new { message = "Course not found in the cart." });
            }

            return Ok(new { message = "Course removed from cart successfully" });
        }


        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized( new { message = "User is not authenticated." });
            }

            var courseDetails = await _cartService.GetCourseDetailsAsync(courseId);

            if (courseDetails == null)
            {
                return NotFound(new { message = "Course not found."});
            }

            return Ok(courseDetails);
        }


        [HttpPost("move-to-wishlist/{courseId}")]
        public async Task<IActionResult> MoveToWishlist(int courseId)
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                var isInWishlist = await _wishlistService.IsCourseInWishlistAsync(userId, courseId);
                if (isInWishlist)
                {
                    return BadRequest(new { message = "Course is already in the wishlist." });
                }

                
                await _wishlistService.AddCourseToWishlistAsync(userId, courseId);

                await _cartService.RemoveCourseFromCartAsync(userId, courseId);

                return Ok(new { message = "Course moved to wishlist successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


    }
}
