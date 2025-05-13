using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Repositories.Implementation;
using StudyJet.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.RepositoryTests
{
    public class UserPurchaseCourseRepoTest
    {

        private readonly ApplicationDbContext _context;
        private readonly UserPurchaseCourseRepo _userPurchaseCourseRepo;

        public UserPurchaseCourseRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _userPurchaseCourseRepo = new UserPurchaseCourseRepo(_context);
        }


        [Fact]
        public async Task SelectPurchasedCourseAsync_ReturnsCourses_WhenUserHasPurchases()
        {
            // Arrange
            var userId = "user1";
            var instructor = new User { Id = "instructor1", FullName = "John Doe", UserName = "jdoe" };
            var category = new Category { CategoryID = 1, Name = "Development" };

            var course = new Course
            {
                CourseID = 1,
                Title = "C# Basics",
                Description = "Learn C# from scratch",
                ImageUrl = "img.jpg",
                Price = 50,
                VideoUrl = "video.mp4",
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                Status = CourseStatus.Approved
            };

            var userPurchase = new UserPurchaseCourse
            {
                UserID = userId,
                UserName = "testuser",
                CourseID = course.CourseID,
                Course = course,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Users.Add(instructor);
            _context.Courses.Add(course);
            _context.UserPurchaseCourse.Add(userPurchase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userPurchaseCourseRepo.SelectPurchasedCourseAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var item = result.First();
            Assert.Equal(course.CourseID, item.CourseID);
            Assert.Equal(course.Title, item.Title);
            Assert.Equal(course.Description, item.Description);
            Assert.Equal(course.ImageUrl, item.ImageUrl);
            Assert.Equal(course.VideoUrl, item.VideoUrl);
            Assert.Equal(course.LastUpdatedDate, item.LastUpdateDate);
            Assert.Equal(instructor.FullName, item.InstructorName);
            Assert.Equal(course.Price, item.TotalPrice);
        }

        [Fact]
        public async Task SelectPurchasedCourseAsync_ReturnsEmptyList_WhenUserHasNoPurchases()
        {
            // Arrange
            var userId = "userWithNoPurchases";

            // Act
            var result = await _userPurchaseCourseRepo.SelectPurchasedCourseAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task HasUserPurchasedCourseAsync_ReturnsTrue_WhenUserHasPurchasedCourse()
        {
            // Arrange
            var userId = "user1";
            var course = new Course
            {
                CourseID = 1,
                Title = "C# Basics",
                Description = "Learn C# from scratch",
                ImageUrl = "img.jpg",
                Price = 50,
                VideoUrl = "video.mp4",
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                InstructorID = "instructor1",
                Instructor = new User { Id = "instructor1", UserName = "instructor", FullName = " Instrucotr 1" },
                CategoryID = 1,
                Category = new Category { CategoryID = 1, Name = "Development" },
                Status = CourseStatus.Approved
            };

            var userPurchase = new UserPurchaseCourse
            {
                UserID = userId,
                UserName = "user1",
                CourseID = course.CourseID,
                Course = course,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            _context.UserPurchaseCourse.Add(userPurchase);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userPurchaseCourseRepo.HasUserPurchasedCourseAsync(userId, course.CourseID);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasUserPurchasedCourseAsync_ReturnsFalse_WhenUserHasNotPurchasedCourse()
        {
            // Arrange
            var userId = "user2";  // Different user (not the one who purchased the course)
            var courseId = 1;  // Course ID that user hasn't purchased

            // Act
            var result = await _userPurchaseCourseRepo.HasUserPurchasedCourseAsync(userId, courseId);

            // Assert
            Assert.False(result); // Assert that the user has not purchased the course
        }



        [Fact]
        public async Task InsertPurchaseAsync_SuccessfullyInsertsPurchase_WhenValidData()
        {
            // Arrange
            var userId = "user1";
            var course = new Course
            {
                CourseID = 1,
                Title = "C# Basics",
                Description = "Learn C# from scratch",
                ImageUrl = "img.jpg",
                Price = 50,
                VideoUrl = "video.mp4",
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                InstructorID = "instructor1",
                Instructor = new User { Id = "instructor1", UserName = "instructor", FullName = " Instructor 1" },
                CategoryID = 1,
                Category = new Category { CategoryID = 1, Name = "Development" },
                Status = CourseStatus.Approved
            };

            var purchase = new UserPurchaseCourse
            {
                UserID = userId,
                UserName = "user1",
                CourseID = course.CourseID,
                Course = course,
                PurchaseDate = DateTime.UtcNow
            };

            // Act
            await _userPurchaseCourseRepo.InsertPurchaseAsync(purchase);
            await _context.SaveChangesAsync();

            // Assert
            var insertedPurchase = await _context.UserPurchaseCourse
                .FirstOrDefaultAsync(upc => upc.UserID == userId && upc.CourseID == course.CourseID);
            Assert.NotNull(insertedPurchase);
            Assert.Equal(userId, insertedPurchase.UserID);
            Assert.Equal(course.CourseID, insertedPurchase.CourseID);
        }

        [Fact]
        public async Task InsertPurchaseAsync_Fails_WhenRequiredFieldsAreMissing()
        {
            // Arrange
            var purchase = new UserPurchaseCourse
            {
                // Missing UserID and CourseID should cause a failure
                PurchaseDate = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _userPurchaseCourseRepo.InsertPurchaseAsync(purchase);
                await _context.SaveChangesAsync();
            });
        }



        [Fact]
        public async Task SelectSuggestedCoursesAsync_ReturnsSuggestedCourses_WhenUserHasNotPurchasedAnyCourses()
        {
            // Arrange
            var userId = "user123"; // Valid 

            var course1 = new Course
            {
                CourseID = 1,
                Title = "Course 1",
                Description = "Description of Course 1",
                ImageUrl = "http://example.com/image1.jpg",
                Price = 99.99m,
                Instructor = new User { FullName = "Instructor 1", UserName = "instructor1" },
                VideoUrl = "http://example.com/video1.mp4",
                LastUpdatedDate = DateTime.Now.AddDays(-1),
                Status = CourseStatus.Approved
            };
            var course2 = new Course
            {
                CourseID = 2,
                Title = "Course 2",
                Description = "Description of Course 2",
                ImageUrl = "http://example.com/image2.jpg",
                Price = 89.99m,
                Instructor = new User { FullName = "Instructor 2", UserName = "instructor2" },
                VideoUrl = "http://example.com/video2.mp4",
                LastUpdatedDate = DateTime.Now,
                Status = CourseStatus.Approved
            };

            _context.Courses.Add(course1);
            _context.Courses.Add(course2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userPurchaseCourseRepo.SelectSuggestedCoursesAsync(userId, 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.CourseID == 1);
            Assert.Contains(result, r => r.CourseID == 2);
            Assert.Equal("Instructor 1", result.First(r => r.CourseID == 1).InstructorName);
            Assert.Equal("Instructor 2", result.First(r => r.CourseID == 2).InstructorName);

        }

        [Fact]
        public async Task SelectSuggestedCoursesAsync_ReturnsLimitedCourses_WhenLimitIsSpecified()
        {
            // Arrange
            var userId = "user123";
            var limit = 1;

            // Create multiple unpurchased courses
            for (int i = 1; i <= 3; i++)
            {
                _context.Courses.Add(new Course
                {
                    CourseID = i,
                    Title = $"Course {i}",
                    Description = $"Description {i}",
                    ImageUrl = $"http://example.com/image{i}.jpg",
                    Price = 99.99m,
                    Instructor = new User { FullName = $"Instructor {i}" },
                    VideoUrl = $"http://example.com/video{i}.mp4",
                    LastUpdatedDate = DateTime.Now.AddDays(-i),
                    Status = CourseStatus.Approved
                });
            }

            await _context.SaveChangesAsync();

            // Act
            var result = await _userPurchaseCourseRepo.SelectSuggestedCoursesAsync(userId, limit);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(limit, result.Count); 
            Assert.Equal(1, result[0].CourseID); 
        }



        [Fact]
        public async Task SelectPurchasedCourseIdsAsync_ReturnsCorrectPurchasedCourseIds()
        {
            // Arrange
            var userId = "user123";

            var purchase1 = new UserPurchaseCourse
            {
                UserID = userId,
                CourseID = 1,
                PurchaseDate = DateTime.Now.AddDays(-1),
                UserName = "user123"
            };
            var purchase2 = new UserPurchaseCourse
            {
                UserID = userId,
                CourseID = 2,
                PurchaseDate = DateTime.Now.AddDays(-2),
                UserName = "user123"
            };

            _context.UserPurchaseCourse.Add(purchase1);
            _context.UserPurchaseCourse.Add(purchase2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userPurchaseCourseRepo.SelectPurchasedCourseIdsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
        }

        [Fact]
        public async Task SelectPurchasedCourseIdsAsync_ReturnsEmpty_WhenUserHasNoPurchases()
        {
            // Arrange
            var userId = "user123"; // Valid user ID, but the user hasn't purchased any courses

            // Act
            var result = await _userPurchaseCourseRepo.SelectPurchasedCourseIdsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);










        }

    }
}
