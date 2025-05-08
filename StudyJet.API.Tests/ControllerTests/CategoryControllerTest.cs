using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.DTOs.Category;
using StudyJet.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ControllerTests
{
    public class CategoryControllerTest
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoryController _controller;


        public CategoryControllerTest()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoryController(_mockCategoryService.Object);
        }


        [Fact]
        public async Task GetAllCategories_ReturnsOkResult_WithListOfCategories()
        {
            // Arrange
            var mockCategories = new List<CategoryResponseDTO>
            {
                new CategoryResponseDTO { CategoryID = 1, Name = "Electronics" },
                new CategoryResponseDTO { CategoryID = 2, Name = "Books" }
            };
            _mockCategoryService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockCategories);

            // Act
            var result = await _controller.GetAllCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<CategoryResponseDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAllCategories_WhenServiceThrowsException_Returns500WithMessage()
        {
            // Arrange
            _mockCategoryService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllCategories();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result); 
            Assert.Equal(500, objectResult.StatusCode);  
            Assert.Equal("Internal server error: Database error", objectResult.Value); 
        }




        [Fact]
        public async Task GetCategoryById_ReturnsOkResult_WhenCategoryExists()
        {
            // Arrange
            var categoryId = 1;
            var mockCategory = new CategoryResponseDTO { CategoryID = categoryId, Name = "Electronics" };
            _mockCategoryService.Setup(s => s.GetByIdAsync(categoryId)).ReturnsAsync(mockCategory);

            // Act
            var result = await _controller.GetCategoryById(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CategoryResponseDTO>(okResult.Value);
            Assert.Equal(categoryId, returnValue.CategoryID);
            Assert.Equal("Electronics", returnValue.Name);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsNotFoundResult_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = 1;
            _mockCategoryService.Setup(s => s.GetByIdAsync(categoryId)).ReturnsAsync((CategoryResponseDTO)null);

            // Act
            var result = await _controller.GetCategoryById(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }



        [Fact]
        public async Task AddCategory_ReturnsCreatedAtAction_WhenCategoryIsCreatedSuccessfully()
        {
            // Arrange
            var categoryDto = new CategoryRequestDTO { Name = "New Category" };
            var createdCategoryId = 1; // Mocked return value for the created category ID
            _mockCategoryService.Setup(s => s.AddAsync(categoryDto)).ReturnsAsync(createdCategoryId);

            // Act
            var result = await _controller.AddCategory(categoryDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal("GetCategoryById", createdAtActionResult.ActionName);
            Assert.Equal(createdCategoryId, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(createdCategoryId, createdAtActionResult.Value);
        }

        [Fact]
        public async Task AddCategory_ReturnsBadRequest_WhenCategoryDataIsInvalid()
        {
            // Arrange
            var invalidCategoryDto = new CategoryRequestDTO();  

            _controller.ModelState.AddModelError("Name", "Category name is required");

            // Act
            var result = await _controller.AddCategory(invalidCategoryDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);  
            Assert.Equal(400, badRequestResult.StatusCode);  

            var errors = badRequestResult.Value as SerializableError;
            Assert.NotNull(errors);  

            var errorMessages = errors["Name"] as IEnumerable<string>;
            Assert.Contains("Category name is required", errorMessages);
        }


        [Fact]
        public async Task UpdateCategory_ReturnsBadRequest_WhenUpdateIsNotAllowed()
        {
            // Arrange
            int categoryId = 1;
            var categoryDto = new CategoryRequestDTO { Name = "Updated Category" }; 

            // Act
            var result = await _controller.UpdateCategory(categoryId, categoryDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); 
            Assert.Equal(400, badRequestResult.StatusCode);  
            Assert.Contains("Category update is not allowed at the moment", badRequestResult.Value.ToString());  
        }


        [Fact]
        public async Task DeleteCategory_ReturnsBadRequest_WhenDeletionIsNotAllowed()
        {
            // Arrange
            int categoryId = 1;  

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); 
            Assert.Equal(400, badRequestResult.StatusCode); 
            Assert.Contains("Category deletion is not allowed at the moment", badRequestResult.Value.ToString());  
        }



        [Fact]
        public async Task CategoryExists_ReturnsTrue_WhenCategoryExists()
        {
            // Arrange
            int categoryId = 1;
            _mockCategoryService.Setup(s => s.ExistsAsync(categoryId)).ReturnsAsync(true);

            // Act
            var result = await _controller.CategoryExists(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);  // Expecting a 200 OK response
            Assert.Equal(200, okResult.StatusCode);  // Ensure the status code is 200
            Assert.True((bool)okResult.Value);  // Ensure the returned value is 'true'
        }


        [Fact]
        public async Task CategoryExists_ReturnsFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            int categoryId = 2;
            _mockCategoryService.Setup(s => s.ExistsAsync(categoryId)).ReturnsAsync(false);

            // Act
            var result = await _controller.CategoryExists(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);  // Expecting a 200 OK response
            Assert.Equal(200, okResult.StatusCode);  // Ensure the status code is 200
            Assert.False((bool)okResult.Value);  // Ensure the returned value is 'false'
        }





    }


}
