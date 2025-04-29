using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;
using StudyJet.API.Services.Interface;
using StudyJet.API.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace StudyJet.API.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        public AuthService(UserManager<User> userManager, IUserService userService, RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, IConfiguration configuration, IEmailService emailService, HttpClient httpClient)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _emailService = emailService;
            _httpClient = httpClient;
            _userService = userService;
        }

        public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDTO registrationDto)
        {
            var defaultPic = _configuration["DefaultPaths:ProfilePicture"];

            // Check if email already exists
            if (await _userService.CheckIfEmailExistsAsync(registrationDto.Email))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email is already in use." });
            }

            // Check if username already exists
            if (await _userService.CheckUsernameExistsAsync(registrationDto.UserName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Username is already in use." });
            }

            // Validate password match
            if (!_userService.ValidatePassword(registrationDto.Password, registrationDto.ConfirmPassword))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Passwords do not match." });
            }

            // Create user object
            var user = new User
            {
                FullName = registrationDto.FullName,
                UserName = registrationDto.UserName,
                Email = registrationDto.Email,
                EmailConfirmed = false,
                ProfilePictureUrl = registrationDto.ProfilePictureUrl ?? defaultPic
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, registrationDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => MapErrorToUserFriendlyMessage(e)).ToArray();
                return IdentityResult.Failed(errors);
            }

            // Ensure role exists and assign it
            await _userService.EnsureDefaultRoleExistsAsync();
            var roleResult = await _userService.AssignUserRoleAsync(user, "Student");

            if (!roleResult.Succeeded)
            {
                return IdentityResult.Failed(roleResult.Errors.ToArray());
            }

            // Send email confirmation
            var confirmationLink = await GenerateEmailConfirmationLinkAsync(user);
            var emailResult = await SendConfirmationEmailAsync(user.Email, confirmationLink);

            if (!emailResult.Succeeded)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error sending confirmation email: " + emailResult.Errors.FirstOrDefault()?.Description });
            }

            return IdentityResult.Success;
        }

        public async Task<object> LoginUserAsync(UserLoginDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            if (!user.EmailConfirmed)
            {
                throw new InvalidOperationException("Email not confirmed. Please check your inbox.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid password.");
            }


            if (user.NeedToChangePassword)
            {
                // Generate a temporary password reset token
                var resetToken = WebUtility.UrlEncode(await _userManager.GeneratePasswordResetTokenAsync(user));

                return new
                {
                    requiresPasswordChange = true,
                    message = "You need to change your password before proceeding.",
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName,
                    roles = await _userManager.GetRolesAsync(user),
                    resetToken = resetToken
                };
            }

            // Generate JWT Token
            return new
            {
                token = await GenerateJwtTokenAsync(user),
                username = user.UserName,
                email = user.Email,
                fullName = user.FullName,
                roles = await _userManager.GetRolesAsync(user)
            };
        }

        public async Task<Result> ConfirmEmailAsync(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new Result { Succeeded = false, Errors = new List<string> { "Invalid email." } };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return new Result
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            return new Result { Succeeded = true };
        }

        public async Task<bool> Is2faEnabledAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }
            var isEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            return isEnabled;
        }

        public async Task<TwoFactorResultDTO> Enable2faAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var key = await GetOrCreateAuthenticatorKeyAsync(user);

            if (string.IsNullOrWhiteSpace(key))
            {
                return new TwoFactorResultDTO
                {
                    Success = false,
                    ErrorMessage = "Failed to retrieve a valid authenticator key."
                };
            }

            var enable2FaResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!enable2FaResult.Succeeded)
            {
                return new TwoFactorResultDTO
                {
                    Success = false,
                    ErrorMessage = "Failed to enable two-factor authentication."
                };
            }

            var qrCodeUri = Generate2faQrCodeUri(user.Email, key);
            var qrCodeImage = GenerateQRCodeImage(qrCodeUri);

            return new TwoFactorResultDTO
            {
                Success = true,
                Key = key,
                QrCodeImage = qrCodeImage
            };
        }

        public async Task<bool> Verify2faCodeAsync(User user, string verificationCode)
        {
            return await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, verificationCode);

        }

        public async Task<bool> Disable2faAsync(string userId)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Disable 2FA for the user
            user.TwoFactorEnabled = false;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var appUrl = _configuration["AppUrl"];
            if (string.IsNullOrEmpty(appUrl))
            {
                return false;
            }

            //var resetLink = $"{appUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(resetToken)}";

            var resetLink = $"{appUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            var emailResult = await _emailService.SendEmailAsync(
                recipientEmail: user.Email,
                subject: "Password Reset",
                body: $"Click the link to reset your password: <a href='{resetLink}'>Reset Password</a>"
            );

            return emailResult.Succeeded;
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var isPasswordSame = await _userManager.CheckPasswordAsync(user, newPassword);
            if (isPasswordSame)
            {
                return IdentityResult.Failed(new IdentityError { Description = "New password cannot be the same as the current password." });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return IdentityResult.Failed(new IdentityError { Description = result.Errors.FirstOrDefault()?.Description ?? "Failed to reset password." });
            }
            user.NeedToChangePassword = false;
            await _userManager.UpdateAsync(user);
            return IdentityResult.Success;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDTO model)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(appUser, model.CurrentPassword, model.NewPassword);
            return result.Succeeded;
        }


        // Helper methods
        public static IdentityError MapErrorToUserFriendlyMessage(IdentityError error)
        {
            if (error.Description.Contains("password"))
            {
                return new IdentityError { Description = "Password must be at least 8 characters long and include a digit, an uppercase letter, a lowercase letter, and a special character." };
            }
            return error;
        }

        public async Task<string> GenerateEmailConfirmationLinkAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);

            var apiBase = _configuration["Backend:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(apiBase))
                throw new InvalidOperationException("Backend:BaseUrl is not configured.");

            return $"{apiBase}/api/auth/confirm-email?token={encodedToken}"
                 + $"&email={HttpUtility.UrlEncode(user.Email)}";
        }



        public async Task<IdentityResult> SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            var subject = "Email Confirmation";
            var body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>";

            return await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            // Creating claims for the token
            var claims = new List<Claim>
    {
                new Claim(CustomClaimTypes.FullName, user.FullName),
                new Claim(CustomClaimTypes.UserName, user.UserName),
                new Claim(CustomClaimTypes.UserId, user.Id.ToString()),
                new Claim(CustomClaimTypes.Email, user.Email),
                new Claim(CustomClaimTypes.Jti, Guid.NewGuid().ToString()),
                new Claim(CustomClaimTypes.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64)
    };

            // Adding user roles as claims
            var userRoles = await _userService.GetUserRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(CustomClaimTypes.Role, role));
            }

            // Defining the token descriptor with expiration and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            // Creating and returning the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> Generate2faSecretAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(key))
            {
                var result = await _userManager.ResetAuthenticatorKeyAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Failed to reset authenticator key.");
                }

                key = await _userManager.GetAuthenticatorKeyAsync(user);

            }
            return key;

        }

        public string Generate2faQrCodeUri(string email, string unformattedKey)
        {
            // Issueing Time-based One-Time Password generation
            const string issuer = "StudyJetApp";

            // Constructing the URI format for the QR code, used by authentication apps
            return $"otpauth://totp/{issuer}:{email}?secret={unformattedKey}&issuer={issuer}&digits=6";
        }

        public byte[] GenerateQRCodeImage(string qrCodeUri)
        {
            if (string.IsNullOrEmpty(qrCodeUri))
            {
                throw new ArgumentNullException(nameof(qrCodeUri), "The QR code URI cannot be null or empty.");
            }

            using (var generator = new QRCodeGenerator())
            {
                // Generating the QR code data using the provided URI
                var qrCodeData = generator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);

                // Returning the QR code as a byte array in PNG format
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }

        public async Task<SignInResult> PasswordSignInAsync(User user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            return result;
        }

        public string GenerateTempTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(CustomClaimTypes.UserId, user.Id.ToString()),
                new Claim(CustomClaimTypes.Email, user.Email),
                new Claim("Requires2FA", "true"), // Flag to indicate this token is for 2FA
                new Claim(CustomClaimTypes.Jti, Guid.NewGuid().ToString()),
                new Claim(CustomClaimTypes.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5), // Short expiry for security (e.g., 5 minutes)
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<User?> ValidateTempTokenAsync(string tempToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Token expires exactly as intended
                };

                // Validate and get claims
                var principal = tokenHandler.ValidateToken(tempToken, validationParameters, out SecurityToken validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null)
                {
                    return null;
                }

                // Check for the 2FA-related claim
                var requires2FAClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Requires2FA" && c.Value == "true");
                if (requires2FAClaim == null)
                {
                    return null;
                }

                // Extract user ID
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.UserId);
                if (userIdClaim == null)
                {
                    return null;
                }

                var userId = userIdClaim.Value;
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return null;
                }
                return user;
            }
            catch (SecurityTokenExpiredException)
            {
                return null;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetOrCreateAuthenticatorKeyAsync(User user)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                var result = await _userManager.ResetAuthenticatorKeyAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Failed to reset the authenticator key.");
                }
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            return key;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }


    }
}
