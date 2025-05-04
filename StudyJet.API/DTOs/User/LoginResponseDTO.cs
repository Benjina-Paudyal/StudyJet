namespace StudyJet.API.DTOs.User
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public List<string> Roles { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string UserID { get; set; }
    }
}
