using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.DTOs.Wishlist;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Security.Claims;

namespace StudyJet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICartService _cartService;

        public WishlistController(IWishlistService wishlistService, ICartService cartService)
        {
            _wishlistService = wishlistService;
            _cartService = cartService;
        }


        // Get the authenticated user's wishlist items
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            try
            {
                var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var wishlist = await _wishlistService.GetWishlistAsync(userId);

                // Instead of returning 404 if empty, just return OK with empty list
                return Ok(wishlist ?? Enumerable.Empty<WishlistCourseDTO>());

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        // Add a course to the authenticated user's wishlist
        [HttpPost("{courseId}")]
        public async Task<IActionResult> AddToWishlist(int courseId)
        {
            try
            {
                var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

                if (userId == null)
                {
                    return Unauthorized(new { success = false, message = "User is not authenticated." });
                }


                var isInCart = await _cartService.IsCourseInCartAsync(userId, courseId);
                if (isInCart)
                {
                    await _cartService.RemoveCourseFromCartAsync(userId, courseId);
                }

                // Add course to wishlist
                var result = await _wishlistService.AddCourseToWishlistAsync(userId, courseId);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to add course to wishlist. Course might already be in the wishlist." });
                }

                return Ok(new { success = true, message = "Course added to wishlist." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }


        // Check if a course is in the authenticated user's wishlist
        [HttpGet("is-in-wishlist/{courseId}")]
        public async Task<IActionResult> IsCourseInWishlist(int courseId)
        {
            try
            {
                var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User is not authenticated." });
                }

                var isInWishlist = await _wishlistService.IsCourseInWishlistAsync(userId, courseId);

                return Ok(new { success = true, isInWishlist });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }


        // Remove a course from the authenticated user's wishlist
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> RemoveFromWishlist(int courseId)
        {
            try
            {
                var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User is not authenticated." });
                }

                // Remove course from wishlist
                var result = await _wishlistService.RemoveCourseFromWishlistAsync(userId, courseId);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to remove course from wishlist. Course might not exist in the wishlist." });
                }

                return Ok(new { success = true, message = "Course removed from wishlist." });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

    }
}
