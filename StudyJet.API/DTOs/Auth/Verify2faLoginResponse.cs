namespace StudyJet.API.DTOs.Auth
{
    public class Verify2faLoginResponse
    {
        public string Token { get; set; }
        public IList<string> Roles { get; set; }
        public string Username { get; set; }
        public string ProfilePictureUrl { get; set; }

    }
}
