namespace StudyJet.API.DTOs.Course
{
    public class CourseUpdateDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public string? VideoUrl { get; set; }
        public string? InstructorID { get; set; }
    }
}
