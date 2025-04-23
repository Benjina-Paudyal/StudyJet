using Moq;
using StudyJet.API.Controllers;
using StudyJet.API.Data.Entities;
using StudyJet.API.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.DTOs.User;
using StudyJet.API.DTOs;
using StudyJet.API.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudyJet.API.DTOs.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace StudyJet.API.Tests.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IFileStorageService> _fileServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _fileServiceMock = new Mock<IFileStorageService>();
            _userServiceMock = new Mock<IUserService>();
            _emailServiceMock = new Mock<IEmailService>();

            _userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(),
                                                           Mock.Of<IOptions<IdentityOptions>>(),
                                                           Mock.Of<IPasswordHasher<User>>(),
                                                           new IUserValidator<User>[0],
                                                           new IPasswordValidator<User>[0],
                                                           Mock.Of<ILookupNormalizer>(),
                                                           Mock.Of<IdentityErrorDescriber>(),
                                                           Mock.Of<IServiceProvider>(),
                                                           Mock.Of<ILogger<UserManager<User>>>());

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["Client:BaseUrl"]).Returns("https://localhost:4200");

            _controller = new AuthController(
                _authServiceMock.Object,
                _fileServiceMock.Object,
                _userServiceMock.Object,
                _emailServiceMock.Object,
                _userManagerMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameExists()
        {
            // Arrange
            var registrationDto = new UserRegistrationSwaggerDTO { UserName = "existingUser", Email = "test@example.com" };
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(registrationDto.UserName)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(registrationDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("usernameExists", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailExists()
        {
            // Arrange
            var registrationDto = new UserRegistrationSwaggerDTO { UserName = "newUser", Email = "existing@example.com" };
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(registrationDto.Email)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(registrationDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("emailExists", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var registrationDto = new UserRegistrationSwaggerDTO { UserName = "newUser", Email = "new@example.com", Password = "password123", ConfirmPassword = "password123" };
            var userRegistrationDto = new UserRegistrationDTO { UserName = "newUser", Email = "new@example.com", Password = "password123", ConfirmPassword = "password123" };

            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(registrationDto.UserName)).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(registrationDto.Email)).ReturnsAsync(false);
            _authServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<UserRegistrationDTO>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registrationDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as RegistrationResponseDTO;
            Assert.Equal("User registered successfully. Please check your email to confirm.", response.Message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var registrationDto = new UserRegistrationSwaggerDTO { UserName = "newUser", Email = "new@example.com", Password = "password123", ConfirmPassword = "password123" };
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(registrationDto.UserName)).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(registrationDto.Email)).ReturnsAsync(false);
            _authServiceMock.Setup(x => x.RegisterUserAsync(It.IsAny<UserRegistrationDTO>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _controller.Register(registrationDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = badRequestResult.Value as ErrorResponseDTO;
            Assert.Contains("Error", errorResponse.Errors);
        }



        [Fact]
        public async Task RegisterInstructor_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var dto = new InstructorRegistrationDTO { Email = "instructor@example.com" };
                _userServiceMock
                    .Setup(s => s.RegisterInstructorAsync(dto))
                    .ReturnsAsync((true, "Instructor registered successfully."));

            // Act
            var result = await _controller.RegisterInstructor(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Instructor registered successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task RegisterInstructor_ReturnsBadRequest_WhenFailed()
        {
            // Arrange
            var dto = new InstructorRegistrationDTO { Email = "fail@example.com" };
            _userServiceMock
                .Setup(s => s.RegisterInstructorAsync(dto))
                 .ReturnsAsync((false, "Email already exists."));

            // Act
            var result = await _controller.RegisterInstructor(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Email already exists", badRequestResult.Value.ToString());
        }



        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var loginDto = new UserLoginDTO { Email = "nonexistent@example.com", Password = "password123" };
            _userServiceMock
                .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(unauthorizedResult.Value);
            Assert.Contains("User not found.", errorResponse.Errors);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenEmailNotConfirmed()
        {
            // Arrange
            var loginDto = new UserLoginDTO { Email = "user@example.com", Password = "password123" };
            var user = new User { Email = "user@example.com", EmailConfirmed = false };
            _userServiceMock
                .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(unauthorizedResult.Value);
            Assert.Contains("Email not confirmed.", errorResponse.Errors);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
        {
            // Arrange
            var loginDto = new UserLoginDTO { Email = "user@example.com", Password = "wrongpassword" };
            var user = new User { Email = "user@example.com", EmailConfirmed = true };
            _userServiceMock
                .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _authServiceMock
                .Setup(s => s.VerifyPasswordAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(unauthorizedResult.Value);
            Assert.Contains("Invalid email or password.", errorResponse.Errors);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenUserNeedsPasswordChange()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Email = "user@example.com",
                EmailConfirmed = true,
                NeedToChangePassword = true,
                UserName = "user123",
                FullName = "User One",
                Id = "user-id-123"
            };

            _userServiceMock
                .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _authServiceMock
                .Setup(s => s.VerifyPasswordAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(true);

            _authServiceMock
                .Setup(s => s.GeneratePasswordResetTokenAsync(user.Email))
                .ReturnsAsync("reset-token");

            _userServiceMock
                .Setup(s => s.GetUserRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PasswordChangeResponseDTO>(okResult.Value);

            Assert.True(response.RequiresPasswordChange);
            Assert.Equal("You need to change your password before proceeding.", response.Message);
            Assert.Equal("user123", response.Username);
            Assert.Equal("user@example.com", response.Email);
            Assert.Equal("User One", response.FullName);
            Assert.Equal("reset-token", response.ResetToken);

            Assert.NotNull(response.Roles);
            Assert.Contains("User", response.Roles);
        }

        [Fact]
        public async Task Login_ReturnsOk_When2FARequired()
        {
            // Arrange
            var loginDto = new UserLoginDTO { Email = "user@example.com", Password = "password123" };
            var user = new User
            {
                Email = "user@example.com",
                EmailConfirmed = true,
                TwoFactorEnabled = true,
                UserName = "user123",
                FullName = "User One"
            };

            _userServiceMock
                .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _authServiceMock
                .Setup(s => s.VerifyPasswordAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(true);

            _authServiceMock
                .Setup(s => s.GenerateTempTokenAsync(user))
                .Returns("temp-token");

            _userServiceMock
                .Setup(s => s.GetUserRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TwoFactorResponseDTO>(okResult.Value);

            Assert.True(response.Requires2FA);
            Assert.Equal("temp-token", response.TempToken);
            Assert.Equal("user123", response.Username);
            Assert.Equal("user@example.com", response.Email);
            Assert.Equal("User One", response.FullName);
            Assert.NotNull(response.Roles);
            Assert.Contains("User", response.Roles);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var loginDto = new UserLoginDTO { Email = "user@example.com", Password = "password123" };
            var user = new User
            {
                Email = "user@example.com",
                EmailConfirmed = true,
                TwoFactorEnabled = false,
                UserName = "username",
                FullName = "Full Name",
                ProfilePictureUrl = "/images/profiles/profilepic.png",
                Id = "user-id"
            };
            _userServiceMock
                .Setup(s => s.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _authServiceMock
                .Setup(s => s.VerifyPasswordAsync(loginDto.Email, loginDto.Password))
                .ReturnsAsync(true);
            _authServiceMock
                .Setup(s => s.GenerateJwtTokenAsync(user))
                .ReturnsAsync("jwt-token");
            _userServiceMock
                .Setup(s => s.GetUserRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponseDTO>(okResult.Value);
            Assert.Equal("jwt-token", response.Token);
        }




        [Fact]
        public async Task ConfirmationEmail_ReturnsRedirect_WhenSuccessful()
        {
            // Arrange
            var token = "valid-token";
            var email = "user@example.com";
            var user = new User { Email = email };

           
            _configurationMock.Setup(config => config["Client:BaseUrl"]).Returns("https://localhost:4200");

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            _authServiceMock.Setup(s => s.ConfirmEmailAsync(token, email))
                .ReturnsAsync(new Result { Succeeded = true });

            _authServiceMock.Setup(s => s.GeneratePasswordResetTokenAsync(email))
                .ReturnsAsync("reset-token");

            // Act
            var result = await _controller.ConfirmationEmail(token, email);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.StartsWith("https://localhost:4200/confirmation?", redirectResult.Url);
            Assert.Contains("confirmed=true", redirectResult.Url);
            Assert.Contains(Uri.EscapeDataString(email), redirectResult.Url);
            Assert.Contains("token=", redirectResult.Url);
        }


        [Fact]
        public async Task EnableTwoFactorAuthentication_ReturnsFileResult_WhenSuccessful()
        {
            // Arrange
            var email = "user@example.com";
            var user = new User { Email = email };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
            };

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.Generate2faSecretAsync(user)).ReturnsAsync("secret-key");
            _authServiceMock.Setup(s => s.Generate2faQrCodeUri(user.Email, "secret-key")).Returns("https://example.com/qrcode");
            _authServiceMock.Setup(s => s.GenerateQRCodeImage(It.IsAny<string>())).Returns(new byte[] { 1, 2, 3 });

            // Act
            var result = await _controller.EnableTwoFactorAuthentication();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/png", fileResult.ContentType);
            Assert.NotNull(fileResult.FileContents);
        }

        [Fact]
        public async Task EnableTwoFactorAuthentication_ReturnsBadRequest_WhenUserNotFound()
        {
            // Arrange
            var email = "user@example.com";

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
            };

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync((User)null); 

            // Act
            var result = await _controller.EnableTwoFactorAuthentication();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);  
            var errorResponse = Assert.IsType<ErrorResponseDTO>(unauthorizedResult.Value);
            Assert.Contains("User not found", errorResponse.Errors[0]);
        }

        [Fact]
        public async Task EnableTwoFactorAuthentication_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var email = "user@example.com";
            var user = new User { Email = email };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
            };

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.Generate2faSecretAsync(user)).Throws(new Exception("Internal error"));

            // Act
            var result = await _controller.EnableTwoFactorAuthentication();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(statusCodeResult.Value);
            Assert.Contains("Internal server error", errorResponse.Errors[0]);
        }



        [Fact]
        public async Task VerifyTwoFactorCode_ReturnsBadRequest_When2FACodeIsMissing()
        {
            // Arrange
            var model = new Verify2faDTO { Email = "user@example.com", Code = null };

            // Act
            var result = await _controller.VerifyTwoFactorCode(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var errorProperty = badRequestResult.Value.GetType().GetProperty("error");
            var errorValue = errorProperty?.GetValue(badRequestResult.Value)?.ToString();

            Assert.Equal("2FA code is required.", errorValue);
        }

        [Fact]
        public async Task VerifyTwoFactorCode_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var model = new Verify2faDTO { Email = "user@example.com", Code = "123456" };

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(model.Email)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.VerifyTwoFactorCode(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ErrorResponseDTO>(unauthorizedResult.Value);
            Assert.Contains("User not found", errorResponse.Errors[0]);
        }

        [Fact]
        public async Task VerifyTwoFactorCode_ReturnsUnauthorized_When2FACodeIsInvalid()
        {
            // Arrange
            var model = new Verify2faDTO { Email = "user@example.com", Code = "invalid-code" };
            var user = new User { Email = "user@example.com" };

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(model.Email)).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.Verify2faCodeAsync(user, model.Code)).ReturnsAsync(false);

            // Act
            var result = await _controller.VerifyTwoFactorCode(model);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            // reflection 
            var errorProperty = unauthorizedResult.Value.GetType().GetProperty("error");
            var errorValue = errorProperty?.GetValue(unauthorizedResult.Value)?.ToString();

            Assert.Equal("Invalid 2FA code.", errorValue);
        }

        [Fact]
        public async Task VerifyTwoFactorCode_ReturnsOk_When2FACodeIsValid()
        {
            // Arrange
            var model = new Verify2faDTO { Email = "user@example.com", Code = "valid-code" };
            var user = new User { Email = "user@example.com" };
            var roles = new List<string> { "User", "Admin" };
            var token = "jwt-token";

            _userServiceMock.Setup(s => s.GetUserByEmailAsync(model.Email)).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.Verify2faCodeAsync(user, model.Code)).ReturnsAsync(true);
            _authServiceMock.Setup(s => s.GenerateJwtTokenAsync(user)).ReturnsAsync(token);
            _userServiceMock.Setup(s => s.GetUserRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var result = await _controller.VerifyTwoFactorCode(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Use reflection to access properties
            var value = okResult.Value;
            var tokenProp = value.GetType().GetProperty("Token");
            var rolesProp = value.GetType().GetProperty("Roles");

            var tokenValue = tokenProp?.GetValue(value)?.ToString();
            var rolesValue = rolesProp?.GetValue(value) as IEnumerable<string>;

            Assert.Equal(token, tokenValue);
            Assert.Equal(roles, rolesValue);
        }

        [Fact]
        public async Task VerifyTwoFactorCode_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var model = new Verify2faDTO { Email = "user@example.com", Code = "valid-code" };
            _userServiceMock.Setup(s => s.GetUserByEmailAsync(model.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.VerifyTwoFactorCode(model);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);

            // reflection 
            var value = objectResult.Value;
            var errorProp = value.GetType().GetProperty("error");
            var errorMessage = errorProp?.GetValue(value)?.ToString();

            Assert.Contains("Internal server error", errorMessage);
            Assert.Contains("Database error", errorMessage);
        }



        [Fact]
        public async Task VerifyTwoFactorLogin_ShouldReturnBadRequest_WhenInvalidModel()
        {
            // Arrange
            var verify2faLoginDTO = new Verify2faLoginDTO { Code = "", TempToken = "" };

            // Act
            var result = await _controller.VerifyTwoFactorLogin(verify2faLoginDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task VerifyTwoFactorLogin_ShouldReturnBadRequest_WhenTempTokenInvalid()
        {
            // Arrange
            var verify2faLoginDTO = new Verify2faLoginDTO { Code = "123456", TempToken = "invalid-token" };
            _authServiceMock.Setup(s => s.ValidateTempTokenAsync(It.IsAny<string>()))
                           .ReturnsAsync((User)null);

            // Act
            var result = await _controller.VerifyTwoFactorLogin(verify2faLoginDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            // anonymous types
            var value = badRequestResult.Value;
            Assert.NotNull(value);

            var jObject = JObject.FromObject(value);
            Assert.Equal("Invalid or expired temporary token.", jObject["error"]?.Value<string>());
        }

        [Fact]
        public async Task VerifyTwoFactorLogin_ShouldReturnUnauthorized_WhenInvalid2FACode()
        {
            // Arrange
            var verify2faLoginDTO = new Verify2faLoginDTO { Code = "123456", TempToken = "valid-token" };
            var user = new User { UserName = "testuser", ProfilePictureUrl = null };
            _authServiceMock.Setup(s => s.ValidateTempTokenAsync(It.IsAny<string>())).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.Verify2faCodeAsync(user, It.IsAny<string>())).ReturnsAsync(false); 

            // Act
            var result = await _controller.VerifyTwoFactorLogin(verify2faLoginDTO);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var value = unauthorizedResult.Value;
            Assert.NotNull(value);

            var jObject = JObject.FromObject(value); 
            Assert.Equal("Invalid 2FA code.", jObject["error"]?.Value<string>());
        }

        [Fact]
        public async Task VerifyTwoFactorLogin_ReturnsOk_When2FAIsValid()
        {
            // Arrange
            var user = new User { UserName = "user", ProfilePictureUrl = null };
            var token = "jwt-token";
            var roles = new List<string> { "Student" };
            var dto = new Verify2faLoginDTO { Code = "valid-code", TempToken = "valid-temp-token" };

            _authServiceMock.Setup(x => x.ValidateTempTokenAsync(dto.TempToken)).ReturnsAsync(user);
            _authServiceMock.Setup(x => x.Verify2faCodeAsync(user, dto.Code)).ReturnsAsync(true);
            _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user)).ReturnsAsync(token);
            _userServiceMock.Setup(x => x.GetUserRolesAsync(user)).ReturnsAsync(roles);
            _configurationMock.Setup(x => x["DefaultProfilePicPaths:ProfilePicture"]).Returns("default.png");

            // Act
            var result = await _controller.VerifyTwoFactorLogin(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value; 

            Assert.Equal(token, response.Token);
            Assert.Equal("user", response.Username);
            Assert.Equal("default.png", response.ProfilePictureUrl);
        }



        [Fact]
        public async Task Check2FAStatus_ShouldReturnBadRequest_WhenUsernameNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) 
                }
            };

            // Act
            var result = await _controller.Check2FAStatus();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var jObject = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Username not found.", jObject["message"]?.ToString());
        }

        [Fact]
        public async Task Check2FAStatus_ShouldReturnOk_When2FADisabled()
        {
            // Arrange
            var username = "testuser";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) })) }
            };
            _authServiceMock.Setup(s => s.Is2faEnabledAsync(username)).ReturnsAsync(false);

            // Act
            var result = await _controller.Check2FAStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var jObject = JObject.FromObject(okResult.Value);

            Assert.False(jObject["isEnabled"]?.ToObject<bool>());
        }

        [Fact]
        public async Task Check2FAStatus_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var username = "testuser";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) })) }
            };
            _authServiceMock.Setup(s => s.Is2faEnabledAsync(username)).ThrowsAsync(new Exception("Some error"));

            // Act
            var result = await _controller.Check2FAStatus();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result); 
            Assert.Equal(500, objectResult.StatusCode);

            var jObject = JObject.FromObject(objectResult.Value);
            Assert.Equal("Internal server error.", jObject["message"]?.ToString());
        }



        [Fact]
        public async Task Disable2FA_ShouldReturnUnauthorized_WhenUsernameNotFoundInClaims()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) } // No username in claims
            };

            // Act
            var result = await _controller.Disable2FA();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var jObject = JObject.FromObject(unauthorizedResult.Value);
            Assert.Equal("Username not found in claims.", jObject["message"]?.ToString());
        }



        [Fact]
        public async Task ForgotPassword_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var forgotPasswordDto = new ForgotPasswordDTO { Email = "invalid-email" }; // Assuming validation will fail for this email format
            _controller.ModelState.AddModelError("Email", "Invalid email format");

            // Act
            var result = await _controller.ForgotPassword(forgotPasswordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }



        [Fact]
        public async Task VerifyPassword_ShouldReturnBadRequest_WhenPasswordIsInvalid()
        {
            // Arrange
            var request = new VerifyPasswordRequestDTO { Email = "testuser@example.com", Password = "wrongpassword" };
            _authServiceMock.Setup(s => s.VerifyPasswordAsync(request.Email, request.Password)).ReturnsAsync(false); // Simulate invalid password

            // Act
            var result = await _controller.VerifyPassword(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var jObject = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Invalid password.", jObject["message"]?.ToString());
        }

        [Fact]
        public async Task VerifyPassword_ShouldReturnOk_WhenPasswordIsValid()
        {
            // Arrange
            var request = new VerifyPasswordRequestDTO { Email = "testuser@example.com", Password = "correctpassword" };
            _authServiceMock.Setup(s => s.VerifyPasswordAsync(request.Email, request.Password)).ReturnsAsync(true); // Simulate valid password

            // Act
            var result = await _controller.VerifyPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var jObject = JObject.FromObject(okResult.Value);
            Assert.Equal("Password verified.", jObject["message"]?.ToString());
        }

        [Fact]
        public async Task VerifyPassword_ShouldReturnNotFound_WhenExceptionOccurs()
        {
            // Arrange
            var request = new VerifyPasswordRequestDTO { Email = "testuser@example.com", Password = "anyPassword" };
            _authServiceMock.Setup(s => s.VerifyPasswordAsync(request.Email, request.Password)).ThrowsAsync(new Exception("Some error"));

            // Act
            var result = await _controller.VerifyPassword(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);

            var jObject = JObject.FromObject(notFoundResult.Value);
            Assert.Equal("Some error", jObject["message"]?.ToString());
        }



        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenUsernameNotFoundInToken()
        {
            // Arrange
            var model = new ChangePasswordDTO { CurrentPassword = "oldPassword", NewPassword = "newPassword" };
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) } // No username in claims
            };

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var jObject = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Username not found in token.", jObject["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenPasswordChangedSuccessfully()
        {
            // Arrange
            var model = new ChangePasswordDTO { CurrentPassword = "oldPassword", NewPassword = "newPassword" };
            var username = "testuser";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) })) } // Username present in claims
            };
            _authServiceMock.Setup(s => s.ChangePasswordAsync(username, model)).ReturnsAsync(true); 

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var jObject = JObject.FromObject(okResult.Value);
            Assert.Equal("Password changed successfully.", jObject["Message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var model = new ChangePasswordDTO { CurrentPassword = "oldPassword", NewPassword = "newPassword" };
            var username = "testuser";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) })) } // Username present in claims
            };
            _authServiceMock.Setup(s => s.ChangePasswordAsync(username, model)).ReturnsAsync(false); 

            // Act
            var result = await _controller.ChangePassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var jObject = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Current password is incorrect. Please try again.", jObject["message"]?.ToString());
        }



        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new ResetPasswordDTO { Email = "testuser@example.com", Token = "valid-token", NewPassword = "" };  // Simulating invalid model state with empty password
            _controller.ModelState.AddModelError("Password", "Password is required.");

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = badRequestResult.Value as SerializableError;

            var errorMessages = errors?["Password"] as string[];
            var errorMessage = errorMessages?.FirstOrDefault()?.Trim();  

            Assert.Equal("Password is required.", errorMessage);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenUserNotFound()
        {
            // Arrange
            var model = new ResetPasswordDTO { Email = "notfounduser@example.com", Token = "valid-token", NewPassword = "newPassword" };
            _authServiceMock.Setup(s => s.ResetPasswordAsync(model.Email, model.Token, model.NewPassword)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User not found." }));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var jObject = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("User not found.", jObject["Message"]?.ToString());
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenPasswordResetFails()
        {
            // Arrange
            var model = new ResetPasswordDTO { Email = "testuser@example.com", Token = "invalid-token", NewPassword = "newPassword" };

            var user = new User { UserName = "testuser", Email = "testuser@example.com" };  
            _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email)).ReturnsAsync(user);

            _authServiceMock.Setup(s => s.ResetPasswordAsync(model.Email, model.Token, model.NewPassword))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var jObject = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Invalid token.", jObject["Message"]?.ToString());
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnOk_WhenPasswordResetSucceeds()
        {
            // Arrange
            var model = new ResetPasswordDTO { Email = "testuser@example.com", Token = "valid-token", NewPassword = "newPassword" };
            var user = new User { UserName = "testuser", Email = "testuser@example.com" };

            _userManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            _authServiceMock.Setup(s => s.ResetPasswordAsync(model.Email, model.Token, model.NewPassword)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ResetPassword(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var jObject = JObject.FromObject(okResult.Value);
            Assert.Equal("Password has been successfully reset.", jObject["Message"]?.ToString());

            Assert.NotNull(user);  
            Assert.NotNull(user.UserName); 
            Assert.NotNull(user.Email); 
        }

    }

}



















