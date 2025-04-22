using Microsoft.AspNetCore.Identity;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;

namespace StudyJet.API.Services.Interface
{
    public interface IUserService
    {
        Task<bool> CheckIfEmailExistsAsync(string email);
        Task<bool> CheckUsernameExistsAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(string userId);
        Task<IdentityResult> AddUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        bool ValidatePassword(string password, string confirmPassword);
        Task EnsureDefaultRoleExistsAsync();
        Task<IdentityResult> AssignUserRoleAsync(User user, string role);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task<List<UserAdminDTO>> GetUserByRolesAsync(string role);
        Task<int> CountUserByRoleAsync(string role);
        Task<int> CountStudentAsync();
        Task<int> CountInstructorAsync();
        Task<(bool Success, string Message)> RegisterInstructorAsync(InstructorRegistrationDTO registrationDto);

    }
}
