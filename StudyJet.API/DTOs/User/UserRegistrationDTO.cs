using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.DTOs.User
{
    public class UserRegistrationDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public IFormFile? ProfilePicture { get; set; }

        public string? ProfilePictureUrl { get; set; } = "/images/profiles/profilepic.png";


    }
}
