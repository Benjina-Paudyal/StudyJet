using Microsoft.EntityFrameworkCore;
using Moq;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Repositories.Implementation;
using StudyJet.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.RepositoryTests
{
    public class CourseRepoTest
    {
        private readonly ApplicationDbContext _context;
        private readonly CourseRepo _courseRepo;

        public CourseRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _courseRepo = new CourseRepo(_context);
        }


        [Fact]
        public async Task SelectAllAsync_ReturnsAllCourses_WithInstructorAndCategory()
        {
            // Arrange
            var instructor = new User { Id = "instructor1", FullName = "John Doe" };
            var category = new Category { CategoryID = 1, Name = "Development" };
            var course = new Course
            {
                Title = "Test Course",
                Description = "Test Description",
                ImageUrl = "http://example.com/image.png",
                Price = 100,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://example.com/video.mp4",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Course", result[0].Title);
            Assert.NotNull(result[0].Instructor);
            Assert.Equal("John Doe", result[0].Instructor.FullName);
            Assert.NotNull(result[0].Category);
            Assert.Equal("Development", result[0].Category.Name);
        }

        [Fact]
        public async Task SelectAllAsync_WhenNoCourses_ReturnsEmptyList()
        {
            // Act
            var result = await _courseRepo.SelectAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SelectAllAsync_WhenInstructorNotInDb_ReturnsCourseWithNullInstructor()
        {
            // Arrange
            var instructor = new User
            {
                Id = "instructor1",
                UserName = "instructor1",
                Email = "instructor1@example.com",
                FullName = "Instructor One"
            };

            var category = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var course = new Course
            {
                Title = "Test Course",
                Description = "Description",
                ImageUrl = "http://img.com/img.png",
                Price = 99.99m,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/video.mp4",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Course", result[0].Title);
            Assert.NotNull(result[0].Instructor);
        }




        [Fact]
        public async Task SelectCourseByStatusAsync_ReturnsCoursesWithMatchingStatus()
        {
            // Arrange
            var instructor = new User { Id = "instructor-1", FullName = "Test Instructor", UserName = "test@example.com", Email = "test@example.com" };
            var category = new Category { CategoryID = 1, Name = "Programming" };

            var testCourses = new List<Course>
    {
                new Course {
                    CourseID = 1,
                    Title = "Course 1",
                    Description = "Test Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Approved
                 },
                 new Course {
                    CourseID = 2,
                    Title = "Course 2",
                    Description = "Test Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Pending
                },
                new Course {
                    CourseID = 3,
                    Title = "Course 3",
                    Description = "Test Description 3",
                    ImageUrl = "image3.jpg",
                    VideoUrl = "video3.mp4",
                    Price = 69.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Approved
                }
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            var statusesToFilter = new List<CourseStatus> { CourseStatus.Approved };

            // Act
            var result = await _courseRepo.SelectCourseByStatusAsync(statusesToFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, course => Assert.Equal(CourseStatus.Approved, course.Status));
            Assert.All(result, course => Assert.NotNull(course.Instructor));
            Assert.All(result, course => Assert.NotNull(course.Category));
        }

        [Fact]
        public async Task SelectCourseByStatusAsync_ReturnsEmptyListWhenNoMatches()
        {
            // Arrange
            var instructor = new User { Id = "instructor-1", FullName = "Test Instructor", UserName = "test@example.com", Email = "test@example.com" };
            var category = new Category { CategoryID = 1, Name = "Programming" };

            var testCourses = new List<Course>
            {
                new Course {
                    CourseID = 1,
                    Title = "Course 1",
                    Description = "Test Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Approved
            },
                new Course {
                    CourseID = 2,
                    Title = "Course 2",
                    Description = "Test Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Pending
                }
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            var statusesToFilter = new List<CourseStatus> { CourseStatus.Rejected };

            // Act
            var result = await _courseRepo.SelectCourseByStatusAsync(statusesToFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task SelectCourseUpdatesByStatusAsync_WhenStatusIsValid_ReturnsCorrectCourseUpdates()
        {
            // Arrange: Set up the data needed for the test
            var instructor = new User
            {
                Id = "instructor1",
                UserName = "instructor1@example.com",
                FullName = "Instructor One"
            };

            var category = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var course = new Course
            {
                Title = "Test Course",
                Description = "Description",
                ImageUrl = "http://img.com/img.png",
                Price = 99.99m,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/video.mp4",
                Status = CourseStatus.Pending
            };

            // Adding two course updates with different statuses and submission times
            var courseUpdate1 = new CourseUpdate
            {
                CourseID = course.CourseID,
                Status = CourseStatus.Pending,
                SubmittedAt = DateTime.UtcNow.AddDays(-1), // Older update
                Course = course
            };

            var courseUpdate2 = new CourseUpdate
            {
                CourseID = course.CourseID,
                Status = CourseStatus.Pending,
                SubmittedAt = DateTime.UtcNow, // Newer update
                Course = course
            };

            _context.Users.Add(instructor);       // Add instructor to context
            _context.Categories.Add(category);    // Add category to context
            _context.Courses.Add(course);         // Add course to context
            _context.CourseUpdates.Add(courseUpdate1);  // Add first update
            _context.CourseUpdates.Add(courseUpdate2);  // Add second update
            await _context.SaveChangesAsync();    // Save to DB

            // Act: Call the method under test with a specific CourseStatus filter
            var result = await _courseRepo.SelectCourseUpdatesByStatusAsync(new List<CourseStatus> { CourseStatus.Pending });

            // Assert: Verify the results
            Assert.Single(result);  // We expect only one result because it's grouped by CourseID and ordered by SubmittedAt (most recent first)
            Assert.Equal(courseUpdate2.SubmittedAt, result[0].SubmittedAt);  // Assert that the most recent update (courseUpdate2) is returned
            Assert.Equal(course.CourseID, result[0].CourseID);  // Assert the correct course ID is returned
            Assert.Equal(course.Instructor.FullName, result[0].Course.Instructor.FullName);  // Check if instructor is included
            Assert.Equal(course.Category.Name, result[0].Course.Category.Name);  // Check if category is included
        }

        [Fact]
        public async Task SelectCourseUpdatesByStatusAsync_WhenNoMatchingStatus_ReturnsEmpty()
        {
            // Act
            var result = await _courseRepo.SelectCourseUpdatesByStatusAsync(new List<CourseStatus> { CourseStatus.Approved });

            // Assert
            Assert.Empty(result);
        }



        [Fact]
        public async Task SelectLatestCourseUpdateByCourseIdAsync_WhenUpdatesExist_ReturnsLatestUpdate()
        {
            // Arrange
            var instructor = new User
            {
                Id = "instructor1",
                UserName = "instructor1@example.com",
                FullName = "Instructor One"
            };

            var category = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var course = new Course
            {
                Title = "Test Course",
                Description = "Description",
                ImageUrl = "http://img.com/img.png",
                Price = 99.99m,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/video.mp4",
                Status = CourseStatus.Pending
            };

            var courseUpdate1 = new CourseUpdate
            {
                CourseID = course.CourseID,
                Status = CourseStatus.Pending,
                SubmittedAt = DateTime.UtcNow.AddDays(-2), // Older update
                Course = course
            };

            var courseUpdate2 = new CourseUpdate
            {
                CourseID = course.CourseID,
                Status = CourseStatus.Approved,
                SubmittedAt = DateTime.UtcNow.AddDays(-1), // Newer update
                Course = course
            };

            var courseUpdate3 = new CourseUpdate
            {
                CourseID = course.CourseID,
                Status = CourseStatus.Rejected, 
                SubmittedAt = DateTime.UtcNow.AddHours(-1), 
                Course = course
            };

            _context.Users.Add(instructor);  
            _context.Categories.Add(category);  
            _context.Courses.Add(course);  
            _context.CourseUpdates.Add(courseUpdate1);  
            _context.CourseUpdates.Add(courseUpdate2); 
            _context.CourseUpdates.Add(courseUpdate3); 
            await _context.SaveChangesAsync();  

            // Act
            var result = await _courseRepo.SelectLatestCourseUpdateByCourseIdAsync(course.CourseID);

            // Assert
            Assert.NotNull(result);  
            Assert.Equal(courseUpdate2.SubmittedAt, result.SubmittedAt); 
            Assert.Equal(courseUpdate2.Status, result.Status);  
            Assert.Equal(course.CourseID, result.CourseID);  
        }

        [Fact]
        public async Task SelectLatestCourseUpdateByCourseIdAsync_WhenNoMatchingUpdates_ReturnsNull()
        {
            // Arrange
            var instructor = new User
            {
                Id = "instructor2",
                UserName = "instructor2@example.com",
                FullName = "Instructor Two"
            };

            var category = new Category
            {
                CategoryID = 2,
                Name = "Design"
            };

            var course = new Course
            {
                Title = "Design Course",
                Description = "Design Description",
                ImageUrl = "http://img.com/design.png",
                Price = 150.00m,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/design.mp4",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);  
            _context.Categories.Add(category);  
            _context.Courses.Add(course);  
            await _context.SaveChangesAsync();  

            // Act
            var result = await _courseRepo.SelectLatestCourseUpdateByCourseIdAsync(course.CourseID);

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public async Task SelectByIdAsync_ReturnsCourse_WhenExists()
        {
            // Arrange
            var instructor = new User
            {
                Id = "instructor-1",
                FullName = "Test Instructor",
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            var category = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var testCourse = new Course
            {
                CourseID = 1,
                Title = "Test Course",
                Description = "Test Description",
                ImageUrl = "test.jpg",
                VideoUrl = "test.mp4",
                Price = 49.99m,
                Instructor = instructor,
                Category = category,
                InstructorID = "instructor-1",
                CategoryID = 1,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now,
                Status = CourseStatus.Approved
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(testCourse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CourseID);
            Assert.Equal("Test Course", result.Title);
            Assert.NotNull(result.Instructor);
            Assert.Equal("instructor-1", result.Instructor.Id);
            Assert.NotNull(result.Category);
            Assert.Equal(1, result.Category.CategoryID);
        }

        [Fact]
        public async Task SelectByIdAsync_ThrowsKeyNotFoundException_WhenNotExists()
        {
            // Arrange 
            var nonExistentId = 999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _courseRepo.SelectByIdAsync(nonExistentId)
            );

            Assert.Equal($"Course with ID {nonExistentId} was not found.", exception.Message);
        }




        [Fact]
        public async Task SelectByIdsAsync_WhenValidCourseIds_ReturnsCorrectCourses()
        {
            // Arrange
            var instructor1 = new User
            {
                Id = "instructor1",
                UserName = "instructor1@example.com",
                FullName = "Instructor One"
            };

            var instructor2 = new User
            {
                Id = "instructor2",
                UserName = "instructor2@example.com",
                FullName = "Instructor Two"
            };

            var category1 = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var category2 = new Category
            {
                CategoryID = 2,
                Name = "Design"
            };

            var course1 = new Course
            {
                Title = "C# Programming",
                Description = "Learn C# programming",
                ImageUrl = "http://img.com/csharp.png",
                Price = 99.99m,
                InstructorID = instructor1.Id,
                Instructor = instructor1,
                CategoryID = category1.CategoryID,
                Category = category1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/csharp.mp4",
                Status = CourseStatus.Pending
            };

            var course2 = new Course
            {
                Title = "UI/UX Design",
                Description = "Learn Design Principles",
                ImageUrl = "http://img.com/design.png",
                Price = 120.00m,
                InstructorID = instructor2.Id,
                Instructor = instructor2,
                CategoryID = category2.CategoryID,
                Category = category2,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/design.mp4",
                Status = CourseStatus.Approved
            };

            var course3 = new Course
            {
                Title = "JavaScript Basics",
                Description = "Learn the basics of JavaScript",
                ImageUrl = "http://img.com/js.png",
                Price = 89.99m,
                InstructorID = instructor1.Id,
                Instructor = instructor1,
                CategoryID = category1.CategoryID,
                Category = category1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/js.mp4",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor1);  
            _context.Users.Add(instructor2);
            _context.Categories.Add(category1);  
            _context.Categories.Add(category2);
            _context.Courses.Add(course1);     
            _context.Courses.Add(course2);
            _context.Courses.Add(course3);
            await _context.SaveChangesAsync();  

            // Act
            var courseIds = new List<int> { course1.CourseID, course2.CourseID };
            var result = await _courseRepo.SelectByIdsAsync(courseIds);

            // Assert
            Assert.Equal(2, result.Count);  
            Assert.Contains(result, c => c.CourseID == course1.CourseID); 
            Assert.Contains(result, c => c.CourseID == course2.CourseID); 
            Assert.DoesNotContain(result, c => c.CourseID == course3.CourseID); 
        }

        [Fact]
        public async Task SelectByIdsAsync_WhenNoMatchingCourseIds_ReturnsEmptyList()
        {
            // Arrange
            var instructor1 = new User
            {
                Id = "instructor1",
                UserName = "instructor1@example.com",
                FullName = "Instructor One"
            };

            var category1 = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var course1 = new Course
            {
                Title = "C# Programming",
                Description = "Learn C# programming",
                ImageUrl = "http://img.com/csharp.png",
                Price = 99.99m,
                InstructorID = instructor1.Id,
                Instructor = instructor1,
                CategoryID = category1.CategoryID,
                Category = category1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.com/csharp.mp4",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor1);  
            _context.Categories.Add(category1);  
            _context.Courses.Add(course1);      
            await _context.SaveChangesAsync();  

            // Act
            var invalidCourseIds = new List<int> { 999, 1000 };  
            var result = await _courseRepo.SelectByIdsAsync(invalidCourseIds);

            // Assert
            Assert.Empty(result);
        }



        [Fact]
        public async Task SelectTitleByIdAsync_ReturnsCourseDetails_WhenCourseExists()
        {
            // Arrange
            var testCourse = new Course
            {
                CourseID = 1,
                Title = "Advanced C# Programming",
                Description = "Test Description",
                ImageUrl = "test.jpg",
                VideoUrl = "test.mp4",
                Price = 99.99m,
                InstructorID = "instructor-1",
                CategoryID = 1,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now,
                Status = CourseStatus.Approved
            };

            _context.Courses.Add(testCourse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectTitleByIdAsync(1);

            // Assert
            Assert.Equal(1, result.CourseID);
            Assert.Equal("Advanced C# Programming", result.Title);
        }

        [Fact]
        public async Task SelectTitleByIdAsync_ThrowsException_WhenCourseNotFound()
        {
            // Arrange 
            var nonExistentId = 999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _courseRepo.SelectTitleByIdAsync(nonExistentId)
            );

            Assert.Equal("Course not found.", exception.Message);
        }



        [Fact]
        public async Task InsertAsync_WithValidCourse_InsertsAndReturnsCourse()
        {
            // Arrange
            var instructor = new User { Id = "inst1", UserName = "inst1@example.com", FullName = "Instructor One" };
            var category = new Category { CategoryID = 1, Name = "Programming" };

            var course = new Course
            {
                Title = "Test Course",
                Description = "A test course",
                ImageUrl = "http://image.url",
                Price = 49.99m,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.InsertAsync(course);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(course.Title, result.Title);
            Assert.True(result.CourseID > 0);
            Assert.Single(_context.Courses);
        }

        [Fact]
        public async Task InsertAsync_WithNullCourse_ThrowsArgumentNullException()
        {
            // Arrange
            Course? course = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _courseRepo.InsertAsync(course));
            Assert.Equal("The course cannot be null. (Parameter 'course')", exception.Message);
        }



        [Fact]
        public async Task UpdateAsync_UpdatesCourse_WhenCourseExists()
        {
            // Arrange
            var originalCourse = new Course
            {
                CourseID = 1,
                Title = "Original Title",
                Description = "Original Description",
                ImageUrl = "original.jpg",
                VideoUrl = "original.mp4",
                Price = 49.99m,
                InstructorID = "instructor-1",
                CategoryID = 1,
                CreationDate = DateTime.Now.AddDays(-10),
                LastUpdatedDate = DateTime.Now.AddDays(-5),
                Status = CourseStatus.Pending
            };

            _context.Courses.Add(originalCourse);
            await _context.SaveChangesAsync();

            var updatedValues = new
            {
                Title = "Updated Title",
                Description = "Updated Description",
                ImageUrl = "updated.jpg",
                VideoUrl = "updated.mp4",
                Price = 59.99m,
                CategoryID = 2,
                LastUpdatedDate = DateTime.Now,
                Status = CourseStatus.Approved
            };

            // Act
            originalCourse.Title = updatedValues.Title;
            originalCourse.Description = updatedValues.Description;
            originalCourse.ImageUrl = updatedValues.ImageUrl;
            originalCourse.VideoUrl = updatedValues.VideoUrl;
            originalCourse.Price = updatedValues.Price;
            originalCourse.CategoryID = updatedValues.CategoryID;
            originalCourse.LastUpdatedDate = updatedValues.LastUpdatedDate;
            originalCourse.Status = updatedValues.Status;

            var result = await _courseRepo.UpdateAsync(originalCourse);

            _context.Entry(originalCourse).Reload();

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Title", originalCourse.Title);
            Assert.Equal("Updated Description", originalCourse.Description);
            Assert.Equal("updated.jpg", originalCourse.ImageUrl);
            Assert.Equal(59.99m, originalCourse.Price);
            Assert.Equal(2, originalCourse.CategoryID);
            Assert.Equal(CourseStatus.Approved, originalCourse.Status);
            Assert.Equal(updatedValues.LastUpdatedDate, originalCourse.LastUpdatedDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenCourseNotFound()
        {
            // Arrange
            var nonExistentCourse = new Course
            {
                CourseID = 999,
                Title = "Non-existent Course",
                // ... other required properties ...
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _courseRepo.UpdateAsync(nonExistentCourse)
            );

            Assert.Equal($"Course with ID {nonExistentCourse.CourseID} was not found.", exception.Message);
        }




        [Fact]
        public async Task DeleteAsync_WithValidCourseId_DeletesCourseAndReturnsTrue()
        {
            // Arrange
            var instructor = new User { Id = "inst1", UserName = "inst1@example.com", FullName = "Instructor One" };
            var category = new Category { CategoryID = 1, Name = "Programming" };

            var course = new Course
            {
                Title = "Course to Delete",
                Description = "This course will be deleted",
                ImageUrl = "http://img.url",
                Price = 10,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.DeleteAsync(course.CourseID);

            // Assert
            Assert.True(result);
            Assert.Empty(_context.Courses);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentCourseId_ReturnsFalse()
        {
            // Act
            var result = await _courseRepo.DeleteAsync(999); // ID that doesn't exist

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task ExistsAsync_WhenCourseExists_ReturnsTrue()
        {
            // Arrange
            var course = new Course
            {
                Title = "Existing Course",
                Description = "Test",
                ImageUrl = "http://image.url",
                Price = 49.99m,
                InstructorID = "inst1",
                CategoryID = 1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Pending
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.ExistsAsync(course.CourseID);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenCourseDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _courseRepo.ExistsAsync(12345); 

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task SelectPopularCoursesAsync_ReturnsPopularApprovedCourses()
        {
            // Arrange
            var instructor = new User
            {
                Id = "instructor-1",
                FullName = "Test Instructor",
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            var category = new Category { CategoryID = 1, Name = "Programming" };

            var testCourses = new List<Course>
            {
                // Popular and approved
                new Course {
                    CourseID = 1,
                    Title = "Popular Course 1",
                    Description = "Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    Instructor = instructor,
                    Category = category,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsPopular = true,
                    Status = CourseStatus.Approved
                },
                // Popular but not approved
                new Course {
                    CourseID = 2,
                    Title = "Popular Course 2",
                    Description = "Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    Instructor = instructor,
                    Category = category,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsPopular = true,
                    Status = CourseStatus.Pending
                },
                // Approved but not popular
                new Course {
                    CourseID = 3,
                    Title = "Regular Course",
                    Description = "Description 3",
                    ImageUrl = "image3.jpg",
                    VideoUrl = "video3.mp4",
                    Price = 69.99m,
                    Instructor = instructor,
                    Category = category,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsPopular = false,
                    Status = CourseStatus.Approved
                }
                };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPopularCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); 
            Assert.Equal(1, result[0].CourseID);
            Assert.True(result[0].IsPopular);
            Assert.Equal(CourseStatus.Approved, result[0].Status);
            Assert.NotNull(result[0].Instructor);
            Assert.NotNull(result[0].Category);
        }

        [Fact]
        public async Task SelectPopularCoursesAsync_ReturnsEmptyList_WhenNoPopularCourses()
        {
            // Arrange
            var instructor = new User
            {
                Id = "instructor-1",
                FullName = "Test Instructor",
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            var category = new Category { CategoryID = 1, Name = "Programming" };

            var testCourses = new List<Course>
            {
                // Not popular but approved
                new Course {
                    CourseID = 1,
                    Title = "Regular Course 1",
                    Description = "Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    Instructor = instructor,
                    Category = category,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsPopular = false,
                    Status = CourseStatus.Approved
            },
                // Popular but not approved
                new Course {
                    CourseID = 2,
                    Title = "Pending Course",
                    Description = "Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    Instructor = instructor,
                    Category = category,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    IsPopular = true,
                    Status = CourseStatus.Pending
            }
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPopularCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }




        [Fact]
        public async Task SearchCoursesAsync_WithMatchingQuery_ReturnsExpectedCourses()
        {
            // Arrange
            var instructor = new User { Id = "inst1", FullName = "John Doe", UserName = "john@example.com" };
            var category = new Category { CategoryID = 1, Name = "Development" };

            var course = new Course
            {
                Title = "C# for Beginners",
                Description = "Learn the basics of C# programming.",
                ImageUrl = "http://img.url",
                Price = 100,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Approved
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var results = await _courseRepo.SearchCoursesAsync("C#");

            // Assert
            Assert.Single(results);
            Assert.Equal("C# for Beginners", results.First().Title);
        }
        
        [Fact]
        public async Task SearchCoursesAsync_WithNonMatchingQuery_ReturnsEmptyList()
        {
            // Arrange — no matching course added
            var results = await _courseRepo.SearchCoursesAsync("nonexistent keyword");

            // Assert
            Assert.Empty(results);
        }



        [Fact]
        public async Task SelectTotalCoursesAsync_ReturnsCorrectCount_WhenCoursesExist()
        {
            // Arrange
            var testCourses = new List<Course>
            {
                new Course
                 {
                    CourseID = 1,
                    Title = "Course 1",
                    Description = "Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Approved
            },
                new Course
                {
                    CourseID = 2,
                    Title = "Course 2",
                    Description = "Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Pending
                }
            };

            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectTotalCoursesAsync();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task SelectTotalCoursesAsync_ReturnsZero_WhenNoCoursesExist()
        {
            // Arrange - Empty database

            // Act
            var result = await _courseRepo.SelectTotalCoursesAsync();

            // Assert
            Assert.Equal(0, result);
        }




        [Fact]
        public async Task SelectPendingCoursesAsync_WhenPendingCoursesExist_ReturnsOnlyPendingCourses()
        {
            // Arrange
            var instructor = new User { Id = "inst1", FullName = "Jane Doe", UserName = "jane@example.com" };
            var category = new Category { CategoryID = 1, Name = "Science" };

            var pendingCourse = new Course
            {
                Title = "Pending Course",
                Description = "This is pending",
                ImageUrl = "http://img.url",
                Price = 50,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Pending
            };

            var approvedCourse = new Course
            {
                Title = "Approved Course",
                Description = "Already approved",
                ImageUrl = "http://img2.url",
                Price = 75,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video2.url",
                Status = CourseStatus.Approved
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.AddRange(pendingCourse, approvedCourse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPendingCoursesAsync();

            // Assert
            var courseList = result.ToList();
            Assert.Single(courseList);
            Assert.Equal(CourseStatus.Pending, courseList[0].Status);
            Assert.Equal("Pending Course", courseList[0].Title);
        }

        [Fact]
        public async Task SelectPendingCoursesAsync_WhenNoPendingCoursesExist_ReturnsEmptyList()
        {
            // Arrange — only non-pending course
            var instructor = new User { Id = "inst2", FullName = "Mark Smith", UserName = "mark@example.com" };
            var category = new Category { CategoryID = 2, Name = "Math" };

            var approvedCourse = new Course
            {
                Title = "Non-Pending Course",
                Description = "Approved or Rejected",
                ImageUrl = "http://img.url",
                Price = 99,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Approved
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(approvedCourse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPendingCoursesAsync();

            // Assert
            Assert.Empty(result);
        }




        [Fact]
        public async Task SelectApprovedCoursesAsync_ReturnsOnlyApprovedCourses()
        {
            // Arrange
            var testCourses = new List<Course>
            {
                new Course
                {
                    CourseID = 1,
                    Title = "Approved Course",
                    Description = "Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Approved  // Should be included
            },
                new Course
                {
                    CourseID = 2,
                    Title = "Pending Course",
                    Description = "Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Pending  // Should be excluded
            },
                new Course
                {
                    CourseID = 3,
                    Title = "Another Approved Course",
                    Description = "Description 3",
                    ImageUrl = "image3.jpg",
                    VideoUrl = "video3.mp4",
                    Price = 69.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Approved  // Should be included
                }
            };

            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectApprovedCoursesAsync();
            var resultList = result.ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, course => Assert.Equal(CourseStatus.Approved, course.Status));
            Assert.Contains(resultList, c => c.CourseID == 1);
            Assert.Contains(resultList, c => c.CourseID == 3);
            Assert.DoesNotContain(resultList, c => c.CourseID == 2);
        }

        [Fact]
        public async Task SelectApprovedCoursesAsync_ReturnsEmptyList_WhenNoApprovedCourses()
        {
            // Arrange
            var testCourses = new List<Course>
            {
                new Course
                {
                    CourseID = 1,
                    Title = "Pending Course",
                    Description = "Description 1",
                    ImageUrl = "image1.jpg",
                    VideoUrl = "video1.mp4",
                    Price = 49.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Pending
                },
                new Course
                {
                    CourseID = 2,
                    Title = "Rejected Course",
                    Description = "Description 2",
                    ImageUrl = "image2.jpg",
                    VideoUrl = "video2.mp4",
                    Price = 59.99m,
                    InstructorID = "instructor-1",
                    CategoryID = 1,
                    CreationDate = DateTime.Now,
                    LastUpdatedDate = DateTime.Now,
                    Status = CourseStatus.Rejected
                }
            };

            _context.Courses.AddRange(testCourses);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectApprovedCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }




        [Fact]
        public async Task ApproveCourseAsync_WithValidCourseId_UpdatesStatusAndReturnsTrue()
        {
            // Arrange
            var instructor = new User { Id = "inst1", FullName = "Instructor One", UserName = "inst1@example.com" };
            var category = new Category { CategoryID = 1, Name = "Tech" };

            var course = new Course
            {
                Title = "Pending Course",
                Description = "To be approved",
                ImageUrl = "http://img.url",
                Price = 20,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.ApproveCourseAsync(course.CourseID);

            // Assert
            Assert.True(result);

            var updatedCourse = await _context.Courses.FindAsync(course.CourseID);
            Assert.Equal(CourseStatus.Approved, updatedCourse.Status);
        }

        [Fact]
        public async Task ApproveCourseAsync_WithInvalidCourseId_ReturnsFalse()
        {
            // Act
            var result = await _courseRepo.ApproveCourseAsync(999); // Non-existent ID

            // Assert
            Assert.False(result);
        }




        [Fact]
        public async Task RejectCourseAsync_WithValidCourseId_UpdatesStatusAndReturnsTrue()
        {
            // Arrange
            var instructor = new User { Id = "inst1", FullName = "Instructor Test", UserName = "inst1@example.com" };
            var category = new Category { CategoryID = 1, Name = "Test Category" };

            var course = new Course
            {
                Title = "Course to Reject",
                Description = "Test description",
                ImageUrl = "http://img.url",
                Price = 100,
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.RejectCourseAsync(course.CourseID);

            // Assert
            Assert.True(result);

            var updatedCourse = await _context.Courses.FindAsync(course.CourseID);
            Assert.Equal(CourseStatus.Rejected, updatedCourse.Status);
        }

        [Fact]
        public async Task RejectCourseAsync_WithInvalidCourseId_ReturnsFalse()
        {
            // Act
            var result = await _courseRepo.RejectCourseAsync(999); // Non-existent course ID

            // Assert
            Assert.False(result);
        }





        [Fact]
        public async Task RejectPendingUpdatesAsync_WhenPendingUpdateExists_RejectsAndReturnsTrue()
        {
            // Arrange
            var course = new Course
            {
                Title = "Test Course",
                Description = "Course with pending update",
                ImageUrl = "http://image.url",
                Price = 50,
                InstructorID = "inst1",
                CategoryID = 1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Approved
            };

            var update = new CourseUpdate
            {
                Course = course,
                CourseID = course.CourseID,
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                Status = CourseStatus.Pending
            };

            _context.Courses.Add(course);
            _context.CourseUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.RejectPendingUpdatesAsync(course.CourseID);

            // Assert
            Assert.True(result);

            var updated = await _context.CourseUpdates.FirstAsync();
            Assert.Equal(CourseStatus.Rejected, updated.Status);
        }

        [Fact]
        public async Task RejectPendingUpdatesAsync_WhenNoPendingUpdateExists_ReturnsFalse()
        {
            // Arrange
            var course = new Course
            {
                Title = "Course with no pending update",
                Description = "All updates are approved or rejected",
                ImageUrl = "http://image.url",
                Price = 60,
                InstructorID = "inst2",
                CategoryID = 2,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Approved
            };

            var update = new CourseUpdate
            {
                Course = course,
                CourseID = course.CourseID,
                SubmittedAt = DateTime.UtcNow.AddDays(-2),
                Status = CourseStatus.Approved
            };

            _context.Courses.Add(course);
            _context.CourseUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.RejectPendingUpdatesAsync(course.CourseID);

            // Assert
            Assert.False(result);
        }




        [Fact]
        public async Task SelectTotalCoursesByInstructorIdAsync_WithValidInstructorId_ReturnsCorrectCount()
        {
            // Arrange
            var instructor = new User { Id = "inst1", UserName = "inst1@example.com", FullName = "Instructor One" };
            var category = new Category { CategoryID = 1, Name = "Category 1" };

            var course1 = new Course
            {
                Title = "Course 1",
                Description = "Desc 1",
                ImageUrl = "img1",
                Price = 100,
                InstructorID = instructor.Id,
                CategoryID = category.CategoryID,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "vid1",
                Status = CourseStatus.Approved
            };

            var course2 = new Course
            {
                Title = "Course 2",
                Description = "Desc 2",
                ImageUrl = "img2",
                Price = 150,
                InstructorID = instructor.Id,
                CategoryID = category.CategoryID,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "vid2",
                Status = CourseStatus.Pending
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.AddRange(course1, course2);
            await _context.SaveChangesAsync();

            // Act
            var count = await _courseRepo.SelectTotalCoursesByInstructorIdAsync(instructor.Id);

            // Assert
            Assert.Equal(2, count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SelectTotalCoursesByInstructorIdAsync_WithNullOrEmptyInstructorId_ThrowsArgumentException(string instructorId)
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _courseRepo.SelectTotalCoursesByInstructorIdAsync(instructorId));
            Assert.Equal("InstructorId must be provided.", ex.Message);
        }



        [Fact]
        public async Task SelectEnrolledStudentsByCourseIdAsync_WithValidCourseId_ReturnsEnrolledStudents()
        {
            // Arrange
            var course = new Course
            {
                Title = "Test Course",
                Description = "Course description",
                ImageUrl = "http://image.url",
                Price = 100,
                InstructorID = "instructor1",
                CategoryID = 1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.url",
                Status = CourseStatus.Approved
            };

            var student1 = new User
            {
                Id = "student1",
                FullName = "Student One",
                UserName = "student1",  
                Email = "student1@example.com"
            };

            var student2 = new User
            {
                Id = "student2",
                FullName = "Student Two",
                UserName = "student2", 
                Email = "student2@example.com"
            };

            
            _context.Users.AddRange(student1, student2);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync(); 

            var purchase1 = new UserPurchaseCourse
            {
                UserID = student1.Id,
                UserName = student1.UserName,  
                CourseID = course.CourseID
            };

            var purchase2 = new UserPurchaseCourse
            {
                UserID = student2.Id,
                UserName = student2.UserName,  
                CourseID = course.CourseID
            };

            _context.UserPurchaseCourse.AddRange(purchase1, purchase2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectEnrolledStudentsByCourseIdAsync(course.CourseID);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Id == "student1");
            Assert.Contains(result, s => s.Id == "student2");
        }

        [Fact]
        public async Task SelectEnrolledStudentsByCourseIdAsync_WithInvalidCourseId_ReturnsNoStudents()
        {
            // Arrange
            var invalidCourseId = 9999;  

            // Act
            var result = await _courseRepo.SelectEnrolledStudentsByCourseIdAsync(invalidCourseId);

            // Assert
            Assert.Empty(result);  
        }




        [Fact]
        public async Task SelectCoursesWithStudentsForInstructorAsync_WithValidInstructorId_ReturnsCoursesWithStudents()
        {
            // Arrange
            var instructorId = "instructor1";

            var course1 = new Course
            {
                Title = "Course 1",
                InstructorID = instructorId,
                ImageUrl = "http://course1image.url",
                Description = "This is a description for Course 1", 
                VideoUrl = "http://course1video.url", 
                Status = CourseStatus.Approved
            };

            var course2 = new Course
            {
                Title = "Course 2",
                InstructorID = instructorId,
                ImageUrl = "http://course2image.url",
                Description = "This is a description for Course 2", 
                VideoUrl = "http://course2video.url", 
                Status = CourseStatus.Approved
            };

            var student1 = new User
            {
                Id = "student1",
                FullName = "Student One",
                UserName = "student1",  
                Email = "student1@example.com"
            };

            var student2 = new User
            {
                Id = "student2",
                FullName = "Student Two",
                UserName = "student2",  
                Email = "student2@example.com"
            };

            _context.Users.AddRange(student1, student2);
            _context.Courses.AddRange(course1, course2);
            await _context.SaveChangesAsync();

            var purchase1 = new UserPurchaseCourse
            {
                UserID = student1.Id,
                UserName = student1.UserName, 
                CourseID = course1.CourseID
            };

            var purchase2 = new UserPurchaseCourse
            {
                UserID = student2.Id,
                UserName = student2.UserName,  
                CourseID = course2.CourseID
            };

            _context.UserPurchaseCourse.AddRange(purchase1, purchase2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectCoursesWithStudentsForInstructorAsync(instructorId);

            // Assert
            Assert.Equal(2, result.Count);  
            Assert.Contains(result, c => c.CourseID == course1.CourseID && c.Students.Count == 1);  
            Assert.Contains(result, c => c.CourseID == course2.CourseID && c.Students.Count == 1);  
        }

        [Fact]
        public async Task SelectCoursesWithStudentsForInstructorAsync_WithInvalidInstructorId_ReturnsEmptyList()
        {
            // Arrange
            var invalidInstructorId = "invalid_instructor";  // Invalid ID

            // Act
            var result = await _courseRepo.SelectCoursesWithStudentsForInstructorAsync(invalidInstructorId);

            // Assert
            Assert.Empty(result); 
        }




        [Fact]
        public async Task SelectCourseForUpdateAsync_WithValidCourseAndInstructor_ReturnsCourseUpdateDTO()
        {
            // Arrange
            var course = new Course
            {
                Title = "Test Course",
                Description = "Test Description",
                ImageUrl = "http://image.url",
                Price = 100,
                VideoUrl = "http://video.url",
                InstructorID = "instructor1",
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                CategoryID = 1,
                Status = CourseStatus.Approved
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectCourseForUpdateAsync(course.CourseID, course.InstructorID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(course.Title, result.Title);
            Assert.Equal(course.Description, result.Description);
        }

        [Fact]
        public async Task SelectCourseForUpdateAsync_WithInvalidInstructorOrCourse_ReturnsNull()
        {
            // Arrange
            var course = new Course
            {
                Title = "Course",
                Description = "Desc",
                ImageUrl = "http://image.url",
                Price = 100,
                VideoUrl = "http://video.url",
                InstructorID = "instructor1",
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                CategoryID = 1,
                Status = CourseStatus.Approved
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectCourseForUpdateAsync(course.CourseID, "wrong_instructor");

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public async Task SelectPendingCourseUpdateAsync_WithPendingUpdateExists_ReturnsUpdate()
        {
            // Arrange
            var update = new CourseUpdate
            {
                CourseID = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                Status = CourseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _context.CourseUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPendingCourseUpdateAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(CourseStatus.Pending, result.Status);
            Assert.Equal("Updated Title", result.Title);
        }

        [Fact]
        public async Task SelectPendingCourseUpdateAsync_WithNoPendingUpdate_ReturnsNull()
        {
            // Arrange
            var update = new CourseUpdate
            {
                CourseID = 2,
                Title = "Approved Update",
                Description = "Approved Desc",
                Status = CourseStatus.Approved,
                SubmittedAt = DateTime.UtcNow
            };

            _context.CourseUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPendingCourseUpdateAsync(2);

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public async Task ApprovePendingUpdatesAsync_WithValidPendingUpdate_ApprovesUpdateAndCourse()
        {
            // Arrange
            var course = new Course
            {
                CourseID = 1,
                Title = "Original Title",
                Description = "Original Desc",
                Price = 100,
                ImageUrl = "img1.jpg",
                VideoUrl = "vid1.mp4",
                Status = CourseStatus.Pending,
                InstructorID = "instructor1",
                CategoryID = 1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            var pendingUpdate = new CourseUpdate
            {
                CourseID = 1,
                Title = "Updated Title",
                Description = "Updated Desc",
                Price = 200,
                ImageUrl = "img2.jpg",
                VideoUrl = "vid2.mp4",
                Status = CourseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            _context.CourseUpdates.Add(pendingUpdate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.ApprovePendingUpdatesAsync(course.CourseID);

            // Assert
            Assert.True(result);

            var updatedCourse = await _context.Courses.FindAsync(course.CourseID);
            Assert.Equal("Updated Title", updatedCourse.Title);
            Assert.Equal(200, updatedCourse.Price);
            Assert.Equal(CourseStatus.Approved, updatedCourse.Status);

            var stillExists = await _context.CourseUpdates.AnyAsync();
            Assert.False(stillExists); // update removed
        }

        [Fact]
        public async Task ApprovePendingUpdatesAsync_WithNoPendingUpdate_ReturnsFalse()
        {
            // Arrange
            var course = new Course
            {
                CourseID = 2,
                Title = "No Pending",
                Status = CourseStatus.Pending,
                Description = "Original Desc",
                Price = 100,
                ImageUrl = "img1.jpg",
                VideoUrl = "vid1.mp4",
                InstructorID = "instructor1",
                CategoryID = 1,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.ApprovePendingUpdatesAsync(course.CourseID);

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task SelectByInstructorIdAsync_WithValidInstructorId_ReturnsCourses()
        {
            // Arrange
            var instructor = new User { Id = "instructor1", FullName = "Instructor One", UserName = "instructor1", Email = "instructor1@example.com" };
            var category = new Category { CategoryID = 1, Name = "Programming" };
            var course = new Course
            {
                CourseID = 1,
                Title = "C# Basics",
                Description = "Learn C#",
                InstructorID = instructor.Id,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                Status = CourseStatus.Approved,
                ImageUrl = "img.jpg",
                VideoUrl = "video.mp4"
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectByInstructorIdAsync(instructor.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal("C# Basics", result.First().Title);
            Assert.NotNull(result.First().Instructor);
            Assert.NotNull(result.First().Category);
        }

        [Fact]
        public async Task SelectByInstructorIdAsync_WithUnknownInstructorId_ReturnsEmptyList()
        {
            // Act
            var result = await _courseRepo.SelectByInstructorIdAsync("nonexistent-instructor");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task SelectPendingUpdatesByInstructorIdAsync_WithValidInstructorId_ReturnsPendingUpdates()
        {
            // Arrange
            var instructor = new User { Id = "instructor1", UserName = "instructor1", Email = "i@example.com" , FullName = "instructor 1"};
            var category = new Category { CategoryID = 1, Name = "Dev" };
            var course = new Course
            {
                CourseID = 1,
                Title = "C#",
                Description = "Basics",
                InstructorID = instructor.Id,
                CategoryID = category.CategoryID,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                Status = CourseStatus.Approved,
                IsArchived = false,
                VideoUrl = "url",
                ImageUrl = "img"
            };
            var update = new CourseUpdate
            {
                CourseID = course.CourseID,
                Course = course,
                Title = "Updated C#",
                Status = CourseStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            _context.CourseUpdates.Add(update);
            await _context.SaveChangesAsync();

            // Act
            var result = await _courseRepo.SelectPendingUpdatesByInstructorIdAsync(instructor.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal("Updated C#", result[0].Title);
        }

        [Fact]
        public async Task SelectPendingUpdatesByInstructorIdAsync_WithNoPendingUpdates_ReturnsEmptyList()
        {
            // Arrange
            var instructorId = "no-updates-instructor";

            // Act
            var result = await _courseRepo.SelectPendingUpdatesByInstructorIdAsync(instructorId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }



        [Fact]
        public async Task SubmitCourseUpdateAsync_WithValidData_SavesPendingUpdate()
        {
            // Arrange
            var course = new Course
            {
                CourseID = 1,
                Title = "Original",
                Description = "Original Desc",
                Price = 50,
                InstructorID = "instructor1",
                CategoryID = 1,
                ImageUrl = "original.jpg",
                VideoUrl = "video.mp4",
                Status = CourseStatus.Approved,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "Updated Title",
                Description = "Updated Desc",
                Price = 99,
                VideoUrl = "updated.mp4",
                ImageFile = null
            };

            var fakeFileService = new Mock<IFileStorageService>();
            var repo = new CourseRepo(_context);

            // Act
            var result = await repo.SubmitCourseUpdateAsync(course.CourseID, updateDto, fakeFileService.Object);

            // Assert
            Assert.True(result);
            var update = await _context.CourseUpdates.FirstOrDefaultAsync();
            Assert.NotNull(update);
            Assert.Equal("Updated Title", update.Title);
            Assert.Equal(CourseStatus.Pending, update.Status);
        }

        [Fact]
        public async Task SubmitCourseUpdateAsync_WithInvalidCourseId_ReturnsFalse()
        {
            // Arrange
            var updateDto = new UpdateCourseRequestDTO
            {
                Title = "Update",
                Description = "Update",
                Price = 99,
                VideoUrl = "url",
                ImageFile = null
            };

            var fakeFileService = new Mock<IFileStorageService>();
            var repo = new CourseRepo(_context);

            // Act
            var result = await repo.SubmitCourseUpdateAsync(999, updateDto, fakeFileService.Object); // Non-existent ID

            // Assert
            Assert.False(result);
        }





    }
}
