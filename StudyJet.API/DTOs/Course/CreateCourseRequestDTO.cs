using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.DTOs.Course
{
    public class CreateCourseRequestDTO
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        public IFormFile ImageFile { get; set; } 

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Instructor ID is required.")]
        public string InstructorID { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryID { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string VideoUrl { get; set; }
    }
}
