using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe.Checkout;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using StudyJet.API.Services;
using Stripe;

namespace StudyJet.API.Tests.ServiceTests
{
    public class UserPurchaseCourseServiceTest
    {
        private readonly Mock<IUserPurchaseCourseRepo> _mockUserPurchaseRepo;
        private readonly Mock<ICourseRepo> _mockCourseRepo;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<UserPurchaseCourseService>> _mockLogger;
        private readonly UserPurchaseCourseService _service;

        public UserPurchaseCourseServiceTest()
        {
            _mockUserPurchaseRepo = new Mock<IUserPurchaseCourseRepo>();
            _mockCourseRepo = new Mock<ICourseRepo>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<UserPurchaseCourseService>>();

            _service = new UserPurchaseCourseService(
                _mockUserPurchaseRepo.Object,
                _mockCourseRepo.Object,
                _mockConfiguration.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetPurchasedCoursesAsync_ReturnsCourses()
        {
            // Arrange
            var userId = "user123";
            var expectedCourses = new List<UserPurchaseCourseDTO>
            {
                new UserPurchaseCourseDTO { CourseID = 12, Description = "Math 101" }
            };

            _mockUserPurchaseRepo
                .Setup(repo => repo.SelectPurchasedCourseAsync(userId))
                .ReturnsAsync(expectedCourses);

            // Act
            var result = await _service.GetPurchasedCoursesAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(12, result[0].CourseID);  
        }

        [Fact]
        public async Task GetPurchasedCoursesAsync_ReturnsEmptyList_WhenNoCourses()
        {
            // Arrange
            var userId = "user456";
            _mockUserPurchaseRepo
                .Setup(repo => repo.SelectPurchasedCourseAsync(userId))
                .ReturnsAsync(new List<UserPurchaseCourseDTO>());

            // Act
            var result = await _service.GetPurchasedCoursesAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }




        [Fact]
        public async Task GetSuggestedCoursesAsync_ReturnsSuggestedCourses()
        {
            // Arrange
            var userId = "user789";
            var expectedCourses = new List<CourseResponseDTO>
            {
                new CourseResponseDTO { CourseID = 101, Title = "Physics" }
            };

            _mockUserPurchaseRepo
                .Setup(repo => repo.SelectSuggestedCoursesAsync(userId, 3)) 
                .ReturnsAsync(expectedCourses);

            // Act
            var result = await _service.GetSuggestedCoursesAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(101, result[0].CourseID);
        }

        [Fact]
        public async Task GetSuggestedCoursesAsync_ThrowsException_WhenRepoFails()
        {
            // Arrange
            var userId = "user789";
            _mockUserPurchaseRepo
                .Setup(repo => repo.SelectSuggestedCoursesAsync(userId, 3)) 
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetSuggestedCoursesAsync(userId));
        }



        [Fact]
        public async Task PurchaseCourseAsync_ReturnsTrue_WhenNewCoursesPurchased()
        {
            // Arrange
            var userId = "user123";
            var courseIds = new List<int> { 1, 2 };

            var user = new User { Id = userId, UserName = "testuser" };
            var alreadyPurchased = new List<int> { 1 }; 

            _mockUserPurchaseRepo.Setup(r => r.SelectUserByIdAsync(userId))
                                 .ReturnsAsync(user);
            _mockUserPurchaseRepo.Setup(r => r.SelectPurchasedCourseIdsAsync(userId))
                                 .ReturnsAsync(alreadyPurchased);
            _mockUserPurchaseRepo.Setup(r => r.InsertPurchaseAsync(It.IsAny<UserPurchaseCourse>()))
                                 .Returns(Task.CompletedTask);
            _mockUserPurchaseRepo.Setup(r => r.SaveChangesAsync())
                                 .ReturnsAsync(1);

            // Act
            var result = await _service.PurchaseCourseAsync(userId, courseIds);

            // Assert
            Assert.True(result);
            _mockUserPurchaseRepo.Verify(r => r.InsertPurchaseAsync(It.Is<UserPurchaseCourse>(p => p.CourseID == 2)), Times.Once);
            _mockUserPurchaseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PurchaseCourseAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = "user123";
            var courseIds = new List<int> { 1, 2 };

            _mockUserPurchaseRepo.Setup(r => r.SelectUserByIdAsync(userId))
                                 .ReturnsAsync((User)null);

            // Act
            var result = await _service.PurchaseCourseAsync(userId, courseIds);

            // Assert
            Assert.False(result);
            _mockUserPurchaseRepo.Verify(r => r.InsertPurchaseAsync(It.IsAny<UserPurchaseCourse>()), Times.Never);
            _mockUserPurchaseRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }



        
























    }
}
