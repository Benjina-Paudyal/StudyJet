using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.Category;
using StudyJet.API.DTOs.Course;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;

        public CategoryService(ICategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            var categories = await _categoryRepo.SelectAllAsync();
            return categories.Select(category => new CategoryResponseDTO
            {
                CategoryID = category.CategoryID,
                Name = category.Name,


            }).ToList();
        }

        public async Task<CategoryResponseDTO> GetByIdAsync(int categoryId)
        {
            var category = await _categoryRepo.SelectByIdAsync(categoryId);

            if (category == null)
            {
                return null; 
            }

            return new CategoryResponseDTO
            {
                CategoryID = category.CategoryID,
                Name = category.Name,
                Courses = category.Courses.Select(c => new CourseResponseDTO
                {
                    CourseID = c.CourseID,
                    Title = c.Title,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Price = c.Price,
                    InstructorID = c.InstructorID,
                    InstructorName = c.Instructor?.FullName,
                    CategoryID = c.CategoryID,
                    CategoryName = c.Category?.Name,
                    CreationDate = c.CreationDate,
                    LastUpdatedDate = c.LastUpdatedDate,
                    VideoUrl = c.VideoUrl
                }).ToList()
            };
        }

        public async Task<int> AddAsync(CategoryRequestDTO categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name
            };

            var addedCategory = await _categoryRepo.InsertAsync(category);
            return addedCategory.CategoryID;

        }

        public async Task UpdateAsync(int categoryId, CategoryRequestDTO categoryDto)
        {
            var existingCategory = await _categoryRepo.SelectByIdAsync(categoryId);
            if (existingCategory == null)
                throw new KeyNotFoundException("Category not found");

            existingCategory.Name = categoryDto.Name;

            await _categoryRepo.UpdateAsync(existingCategory);
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var exists = await _categoryRepo.ExistsAsync(categoryId);
            if (!exists)
                return false;

            await _categoryRepo.DeleteAsync(categoryId);
            return true;
        }

        public async Task<bool> ExistsAsync(int categoryId)
        {
            return await _categoryRepo.ExistsAsync(categoryId);
        }

    }
}
