using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.Repositories.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.RepositoryTests
{
    public class NotificationRepoTest
    {

        private readonly ApplicationDbContext _context;
        private readonly NotificationRepo _notificationRepo;

        public NotificationRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _notificationRepo = new NotificationRepo(_context);
        }


        [Fact]
        public async Task CreateAsync_ShouldAddNotification_WhenNotificationIsValid()
        {
            // Arrange
            var notificationRepo = new NotificationRepo(_context); 
            var notification = new Notification
            {
                UserID = "user123",
                Message = "New course available!",
                IsRead = false,
                DateCreated = DateTime.UtcNow,
                CourseID = 101
            };

            // Act
            await notificationRepo.CreateAsync(notification);

            // Assert
            var savedNotification = await _context.Notifications.FirstOrDefaultAsync(n => n.ID == notification.ID);
            Assert.NotNull(savedNotification);  
            Assert.Equal(notification.Message, savedNotification.Message);  
            Assert.Equal(notification.UserID, savedNotification.UserID);  
            Assert.Equal(notification.IsRead, savedNotification.IsRead);  
            Assert.Equal(notification.CourseID, savedNotification.CourseID);  
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentNullException_WhenNotificationIsNull()
        {
            // Arrange
            var notificationRepo = new NotificationRepo(_context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => notificationRepo.CreateAsync(null));
        }



        [Fact]
        public async Task MarkAsReadAsync_Success()
        {
            // Arrange
            var notificationId = 1;

            // Create a notification with 'IsRead' initially set to false
            var notification = new Notification
            {
                ID = notificationId,
                UserID = "user123",
                Message = "Test notification",
                IsRead = false,
                DateCreated = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Act
            await _notificationRepo.MarkAsReadAsync(notificationId);

            // Assert
            var updatedNotification = await _context.Notifications.FindAsync(notificationId);
            Assert.NotNull(updatedNotification);
            Assert.True(updatedNotification.IsRead);  // Ensure 'IsRead' is set to true
        }

        [Fact]
        public async Task MarkAsReadAsync_InvalidNotificationId()
        {
            // Arrange
            var invalidNotificationId = 0;  // Invalid ID

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _notificationRepo.MarkAsReadAsync(invalidNotificationId));
            Assert.Equal("Invalid notification ID.", exception.Message);  // Ensure the exception message matches
        }




        [Fact]
        public async Task SelectByIdAsync_ReturnsNotificationWithRelationships_WhenExists()
        {
            // Arrange
            var notificationId = 1;
            var userId = "user1";
            var courseId = 101;

            var user = new User
            {
                Id = userId,
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FullName = "Test User", // Added required FullName
                EmailConfirmed = true
            };

            var instructor = new User
            {
                Id = "instructor1",
                UserName = "instructor@example.com",
                Email = "instructor@example.com",
                FullName = "Course Instructor",
                EmailConfirmed = true
            };

            var course = new Course
            {
                CourseID = courseId,
                Title = "Test Course",
                Description = "Test Description",
                ImageUrl = "test.jpg",
                VideoUrl = "test.mp4",
                Price = 99.99m,
                InstructorID = "instructor1",
                Instructor = instructor,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                Status = CourseStatus.Approved
            };

            var testNotification = new Notification
            {
                ID = notificationId,
                UserID = userId,
                User = user,
                CourseID = courseId,
                Course = course,
                Message = "Test notification message",
                IsRead = false,
                DateCreated = DateTime.UtcNow
            };

            _context.Users.AddRange(user, instructor);
            _context.Courses.Add(course);
            _context.Notifications.Add(testNotification);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepo.SelectByIdAsync(notificationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(notificationId, result.ID);
            Assert.Equal(userId, result.UserID);
            Assert.Equal(courseId, result.CourseID);
            Assert.Equal("Test notification message", result.Message);
            Assert.False(result.IsRead);
            Assert.NotNull(result.User);
            Assert.Equal("Test User", result.User.FullName); // Verify relationship
            Assert.NotNull(result.Course);
            Assert.Equal("Test Course", result.Course.Title);
        }

        [Fact]
        public async Task SelectByIdAsync_ThrowsArgumentException_WhenIdIsInvalid()
        {
            // Arrange
            var invalidNotificationId = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _notificationRepo.SelectByIdAsync(invalidNotificationId));

            Assert.Equal("Invalid notification ID.", exception.Message);
        }

        [Fact]
        public async Task SelectByIdAsync_ReturnsNull_WhenNotificationNotFound()
        {
            // Arrange
            var nonExistentId = 999;

            // Act
            var result = await _notificationRepo.SelectByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }




        [Fact]
        public async Task UpdateAsync_Success()
        {
            // Arrange
            var notification = new Notification
            {
                ID = 1,
                UserID = "user123",
                Message = "Initial message",
                IsRead = false,
                DateCreated = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Modify the same tracked entity
            notification.Message = "Updated message";
            notification.IsRead = true;

            // Act
            await _notificationRepo.UpdateAsync(notification);

            // Assert
            var updated = await _context.Notifications.FindAsync(notification.ID);
            Assert.NotNull(updated);
            Assert.Equal("Updated message", updated.Message);
            Assert.True(updated.IsRead);
        }


        [Fact]
        public async Task UpdateAsync_NullNotification()
        {
            // Arrange
            Notification notification = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _notificationRepo.UpdateAsync(notification));
            Assert.Equal("Value cannot be null. (Parameter 'notification')", exception.Message);  
        }



        [Fact]
        public async Task SelectByUserIdAsync_ReturnsNotifications_ForValidUser()
        {
            // Arrange
            var userId = "user1";
            var testNotifications = new List<Notification>
             {
                new Notification { ID = 1, UserID = userId, Message = "Message 1", DateCreated = DateTime.UtcNow.AddHours(-1) },
                new Notification { ID = 2, UserID = userId, Message = "Message 2", DateCreated = DateTime.UtcNow },
                new Notification { ID = 3, UserID = "otherUser", Message = "Other User", DateCreated = DateTime.UtcNow }
            };

            _context.Notifications.AddRange(testNotifications);
            await _context.SaveChangesAsync();

            // Act
            var result = await _notificationRepo.SelectByUserIdAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, n => Assert.Equal(userId, n.UserID));
            Assert.Equal(2, result[0].ID); 
            Assert.Equal(1, result[1].ID);
        }


        [Fact]
        public async Task SelectByUserIdAsync_ReturnsPagedResults_WhenPageSizeSpecified()
        {
            // Arrange
            var userId = "user1";
            var testNotifications = Enumerable.Range(1, 15)
                .Select(i => new Notification
                {
                    ID = i,
                    UserID = userId,
                    Message = $"Message {i}",
                    DateCreated = DateTime.UtcNow.AddMinutes(-i)
                })
                .ToList();

            _context.Notifications.AddRange(testNotifications);
            await _context.SaveChangesAsync();

            // Act
            var page1 = await _notificationRepo.SelectByUserIdAsync(userId, page: 1, pageSize: 5);
            var page2 = await _notificationRepo.SelectByUserIdAsync(userId, page: 2, pageSize: 5);

            // Assert
            Assert.Equal(5, page1.Count);
            Assert.Equal(5, page2.Count);
            Assert.Equal(1, page1[0].ID); 
            Assert.Equal(6, page2[0].ID);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SelectByUserIdAsync_ShouldThrowArgumentException_WhenUserIdIsInvalid(string invalidUserId)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _notificationRepo.SelectByUserIdAsync(invalidUserId));

            Assert.Equal("User ID cannot be null or empty.", exception.Message);
        }



    }
}
