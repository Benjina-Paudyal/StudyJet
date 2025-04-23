namespace StudyJet.API.DTOs.User
{
    public class CourseWithStudentsDTO
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public List<StudentDTO> Students { get; set; }
    }
}
