using StudyJet.API.DTOs.Course;

namespace StudyJet.API.DTOs.User
{
    public class UserAdminDTO
    {
        public string ID { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfilePictureUrl { get; set; }
        public IList<string> Roles { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<CourseResponseDTO> PurchasedCourses { get; set; }
        public List<CourseResponseDTO> CreatedCourses { get; set; }


    }
}
