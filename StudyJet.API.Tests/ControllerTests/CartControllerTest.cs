using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.DTOs.Cart;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using StudyJet.API.Data.Entities;
using System.Text.Json;

namespace StudyJet.API.Tests.ControllerTests
{
    public class CartControllerTest
    {
        private readonly Mock<ICartService> _mockCartService;
        private readonly Mock<IWishlistService> _mockWishlistService;
        private readonly Mock<ICourseService> _mockCourseService;
        private readonly CartController _controller;
        private readonly ClaimsPrincipal _user;

        public CartControllerTest()
        {
            _mockCartService = new Mock<ICartService>();
            _mockWishlistService = new Mock<IWishlistService>();
            _mockCourseService = new Mock<ICourseService>();


            _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(CustomClaimTypes.UserId, "123")
            }, "mock"));

            // Set up the controller with mocks and the user
            _controller = new CartController(
                _mockCartService.Object,
                _mockWishlistService.Object,
                _mockCourseService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _user }
                }
            };
        }


        [Fact]
        public async Task GetCartItems_ItemsInCart_ReturnsOk()
        {
            // Arrange: Simulate an authenticated user
            var userId = "123";
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(CustomClaimTypes.UserId, userId) })
            );

            // Simulate some items in the cart for the user (DTO version)
            var cartItemsDTO = new List<CartItemDTO>
            {
                new CartItemDTO
                {
                    CartID = 1,
                    CourseID = 101,
                    CourseTitle = "Course 1",
                    CourseDescription = "Description 1",
                    InstructorName = "Instructor 1",
                    ImageUrl = "image1.jpg",
                    Price = 100.0m
            },
                new CartItemDTO
                {
                    CartID = 2,
                    CourseID = 102,
                    CourseTitle = "Course 2",
                    CourseDescription = "Description 2",
                    InstructorName = "Instructor 2",
                    ImageUrl = "image2.jpg",
                    Price = 200.0m
                }
            };

            _mockCartService.Setup(x => x.GetCartItemsAsync(userId)).ReturnsAsync(cartItemsDTO);

            var result = await _controller.GetCartItems();

            // Assert
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var value = objectResult.Value as List<CartItemDTO>;

            Assert.NotNull(value);

            Assert.Equal(2, value.Count);

            Assert.Equal("Course 1", value[0].CourseTitle);
            Assert.Equal(100.0m, value[0].Price);
            Assert.Equal("Course 2", value[1].CourseTitle);
            Assert.Equal(200.0m, value[1].Price);
        }

        [Fact]
        public async Task GetCartItems_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange: Create a controller and simulate an unauthenticated user (no UserId claim)
            var controller = new CartController(
                _mockCartService.Object,
                _mockWishlistService.Object,
                _mockCourseService.Object
            );

            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // No claims, so unauthenticated

            // Act
            var result = await controller.GetCartItems();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }



        [Fact]
        public async Task RemoveFromCart_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new CartController(null, null, null);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.RemoveFromCart(1);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);

            var json = JsonSerializer.Serialize(unauthorized.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            Assert.NotNull(dict);
            Assert.Equal("User is not authenticated.", dict["message"]);
        }

        [Fact]
        public async Task RemoveFromCart_ValidUserAndCourse_ReturnsOk()
        {
            // Arrange
            var mockCartService = new Mock<ICartService>();
            mockCartService.Setup(x => x.RemoveCourseFromCartAsync("123", 1)).ReturnsAsync(true);

            var controller = new CartController(mockCartService.Object, null, null);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(CustomClaimTypes.UserId, "123")
            }, "mock"))
                }
            };

            // Act
            var result = await controller.RemoveFromCart(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(ok.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.Equal("Course removed from cart successfully", dict["message"]);
        }




        [Fact]
        public async Task GetCourseDetails_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var mockCartService = new Mock<ICartService>();
            var controller = new CartController(mockCartService.Object, null, null);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.GetCourseDetails(1);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var json = JsonSerializer.Serialize(unauthorized.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.Equal("User is not authenticated.", dict["message"]);
        }

        [Fact]
        public async Task GetCourseDetails_CourseNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockCartService = new Mock<ICartService>();
            mockCartService.Setup(x => x.GetCourseDetailsAsync(1)).ReturnsAsync((Course)null);

            var controller = new CartController(mockCartService.Object, null, null);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(CustomClaimTypes.UserId, "123")
            }, "mock"))
                }
            };

            // Act
            var result = await controller.GetCourseDetails(1);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var json = JsonSerializer.Serialize(notFound.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.Equal("Course not found.", dict["message"]);
        }



        [Fact]
        public async Task MoveToWishlist_ValidUser_CourseNotInWishlist_ReturnsOk()
        {
            // Arrange
            var userId = "123";
            var courseId = 1;

            var mockWishlistService = new Mock<IWishlistService>();
            var mockCartService = new Mock<ICartService>();

            mockWishlistService.Setup(x => x.IsCourseInWishlistAsync(userId, courseId))
                               .ReturnsAsync(false);
            mockWishlistService.Setup(x => x.AddCourseToWishlistAsync(userId, courseId))
                               .ReturnsAsync(true);
            mockCartService.Setup(x => x.RemoveCourseFromCartAsync(userId, courseId))
                           .ReturnsAsync(true);

            var controller = new CartController(mockCartService.Object, mockWishlistService.Object, null)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                    new Claim(CustomClaimTypes.UserId, userId)
                }, "mock"))
                    }
                }
            };

            // Act
            var result = await controller.MoveToWishlist(courseId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var message = ok.Value.GetType().GetProperty("message")?.GetValue(ok.Value) as string;
            Assert.Equal("Course moved to wishlist successfully.", message);
        }

        [Fact]
        public async Task MoveToWishlist_CourseAlreadyInWishlist_ReturnsBadRequest()
        {
            // Arrange
            var userId = "123";
            var courseId = 1;

            var mockWishlistService = new Mock<IWishlistService>();
            var mockCartService = new Mock<ICartService>();

            mockWishlistService.Setup(x => x.IsCourseInWishlistAsync(userId, courseId))
                               .ReturnsAsync(true);

            var controller = new CartController(mockCartService.Object, mockWishlistService.Object, null)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                    new Claim(CustomClaimTypes.UserId, userId)
                }, "mock"))
                    }
                }
            };

            // Act
            var result = await controller.MoveToWishlist(courseId);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var message = badRequest.Value.GetType().GetProperty("message")?.GetValue(badRequest.Value) as string;
            Assert.Equal("Course is already in the wishlist.", message);
        }



    }
}
