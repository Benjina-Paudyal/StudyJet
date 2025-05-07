using Moq;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Category;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class CategoryServiceTest
    {
        private readonly Mock<ICategoryRepo> _mockRepo;
        private readonly CategoryService _categoryService;

        public CategoryServiceTest()
        {
            // Arrange
            _mockRepo = new Mock<ICategoryRepo>(); ;
            _categoryService = new CategoryService(_mockRepo.Object);
        }


        [Fact]
        public async Task GetAllAsync_ReturnsMappedCategoryResponseDTOList_Success()
        {
            // Arrange
            var categories = new List<Category>
        {
            new Category { CategoryID = 1, Name = "Development" },
            new Category { CategoryID = 2, Name = "Design" }
        };

            _mockRepo.Setup(repo => repo.SelectAllAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetAllAsync();

            // Assert: 
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Development", result[0].Name);
            Assert.Equal(1, result[0].CategoryID);
            Assert.Equal("Design", result[1].Name);
            Assert.Equal(2, result[1].CategoryID);
        }

        [Fact]
        public async Task GetAllAsync_ThrowsException_WhenRepoFails()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.SelectAllAsync())
                .ThrowsAsync(new System.Exception("Repository error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => _categoryService.GetAllAsync());
            Assert.Equal("Repository error", exception.Message);
        }



        [Fact]
        public async Task GetByIdAsync_ReturnsCategoryResponseDTO_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;

            var mockCategory = new Category
            {
                CategoryID = categoryId,
                Name = "Test Category",
                Courses = new List<Course>
                {
                    new Course
                    {
                        CourseID = 1,
                        Title = "Test Course 1",
                        Description = "Description 1",
                        ImageUrl = "http://example.com/image1.jpg",
                        Price = 100,
                        InstructorID = "1", 
                        Instructor = new User { FullName = "Instructor 1" },
                        CategoryID = categoryId,
                        Category = new Category { Name = "Test Category" },
                        CreationDate = DateTime.Now,
                        LastUpdatedDate = DateTime.Now,
                        VideoUrl = "http://example.com/video1.mp4"
                    }
                }
            };
            _mockRepo.Setup(repo => repo.SelectByIdAsync(categoryId))
                     .ReturnsAsync(mockCategory);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryID);
            Assert.Equal("Test Category", result.Name);
            Assert.Single(result.Courses);
            Assert.Equal("Test Course 1", result.Courses[0].Title);
            Assert.Equal("Instructor 1", result.Courses[0].InstructorName);

            _mockRepo.Verify(repo => repo.SelectByIdAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;

            _mockRepo.Setup(repo => repo.SelectByIdAsync(categoryId))
                     .ReturnsAsync((Category)null);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(repo => repo.SelectByIdAsync(categoryId), Times.Once);
        }




        [Fact]
        public async Task AddAsync_ReturnsCategoryId_WhenCategoryIsAddedSuccessfully()
        {
            // Arrange
            var categoryDto = new CategoryRequestDTO { Name = "Game Development" };
            var createdCategory = new Category { CategoryID = 42, Name = "Game Development" };

            _mockRepo.Setup(repo => repo.InsertAsync(It.IsAny<Category>()))
                     .ReturnsAsync(createdCategory);

            // Act
            var result = await _categoryService.AddAsync(categoryDto);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenInsertFails()
        {
            // Arrange
            var categoryDto = new CategoryRequestDTO { Name = "Invalid" };

            _mockRepo.Setup(repo => repo.InsertAsync(It.IsAny<Category>()))
                     .ThrowsAsync(new System.Exception("Insert failed"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<System.Exception>(() => _categoryService.AddAsync(categoryDto));
            Assert.Equal("Insert failed", ex.Message);
        }




        [Fact]
        public async Task UpdateAsync_UpdatesCategory_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var categoryDto = new CategoryRequestDTO { Name = "Updated Name" };
            var existingCategory = new Category { CategoryID = categoryId, Name = "Old Name" };

            _mockRepo.Setup(repo => repo.SelectByIdAsync(categoryId))
                     .ReturnsAsync(existingCategory);

            _mockRepo.Setup(repo => repo.UpdateAsync(existingCategory))
                     .Returns(Task.CompletedTask);

            // Act
            await _categoryService.UpdateAsync(categoryId, categoryDto);

            // Assert
            _mockRepo.Verify(repo => repo.SelectByIdAsync(categoryId), Times.Once);
            _mockRepo.Verify(repo => repo.UpdateAsync(It.Is<Category>(c => c.Name == "Updated Name")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;
            var categoryDto = new CategoryRequestDTO { Name = "Any Name" };

            _mockRepo.Setup(repo => repo.SelectByIdAsync(categoryId))
                     .ReturnsAsync((Category)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.UpdateAsync(categoryId, categoryDto));
            Assert.Equal("Category not found", ex.Message);
        }



        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenCategoryExistsAndIsDeleted()
        {
            // Arrange
            var categoryId = 1;

            _mockRepo.Setup(repo => repo.ExistsAsync(categoryId))
                     .ReturnsAsync(true);

            _mockRepo.Setup(repo => repo.DeleteAsync(categoryId))
                     .ReturnsAsync(true); 

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            Assert.True(result); 
            _mockRepo.Verify(repo => repo.DeleteAsync(categoryId), Times.Once); 
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;

            _mockRepo.Setup(repo => repo.ExistsAsync(categoryId))
                     .ReturnsAsync(false);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId);

            // Assert
            Assert.False(result); 
            _mockRepo.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never); 
        }




        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;

            _mockRepo.Setup(repo => repo.ExistsAsync(categoryId))
                     .ReturnsAsync(true);

            // Act
            var result = await _categoryService.ExistsAsync(categoryId);

            // Assert
            Assert.True(result); 
            _mockRepo.Verify(repo => repo.ExistsAsync(categoryId), Times.Once); 
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 999;

            _mockRepo.Setup(repo => repo.ExistsAsync(categoryId))
                     .ReturnsAsync(false);

            // Act
            var result = await _categoryService.ExistsAsync(categoryId);

            // Assert
            Assert.False(result); 
            _mockRepo.Verify(repo => repo.ExistsAsync(categoryId), Times.Once); 
        }

    }
}

