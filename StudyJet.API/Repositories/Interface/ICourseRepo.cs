using Microsoft.EntityFrameworkCore;
using Stripe;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Implementation;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Repositories.Interface
{
    public interface ICourseRepo
    {
        Task<List<Course>> SelectAllAsync();
        Task<List<Course>> SelectCourseByStatusAsync(List<CourseStatus> status);
        Task<List<CourseUpdate>> SelectCourseUpdatesByStatusAsync(List<CourseStatus> status);
        Task<CourseUpdate?> SelectLatestCourseUpdateByCourseIdAsync(int courseId);
        Task<Course> SelectByIdAsync(int courseId);
        Task<List<Course>> SelectByIdsAsync(List<int> courseIds);
        Task<(int CourseID, string Title)> SelectTitleByIdAsync(int courseId);
        Task<Course> InsertAsync(Course course);
        Task<bool> UpdateAsync(Course course);
        Task<bool> DeleteAsync(int courseId);
        Task<bool> ExistsAsync(int courseId);
        Task<List<Course>> SelectPopularCoursesAsync();
        Task<List<CourseResponseDTO>> SearchCoursesAsync(string query);
        Task<int> SelectTotalCoursesAsync();
        Task<IEnumerable<Course>> SelectPendingCoursesAsync();
        Task<IEnumerable<Course>> SelectApprovedCoursesAsync();
        Task<bool> ApproveCourseAsync(int courseId);
        Task<bool> RejectCourseAsync(int courseId);
        Task<bool> RejectPendingUpdatesAsync(int courseId);
        Task<int> SelectTotalCoursesByInstructorIdAsync(string instructorId);
        Task<List<User>> SelectEnrolledStudentsByCourseIdAsync(int courseId);
        Task<List<CourseWithStudentsDTO>> SelectCoursesWithStudentsForInstructorAsync(string instructorId);
        Task<CourseUpdateDTO> SelectCourseForUpdateAsync(int courseId, string instructorId);
        Task<CourseUpdate> SelectPendingCourseUpdateAsync(int courseId);
        Task<bool> ApprovePendingUpdatesAsync(int courseId);
        Task<List<Course>> SelectByInstructorIdAsync(string instructorId);
        Task<List<CourseUpdate>> SelectPendingUpdatesByInstructorIdAsync(string instructorId);
        Task<bool> SubmitCourseUpdateAsync(int courseId, UpdateCourseRequestDTO updateDto, IFileStorageService fileService);

       


    }
}
