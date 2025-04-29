using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IFileStorageService fileservice)
        {
            _userService = userService;
            
        }

        [HttpGet("username-exists")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { message = "Username is required." });
            }

            bool usernameExists = await _userService.CheckUsernameExistsAsync(username);
            return Ok(new { usernameExists });
        }


        [HttpGet("email-exists")]
        public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var emailInUse = await _userService.CheckIfEmailExistsAsync(email);

            return Ok(new { emailExists = emailInUse });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("GetUsersByRole/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return BadRequest(new { message = "Role parameter cannot be empty." });
            }

            var users = await _userService.GetUserByRolesAsync(role);

            if (users == null || !users.Any())
            {
                return NotFound(new { message = $"No users found with role: {role}" });
            }

            return Ok(users);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("count-students")]
        public async Task<IActionResult> CountStudents()
        {
            int count = await _userService.CountStudentAsync();
            return Ok(count);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("count-instructors")]
        public async Task<IActionResult> CountInstructors()
        {
            int count = await _userService.CountInstructorAsync();
            return Ok(count);
        }


    }
}
