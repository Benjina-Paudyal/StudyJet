using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Repositories.Interface;

namespace StudyJet.API.Repositories.Implementation
{
    public class UserPurchaseCourseRepo: IUserPurchaseCourseRepo
    {
        private readonly ApplicationDbContext _context;

        public UserPurchaseCourseRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<UserPurchaseCourseDTO>> SelectPurchasedCourseAsync(string userId)
        {

            var result = await _context.UserPurchaseCourse
                .Where(upc => upc.UserID == userId)
                .Select(upc => new UserPurchaseCourseDTO
                {
                    CourseID = upc.CourseID,
                    Title = upc.Course.Title,
                    Description = upc.Course.Description,
                    ImageUrl = upc.Course.ImageUrl,
                    VideoUrl = upc.Course.VideoUrl,
                    LastUpdateDate = upc.Course.LastUpdatedDate,
                    InstructorName = upc.Course.Instructor.FullName ?? upc.Course.Instructor.UserName,
                    PurchaseDate = upc.PurchaseDate,
                    TotalPrice = upc.Course.Price
                })
            .ToListAsync();

            return result;
        }

        public async Task<bool> HasUserPurchasedCourseAsync(string userId, int courseId)
        {
            return await _context.UserPurchaseCourse
                .AnyAsync(upc => upc.UserID == userId && upc.CourseID == courseId);
        }

        public async Task InsertPurchaseAsync(UserPurchaseCourse purchase)
        {
            await _context.UserPurchaseCourse.AddAsync(purchase);
        }

        public async Task<User> SelectUserByIdAsync(string userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<CourseResponseDTO>> SelectSuggestedCoursesAsync(string userId, int limit = 3)
        {
           
            var purchasedCourseIds = await _context.UserPurchaseCourse
                .Where(upc => upc.UserID == userId)
                .Select(upc => upc.CourseID)
                .ToListAsync();

            var suggestedCourses = await _context.Courses
                .Where(c => !purchasedCourseIds.Contains(c.CourseID))
                .OrderByDescending(c => c.LastUpdatedDate)
                .Take(limit)
                .Select(c => new CourseResponseDTO
                {
                    CourseID = c.CourseID,
                    Title = c.Title,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    InstructorName = c.Instructor.FullName ?? c.Instructor.UserName,
                    LastUpdatedDate = c.LastUpdatedDate,
                    Price = c.Price
                })
                .ToListAsync();

            if (!suggestedCourses.Any())
            {
                suggestedCourses = await _context.Courses
                    .OrderByDescending(c => c.LastUpdatedDate)
                    .Take(limit)
                    .Select(c => new CourseResponseDTO
                    {
                        CourseID = c.CourseID,
                        Title = c.Title,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        InstructorName = c.Instructor.FullName ?? c.Instructor.UserName,
                        LastUpdatedDate = c.LastUpdatedDate,
                        Price = c.Price
                    })
                    .ToListAsync();
            }

            return suggestedCourses;
        }

        public async Task<List<int>> SelectPurchasedCourseIdsAsync(string userId)
        {
            return await _context.UserPurchaseCourse
                .Where(upc => upc.UserID == userId)
                .Select(upc => upc.CourseID)
                .ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


    }
}

