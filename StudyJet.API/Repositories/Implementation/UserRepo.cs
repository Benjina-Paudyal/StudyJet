using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Course;
using StudyJet.API.DTOs.User;
using StudyJet.API.Repositories.Interface;

namespace StudyJet.API.Repositories.Implementation
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _context;

        public UserRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> SelectByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.UserName == username);
        }

        public async Task<List<UserAdminDTO>> SelectUsersByRoleAsync(string role)
        {
            // Fetch users that match the role
            var users = await (from user in _context.Users
                               join userRole in _context.UserRoles on user.Id equals userRole.UserId
                               join roleEntity in _context.Roles on userRole.RoleId equals roleEntity.Id
                               where roleEntity.Name == role
                               select user)
                               .ToListAsync();

            var userAdminDtos = new List<UserAdminDTO>();

            foreach (var user in users)
            {
                // Fetch roles for the user
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, role => role.Id, (ur, role) => role.Name)
                    .ToListAsync();

                // Fetch purchased courses for students
                var purchasedCourses = await _context.UserPurchaseCourse
                    .Where(pc => pc.UserID == user.Id)
                    .Select(pc => new CourseResponseDTO
                    {
                        CourseID = pc.Course.CourseID,
                        Title = pc.Course.Title,
                        Description = pc.Course.Description,
                        ImageUrl = pc.Course.ImageUrl,
                        Price = pc.Course.Price,
                        InstructorID = pc.Course.InstructorID,
                        InstructorName = pc.Course.Instructor.FullName,
                        CategoryID = pc.Course.CategoryID,
                        CategoryName = pc.Course.Category.Name,
                        CreationDate = pc.Course.CreationDate,
                        LastUpdatedDate = pc.Course.LastUpdatedDate,
                        VideoUrl = pc.Course.VideoUrl
                    })
                    .ToListAsync();

                // Fetch courses created by the instructor
                var createdCourses = await _context.Courses
                    .Where(c => c.InstructorID == user.Id)
                    .Select(c => new CourseResponseDTO
                    {
                        CourseID = c.CourseID,
                        Title = c.Title,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        Price = c.Price,
                        InstructorID = c.InstructorID,
                        InstructorName = user.FullName,
                        CategoryID = c.CategoryID,
                        CategoryName = c.Category.Name,
                        CreationDate = c.CreationDate,
                        LastUpdatedDate = c.LastUpdatedDate,
                        VideoUrl = c.VideoUrl
                    })
                    .ToListAsync();

                // Add the user DTO to the list
                userAdminDtos.Add(new UserAdminDTO
                {
                    ID = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Roles = roles,
                    EmailConfirmed = user.EmailConfirmed,
                    PurchasedCourses = purchasedCourses,
                    CreatedCourses = createdCourses 
                });
            }

            // Return the list of user DTOs
            return userAdminDtos;
        }

        public async Task<int> CountUsersByRoleAsync(string role)
        {
            return await (from user in _context.Users
                          join userRole in _context.UserRoles on user.Id equals userRole.UserId
                          join roleEntity in _context.Roles on userRole.RoleId equals roleEntity.Id
                          where roleEntity.Name == role
                          select user).CountAsync();
        }


    }
}
