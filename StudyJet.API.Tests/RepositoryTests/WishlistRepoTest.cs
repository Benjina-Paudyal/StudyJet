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
    public class WishlistRepoTest
    {

        private readonly ApplicationDbContext _context;
        private readonly WishlistRepo _cartRepo;

        public WishlistRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _cartRepo = new WishlistRepo(_context);
        }

        [Fact]
        public async Task SelectWishlistByIdAsync_ReturnsWishlist_WhenUserExists()
        {
            // Arrange
            var userId = "user123";
            var instructorId = "instructor456";

            var instructor = new User
            {
                Id = instructorId,
                FullName = "John Doe"
            };

            var category = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var course = new Course
            {
                CourseID = 1,
                Title = "Test Course",
                Description = "A test course description",
                ImageUrl = "http://example.com/image.jpg",
                Price = 49.99m,
                InstructorID = instructorId,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://example.com/video.mp4",
                Status = CourseStatus.Approved
            };

            var wishlist = new Wishlist
            {
                UserID = userId,
                CourseID = course.CourseID,
                Course = course
            };

            _context.Users.Add(instructor);       // Add the instructor to context
            _context.Categories.Add(category);    // Add the category to context
            _context.Courses.Add(course);         // Add the course to context
            _context.Wishlists.Add(wishlist);     // Add the wishlist entry to context
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.SelectWishlistByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Expect exactly one course in the wishlist

            var wishlistItem = result.First();
            Assert.Equal(course.CourseID, wishlistItem.CourseID);
            Assert.Equal(course.Title, wishlistItem.Title);
            Assert.Equal(course.Description, wishlistItem.Description);
            Assert.Equal(course.ImageUrl, wishlistItem.ImageUrl);
            Assert.Equal(course.Price, wishlistItem.Price);
            Assert.Equal(instructor.FullName, wishlistItem.InstructorName);
            Assert.Equal(category.Name, wishlistItem.CategoryName);
            Assert.Equal(course.CreationDate, wishlistItem.CreationDate);
            Assert.Equal(course.LastUpdatedDate, wishlistItem.LastUpdatedDate);
        }

        [Fact]
        public async Task SelectWishlistByIdAsync_ReturnsEmpty_WhenUserHasNoWishlist()
        {
            // Arrange
            var userId = "nonexistent_user"; 

            // Act
            var result = await _cartRepo.SelectWishlistByIdAsync(userId);

            // Assert
            Assert.NotNull(result); 
            Assert.Empty(result);   
        }



        [Fact]
        public async Task InsertCourseToWishlistAsync_ReturnsTrue_WhenValidUserAndCourse()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            var user = new User { Id = userId, FullName = "Test User" };
            var course = new Course
            {
                CourseID = courseId,
                Title = "Test Course",
                Description = "Test Description",
                ImageUrl = "http://img.jpg",
                Price = 50,
                InstructorID = "instructor1",
                Instructor = new User { Id = "instructor1", FullName = "Instructor" },
                CategoryID = 1,
                Category = new Category { CategoryID = 1, Name = "Category" },
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.mp4",
                Status = CourseStatus.Approved
            };

            _context.Users.Add(user);
            _context.Users.Add(course.Instructor);
            _context.Categories.Add(course.Category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.InsertCourseToWishlistAsync(userId, courseId);

            // Assert
            Assert.True(result);
            var wishlistEntry = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserID == userId && w.CourseID == courseId);
            Assert.NotNull(wishlistEntry);
        }

        [Fact]
        public async Task InsertCourseToWishlistAsync_ReturnsFalse_WhenCourseAlreadyInWishlist()
        {
            // Arrange
            var userId = "user123";
            var courseId = 1;

            var user = new User { Id = userId, FullName = "User" };
            var course = new Course
            {
                CourseID = 1,
                Title = "Test Course",
                Description = "Sample description", 
                ImageUrl = "img.jpg",
                Price = 20,
                InstructorID = "inst1",
                Instructor = new User { Id = "inst1", FullName = "Instructor" },
                CategoryID = 1,
                Category = new Category { CategoryID = 1, Name = "Cat" },
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "url",
                Status = CourseStatus.Approved
            };

            user.Wishlists.Add(new Wishlist { UserID = userId, CourseID = courseId });

            _context.Users.Add(user);
            _context.Users.Add(course.Instructor);
            _context.Categories.Add(course.Category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.InsertCourseToWishlistAsync(userId, courseId);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task DeleteCourseFromWishlistAsync_ReturnsTrue_WhenCourseIsInWishlist()
        {
            // Arrange
            var userId = "user123";
            var instructorId = "instructor456";
            var courseId = 1;

            var user = new User
            {
                Id = userId,
                FullName = "Test User"
            };

            var instructor = new User
            {
                Id = instructorId,
                FullName = "Instructor Name"
            };

            var category = new Category
            {
                CategoryID = 1,
                Name = "Programming"
            };

            var course = new Course
            {
                CourseID = courseId,
                Title = "Sample Course",
                Description = "Sample Description",
                ImageUrl = "http://image.jpg",
                Price = 100,
                InstructorID = instructorId,
                Instructor = instructor,
                CategoryID = category.CategoryID,
                Category = category,
                CreationDate = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow,
                VideoUrl = "http://video.mp4",
                Status = CourseStatus.Approved
            };

            var wishlist = new Wishlist
            {
                UserID = userId,
                CourseID = courseId,
                Course = course
            };

            user.Wishlists.Add(wishlist);

            _context.Users.AddRange(user, instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.DeleteCourseFromWishlistAsync(userId, courseId);

            // Assert
            Assert.True(result);
            var wishlistEntry = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserID == userId && w.CourseID == courseId);
            Assert.Null(wishlistEntry);
        }


        [Fact]
        public async Task DeleteCourseFromWishlistAsync_ReturnsFalse_WhenCourseNotInWishlist()
        {
            // Arrange
            var userId = "user123";
            var user = new User { Id = userId, FullName = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.DeleteCourseFromWishlistAsync(userId, courseId: 99);

            // Assert
            Assert.False(result); 
        }


        [Fact]
        public async Task DeleteCourseFromWishlistAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = "nonexistent_user";
            var courseId = 1;

            // Act
            var result = await _cartRepo.DeleteCourseFromWishlistAsync(userId, courseId);

            // Assert
            Assert.False(result);
        }




    }
}
