
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using StudyJet.API.Data.Entities;
using StudyJet.API.Services.Implementation;
using StudyJet.API.Services.Interface;
using Microsoft.Extensions.Configuration;
using StudyJet.API.DTOs.User;
using StudyJet.API.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Web;


namespace StudyJet.API.Tests.ServiceTests
{
    public class AuthServiceTest
    {

        private readonly AuthService _authService;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEmailService> _emailServiceMock;


        public AuthServiceTest()
        {
            // UserManager and SignInManager mock
            var store = new Mock<IUserStore<User>>();
            var roleStore = new Mock<IRoleStore<IdentityRole>>();

            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null
            );

            _userServiceMock = new Mock<IUserService>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
            _configurationMock = new Mock<IConfiguration>();
            _emailServiceMock = new Mock<IEmailService>();

            
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("your-issuer");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("your-audience");
            _configurationMock.Setup(x => x["Jwt:Key"]).Returns("12345678901234567890123456789012");

            
            _authService = new AuthService(
                _userManagerMock.Object,
                _userServiceMock.Object,
                _roleManagerMock.Object,
                _signInManagerMock.Object,
                _configurationMock.Object,
                _emailServiceMock.Object,
                new HttpClient()
            );
        }

       
        [Fact]
        public async Task RegisterUserAsync_EmailAlreadyExists_ReturnsEmailInUserError()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
               
            };
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Email is already in use.", result.Errors.FirstOrDefault()?.Description);
        }
        
        [Fact]
        public async Task RegisterUserAsync_UsernameAlreadyExists_ReturnsUsernameInUseError()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
            };
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Username is already in use.", result.Errors.FirstOrDefault()?.Description);
        }
       
        [Fact]
        public async Task RegisterUserAsync_PasswordsDoNotMatch_ReturnsPasswordMismatchError()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd@"
            };

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Passwords do not match.", result.Errors.FirstOrDefault()?.Description);
        }
       
        [Fact]
        public async Task RegisterUserAsync_UserCreationFails_ReturnsUserCreationError()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
            };

            _userServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Creation failed", result.Errors.FirstOrDefault()?.Description);
        }

        [Fact]
        public async Task RegisterUserAsync_RoleAssignmentFails_ReturnsRoleAssignmentError()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
            };
            _userServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userServiceMock.Setup(x => x.AssignUserRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed" }));

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Role assignment failed", result.Errors.FirstOrDefault()?.Description);
        }

        [Fact]
        public async Task RegisterUserAsync_EmailSendingFails_ReturnsEmailSendingError()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
            };
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userServiceMock.Setup(x => x.AssignUserRoleAsync(It.IsAny<User>(), "Student")).ReturnsAsync(IdentityResult.Success);
            _userServiceMock.Setup(x => x.EnsureDefaultRoleExistsAsync()).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "SMTP server not reachable" }));

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.StartsWith("Error sending confirmation email", result.Errors.FirstOrDefault()?.Description);
        }

        [Fact]
        public async Task RegisterUserAsync_SuccessfulRegistration_ReturnsSuccess()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
            };
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.ValidatePassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _userServiceMock.Setup(x => x.EnsureDefaultRoleExistsAsync()).Returns(Task.CompletedTask);
            _userServiceMock.Setup(x => x.AssignUserRoleAsync(It.IsAny<User>(), "Student")).ReturnsAsync(IdentityResult.Success);
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.True(result.Succeeded);
        }




        [Fact]
        public async Task LoginUserAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "Password123"
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User)null); 

            // Act
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginUserAsync(loginDto));

            // Assert
            Assert.Equal("User not found.", exception.Message);
        }

        [Fact]
        public async Task LoginUserAsync_EmailNotConfirmed_ThrowsInvalidOperationException()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "ValidPassword123"
            };

            var user = new User
            {
                Email = loginDto.Email,
                EmailConfirmed = false 
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Act
            var act = () => _authService.LoginUserAsync(loginDto);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            Assert.Equal("Email not confirmed. Please check your inbox.", exception.Message);
        }

        [Fact]
        public async Task LoginUserAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "WrongPassword123"
            };

            var user = new User
            {
                Email = loginDto.Email,
                EmailConfirmed = true
            };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            Func<Task> act = () => _authService.LoginUserAsync(loginDto);

            // Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
            Assert.Equal("Invalid password.", exception.Message);

        }

        [Fact]
        public async Task LoginUserAsync_PasswordChangeRequired_ReturnsPasswordChangeObject()
        {
            // Arrange
            var loginDto = new UserLoginDTO
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            var user = new User
            {
                Email = loginDto.Email,
                UserName = "testuser",
                FullName = "Test User",
                EmailConfirmed = true,
                NeedToChangePassword = true
            };

            var resetToken = "mock-token";
            var roles = new List<string> { "Student" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false)).ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(resetToken);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

            // Act
            var result = await _authService.LoginUserAsync(loginDto);

            // Reflection to access anonymous object properties
            var resultType = result.GetType();

            var requiresPasswordChange = (bool)resultType.GetProperty("requiresPasswordChange").GetValue(result);
            var message = (string)resultType.GetProperty("message").GetValue(result);
            var username = (string)resultType.GetProperty("username").GetValue(result);
            var email = (string)resultType.GetProperty("email").GetValue(result);
            var fullName = (string)resultType.GetProperty("fullName").GetValue(result);
            var returnedRoles = (IList<string>)resultType.GetProperty("roles").GetValue(result);
            var returnedToken = (string)resultType.GetProperty("resetToken").GetValue(result);

            // Assert
            Assert.True(requiresPasswordChange);
            Assert.Equal("You need to change your password before proceeding.", message);
            Assert.Equal(user.UserName, username);
            Assert.Equal(user.Email, email);
            Assert.Equal(user.FullName, fullName);
            Assert.Equal(roles, returnedRoles);
            Assert.Equal(WebUtility.UrlEncode(resetToken), returnedToken);
        }




        [Fact]
        public async Task RegisterUserAsync_AllValid_ReturnsSuccess()
        {
            // Arrange
            var registrationDto = new UserRegistrationDTO
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FullName = "New User",
                UserName = "newuser"
            };

            // Mock behaviors
            _userServiceMock.Setup(x => x.CheckIfEmailExistsAsync(registrationDto.Email)).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.CheckUsernameExistsAsync(registrationDto.UserName)).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.ValidatePassword(registrationDto.Password, registrationDto.ConfirmPassword)).Returns(true);

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), registrationDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userServiceMock
                .Setup(x => x.EnsureDefaultRoleExistsAsync())
                .Returns(Task.CompletedTask);

            _userServiceMock
                .Setup(x => x.AssignUserRoleAsync(It.IsAny<User>(), "Student"))
                .ReturnsAsync(IdentityResult.Success);

            _emailServiceMock
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterUserAsync(registrationDto);

            // Assert
            Assert.True(result.Succeeded);
        }



        [Fact]
        public async Task GenerateEmailConfirmationLinkAsync_GeneratesValidLink()
        {
            // Arrange
            var user = new User
            {
                Email = "testuser@example.com",
                UserName = "testuser"
            };

            var token = "sampleToken"; 
            var expectedUrl = $"https://localhost:7017/api/auth/confirm-email?token={HttpUtility.UrlEncode(token)}&email={user.Email}";

            
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);

            // Act
            var result = await _authService.GenerateEmailConfirmationLinkAsync(user);

            // Assert
            Assert.Equal(expectedUrl, result); 
        }

        [Fact]
        public async Task GenerateEmailConfirmationLinkAsync_GeneratesCorrectLink()
        {
            // Arrange
            var user = new User
            {
                Email = "testuser@example.com",
                UserName = "testuser"
            };

            var token = "sampleToken";
            var expectedUrl = $"https://localhost:7017/api/auth/confirm-email?token={HttpUtility.UrlEncode(token)}&email={user.Email}";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);

            // Act
            var result = await _authService.GenerateEmailConfirmationLinkAsync(user);

            // Assert
            Assert.Equal(expectedUrl, result);
        }



        [Fact]
        public async Task SendConfirmationEmailAsync_SuccessfulEmailSend_ReturnsSuccess()
        {
            // Arrange
            var email = "testuser@example.com";
            var confirmationLink = "https://localhost:7208/api/auth/confirm-email?token=sampleToken&email=testuser@example.com";
            var subject = "Email Confirmation";
            var body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";

            _emailServiceMock.Setup(x => x.SendEmailAsync(email, subject, body))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.SendConfirmationEmailAsync(email, confirmationLink);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task SendConfirmationEmailAsync_SuccessfulEmailSent_ReturnsSuccessResult()
        {
            // Arrange
            var email = "testuser@example.com";
            var confirmationLink = "https://localhost:7208/api/auth/confirm-email?token=sampleToken&email=testuser@example.com";
            var subject = "Email Confirmation";
            var body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";

            _emailServiceMock.Setup(x => x.SendEmailAsync(email, subject, body))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.SendConfirmationEmailAsync(email, confirmationLink);

            // Assert
            Assert.True(result.Succeeded);  
        }



        [Fact]
        public async Task GenerateJwtTokenAsync_ReturnsValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@example.com"
            };

            var expectedRoles = new List<string> { "Admin", "User" };

            _userServiceMock.Setup(x => x.GetUserRolesAsync(user)).ReturnsAsync(expectedRoles);

            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("your-issuer");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("your-audience");
            _configurationMock.Setup(x => x["Jwt:Key"]).Returns("12345678901234567890123456789012"); // 32 bytes

            // Act
            var token = await _authService.GenerateJwtTokenAsync(user);

            // Assert
            Assert.NotNull(token); 

            // Decode and check the token content
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Validate claims
            var claims = jwtToken.Claims.ToList();

            Assert.Contains(claims, c => c.Type == CustomClaimTypes.FullName && c.Value == user.FullName);
            Assert.Contains(claims, c => c.Type == CustomClaimTypes.UserName && c.Value == user.UserName);
            Assert.Contains(claims, c => c.Type == CustomClaimTypes.UserId && c.Value == user.Id.ToString());
            Assert.Contains(claims, c => c.Type == CustomClaimTypes.Email && c.Value == user.Email);
            Assert.Contains(claims, c => c.Type == CustomClaimTypes.Role && c.Value == "Admin");
            Assert.Contains(claims, c => c.Type == CustomClaimTypes.Role && c.Value == "User");

            // Check if token has correct expiration
            var expirationClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.Iat);
            Assert.NotNull(expirationClaim); 
            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationClaim.Value)).UtcDateTime;
            Assert.True(expirationDate > DateTime.UtcNow.AddMinutes(-1));
        }



        [Fact]
        public async Task Generate2FASecretAsync_UserHasAuthenticatorKey_ReturnsKey()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid().ToString(), UserName = "testuser", Email = "test@example.com" };
            var existingKey = "existingKey123";

            _userManagerMock.Setup(x => x.GetAuthenticatorKeyAsync(user))
                            .ReturnsAsync(existingKey);

            // Act
            var result = await _authService.Generate2faSecretAsync(user);

            // Assert
            Assert.Equal(existingKey, result); 
        }



        [Fact]
        public void Generate2FAQrCodeUri_ValidInputs_ReturnsCorrectUri()
        {
            // Arrange
            var email = "test@example.com";
            var unformattedKey = "1234567890ABCDEF";
            var expectedUri = "otpauth://totp/StudyJetApp:test@example.com?secret=1234567890ABCDEF&issuer=StudyJetApp&digits=6";

            // Act
            var result = _authService.Generate2faQrCodeUri(email, unformattedKey);

            // Assert
            Assert.Equal(expectedUri, result); 
        }

        [Fact]
        public void Generate2FAQrCodeUri_EmailWithSpecialCharacters_ReturnsCorrectUri()
        {
            // Arrange
            var email = "test+@example.com";
            var unformattedKey = "A1B2C3D4E5F6G7H8";
            var expectedUri = "otpauth://totp/StudyJetApp:test+@example.com?secret=A1B2C3D4E5F6G7H8&issuer=StudyJetApp&digits=6";

            // Act
            var result = _authService.Generate2faQrCodeUri(email, unformattedKey);

            // Assert
            Assert.Equal(expectedUri, result);
        }



        [Fact]
        public void GenerateQRCodeImage_ValidInput_ReturnsQRCodeImage()
        {
            // Arrange
            var qrCodeUri = "otpauth://totp/workhardApp:test@example.com?secret=1234567890ABCDEF&issuer=workhardApp&digits=6";

            // Act
            var result = _authService.GenerateQRCodeImage(qrCodeUri);

            // Assert
            Assert.NotNull(result); 
            Assert.NotEmpty(result); 
        }

        [Fact]
        public void GenerateQRCodeImage_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            string qrCodeUri = null;

            // Act
            var act = () => _authService.GenerateQRCodeImage(qrCodeUri);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(act);
            Assert.Equal("The QR code URI cannot be null or empty. (Parameter 'qrCodeUri')", exception.Message);

        }

        [Fact]
        public void GenerateQRCodeImage_EmptyInput_ThrowsArgumentNullException()
        {
            // Arrange
            var qrCodeUri = "";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _authService.GenerateQRCodeImage(qrCodeUri));
            Assert.Equal("The QR code URI cannot be null or empty. (Parameter 'qrCodeUri')", exception.Message);
        }

        [Fact]
        public void GenerateQRCodeImage_SpecialCharactersInURI_ReturnsQRCodeImage()
        {
            // Arrange
            var qrCodeUri = "otpauth://totp/workhardApp:test+workhard@example.com?secret=A1B2C3D4E5F6G7H8&issuer=workhardApp&digits=6";

            // Act
            var result = _authService.GenerateQRCodeImage(qrCodeUri);

            // Assert
            Assert.NotNull(result); 
            Assert.NotEmpty(result); 
        }



        [Fact]
        public async Task Verify2FACodeAsync_ValidCode_ReturnsTrue()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var verificationCode = "123456"; 

            _userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, verificationCode))
                            .ReturnsAsync(true);

            // Act
            var result = await _authService.Verify2faCodeAsync(user, verificationCode);

            // Assert
            Assert.True(result); 
        }

        [Fact]
        public async Task Verify2FACodeAsync_InvalidCode_ReturnsFalse()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var invalidCode = "wrongCode"; 

            _userManagerMock.Setup(x => x.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, invalidCode))
                            .ReturnsAsync(false);

            // Act
            var result = await _authService.Verify2faCodeAsync(user, invalidCode);

            // Assert
            Assert.False(result); 
        }



        [Fact]
        public async Task PasswordSignInAsync_ValidPassword_ReturnsSuccess()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var password = "correctPassword";

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
                              .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _authService.PasswordSignInAsync(user, password);

            // Assert
            Assert.Equal(SignInResult.Success, result); 
        }

        [Fact]
        public async Task PasswordSignInAsync_InvalidPassword_ReturnsFailed()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var invalidPassword = "wrongPassword";

            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, invalidPassword, false))
                              .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _authService.PasswordSignInAsync(user, invalidPassword);

            // Assert
            Assert.Equal(SignInResult.Failed, result); 
        }

    }
}

