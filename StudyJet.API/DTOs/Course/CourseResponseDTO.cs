using StudyJet.API.Data.Enums;

namespace StudyJet.API.DTOs.Course
{
    public class CourseResponseDTO
    {
        public int CourseID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public decimal Price { get; set; }

        public string InstructorID { get; set; }

        public string InstructorName { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public string VideoUrl { get; set; }

        public CourseStatus Status { get; set; }

        public bool? IsUpdate { get; set; }
        public int? UpdateId { get; set; }

    }
}
