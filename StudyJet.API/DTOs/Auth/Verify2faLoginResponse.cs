namespace StudyJet.API.DTOs.Auth
{
    public class Verify2faLoginResponse
    {
        public string Token { get; set; }
        public IList<string> Roles { get; set; }
        public string UserName { get; set; }

        public string ProfilePictureUrl { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }

        public string UserID { get; set; }

    }
}




