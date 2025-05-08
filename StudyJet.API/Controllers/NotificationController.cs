using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Services.Implementation;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Security.Claims;

namespace StudyJet.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;

        public NotificationController(INotificationService notificationService, ICourseService courseService, IUserService userService)
        {
            _notificationService = notificationService;
            _courseService = courseService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            try
            {
                var notifications = await _notificationService.GetNotificationByUserIdAsync(userId);

                if (notifications == null || notifications.Count == 0)
                    return NoContent();

                return Ok(notifications);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while fetching notifications." });
            }
        }


        [HttpPost("notify-instructor-on-course-status")]
        public async Task<IActionResult> NotifyInstructorOnCourseStatus([FromBody] CourseApprovalRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.InstructorID) || string.IsNullOrEmpty(request.CourseID) || string.IsNullOrEmpty(request.Status))
                return BadRequest(new { message = "Invalid input, all fields are required." });

            if (!int.TryParse(request.CourseID, out int courseId))
                return BadRequest(new { message = "Invalid CourseId format." });

            try
            {
                var instructorExists = await _userService.GetUserByIdAsync(request.InstructorID);
                var courseExists = await _courseService.GetByIdAsync(courseId);

                if (instructorExists == null)
                {
                    return NotFound(new { message = "Instructor not found." });
                }

                if (courseExists == null)
                {
                    return NotFound(new { message = "Course not found." });
                }

                // notify instructor
                await _notificationService.NotifyInstructorOnCourseApprovalStatusAsync(courseId, request.Status);
                return Ok(new { message = "Notification sent to instructor" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while notifying the instructor." });
            }
        }



        [HttpPut("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var userId = User.FindFirst(CustomClaimTypes.UserId)?.Value;
            var userRole = User.FindFirstValue("role");

            if (string.IsNullOrEmpty(userId))
            {

                return Unauthorized(new { message = "User not authenticated." });
            }

            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(notificationId);

                if (notification == null)
                {
                    return NotFound(new { message = "Notification not found." });
                }

                if (userRole == "Admin" || notification.UserID == userId)
                {
                    notification.IsRead = true;
                    await _notificationService.UpdateNotificationAsync(notification);

                    return Ok(new { message = "Notification marked as read successfully" });
                }
                else
                {
                    return Forbid("You can only mark your own notifications as read");

                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while marking the notification as read." });
            }
        }


    }
}
