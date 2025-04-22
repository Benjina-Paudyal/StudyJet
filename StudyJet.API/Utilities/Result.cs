namespace StudyJet.API.Utilities
{
    public class Result
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
