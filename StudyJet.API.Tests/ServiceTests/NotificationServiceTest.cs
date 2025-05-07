using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class NotificationServiceTest
    {
        private readonly Mock<INotificationRepo> _mockNotificationRepo;
        private readonly Mock<ICourseRepo> _mockCourseRepo;
        private readonly NotificationService _notificationService;
        private readonly Mock<UserManager<User>> _mockUserManager;

        public NotificationServiceTest()
        {
            // Mocking the dependencies
            _mockNotificationRepo = new Mock<INotificationRepo>();
            _mockCourseRepo = new Mock<ICourseRepo>();

            //  mock of UserManager<User>
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null,
                new PasswordHasher<User>(),
                new List<IUserValidator<User>> { new Mock<IUserValidator<User>>().Object },
                new List<IPasswordValidator<User>> { new Mock<IPasswordValidator<User>>().Object },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Mock<ILogger<UserManager<User>>>().Object
            );

            // Create instance of NotificationService 
            _notificationService = new NotificationService(
                _mockNotificationRepo.Object,
                  _mockUserManager.Object,
                _mockCourseRepo.Object
            );
        }

        private UserManager<User> GetMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var userValidators = new List<IUserValidator<User>> { new Mock<IUserValidator<User>>().Object };
            var passwordValidators = new List<IPasswordValidator<User>> { new Mock<IPasswordValidator<User>>().Object };

            return new UserManager<User>(
                store.Object,
                null,
                new PasswordHasher<User>(),
                userValidators,
                passwordValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Mock<ILogger<UserManager<User>>>().Object
            );
        }


        [Fact]
        public async Task CreateNotificationAsync_ShouldCreateNotification_WhenValidInputs()
        {
            // Arrange
            var userId = "user123";
            var message = "This is a test notification.";

            _mockNotificationRepo.Setup(repo => repo.CreateAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            // Act
            await _notificationService.CreateNotificationAsync(userId, message);

            // Assert
            _mockNotificationRepo.Verify(repo => repo.CreateAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task CreateNotificationAsync_ShouldThrowArgumentException_WhenUserIdIsNull()
        {
            // Arrange
            var invalidUserId = ""; // Invalid userId (empty string)
            var message = "Test message";

            // Act & Assert: Should throw an ArgumentException
            await Assert.ThrowsAsync<ArgumentException>(() => _notificationService.CreateNotificationAsync(invalidUserId, message));
        }



        [Fact]
        public async Task GetNotificationByIdAsync_ShouldReturnNotification_WhenValidId()
        {
            // Arrange
            int validId = 1;
            var expectedNotification = new Notification { ID = validId, Message = "Test" };

            _mockNotificationRepo
                .Setup(repo => repo.SelectByIdAsync(validId))
                .ReturnsAsync(expectedNotification);

            // Act
            var result = await _notificationService.GetNotificationByIdAsync(validId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedNotification.ID, result.ID);
            Assert.Equal(expectedNotification.Message, result.Message);
        }

        [Fact]
        public async Task GetNotificationByIdAsync_ShouldThrowArgumentException_WhenIdIsInvalid()
        {
            // Arrange
            int invalidId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _notificationService.GetNotificationByIdAsync(invalidId));
        }




        [Fact]
        public async Task GetNotificationByUserIdAsync_ShouldReturnNotifications_WhenUserIdIsValid()
        {
            // Arrange
            var userId = "user123";
            var expectedNotifications = new List<Notification>
            {
                new Notification { ID = 1, UserID = userId, Message = "Message 1" },
                new Notification { ID = 2, UserID = userId, Message = "Message 2" }
            };

            _mockNotificationRepo
                .Setup(repo => repo.SelectByUserIdAsync(userId, 1, 10))
                .ReturnsAsync(expectedNotifications);

            // Act
            var result = await _notificationService.GetNotificationByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, n => Assert.Equal(userId, n.UserID));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetNotificationByUserIdAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(string invalidUserId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _notificationService.GetNotificationByUserIdAsync(invalidUserId));
        }




        [Fact]
        public async Task UpdateNotificationAsync_ShouldCallUpdateAsync_WhenNotificationIsValid()
        {
            // Arrange
            var notification = new Notification { ID = 1, Message = "Updated message" };

            _mockNotificationRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            // Act
            await _notificationService.UpdateNotificationAsync(notification);

            // Assert
            _mockNotificationRepo.Verify(repo => repo.UpdateAsync(It.Is<Notification>(n => n.ID == notification.ID)), Times.Once);
        }

        [Fact]
        public async Task UpdateNotificationAsync_ShouldThrowArgumentNullException_WhenNotificationIsNull()
        {
            // Arrange
            Notification notification = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _notificationService.UpdateNotificationAsync(notification));
        }




        [Fact]
        public async Task NotifyAdminForCourseAdditionOrUpdateAsync_ShouldCreateNotification_WhenValidInputs()
        {
            // Arrange
            var instructorId = "instructor123";
            var message = "added a new course";
            var courseId = 1;

            var instructor = new User { Id = instructorId, FullName = "Instructor Name" };
            var adminUsers = new List<User>
            {
                new User { Id = "admin1", FullName = "Admin One" },
                new User { Id = "admin2", FullName = "Admin Two" }
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(instructorId)).ReturnsAsync(instructor);
            _mockUserManager.Setup(um => um.GetUsersInRoleAsync("Admin")).ReturnsAsync(adminUsers);

            _mockNotificationRepo.Setup(repo => repo.CreateAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifyAdminForCourseAdditionOrUpdateAsync(instructorId, message, courseId);

            // Assert
            _mockNotificationRepo.Verify(repo => repo.CreateAsync(It.Is<Notification>(n =>
                n.Message == "Instructor Instructor Name added a new course" && n.UserID != instructorId)), Times.Exactly(2));
        }

        [Fact]
        public async Task NotifyAdminForCourseAdditionOrUpdateAsync_ShouldThrowException_WhenInstructorNotFound()
        {
            // Arrange
            var instructorId = "nonexistentInstructorId";
            var message = "added a new course";

            _mockUserManager.Setup(um => um.FindByIdAsync(instructorId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _notificationService.NotifyAdminForCourseAdditionOrUpdateAsync(instructorId, message));
        }



        

    }
}








    

