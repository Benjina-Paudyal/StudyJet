namespace StudyJet.API.DTOs.Wishlist
{
    public class WishlistCourseDTO
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string InstructorName { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
