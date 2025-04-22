using Microsoft.AspNetCore.Identity;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserRepo _userRepo;
        private readonly IFileStorageService _fileService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserRepo userRepo, IFileStorageService fileService, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepo = userRepo;
            _fileService = fileService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<bool> CheckIfEmailExistsAsync(string email) => await _userManager.FindByEmailAsync(email) != null;

        public async Task<bool> CheckUsernameExistsAsync(string username) => await _userManager.FindByNameAsync(username) != null;

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(",", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update user: {errorMessage}");
            }
        }

        public bool ValidatePassword(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }

        public async Task EnsureDefaultRoleExistsAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Student"));
            }
        }

        public async Task<IdentityResult> AssignUserRoleAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            //return await _userManager.GetRolesAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();  // Ensure it returns List<string>

        }

        public async Task<List<UserAdminDTO>> GetUserByRolesAsync(string role)
        {
            return await _userRepo.SelectUsersByRoleAsync(role);
        }

        public async Task<int> CountUserByRoleAsync(string role)
        {
            var users = await GetUserByRolesAsync(role);
            return users.Count();
        }

        public async Task<int> CountStudentAsync()
        {
            return await _userRepo.CountUsersByRoleAsync("Student");
        }

        public async Task<int> CountInstructorAsync()
        {
            return await _userRepo.CountUsersByRoleAsync("Instructor");
        }

        public async Task<(bool Success, string Message)> RegisterInstructorAsync(InstructorRegistrationDTO instructorRegistrationDto)
        {
            var defaultPic = _configuration["DefaultPaths:ProfilePicture"];
            var instructorPassword = _configuration["DefaultPaths:InstructorPassword"];

            if (string.IsNullOrEmpty(defaultPic) || string.IsNullOrEmpty(instructorPassword))
            {
                return (false, "Missing default configuration for profile picture or password.");
            }

            if (await _userManager.FindByEmailAsync(instructorRegistrationDto.Email) != null)
            {
                return (false, "Email already exists.");
            }

            if (await _userManager.FindByNameAsync(instructorRegistrationDto.UserName) != null)
            {
                return (false, "Username already exists.");
            }

            var instructor = new User
            {
                UserName = instructorRegistrationDto.UserName,
                Email = instructorRegistrationDto.Email,
                FullName = instructorRegistrationDto.FullName,
                EmailConfirmed = false,
                NeedToChangePassword = true,
            };

            if (instructorRegistrationDto.ProfilePicture != null)
            {
                string profilePicUrl = await _fileService.SaveProfilePictureAsync(instructorRegistrationDto.ProfilePicture);
                instructor.ProfilePictureUrl = profilePicUrl;
            }
            else
            {
                instructor.ProfilePictureUrl = defaultPic;

            }
            var result = await _userManager.CreateAsync(instructor, instructorPassword);
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await _userManager.AddToRoleAsync(instructor, "Instructor");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(instructor);

            var appUrl = _configuration["AppUrl"];
            var confirmationLink = $"{appUrl}/api/auth/confirm-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(instructor.Email)}";

            await _emailService.SendEmailAsync(instructor.Email, "Confirm Your Email",
                $"Click <a href='{confirmationLink}'>here</a> to confirm your email. Your temporary password is: <strong>{instructorPassword}</strong>");

            return (true, "Instructor registered. Email confirmation sent with temporary password.");
        }


    }
}
