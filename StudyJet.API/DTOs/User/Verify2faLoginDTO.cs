namespace StudyJet.API.DTOs.User
{
    public class Verify2faLoginDTO
    {
        public string TempToken { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }
}
