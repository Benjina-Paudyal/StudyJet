namespace StudyJet.API.DTOs.User
{
    public class Verify2faDTO
    {
        public string Email { get; set; }
        public string Code { get; set; } // the 2FA code entered by the user
    }
}
