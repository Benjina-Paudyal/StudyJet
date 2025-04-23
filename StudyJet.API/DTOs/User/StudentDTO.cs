namespace StudyJet.API.DTOs.User
{
    public class StudentDTO
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}
