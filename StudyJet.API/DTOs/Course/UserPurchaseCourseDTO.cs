namespace StudyJet.API.DTOs.Course
{
    public class UserPurchaseCourseDTO
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string InstructorName { get; set; }
        public string VideoUrl { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalPrice { get; set; }

    }
}
