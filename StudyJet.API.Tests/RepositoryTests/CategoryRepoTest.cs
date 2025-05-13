using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.Repositories.Implementation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.RepositoryTests
{
    public class CategoryRepoTest
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryRepo _categoryRepo;

        public CategoryRepoTest() 
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext> ()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _categoryRepo = new CategoryRepo(_context);
        
        
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertCategorySuccessfully()
        {
            // Arrange
            var category = new Category
            {
                Name = "Programming"
            };

            // Act
            var result = await _categoryRepo.InsertAsync(category);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Programming", result.Name);
            Assert.True(result.CategoryID > 0); 

            
            var categoryInDb = await _context.Categories.FindAsync(result.CategoryID);
            Assert.NotNull(categoryInDb);
            Assert.Equal("Programming", categoryInDb.Name);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenNameIsMissing()
        {
            // Arrange
            var invalidCategory = new Category
            {
                // Name is required but not set
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => _categoryRepo.InsertAsync(invalidCategory));
        }



        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory_WhenCategoryExists()
        {
            // Arrange
            var category = new Category { Name = "Original Name" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Modify the category
            category.Name = "Updated Name";

            // Act
            await _categoryRepo.UpdateAsync(category);

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(category.CategoryID);
            Assert.NotNull(updatedCategory);
            Assert.Equal("Updated Name", updatedCategory.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCategoryNameIsMissing()
        {
            // Arrange
            var category = new Category { Name = "Initial" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            category.Name = null!;

            var validationContext = new ValidationContext(category);

            // Act & Assert
            Assert.Throws<ValidationException>(() => Validator.ValidateObject(category, validationContext, validateAllProperties: true));
        }


        [Fact]
        public async Task DeleteAsync_ReturnsTrueAndDeletesCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category
            {
                CategoryID = categoryId,
                Name = "Test Category",
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _categoryRepo.DeleteAsync(categoryId);

            // Assert
            Assert.True(result);
            var deletedCategory = await _context.Categories.FindAsync(categoryId);
            Assert.Null(deletedCategory);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var nonExistentCategoryId = 999;

            // Act
            var result = await _categoryRepo.DeleteAsync(nonExistentCategoryId);

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenCategoryExists()
        {
            // Arrange
            var category = new Category { CategoryID = 1, Name = "Programming" };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var repo = new CategoryRepo(_context);

            // Act
            var result = await repo.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var repo = new CategoryRepo(_context);

            // Act
            var result = await repo.ExistsAsync(999); //  non-existent ID

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task SelectAllAsync_ReturnsCategoriesWithApprovedCourses_WhenDataExists()
        {
            // Arrange
            var instructor = new User { Id = "instructor1", FullName = "John Doe" };

            var approvedCategory = new Category
            {
                CategoryID = 1,
                Name = "Programming",
                Courses = new List<Course>
        {
            new Course
            {
                CourseID = 101,
                Title = "C# Basics",
                Description = "Learn C# programming",
                ImageUrl = "csharp.jpg",
                VideoUrl = "intro.mp4",
                Price = 99.99m,
                InstructorID = "instructor1",
                Instructor = instructor,
                Status = CourseStatus.Approved,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now,
                CategoryID = 1
            }
        }
            };

            var emptyCategory = new Category
            {
                CategoryID = 2,
                Name = "Design",
                Courses = new List<Course>() 
            };

            _context.Users.Add(instructor);
            _context.Categories.AddRange(approvedCategory, emptyCategory);
            await _context.SaveChangesAsync();

            var repo = new CategoryRepo(_context);

            // Act
            var result = await repo.SelectAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Programming", result[0].Name);
            Assert.Single(result[0].Courses); 
            Assert.Equal("John Doe", result[0].Courses.First().Instructor.FullName);

            Assert.Equal("Design", result[1].Name);
            Assert.Empty(result[1].Courses); 
        }

        [Fact]
        public async Task SelectAllAsync_ReturnsEmptyList_WhenNoCategoriesExist()
        {
            // Act
            var result = await _categoryRepo.SelectAllAsync();

            // Assert
            Assert.Empty(result); 
        }




        [Fact]
        public async Task SelectByIdAsync_ReturnsCategoryWithApprovedCourses_WhenExists()
        {
            // Arrange
            var categoryId = 1;
            var instructor = new User
            {
                Id = "instructor1",
                FullName = "John Doe",
                Email = "john@example.com",
                UserName = "john@example.com"
            };

            // Create approved course
            var approvedCourse = new Course
            {
                CourseID = 101,
                Title = "C# Basics",
                Description = "Learn C#",
                ImageUrl = "csharp.jpg",
                VideoUrl = "intro.mp4",
                Price = 99.99m,
                Status = CourseStatus.Approved, 
                Instructor = instructor,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            // Create pending course 
            var pendingCourse = new Course
            {
                CourseID = 102,
                Title = "Java Basics",
                Description = "Learn Java",
                ImageUrl = "java.jpg",
                VideoUrl = "intro.mp4",
                Price = 89.99m,
                Status = CourseStatus.Pending,
                Instructor = instructor,
                CreationDate = DateTime.Now,
                LastUpdatedDate = DateTime.Now
            };

            var category = new Category
            {
                CategoryID = categoryId,
                Name = "Programming",
                Courses = new List<Course> { approvedCourse } 
            };

            _context.Users.Add(instructor);
            _context.Categories.Add(category);
            _context.Courses.Add(pendingCourse); 
            await _context.SaveChangesAsync();

            // Act
            var result = await _categoryRepo.SelectByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryID);
            Assert.Single(result.Courses); 
            Assert.Equal("C# Basics", result.Courses.First().Title);
            Assert.Equal("John Doe", result.Courses.First().Instructor.FullName);
        }

        [Fact]
        public async Task SelectByIdAsync_ShouldThrowException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var repo = new CategoryRepo(_context);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.SelectByIdAsync(999) // Non-existent ID
            );

            Assert.Equal("Category with ID 999 was not found.", exception.Message);
        }


    }
}
