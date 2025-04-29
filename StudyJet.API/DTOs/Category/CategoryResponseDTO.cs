using StudyJet.API.DTOs.Course;

namespace StudyJet.API.DTOs.Category
{
    public class CategoryResponseDTO
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }

        public List<CourseResponseDTO> Courses { get; set; }
    }
}
