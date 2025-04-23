namespace StudyJet.API.DTOs.Auth
{
    public class PasswordChangeResponseDTO
    {
        
            public bool RequiresPasswordChange { get; set; }
            public string Message { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public List<string> Roles { get; set; }
            public string ResetToken { get; set; }
        

    }
}
