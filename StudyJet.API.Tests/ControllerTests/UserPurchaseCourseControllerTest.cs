using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using StudyJet.API.Controllers;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using System.Text;
using Newtonsoft.Json;
using Stripe.Checkout;
using Stripe;

namespace StudyJet.API.Tests.ControllerTests
{
    public class UserPurchaseCourseControllerTest
    {
        private readonly Mock<IUserPurchaseCourseService> _mockUserPurchaseCourseService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ICartService> _mockCartService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly UserPurchaseCourseController _controller;

        public UserPurchaseCourseControllerTest()
        {
            _mockUserPurchaseCourseService = new Mock<IUserPurchaseCourseService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockCartService = new Mock<ICartService>();
            _mockConfiguration = new Mock<IConfiguration>();


            // Mocking UserManager<User>
            var mockUserStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                mockUserStore.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<User>>(),
                new List<IUserValidator<User>>(),
                new List<IPasswordValidator<User>>(),
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<User>>>()
            );

            _controller = new UserPurchaseCourseController(
                _mockUserPurchaseCourseService.Object,
                _mockNotificationService.Object,
                _mockCartService.Object,
                _mockUserManager.Object, 
                _mockConfiguration.Object
            );
        }
    

        [Fact]
        public async Task GetPurchasedCourse_ShouldReturnOkWithCourses_WhenCoursesExist()
        {
            // Arrange
            var userId = "user123";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(CustomClaimTypes.UserId, userId)
                }))
                }
            };

            var courses = new List<UserPurchaseCourseDTO>
        {
            new UserPurchaseCourseDTO { CourseID = 1, Title = "Course 1" },
            new UserPurchaseCourseDTO { CourseID = 2, Title = "Course 2" }
        };

            _mockUserPurchaseCourseService
                .Setup(s => s.GetPurchasedCoursesAsync(userId))
                .ReturnsAsync(courses);

            // Act
            var result = await _controller.GetPurchasedCourse();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourses = Assert.IsAssignableFrom<IEnumerable<UserPurchaseCourseDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourses.Count());
        }

        [Fact]
        public async Task GetPurchasedCourse_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            // Arrange: no user ID claim
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) 
                }
            };

            // Act
            var result = await _controller.GetPurchasedCourse();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            var messageProp = unauthorizedResult.Value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var messageValue = messageProp.GetValue(unauthorizedResult.Value)?.ToString();
            Assert.Equal("User not found", messageValue);
        }




        [Fact]
        public async Task GetSuggestedCourses_ShouldReturnOkWithSuggestedCourses_WhenCoursesExist()
        {
            // Arrange
            var userId = "user123";
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(CustomClaimTypes.UserId, userId) }));
            _controller.ControllerContext = context;

            var suggestedCourses = new List<CourseResponseDTO>
            {
                new CourseResponseDTO { CourseID = 1, Title = "Suggested Course 1" },
                new CourseResponseDTO { CourseID = 2, Title = "Suggested Course 2" }
            };

            _mockUserPurchaseCourseService
                .Setup(s => s.GetSuggestedCoursesAsync(userId))
                .ReturnsAsync(suggestedCourses);

            // Act
            var result = await _controller.GetSuggestedCourses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCourses = Assert.IsAssignableFrom<IEnumerable<CourseResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnedCourses.Count());
        }

        [Fact]
        public async Task GetSuggestedCourses_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = context;

            // Act
            var result = await _controller.GetSuggestedCourses();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            var returnValue = unauthorizedResult.Value;
            var messageProperty = returnValue.GetType().GetProperty("message");
            var message = messageProperty?.GetValue(returnValue);

            Assert.Equal("User not found", message);
        }




        [Fact]
        public async Task CreateCheckoutSession_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var userId = "user123";
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
        new Claim(CustomClaimTypes.UserId, userId)
    }));
            _controller.ControllerContext = context;

            var request = new PurchaseRequestDTO
            {
                CourseIDs = new List<int> { 1, 2 }
            };

            var sessionUrl = "https://checkout.example.com/session";

            _mockUserPurchaseCourseService
                .Setup(s => s.CreateCheckoutSession(userId, request.CourseIDs))
                .ReturnsAsync(sessionUrl);

            // Act
            var result = await _controller.CreateCheckoutSession(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var urlProperty = value.GetType().GetProperty("url");
            Assert.NotNull(urlProperty);
            var actualUrl = urlProperty.GetValue(value)?.ToString();
            Assert.Equal(sessionUrl, actualUrl);
        }

        [Fact]
        public async Task CreateCheckoutSession_ShouldReturnBadRequest_WhenNoCoursesProvided()
        {
            // Arrange
            var userId = "user123";
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(CustomClaimTypes.UserId, userId)
            }));
            _controller.ControllerContext = context;

            var request = new PurchaseRequestDTO
            {
                CourseIDs = new List<int>() 
            };

            // Act
            var result = await _controller.CreateCheckoutSession(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value!;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var actualMessage = messageProperty.GetValue(value)?.ToString();
            Assert.Equal("No courses provided.", actualMessage);
        }




        [Fact]
        public async Task StripeWebhook_ShouldReturnUnauthorized_WhenStripeSignatureHeaderMissing()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}")); // empty JSON body
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await _controller.StripeWebhook();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value;
            var messageProp = value.GetType().GetProperty("message")?.GetValue(value)?.ToString();
            Assert.Equal("Stripe-Signature header is required", messageProp);
        }


        [Fact(Skip = "Skipping due to difficulty in mocking static methods")]
        public async Task StripeWebhook_ShouldReturnOk_WhenCheckoutSessionCompletedWithValidMetadata()
        {
            // The test is skipped, so no execution will happen here.
        }


























    }
}

