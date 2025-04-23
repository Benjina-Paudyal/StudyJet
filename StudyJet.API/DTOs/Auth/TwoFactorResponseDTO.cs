namespace StudyJet.API.DTOs.Auth
{
    public class TwoFactorResponseDTO
    {
        public bool Requires2FA { get; set; }
        public string TempToken { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
    }
}
