using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ControllerTests
{
    public class NotificationControllerTest
    {

        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly NotificationController _controller;

        public NotificationControllerTest()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _mockCourseService = new Mock<ICourseService>();
            _mockUserService = new Mock<IUserService>();
            _controller = new NotificationController(_mockNotificationService.Object, _mockCourseService.Object, _mockUserService.Object);
        }


        [Fact]
        public async Task GetNotifications_ShouldReturnUnauthorized_WhenUserIdIsNotPresent()
        {
            // Arrange
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext = controllerContext;

            // Act
            var result = await _controller.GetNotifications();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            var resultValue = unauthorizedResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var message = messageProperty.GetValue(resultValue)?.ToString();
            Assert.Equal("User not authenticated", message);
        }

        [Fact]
        public async Task GetNotifications_ShouldReturnNoContent_WhenNoNotificationsFound()
        {
            // Arrange
            var userId = "user123";
            var notifications = new List<Notification>(); 
            _mockNotificationService.Setup(s => s.GetNotificationByUserIdAsync(userId)).ReturnsAsync(notifications);

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(CustomClaimTypes.UserId, userId) }));
            _controller.ControllerContext = controllerContext;

            // Act
            var result = await _controller.GetNotifications();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }



        [Fact]
        public async Task NotifyInstructorOnCourseStatus_ShouldReturnBadRequest_WhenFieldsAreMissing()
        {
            // Arrange
            var invalidRequest = new CourseApprovalRequestDTO
            {
                InstructorID = "",
                CourseID = "",
                Status = ""
            };

            // Act
            var result = await _controller.NotifyInstructorOnCourseStatus(invalidRequest);

            // Assert 
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = badRequestResult.Value;

            // Use reflection 
            var messageProperty = resultValue.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var message = messageProperty.GetValue(resultValue)?.ToString();
            Assert.Equal("Invalid input, all fields are required.", message);
        }


        [Fact]
        public async Task NotifyInstructorOnCourseStatus_ShouldReturnOk_WhenNotificationIsSent()
        {
            // Arrange
            var validRequest = new CourseApprovalRequestDTO
            {
                InstructorID = "instructor123",
                CourseID = "123",
                Status = "Approved"
            };

            _mockUserService.Setup(service => service.GetUserByIdAsync("instructor123"))
                            .ReturnsAsync(new User()); 

            var courseResponse = new CourseResponseDTO
            {
                CourseID = 123,
                Title = "Course Title",
                Description = "Course Description",
                InstructorID = "instructor123",
                InstructorName = "Instructor Name",
                Status = CourseStatus.Pending 
            };

            _mockCourseService.Setup(service => service.GetByIdAsync(123))
                              .ReturnsAsync(courseResponse); 

            _mockNotificationService.Setup(service => service.NotifyInstructorOnCourseApprovalStatusAsync(123, "Approved"))
                                    .Returns(Task.CompletedTask); 

            // Act
            var result = await _controller.NotifyInstructorOnCourseStatus(validRequest);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var resultValue = okResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty);
            var message = messageProperty.GetValue(resultValue)?.ToString();
            Assert.Equal("Notification sent to instructor", message);
        }




        [Fact]
        public async Task MarkNotificationAsRead_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.ControllerContext = controllerContext;

            // Act
            var result = await _controller.MarkNotificationAsRead(1);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            // Use reflection to get the message property from the anonymous object in the response
            var resultValue = unauthorizedResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty); // Ensure the 'message' property exists
            var message = messageProperty.GetValue(resultValue)?.ToString();

            // Assert the message value to confirm the response is correct
            Assert.Equal("User not authenticated.", message);
        }


        [Fact]
        public async Task MarkNotificationAsRead_ShouldReturnOk_WhenNotificationIsMarkedAsReadSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var notificationId = 1;
            var notification = new Notification { UserID = userId, IsRead = false }; // Notification belongs to the user and is not read.

            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(CustomClaimTypes.UserId, userId),
                        new Claim("role", "User")
                    }))
                }
            };
            _controller.ControllerContext = controllerContext;

            _mockNotificationService.Setup(s => s.GetNotificationByIdAsync(notificationId))
                .ReturnsAsync(notification);

            _mockNotificationService.Setup(s => s.UpdateNotificationAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask); 

            // Act
            var result = await _controller.MarkNotificationAsRead(notificationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            
            var resultValue = okResult.Value;
            var messageProperty = resultValue.GetType().GetProperty("message");

            Assert.NotNull(messageProperty); 
            var message = messageProperty.GetValue(resultValue)?.ToString();
            Assert.Equal("Notification marked as read successfully", message);
        }







    }
}
