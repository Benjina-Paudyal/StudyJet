using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using StudyJet.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class UserServiceTest
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IUserRepo> _mockUserRepo;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly UserService _userService;

        public UserServiceTest()
        {
            // Mock the UserManager<User> constructor with necessary parameters
            var storeMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(storeMock.Object,
                                                         Mock.Of<IOptions<IdentityOptions>>(),
                                                         Mock.Of<IPasswordHasher<User>>(),
                                                         Array.Empty<IUserValidator<User>>(),
                                                         Array.Empty<IPasswordValidator<User>>(),
                                                         Mock.Of<ILookupNormalizer>(),
                                                         Mock.Of<IdentityErrorDescriber>(),
                                                         Mock.Of<IServiceProvider>(),
                                                         Mock.Of<ILogger<UserManager<User>>>());

            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(),
                                                                   Array.Empty<IRoleValidator<IdentityRole>>(),
                                                                   Mock.Of<ILookupNormalizer>(),
                                                                   Mock.Of<IdentityErrorDescriber>(),
                                                                   Mock.Of<ILogger<RoleManager<IdentityRole>>>());

            _mockUserRepo = new Mock<IUserRepo>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockConfiguration = new Mock<IConfiguration>();

            _userService = new UserService(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockUserRepo.Object,
                _mockFileStorageService.Object,
                _mockEmailService.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task CheckIfEmailExistsAsync_EmailExists_ReturnsTrue()
        {
            // Arrange
            string email = "test@example.com";

            var user = new User { Email = email };

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);

            // Act
            bool result = await _userService.CheckIfEmailExistsAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckIfEmailExistsAsync_EmailDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string email = "test@example.com";

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync((User)null);

            // Act
            bool result = await _userService.CheckIfEmailExistsAsync(email);

            // Assert
            Assert.False(result);
        }




        [Fact]
        public async Task CheckUsernameExistsAsync_UsernameExists_ReturnsTrue()
        {
            // Arrange
            string username = "testuser";

            var user = new User { UserName = username };

            _mockUserManager.Setup(m => m.FindByNameAsync(username)).ReturnsAsync(user);

            // Act
            bool result = await _userService.CheckUsernameExistsAsync(username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CheckUsernameExistsAsync_UsernameDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string username = "testuser";

            _mockUserManager.Setup(m => m.FindByNameAsync(username)).ReturnsAsync((User)null);

            // Act
            bool result = await _userService.CheckUsernameExistsAsync(username);

            // Assert
            Assert.False(result);
        }



        [Fact]
        public async Task GetUserByEmailAsync_EmailExists_ReturnsUser()
        {
            // Arrange
            string email = "test@example.com";

            var user = new User { Email = email };

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync(user);

            // Act
            User result = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_EmailDoesNotExist_ReturnsNull()
        {
            // Arrange
            string email = "nonexistent@example.com";

            _mockUserManager.Setup(m => m.FindByEmailAsync(email)).ReturnsAsync((User)null);

            // Act
            User result = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }




        [Fact]
        public async Task GetUserByIdAsync_UserExists_ReturnsUser()
        {
            // Arrange
            string userId = "123";
            var user = new User { Id = userId, UserName = "testuser" };

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            User result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            string userId = "123";

            _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            User result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public async Task AddUserAsync_UserCreatedSuccessfully_ReturnsSuccess()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            string password = "P@ssw0rd";
            var identityResult = IdentityResult.Success;

            _mockUserManager.Setup(m => m.CreateAsync(user, password)).ReturnsAsync(identityResult);

            // Act
            IdentityResult result = await _userService.AddUserAsync(user, password);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task AddUserAsync_UserCreationFails_ReturnsFailure()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            string password = "P@ssw0rd";
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "User creation failed" });

            _mockUserManager.Setup(m => m.CreateAsync(user, password)).ReturnsAsync(identityResult);

            // Act
            IdentityResult result = await _userService.AddUserAsync(user, password);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Single(result.Errors);
            Assert.Equal("User creation failed", result.Errors.First().Description);
        }




        [Fact]
        public async Task UpdateUserAsync_UserIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _userService.UpdateUserAsync(null));

            // Verify 
            Assert.Equal("user", exception.ParamName);
            Assert.Contains("User cannot be null", exception.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdateFails_ThrowsExceptionWithErrorMessage()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Failed to update user" });

            _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(identityResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _userService.UpdateUserAsync(user));

            // Verify 
            Assert.Contains("Failed to update user", exception.Message);
        }



        [Fact]
        public void ValidatePassword_PasswordsMatch_ReturnsTrue()
        {
            // Arrange
            string password = "P@ssw0rd";
            string confirmPassword = "P@ssw0rd";

            // Act
            bool result = _userService.ValidatePassword(password, confirmPassword);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidatePassword_PasswordsDoNotMatch_ReturnsFalse()
        {
            // Arrange
            string password = "P@ssw0rd";
            string confirmPassword = "DifferentP@ssw0rd";

            // Act
            bool result = _userService.ValidatePassword(password, confirmPassword);

            // Assert
            Assert.False(result);
        }




        [Fact]
        public async Task EnsureDefaultRoleExistsAsync_RoleExists_DoesNotCreateRole()
        {
            // Arrange
            _mockRoleManager.Setup(m => m.RoleExistsAsync("Student")).ReturnsAsync(true);

            // Act
            await _userService.EnsureDefaultRoleExistsAsync();

            // Assert
            _mockRoleManager.Verify(m => m.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        }

        [Fact]
        public async Task EnsureDefaultRoleExistsAsync_RoleDoesNotExist_CreatesRole()
        {
            // Arrange
            _mockRoleManager.Setup(m => m.RoleExistsAsync("Student")).ReturnsAsync(false);
            _mockRoleManager.Setup(m => m.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            // Act
            await _userService.EnsureDefaultRoleExistsAsync();

            // Assert
            _mockRoleManager.Verify(m => m.CreateAsync(It.Is<IdentityRole>(r => r.Name == "Student")), Times.Once);
        }



        [Fact]
        public async Task AssignUserRoleAsync_Success_ReturnsIdentityResultSuccess()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            string role = "Student";
            var identityResult = IdentityResult.Success;

            _mockUserManager.Setup(m => m.AddToRoleAsync(user, role)).ReturnsAsync(identityResult);

            // Act
            var result = await _userService.AssignUserRoleAsync(user, role);

            // Assert
            Assert.Equal(IdentityResult.Success, result);
        }

        [Fact]
        public async Task AssignUserRoleAsync_Failure_ReturnsFailedIdentityResult()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            string role = "Student";
            var identityError = new IdentityError { Description = "Failed to assign role" };
            var identityResult = IdentityResult.Failed(identityError);

            _mockUserManager.Setup(m => m.AddToRoleAsync(user, role)).ReturnsAsync(identityResult);

            // Act
            var result = await _userService.AssignUserRoleAsync(user, role);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to assign role", result.Errors.First().Description);
        }




        [Fact]
        public async Task GetUserRolesAsync_UserHasRoles_ReturnsRolesList()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            var roles = new List<string> { "Student", "Admin" };

            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var result = await _userService.GetUserRolesAsync(user);

            // Assert
            Assert.Equal(roles, result);
        }

        [Fact]
        public async Task GetUserRolesAsync_UserHasNoRoles_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            var roles = new List<string>(); // No roles

            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var result = await _userService.GetUserRolesAsync(user);

            // Assert
            Assert.Empty(result); // Expecting an empty list since no roles are assigned
        }




        [Fact]
        public async Task GetUserByRolesAsync_ReturnsUserAdminDTOList()
        {
            // Arrange
            var role = "Admin";
            var userAdminDTOList = new List<UserAdminDTO>
        {
            new UserAdminDTO { ID = "1", FullName = "John Doe", UserName = "johndoe", Email = "johndoe@example.com" },
            new UserAdminDTO { ID = "2", FullName = "Jane Smith", UserName = "janesmith", Email = "janesmith@example.com" }
        };

            _mockUserRepo.Setup(repo => repo.SelectUsersByRoleAsync(role)).ReturnsAsync(userAdminDTOList);

            // Act
            var result = await _userService.GetUserByRolesAsync(role);

            // Assert
            Assert.NotNull(result); 
            Assert.Equal(2, result.Count); 
            Assert.Equal("John Doe", result[0].FullName); 
            Assert.Equal("Jane Smith", result[1].FullName); 
        }

        [Fact]
        public async Task GetUserByRolesAsync_ReturnsEmptyList_WhenNoUsersFound()
        {
            // Arrange
            var role = "Admin";

            // Mock the repo to return an empty list when no users are found for the given role
            _mockUserRepo.Setup(repo => repo.SelectUsersByRoleAsync(role)).ReturnsAsync(new List<UserAdminDTO>());

            // Act
            var result = await _userService.GetUserByRolesAsync(role);

            // Assert
            Assert.NotNull(result); // Ensure result is not null
            Assert.Empty(result); // Ensure result is an empty list
        }

        [Fact]
        public async Task GetUserByRolesAsync_ReturnsNull_WhenRepoReturnsNull()
        {
            // Arrange
            var role = "Admin";

            _mockUserRepo.Setup(repo => repo.SelectUsersByRoleAsync(role)).ReturnsAsync((List<UserAdminDTO>)null);

            // Act
            var result = await _userService.GetUserByRolesAsync(role);

            // Assert
            Assert.Null(result); 
        }





        [Fact]
        public async Task CountUserByRoleAsync_ReturnsCorrectCount_WhenUsersFound()
        {
            // Arrange
            var role = "Admin";
            var userAdminDTOList = new List<UserAdminDTO>
        {
            new UserAdminDTO { ID = "1", FullName = "John Doe", UserName = "johndoe", Email = "johndoe@example.com" },
            new UserAdminDTO { ID = "2", FullName = "Jane Smith", UserName = "janesmith", Email = "janesmith@example.com" }
        };

            _mockUserRepo.Setup(repo => repo.SelectUsersByRoleAsync(role)).ReturnsAsync(userAdminDTOList);

            // Act
            var result = await _userService.CountUserByRoleAsync(role);

            // Assert
            Assert.Equal(2, result); 
        }

        [Fact]
        public async Task CountUserByRoleAsync_ReturnsZero_WhenNoUsersFound()
        {
            // Arrange
            var role = "Admin";

            _mockUserRepo.Setup(repo => repo.SelectUsersByRoleAsync(role)).ReturnsAsync(new List<UserAdminDTO>());

            // Act
            var result = await _userService.CountUserByRoleAsync(role);

            // Assert
            Assert.Equal(0, result); 
        }




        [Fact]
        public async Task CountStudentAsync_ReturnsCorrectCount_WhenStudentsExist()
        {
            // Arrange
            var studentCount = 5;

            _mockUserRepo.Setup(repo => repo.CountUsersByRoleAsync("Student")).ReturnsAsync(studentCount);

            // Act
            var result = await _userService.CountStudentAsync();

            // Assert
            Assert.Equal(studentCount, result); 
        }

        [Fact]
        public async Task CountStudentAsync_ReturnsZero_WhenNoStudentsFound()
        {
            // Arrange
            var studentCount = 0;

            _mockUserRepo.Setup(repo => repo.CountUsersByRoleAsync("Student")).ReturnsAsync(studentCount);

            // Act
            var result = await _userService.CountStudentAsync();

            // Assert
            Assert.Equal(studentCount, result); 
        }



        [Fact]
        public async Task CountInstructorAsync_ReturnsCorrectCount_WhenInstructorsExist()
        {
            // Arrange
            var instructorCount = 3;

            _mockUserRepo.Setup(repo => repo.CountUsersByRoleAsync("Instructor")).ReturnsAsync(instructorCount);

            // Act
            var result = await _userService.CountInstructorAsync();

            // Assert
            Assert.Equal(instructorCount, result); 
        }

        [Fact]
        public async Task CountInstructorAsync_ReturnsZero_WhenNoInstructorsFound()
        {
            // Arrange
            var instructorCount = 0;

            _mockUserRepo.Setup(repo => repo.CountUsersByRoleAsync("Instructor")).ReturnsAsync(instructorCount);

            // Act
            var result = await _userService.CountInstructorAsync();

            // Assert
            Assert.Equal(instructorCount, result); 
        }





        [Fact]
        public async Task RegisterInstructorAsync_ReturnsSuccess_WhenInstructorIsRegistered()
        {
            // Arrange
            var instructorRegistrationDto = new InstructorRegistrationDTO
            {
                UserName = "newInstructor",
                Email = "instructor@example.com",
                FullName = "John Instructor"
            };

            _mockConfiguration.Setup(c => c["DefaultProfilePicPaths:ProfilePicture"]).Returns("defaultProfilePicUrl");
            _mockConfiguration.Setup(c => c["DefaultPasswordInstructor:InstructorPassword"]).Returns("TempPassword123");
            _mockConfiguration.Setup(c => c["AppUrl"]).Returns("https://localhost");

            _mockUserManager.Setup(m => m.FindByEmailAsync(instructorRegistrationDto.Email)).ReturnsAsync((User)null);
            _mockUserManager.Setup(m => m.FindByNameAsync(instructorRegistrationDto.UserName)).ReturnsAsync((User)null);
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Instructor")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<User>())).ReturnsAsync("confirmationToken");

            var mockFile = new Mock<IFormFile>();
            _mockFileStorageService.Setup(f => f.SaveProfilePictureAsync(mockFile.Object)).ReturnsAsync("profilePicUrl");

            _mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.RegisterInstructorAsync(instructorRegistrationDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Instructor registered. Email confirmation sent with temporary password.", result.Message);
        }


        [Fact]
        public async Task RegisterInstructorAsync_ReturnsFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var instructorRegistrationDto = new InstructorRegistrationDTO
            {
                UserName = "newInstructor",
                Email = "instructor@example.com",
                FullName = "John Instructor"
            };

            _mockConfiguration.Setup(c => c["DefaultProfilePicPaths:ProfilePicture"]).Returns("defaultProfilePicUrl");
            _mockConfiguration.Setup(c => c["DefaultPasswordInstructor:InstructorPassword"]).Returns("TempPassword123");

            _mockUserManager.Setup(m => m.FindByEmailAsync(instructorRegistrationDto.Email)).ReturnsAsync(new User());
            _mockUserManager.Setup(m => m.FindByNameAsync(instructorRegistrationDto.UserName)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.RegisterInstructorAsync(instructorRegistrationDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email already exists.", result.Message);
        }

        [Fact]
        public async Task RegisterInstructorAsync_ReturnsFailure_WhenUsernameAlreadyExists()
        {
            // Arrange
            var instructorRegistrationDto = new InstructorRegistrationDTO
            {
                UserName = "newInstructor",
                Email = "instructor@example.com",
                FullName = "John Instructor"
            };

            _mockConfiguration.Setup(c => c["DefaultProfilePicPaths:ProfilePicture"]).Returns("defaultProfilePicUrl");
            _mockConfiguration.Setup(c => c["DefaultPasswordInstructor:InstructorPassword"]).Returns("TempPassword123");

            _mockUserManager.Setup(m => m.FindByEmailAsync(instructorRegistrationDto.Email)).ReturnsAsync((User)null);
            _mockUserManager.Setup(m => m.FindByNameAsync(instructorRegistrationDto.UserName)).ReturnsAsync(new User());

            // Act
            var result = await _userService.RegisterInstructorAsync(instructorRegistrationDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username already exists.", result.Message);
        }

    }

}

