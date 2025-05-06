using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Repositories.Implementation;
using StudyJet.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.RepositoryTests
{
    public class CartRepoTest
    {
        private readonly ApplicationDbContext _context;
        private readonly CartRepo _cartRepo;

        public CartRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _cartRepo = new CartRepo(_context);
        }

        [Fact]
        public async Task InsertCourseToCartAsync_Success()
        {
            // Arrange
            var userId = "user1";
            var courseId = 101;
            var price = 199.99m;

            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Carts = new List<Cart>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            await _cartRepo.InsertCourseToCartAsync(userId, courseId, price);

            // Assert
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserID == userId && c.CourseID == courseId);

            Assert.NotNull(cartItem);
            Assert.Equal(courseId, cartItem.CourseID);
            Assert.Equal(price, cartItem.TotalPrice);

            Assert.True(cartItem.CartID > 0);



        }

        [Fact]
        public async Task InsertCourseToCartAsync_UserNotFound_Failure()
        {
            // Arrange
            var userId = "invalidUserId"; 
            var courseId = 101;
            var price = 199.99m;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _cartRepo.InsertCourseToCartAsync(userId, courseId, price);
            });

            Assert.Equal($"User with ID {userId} not found.", exception.Message);
        }



        [Fact]
        public async Task SelectCartItemsAsync_Success()
        {
            // Arrange
            var studentUserId = "student1";
            var instructorUserId1 = "instructor1";
            var instructorUserId2 = "instructor2";
            var courseId1 = 101;
            var courseId2 = 102;
            var price1 = 199.99m;
            var price2 = 299.99m;

            // Create instructor users
            var instructor1 = new User
            {
                Id = instructorUserId1,
                UserName = "instructor1",
                Email = "instructor1@example.com",
                FullName = "Instructor One",
                EmailConfirmed = true
            };

            var instructor2 = new User
            {
                Id = instructorUserId2,
                UserName = "instructor2",
                Email = "instructor2@example.com",
                FullName = "Instructor Two",
                EmailConfirmed = true
            };

            // Create student user who owns the cart
            var studentUser = new User
            {
                Id = studentUserId,
                UserName = "student1",
                Email = "student1@example.com",
                FullName = "John Doe",
                EmailConfirmed = true
            };

            var course1 = new Course
            {
                CourseID = courseId1,
                Title = "Course 1",
                Price = price1,
                Description = "Good course to study",
                ImageUrl = "image1.jpg",
                InstructorID = instructorUserId1,
                Instructor = instructor1,
                VideoUrl = "video1.mp4"
            };

            var course2 = new Course
            {
                CourseID = courseId2,
                Title = "Course 2",
                Price = price2,
                Description = "Good course to study",
                ImageUrl = "image2.jpg",
                InstructorID = instructorUserId2,
                Instructor = instructor2,
                VideoUrl = "video2.mp4"
            };

            var cartItem1 = new Cart
            {
                CartID = 1,
                UserID = studentUserId,
                User = studentUser,
                CourseID = courseId1,
                Course = course1,
                TotalPrice = price1
            };

            var cartItem2 = new Cart
            {
                CartID = 2,
                UserID = studentUserId,
                User = studentUser,
                CourseID = courseId2,
                Course = course2,
                TotalPrice = price2
            };

            instructor1.CoursesTaught.Add(course1);
            instructor2.CoursesTaught.Add(course2);
            studentUser.Carts.Add(cartItem1);
            studentUser.Carts.Add(cartItem2);

            _context.Users.AddRange(studentUser, instructor1, instructor2);
            _context.Courses.AddRange(course1, course2);
            _context.Carts.AddRange(cartItem1, cartItem2);

            _context.Database.EnsureCreated();
            await _context.SaveChangesAsync();

            // Act
            var cartItems = await _cartRepo.SelectCartItemsAsync(studentUserId);

            // Assert
            Assert.NotEmpty(cartItems);
            Assert.Equal(2, cartItems.Count());

            // Assert the first cart item
            var firstCartItem = cartItems.First();
            Assert.Equal(cartItem1.CartID, firstCartItem.CartID);
            Assert.Equal(courseId1, firstCartItem.CourseID);
            Assert.Equal(course1.Title, firstCartItem.CourseTitle);
            Assert.Equal(course1.Description, firstCartItem.CourseDescription);
            Assert.Equal(price1, firstCartItem.Price);
            Assert.Equal(instructor1.FullName, firstCartItem.InstructorName);
            Assert.Equal(course1.ImageUrl, firstCartItem.ImageUrl);

            // Assert the second cart item
            var secondCartItem = cartItems.Last();
            Assert.Equal(cartItem2.CartID, secondCartItem.CartID);
            Assert.Equal(courseId2, secondCartItem.CourseID);
            Assert.Equal(course2.Title, secondCartItem.CourseTitle);
            Assert.Equal(course2.Description, secondCartItem.CourseDescription);
            Assert.Equal(price2, secondCartItem.Price);
            Assert.Equal(instructor2.FullName, secondCartItem.InstructorName);
            Assert.Equal(course2.ImageUrl, secondCartItem.ImageUrl);
        }

        [Fact]
        public async Task SelectCartItemsAsync_NoItemsInCart()
        {
            // Arrange
            var userId = "user1";

            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Carts = new List<Cart>()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var cartItems = await _cartRepo.SelectCartItemsAsync(userId);

            // Assert
            Assert.Empty(cartItems);  
        }



        [Fact]
        public async Task SelectCourseDetailsAsync_Success()
        {
            // Arrange
            var courseId = 101;
            var price = 199.99m;
            var course = new Course
            {
                CourseID = 101,
                Title = "Course 1",
                Price = 199.99m,
                Description = "Good course to study",
                ImageUrl = "image1.jpg", 
                InstructorID = "instructor1", 
                VideoUrl = "video1.mp4" 
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var retrievedCourse = await _cartRepo.SelectCourseDetailsAsync(courseId);

            // Assert
            Assert.NotNull(retrievedCourse);
            Assert.Equal(courseId, retrievedCourse.CourseID);
            Assert.Equal("Course 1", retrievedCourse.Title);
            Assert.Equal(price, retrievedCourse.Price);
        }

        [Fact]
        public async Task SelectCourseDetailsAsync_CourseNotFound_Failure()
        {
            // Arrange
            var nonExistingCourseId = 999;  

            // Act
            var result = await _cartRepo.SelectCourseDetailsAsync(nonExistingCourseId);

            // Assert
            Assert.Null(result);  
        }


        [Fact]
        public async Task SelectUserByIdAsync_Success()
        {
            // Arrange
            var userId = "user1";  
            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Carts = new List<Cart>()
            };

            // Add the user to the context
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.SelectUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);  
            Assert.Equal(userId, result?.Id);  
            Assert.Equal("John Doe", result?.FullName);  
        }

        [Fact]
        public async Task SelectUserByIdAsync_UserNotFound_Failure()
        {
            // Arrange
            var invalidUserId = "invalidUserId";  

            // Act
            var result = await _cartRepo.SelectUserByIdAsync(invalidUserId);

            // Assert
            Assert.Null(result); 
        }



        [Fact]
        public async Task DeleteCourseFromCartAsync_Success()
        {
            // Arrange
            var userId = "user1";
            var courseId = 101;
            var price = 199.99m;

            // Create user
            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Carts = new List<Cart>()
            };

            // Create course with required properties
            var course = new Course
            {
                CourseID = courseId,
                Title = "Course 1",
                Price = price,
                Description = "Good course to study",
                ImageUrl = "image1.jpg",  
                InstructorID = "instructor1",  
                VideoUrl = "video1.mp4" 
            };

            // Create cart item
            var cartItem = new Cart
            {
                CartID = 1,
                UserID = userId,
                User = user,
                CourseID = courseId,
                Course = course,
                TotalPrice = price
            };

            // Add to context
            _context.Users.Add(user);
            _context.Courses.Add(course);
            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.DeleteCourseFromCartAsync(userId, courseId);

            // Assert
            Assert.True(result); 
            Assert.Empty(user.Carts);  
        }

        [Fact]
        public async Task DeleteCourseFromCartAsync_UserNotFound_Failure()
        {
            // Arrange
            var userId = "invalidUserId";
            var courseId = 101;

            // Act
            var result = await _cartRepo.DeleteCourseFromCartAsync(userId, courseId);

            // Assert
            Assert.False(result);  
        }



        [Fact]
        public async Task IsCourseInCartAsync_Success()
        {
            // Arrange
            var userId = "user1";
            var courseId = 101;
            var price = 199.99m;

            // Create user and course
            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Carts = new List<Cart>()
            };

            var course = new Course
            {
                CourseID = courseId,
                Title = "Course 1",
                Price = price,
                Description = "Good course to study",
                ImageUrl = "image1.jpg",  
                InstructorID = "instructor1",
                VideoUrl = "video1.mp4"
            };

            var cartItem = new Cart
            {
                CartID = 1,
                UserID = userId,
                User = user,
                CourseID = courseId,
                Course = course,
                TotalPrice = price
            };

            // Add to context
            _context.Users.Add(user);
            _context.Courses.Add(course);
            _context.Carts.Add(cartItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.IsCourseInCartAsync(userId, courseId);

            // Assert
            Assert.True(result);  
        }

        [Fact]
        public async Task IsCourseInCartAsync_Failure()
        {
            // Arrange
            var userId = "user1";
            var courseId = 101;

            // Create user and course (without adding the course to the cart)
            var user = new User
            {
                Id = userId,
                FullName = "John Doe",
                UserName = "john_doe",
                Email = "john.doe@example.com",
                Carts = new List<Cart>()
            };

            var course = new Course
            {
                CourseID = courseId,
                Title = "Course 1",
                Price = 199.99m,
                Description = "Good course to study",
                ImageUrl = "image1.jpg",  
                InstructorID = "instructor1",
                VideoUrl = "video1.mp4"
            };

            // Add to context but without adding the course to the user's cart
            _context.Users.Add(user);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cartRepo.IsCourseInCartAsync(userId, courseId);

            // Assert
            Assert.False(result);  
        }




    }
}
