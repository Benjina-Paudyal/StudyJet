using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.DTOs.User
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
