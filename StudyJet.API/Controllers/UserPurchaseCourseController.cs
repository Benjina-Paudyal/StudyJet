using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Security.Claims;

namespace StudyJet.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user/purchases")]
    public class UserPurchaseCourseController : ControllerBase
    {
        private readonly IUserPurchaseCourseService _userPurchaseCourseService;
        private readonly INotificationService _notificationService;
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;

        public UserPurchaseCourseController(IUserPurchaseCourseService userPurchaseCourseService, INotificationService notificationService, ICartService cartService, UserManager<User> userManager)
        {
            _userPurchaseCourseService = userPurchaseCourseService;
            _notificationService = notificationService;
            _cartService = cartService;
            _userManager = userManager;
        }


        [HttpGet("my-courses")]
        public async Task<IActionResult> GetPurchasedCourse()
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            var courses = await _userPurchaseCourseService.GetPurchasedCoursesAsync(userId);
            if (!courses.Any())
                return Ok(new { message = "No purchased courses found.", courses = new List<string>() });

            return Ok(courses);
        }


        [HttpGet("suggested-courses")]
        public async Task<IActionResult> GetSuggestedCourses()
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            var suggestedCourses = await _userPurchaseCourseService.GetSuggestedCoursesAsync(userId);

            if (suggestedCourses == null || !suggestedCourses.Any())
                return Ok(new List<DTOs.Course.CourseResponseDTO>());
            return Ok(suggestedCourses);
        }


        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] PurchaseRequestDTO request)
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found" });

            if (request.CourseIDs == null || request.CourseIDs.Count == 0)
                return BadRequest(new { message = "No courses provided." });


            var sessionUrl = await _userPurchaseCourseService.CreateCheckoutSession(userId, request.CourseIDs);

            if (sessionUrl == null)
                return BadRequest(new { message = "Invalid course." });

            return Ok(new { url = sessionUrl });
        }


        [AllowAnonymous]
        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Check for Stripe-Signature header
            if (!Request.Headers.ContainsKey("Stripe-Signature"))
            {
                return Unauthorized(new { message = "Stripe-Signature header is required" });
            }

            var stripeSignature = Request.Headers["Stripe-Signature"];

            try
            {
                // Construct the Stripe event with the webhook secret for verification
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    "whsec_d0e755d201723af31f5ba7e2d91b53feac2ff422b2f5f92326ef49524f5aedfd"
                );

                // Check if the event is related to a completed checkout session
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session?.Metadata == null ||
                        !session.Metadata.ContainsKey("username") ||
                        !session.Metadata.ContainsKey("courseIds"))
                    {
                        return BadRequest(new { message = "Metadata must include username and courseIds" });
                    }

                    var username = session.Metadata["username"];
                    var courseIds = session.Metadata["courseIds"]
                        .Split(',')
                        .Select(int.Parse)
                        .ToList();

                    // Process purchase
                    var purchaseSuccess = await _userPurchaseCourseService.PurchaseCourseAsync(username, courseIds);
                    if (!purchaseSuccess)
                    {
                        return BadRequest(new { message = "Failed to process purchase" });
                    }

                    // Fetch the student based on the username
                    var student = await _userManager.FindByNameAsync(username);
                    if (student == null)
                    {
                        return BadRequest(new { message = "Student not found" });
                    }

                    var studentDisplayName = !string.IsNullOrWhiteSpace(student.FullName)
                        ? student.FullName
                        : student.UserName;

                    // Process each course purchased
                    foreach (var courseId in courseIds)
                    {
                        await _notificationService.NotifyInstructorOnStudentPurchaseAsync(
                            courseId,
                            student.Id,
                            studentDisplayName
                        );

                        var cartItemRemoved = await _cartService.RemoveCourseFromCartAsync(username, courseId);
                        
                    }
                }
            }
            catch (StripeException e)
            {
                return BadRequest(new { message = $"Stripe error: {e.Message}" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Internal server error during webhook processing." });
            }

            return Ok();
        }
    }
}
