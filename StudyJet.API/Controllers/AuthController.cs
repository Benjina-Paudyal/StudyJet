using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;
using StudyJet.API.Services.Interface;
using StudyJet.API.DTOs;
using StudyJet.API.DTOs.Auth;

namespace StudyJet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IFileStorageService _fileService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IFileStorageService fileService, IUserService userService, IEmailService emailService, UserManager<User> userManager,IConfiguration configuration)
        {
            _authService = authService;
            _fileService = fileService;
            _userService = userService;
            _emailService = emailService;
            _userManager = userManager;
            _configuration = configuration;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegistrationSwaggerDTO registrationDto)
        {
            if (registrationDto == null)
            {
                return BadRequest(new { errors = new[] { "Registration data is missing" } });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _userService.CheckUsernameExistsAsync(registrationDto.UserName))
            {
                return BadRequest(new { usernameExists = true });
            }

            if (await _userService.CheckIfEmailExistsAsync(registrationDto.Email))
            {
                return BadRequest(new { emailExists = true });
            }

            // Converting Swagger DTO to internal DTO 
            var userRegistrationDto = new UserRegistrationDTO
            {
                UserName = registrationDto.UserName,
                Email = registrationDto.Email,
                Password = registrationDto.Password,
                ConfirmPassword = registrationDto.ConfirmPassword,
                ProfilePicture = registrationDto.ProfilePicture,
                FullName = string.IsNullOrWhiteSpace(registrationDto.FullName) ? null : registrationDto.FullName
            };
            var defaultProfilePicUrl = _configuration["DefaultProfilePicPaths:ProfilePicture"];

            // Handling profile picture
            if (registrationDto.ProfilePicture != null)
            {
                string profilePicUrl = await _fileService.SaveProfilePictureAsync(registrationDto.ProfilePicture);
                userRegistrationDto.ProfilePictureUrl = profilePicUrl;
            }
            else
            {
                userRegistrationDto.ProfilePictureUrl = defaultProfilePicUrl;
            }

            var result = await _authService.RegisterUserAsync(userRegistrationDto);
            if (result.Succeeded)
            {
                return Ok(new RegistrationResponseDTO
                {
                    Message = "User registered successfully. Please check your email to confirm.",
                    ProfilePictureUrl = userRegistrationDto.ProfilePictureUrl
                });
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            var errorResponse = new ErrorResponseDTO { Errors = errors };
            return BadRequest(errorResponse);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("register-instructor")]
        public async Task<IActionResult> RegisterInstructor([FromForm] InstructorRegistrationDTO instructorRegistrationDTO)
        {
            var result = await _userService.RegisterInstructorAsync(instructorRegistrationDTO);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }
            return Ok(new { message = "Instructor registered successfully." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { "User not found." } });
                }

                if (!user.EmailConfirmed)
                {
                    return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { "Email not confirmed." } });
                }

                var isPasswordValid = await _authService.VerifyPasswordAsync(loginDto.Email, loginDto.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { "Invalid email or password." } });
                }

                //  if user is instructor
                if (user.NeedToChangePassword)
                {
                    var resetToken = await _authService.GeneratePasswordResetTokenAsync(user.Email);

                    return Ok(new PasswordChangeResponseDTO
                    {
                        RequiresPasswordChange = true,
                        Message = "You need to change your password before proceeding.",
                        Username = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Roles = (await _userService.GetUserRolesAsync(user)).ToList(),
                        ResetToken = resetToken
                    });
                }

                if (user.TwoFactorEnabled)
                {
                    var tempToken = _authService.GenerateTempTokenAsync(user);

                    return Ok(new TwoFactorResponseDTO
                    {
                        Requires2FA = true,
                        TempToken = tempToken,
                        Username = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Roles = (await _userService.GetUserRolesAsync(user)).ToList()
                    });
                }

                var token = await _authService.GenerateJwtTokenAsync(user);
                var roles = await _userService.GetUserRolesAsync(user);
                List<string> rolesList = roles.ToList();

                return Ok(new LoginResponseDTO
                {
                    Token = token,
                    Roles = rolesList,
                    Username = user.UserName,
                    FullName = user.FullName,
                    ProfilePictureUrl = user.ProfilePictureUrl ?? "/images/profiles/profilepic.png",
                    Email = user.Email,
                    UserID = user.Id,
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { ex.Message } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponseDTO { Errors = new List<string> { ex.Message } });
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponseDTO { Errors = new List<string> { "An unexpected error occurred." } });
            }
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmationEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest(new ConfirmationResponseDTO
                {
                    IsConfirmed = false,
                    Message = "Email or token is invalid."
                });
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { "User not found." } });
            }

            var result = await _authService.ConfirmEmailAsync(token, email);
            if (!result.Succeeded)
            {
                return BadRequest(new ConfirmationResponseDTO
                {
                    IsConfirmed = false,
                    Message = "Email confirmation failed.",
                    Errors = result.Errors
                });
            }
            var resetToken = await _authService.GeneratePasswordResetTokenAsync(email);

            // Redirection to frontend 
            var clientUrl = _configuration["Client:BaseUrl"];
            return Redirect($"{clientUrl}/confirmation?confirmed=true&email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}");
        }


        [Authorize]
        [HttpPost("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            try
            {
                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email);
                if (emailClaim == null)
                {
                    return BadRequest(new ErrorResponseDTO { Errors = new List<string> { "Email claim is missing." } });
                }

                var email = emailClaim.Value;

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { "User not found." } });
                }

                if (string.IsNullOrEmpty(user.Email))
                {
                    return BadRequest(new ErrorResponseDTO { Errors = new List<string> { "User email is required." } });
                }

                var key = await _authService.Generate2faSecretAsync(user);
                if (string.IsNullOrEmpty(key))
                {
                    return BadRequest(new ErrorResponseDTO { Errors = new List<string> { "Failed to generate 2FA secret key." } });
                }

                user.TwoFactorEnabled = true;
                await _userService.UpdateUserAsync(user);

                var qrCodeUri = _authService.Generate2faQrCodeUri(user.Email, key);
                byte[] qrCodeImage = _authService.GenerateQRCodeImage(qrCodeUri);

                return File(qrCodeImage, "image/png");
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponseDTO
                {
                    Errors = new List<string> { "Internal server error.", ex.Message }
                };

                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactorCode([FromBody] Verify2faDTO model)
        {
            if (model == null || string.IsNullOrEmpty(model.Code))
            {
                return BadRequest(new { error = "2FA code is required." });
            }

            try
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(new ErrorResponseDTO { Errors = new List<string> { "User not found." } });
                }

                var isValid = await _authService.Verify2faCodeAsync(user, model.Code);
                if (!isValid)
                {
                    return Unauthorized(new { error = "Invalid 2FA code." });
                }

                var token = await _authService.GenerateJwtTokenAsync(user);
                var roles = await _userService.GetUserRolesAsync(user);

                return Ok(new { Token = token, Roles = roles });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error: " + ex.Message });
            }
        }


        [HttpPost("verify-2fa-login")]
        public async Task<IActionResult> VerifyTwoFactorLogin([FromBody] Verify2faLoginDTO verify2faLoginDTO)
        {
            if (verify2faLoginDTO == null || string.IsNullOrEmpty(verify2faLoginDTO.Code) || string.IsNullOrEmpty(verify2faLoginDTO.TempToken))
            {
                return BadRequest(new { error = "2FA code and tempToken are required." });
            }

            try
            {
                var user = await _authService.ValidateTempTokenAsync(verify2faLoginDTO.TempToken);
                if (user == null)
                {
                    return BadRequest(new { error = "Invalid or expired temporary token." });
                }

                var isValid = await _authService.Verify2faCodeAsync(user, verify2faLoginDTO.Code);
                if (!isValid)
                {
                    return Unauthorized(new { error = "Invalid 2FA code." });
                }

                // Generate JWT token
                var token = await _authService.GenerateJwtTokenAsync(user);
                var roles = await _userService.GetUserRolesAsync(user);
                var defaultProfilePicUrl = _configuration["DefaultProfilePicPaths:ProfilePicture"];

                return Ok(new Verify2faLoginResponse
                {
                    Token = token,
                    Roles = roles,
                    Username = user.UserName,
                    ProfilePictureUrl = user.ProfilePictureUrl ?? defaultProfilePicUrl,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error: " + ex.Message });
            }
        }


        [HttpGet("check-2fa-status")]
        public async Task<IActionResult> Check2FAStatus()
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Username not found." });
            }

            try
            {
                var is2FAEnabled = await _authService.Is2faEnabledAsync(username);

                return Ok(new { isEnabled = is2FAEnabled });
            }

            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error." });
            }
        }


        [HttpPost("disable-2fa")]
        public async Task<IActionResult> Disable2FA()
        {
            var username = User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Username not found in claims." });
            }

            // Check current 2FA status
            var is2FAEnabled = await _authService.Is2faEnabledAsync(username);
            if (!is2FAEnabled)
            {
                return BadRequest(new { message = "2FA is already disabled." });
            }

            var result = await _authService.Disable2faAsync(username);
            if (!result)
            {
                return BadRequest(new { message = "Failed to disable 2FA." });
            }

            return Ok(new { message = "2FA has been disabled successfully." });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);

            if (!result)
            {
                return Ok(new { Message = "If the email is registered, a password reset link has been sent." });
            }

            return Ok(new { Message = "Password reset link sent successfully." });
        }


        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordRequestDTO request)
        {
            try
            {
                var isPasswordValid = await _authService.VerifyPasswordAsync(request.Email, request.Password);
                if (!isPasswordValid)
                {
                    return BadRequest(new { message = "Invalid password." });
                }

                return Ok(new { message = "Password verified." });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "Username not found in token." });
            }

            var result = await _authService.ChangePasswordAsync(username, model);

            if (result)
            {

                return Ok(new { Message = "Password changed successfully." });
            }
            else
            {
                return BadRequest(new { message = "Current password is incorrect. Please try again." });
            }
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return BadRequest(new { Message = "User not found." });
            }

            var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();
            bool isInstructor = roles.Contains("Instructor");

            var result = await _authService.ResetPasswordAsync(resetPasswordDto.Email, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errorMessage = result.Errors?.FirstOrDefault()?.Description ?? "Password reset failed. Invalid token or other issue.";
                return BadRequest(new { Message = errorMessage });
            }

            if (isInstructor && user.NeedToChangePassword)
            {
                user.NeedToChangePassword = false;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(new { Message = "Failed to update user after password reset." });
                }
            }

            return Ok(new { Message = "Password has been successfully reset." });
        }











        // TESTING AND INSPECTION

        [HttpGet("test1-auth")]
        public IActionResult TestAuth1()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }


        [HttpGet("test2-auth")]
        public IActionResult TestAuth2()
        {
            var userId = User.FindFirstValue("userId");
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var username = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");  // Accessing the correct claim
            var fullname = User.FindFirstValue("fullName");
            var email = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

            return Ok(new { userId, userRole, username, fullname, email });
        }


    }
}
