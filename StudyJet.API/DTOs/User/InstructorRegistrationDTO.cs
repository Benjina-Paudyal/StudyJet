using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.DTOs.User
{
    public class InstructorRegistrationDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(256, ErrorMessage = "Username cannot exceed 256 characters.")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(256)]
        public string FullName { get; set; }

        public IFormFile? ProfilePicture { get; set; }

    }
}
