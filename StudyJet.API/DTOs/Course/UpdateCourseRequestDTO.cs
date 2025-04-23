namespace StudyJet.API.DTOs.Course
{
    public class UpdateCourseRequestDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? VideoUrl { get; set; }
    }
}
