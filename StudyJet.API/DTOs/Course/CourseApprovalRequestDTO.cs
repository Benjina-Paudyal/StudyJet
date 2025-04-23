namespace StudyJet.API.DTOs.Course
{
    public class CourseApprovalRequestDTO
    {
        public string InstructorID { get; set; }
        public string CourseID { get; set; }
        public string Status { get; set; } 
    }
}
