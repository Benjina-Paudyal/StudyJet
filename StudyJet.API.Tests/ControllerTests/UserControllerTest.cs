using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;
using StudyJet.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ControllerTests
{
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTest()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object, null);
        }




        [Fact]
        public async Task CheckUsername_ShouldReturnBadRequest_WhenUsernameIsNullOrWhitespace()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            var controller = new UserController(mockUserService.Object, null);

            // Act
            var result = await controller.CheckUsername(" ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var message = messageProp.GetValue(value)?.ToString();
            Assert.Equal("Username is required.", message);
        }

        [Fact]
        public async Task CheckUsername_ShouldReturnOk_WithUsernameExistsTrue()
        {
            // Arrange
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(s => s.CheckUsernameExistsAsync("existingUser"))
                           .ReturnsAsync(true);

            var controller = new UserController(mockUserService.Object, null);

            // Act
            var result = await controller.CheckUsername("existingUser");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var prop = value.GetType().GetProperty("usernameExists");
            Assert.NotNull(prop);
            var exists = (bool)prop.GetValue(value);
            Assert.True(exists);
        }



        [Fact]
        public async Task CheckEmailExists_ShouldReturnBadRequest_WhenEmailIsNullOrWhitespace()
        {
            // Act
            var result = await _controller.CheckEmailExists(" ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var message = messageProp.GetValue(value)?.ToString();
            Assert.Equal("Email is required.", message);
        }

        [Fact]
        public async Task CheckEmailExists_ShouldReturnOk_WithEmailExistsTrue()
        {
            // Arrange
            _mockUserService.Setup(s => s.CheckIfEmailExistsAsync("existing@email.com"))
                            .ReturnsAsync(true);

            // Act
            var result = await _controller.CheckEmailExists("existing@email.com");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var prop = value.GetType().GetProperty("emailExists");
            Assert.NotNull(prop);
            var exists = (bool)prop.GetValue(value);
            Assert.True(exists);
        }



        [Fact]
        public async Task GetUsersByRole_ShouldReturnBadRequest_WhenRoleIsNullOrEmpty()
        {
            // Act
            var result = await _controller.GetUsersByRole("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequestResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var message = messageProp.GetValue(value)?.ToString();
            Assert.Equal("Role parameter cannot be empty.", message);
        }

        [Fact]
        public async Task GetUsersByRole_ShouldReturnNotFound_WhenNoUsersExistWithGivenRole()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserByRolesAsync("Instructor"))
                            .ReturnsAsync(new List<UserAdminDTO>());

            // Act
            var result = await _controller.GetUsersByRole("Instructor");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = notFoundResult.Value;
            var messageProp = value.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            var message = messageProp.GetValue(value)?.ToString();
            Assert.Equal("No users found with role: Instructor", message);
        }




        [Fact]
        public async Task CountStudents_ShouldReturnOk_WithStudentCount()
        {
            // Arrange
            int expectedCount = 42;
            _mockUserService.Setup(s => s.CountStudentAsync())
                            .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.CountStudents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedCount, okResult.Value);
        }

        [Fact]
        public async Task CountInstructors_ShouldReturnOk_WithInstructorCount()
        {
            // Arrange
            int expectedCount = 15; 
            _mockUserService.Setup(s => s.CountInstructorAsync())
                            .ReturnsAsync(expectedCount);

            // Act
            var result = await _controller.CountInstructors();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedCount, okResult.Value);
        }


    }
}
