using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;

namespace StudyJet.API.Services.Interface
{
    public interface ICourseService
    {
        Task<List<CourseResponseDTO>> GetAllAsync();
        Task<List<CourseResponseDTO>> GetByInstructorIdAsync(string instructorId);
        Task<CourseResponseDTO> GetByIdAsync(int CourseId);
        Task<CourseUpdateDTO> GetCourseForUpdateAsync(int courseId, string instructorId);
        Task<List<User>> GetEnrolledStudentsByCourseIdAsync(int courseId);
        Task<List<CourseWithStudentsDTO>> GetCoursesWithStudentsForInstructorAsync(string instructorId);
        Task<List<CourseResponseDTO>> GetPopularCoursesAsync();
        Task<List<CourseResponseDTO>> SearchCoursesAsync(string query);
        Task<IEnumerable<Course>> GetPendingCoursesAsync();
        Task<IEnumerable<Course>> GetApprovedCoursesAsync();
        Task<int> GetTotalCoursesByInstructorIdAsync(string instructorId);
        Task<int> AddAsync(CreateCourseRequestDTO Course); 
        Task<int> GetTotalCoursesAsync();
        Task<bool> SubmitCourseUpdateAsync(int courseId, UpdateCourseRequestDTO updateDto);
        Task<bool> ApprovePendingUpdatesAsync(int courseId);
        Task<bool> RejectPendingUpdatesAsync(int courseId);
        Task<bool> RejectCourseAsync(int courseId);
        Task<bool> ApproveCourseAsync(int courseId);
        Task<bool> UpdateAsync(int id, UpdateCourseRequestDTO dto);
        Task<bool> ExistsAsync(int CourseId); 



    }
}
