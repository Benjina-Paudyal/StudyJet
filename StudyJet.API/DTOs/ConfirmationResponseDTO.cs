namespace StudyJet.API.DTOs
{
    public class ConfirmationResponseDTO
    {
        public bool IsConfirmed { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
