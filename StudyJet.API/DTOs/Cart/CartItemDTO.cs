namespace StudyJet.API.DTOs.Cart
{
    public class CartItemDTO
    {
        public int CartID { get; set; }
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public string CourseDescription { get; set; }
        public string ImageUrl { get; set; }
        public string InstructorName { get; set; }
        public decimal Price { get; set; }
    }
}
