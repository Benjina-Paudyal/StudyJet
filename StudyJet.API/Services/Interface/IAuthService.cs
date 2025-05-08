using Microsoft.AspNetCore.Identity;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;
using StudyJet.API.Utilities;

namespace StudyJet.API.Services.Interface
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUserAsync(UserRegistrationDTO registrationDto);
        Task<object> LoginUserAsync(UserLoginDTO loginDto);
        Task<Result> ConfirmEmailAsync(string token, string email);
        Task<bool> Is2faEnabledAsync(string userId);
        Task<bool> Verify2faCodeAsync(User user, string verificationCode);
        Task<bool> Disable2faAsync(string userId);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> VerifyPasswordAsync(string email, string password);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<bool> ChangePasswordAsync(string username, ChangePasswordDTO model);





        Task<TwoFactorResultDTO> Initiate2faSetupAsync(User user);
        Task<TwoFactorResultDTO> Confirm2faSetupAsync(User user, string code);









        // Helper
        Task<string> GeneratePasswordResetTokenAsync(string email);
        string GenerateTempTokenAsync(User user);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<string> Generate2faSecretAsync(User user);
        string Generate2faQrCodeUri(string email, string unformattedKey);
        byte[] GenerateQRCodeImage(string qrCodeUri);
        Task<User?> ValidateTempTokenAsync(string tempToken);



    }
}
