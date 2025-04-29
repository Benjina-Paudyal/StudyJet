namespace StudyJet.API.DTOs.User
{
    public class ChangePasswordForInstructorDTO
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
