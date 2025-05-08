using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.DTOs.Wishlist;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Security.Claims;

namespace StudyJet.API.Tests.ControllerTests
{
    public  class WishlistControllerTest
    {

        private readonly Mock<IWishlistService> _mockWishlistService;
        private readonly Mock<ICartService> _mockCartService;
        private readonly ClaimsPrincipal _user;
        private readonly WishlistController _controller;

        public WishlistControllerTest()
        {
            _mockWishlistService = new Mock<IWishlistService>();
            _mockCartService = new Mock<ICartService>();

            // Create a mock user with claims (for the test case scenario)
            _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(CustomClaimTypes.UserId, "123"),  
            new Claim(ClaimTypes.Role, "Student")      
            }, "mock"));

            // Set up the controller with mocks and the user
            _controller = new WishlistController(
                _mockWishlistService.Object,
                _mockCartService.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _user }
                }
            };

        }


        [Fact]
        public async Task GetWishlist_ShouldReturnOk_WhenUserIsAuthenticated()
        {
            // Arrange
            var mockWishlistService = new Mock<IWishlistService>();
            var mockCartService = new Mock<ICartService>();
            var userId = "123"; // Simulate a valid user ID

            // Simulate that the wishlist service returns a list of WishlistCourseDTO items
            mockWishlistService.Setup(s => s.GetWishlistAsync(userId))
                .ReturnsAsync(new List<WishlistCourseDTO>
                {
                    new WishlistCourseDTO { CourseID = 1, Title = "Course1" },
                    new WishlistCourseDTO { CourseID = 2, Title = "Course2" }
                });

            var controller = new WishlistController(mockWishlistService.Object, mockCartService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(CustomClaimTypes.UserId, userId), 
                        new Claim(ClaimTypes.Role, "Student") 
                    }, "mock"))
                    }
                }
            };

            // Act
            var result = await controller.GetWishlist();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var wishlist = Assert.IsType<List<WishlistCourseDTO>>(okResult.Value); 
            Assert.Equal(2, wishlist.Count); 
            Assert.Equal("Course1", wishlist[0].Title);
        }


        [Fact]
        public async Task GetWishlist_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var mockWishlistService = new Mock<IWishlistService>();
            var mockCartService = new Mock<ICartService>();

            var controller = new WishlistController(mockWishlistService.Object, mockCartService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } 
                }
            };

            // Act
            var result = await controller.GetWishlist();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            var messageProperty = unauthorizedResult.Value.GetType().GetProperty("message");
            var message = messageProperty?.GetValue(unauthorizedResult.Value)?.ToString();

            Assert.Equal("User is not authenticated.", message);
        }




        [Fact]
        public async Task AddToWishlist_ShouldReturnOk_WhenCourseSuccessfullyAdded()
        {
            // Arrange
            string userId = "123";
            int courseId = 1;

            _mockCartService.Setup(x => x.IsCourseInCartAsync(userId, courseId)).ReturnsAsync(false);
            _mockWishlistService.Setup(x => x.AddCourseToWishlistAsync(userId, courseId)).ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(CustomClaimTypes.UserId, userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.AddToWishlist(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var messageProp = value.GetType().GetProperty("message");
            var message = messageProp?.GetValue(value)?.ToString();

            var successProp = value.GetType().GetProperty("success");
            var success = (bool?)successProp?.GetValue(value);

            Assert.True(success);
            Assert.Equal("Course added to wishlist.", message);
        }

        [Fact]
        public async Task AddToWishlist_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new WishlistController(_mockWishlistService.Object, _mockCartService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // No claims
                }
            };

            // Act
            var result = await controller.AddToWishlist(1);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value;

            var messageProp = value.GetType().GetProperty("message");
            var message = messageProp?.GetValue(value)?.ToString();

            var successProp = value.GetType().GetProperty("success");
            var success = (bool?)successProp?.GetValue(value);

            Assert.False(success);
            Assert.Equal("User is not authenticated.", message);
        }




        [Fact]
        public async Task IsCourseInWishlist_ShouldReturnOkWithStatus_WhenUserIsAuthenticated()
        {
            // Arrange
            string userId = "123";
            int courseId = 5;

            _mockWishlistService.Setup(x => x.IsCourseInWishlistAsync(userId, courseId)).ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(CustomClaimTypes.UserId, userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.IsCourseInWishlist(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var type = value.GetType();
            var success = (bool)type.GetProperty("success")?.GetValue(value)!;
            var isInWishlist = (bool)type.GetProperty("isInWishlist")?.GetValue(value)!;

            Assert.True(success);
            Assert.True(isInWishlist);
        }

        [Fact]
        public async Task IsCourseInWishlist_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controller = new WishlistController(_mockWishlistService.Object, _mockCartService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal() // no claims = unauthenticated
                    }
                }
            };

            // Act
            var result = await controller.IsCourseInWishlist(1);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value;

            var type = value.GetType();
            var successProp = type.GetProperty("success");
            var messageProp = type.GetProperty("message");

            Assert.NotNull(successProp);
            Assert.NotNull(messageProp);

            Assert.False((bool)successProp.GetValue(value));
            Assert.Equal("User is not authenticated.", messageProp.GetValue(value));
        }




        [Fact]
        public async Task RemoveFromWishlist_ShouldReturnOk_WhenCourseRemovedSuccessfully()
        {
            // Arrange
            string userId = "123";
            int courseId = 5;

            _mockWishlistService
                .Setup(x => x.RemoveCourseFromWishlistAsync(userId, courseId))
                .ReturnsAsync(true);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(CustomClaimTypes.UserId, userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.RemoveFromWishlist(courseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var type = value.GetType();
            var success = (bool)type.GetProperty("success")?.GetValue(value)!;
            var message = (string)type.GetProperty("message")?.GetValue(value)!;

            Assert.True(success);
            Assert.Equal("Course removed from wishlist.", message);
        }

        [Fact]
        public async Task RemoveFromWishlist_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            int courseId = 5;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var result = await _controller.RemoveFromWishlist(courseId);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var value = unauthorizedResult.Value;
            var type = value.GetType();
            var success = (bool)type.GetProperty("success")?.GetValue(value)!;
            var message = (string)type.GetProperty("message")?.GetValue(value)!;

            Assert.False(success);
            Assert.Equal("User is not authenticated.", message);
        }











    }
}
