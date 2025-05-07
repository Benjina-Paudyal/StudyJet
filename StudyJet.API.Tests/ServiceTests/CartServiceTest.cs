using Moq;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Cart;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class CartServiceTest
    {
        private readonly Mock<ICartRepo> _mockRepo;
        private readonly CartService _cartService;

        public CartServiceTest() 
        {
            // Arrange
            _mockRepo = new Mock<ICartRepo>(); ;
            _cartService = new CartService(_mockRepo.Object );

        }

        [Fact]
        public async Task AddToCartAsync_WithValidCourse_AddsCourseToCart()
        {
            // Arrange
            var userId = "user123";
            var courseId = 42;
            var course = new Course { CourseID = courseId, Title = "Test Course", Price = 99 };
            var existingCartItems = new List<CartItemDTO>();

            _mockRepo.Setup(r => r.SelectCourseDetailsAsync(courseId)).ReturnsAsync(course);
            _mockRepo.Setup(r => r.SelectCartItemsAsync(userId)).ReturnsAsync(existingCartItems);
            _mockRepo.Setup(r => r.InsertCourseToCartAsync(userId, courseId, course.Price)).Returns(Task.CompletedTask);

            // Act
            await _cartService.AddToCartAsync(userId, courseId);

            // Assert
            _mockRepo.Verify(r => r.SelectCourseDetailsAsync(courseId), Times.Once);
            _mockRepo.Verify(r => r.SelectCartItemsAsync(userId), Times.Once);
            _mockRepo.Verify(r => r.InsertCourseToCartAsync(userId, courseId, course.Price), Times.Once);
        }

        [Fact]
        public async Task AddToCartAsync_CourseAlreadyInCart_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "user123";
            var courseId = 42;
            var course = new Course { CourseID = courseId, Title = "Test Course", Price = 99 };

            var existingCartItems = new List<CartItemDTO>
            {           
                new CartItemDTO { CourseID = courseId } 
            };

            _mockRepo.Setup(r => r.SelectCourseDetailsAsync(courseId)).ReturnsAsync(course);
            _mockRepo.Setup(r => r.SelectCartItemsAsync(userId)).ReturnsAsync(existingCartItems);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _cartService.AddToCartAsync(userId, courseId));

            Assert.Equal("The course is already in the cart.", ex.Message);

            // Verify Insert was NEVER called
            _mockRepo.Verify(r => r.InsertCourseToCartAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        }



        [Fact]
        public async Task GetCartItemsAsync_ReturnsCartItemsFromRepo()
        {
            // Arrange
            var userId = "user123";
            var expectedCartItems = new List<CartItemDTO>
            {
                new CartItemDTO { CourseID = 1, CourseTitle = "Course 1", Price = 50 },
                new CartItemDTO { CourseID = 2, CourseTitle = "Course 2", Price = 75 }
            };

            _mockRepo.Setup(r => r.SelectCartItemsAsync(userId)).ReturnsAsync(expectedCartItems);

            // Act
            var result = await _cartService.GetCartItemsAsync(userId);

            // Assert
            Assert.Equal(expectedCartItems, result);
            _mockRepo.Verify(r => r.SelectCartItemsAsync(userId), Times.Once);
        }


        [Fact]
        public async Task GetCourseDetailsAsync_WithValidCourseId_ReturnsCourse()
        {
            // Arrange
            var courseId = 1;
            var expectedCourse = new Course { CourseID = courseId, Title = "Sample Course" };
            _mockRepo.Setup(r => r.SelectCourseDetailsAsync(courseId)).ReturnsAsync(expectedCourse);

            // Act
            var result = await _cartService.GetCourseDetailsAsync(courseId);

            // Assert
            Assert.Equal(expectedCourse, result);
            _mockRepo.Verify(r => r.SelectCourseDetailsAsync(courseId), Times.Once);
        }


        [Fact]
        public async Task GetUserByIdAsync_WithValidUserId_ReturnsUser()
        {
            // Arrange
            var userId = "user123";
            var expectedUser = new User { Id = userId, UserName = "testuser" };
            _mockRepo.Setup(r => r.SelectUserByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _cartService.GetUserByIdAsync(userId);

            // Assert
            Assert.Equal(expectedUser, result);
            _mockRepo.Verify(r => r.SelectUserByIdAsync(userId), Times.Once);
        }


        [Fact]
        public async Task RemoveCourseFromCartAsync_WhenCalled_ReturnsExpectedResult()
        {
            // Arrange
            var userId = "user123";
            var courseId = 42;
            _mockRepo.Setup(r => r.DeleteCourseFromCartAsync(userId, courseId)).ReturnsAsync(true);

            // Act
            var result = await _cartService.RemoveCourseFromCartAsync(userId, courseId);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.DeleteCourseFromCartAsync(userId, courseId), Times.Once);
        }



        [Fact]
        public async Task IsCourseInCartAsync_WhenCalled_ReturnsExpectedResult()
        {
            // Arrange
            var userId = "user123";
            var courseId = 42;
            _mockRepo.Setup(r => r.IsCourseInCartAsync(userId, courseId)).ReturnsAsync(true);

            // Act
            var result = await _cartService.IsCourseInCartAsync(userId, courseId);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.IsCourseInCartAsync(userId, courseId), Times.Once);
        }




    }
}

