using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;
using StudyJet.API.Data.Enums;
using StudyJet.API.Repositories.Interface;

namespace StudyJet.API.Repositories.Implementation
{
    public class CategoryRepo: ICategoryRepo
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category> InsertAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryID == categoryId);
        }

        /* public async Task<List<Category>> SelectAllAsync()
         {
             return await _context.Categories
                 .Where(c => c.Courses.Any(course => course.Status == CourseStatus.Approved))
                 .Include(c => c.Courses.Where(course => course.Status == CourseStatus.Approved))
                     .ThenInclude(course => course.Instructor)
                 .ToListAsync();
         } */


        public async Task<List<Category>> SelectAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Courses.Where(course => course.Status == CourseStatus.Approved)) 
                    .ThenInclude(course => course.Instructor)
                .ToListAsync();
        }





        public async Task<Category> SelectByIdAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Courses.Where(course => course.Status == CourseStatus.Approved)) 
                .ThenInclude(course => course.Instructor) 
                .FirstOrDefaultAsync(c => c.CategoryID == categoryId);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryId} was not found.");
            }

            return category;
        }

    }
}
