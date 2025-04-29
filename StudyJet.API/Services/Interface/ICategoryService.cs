using StudyJet.API.DTOs.Category;

namespace StudyJet.API.Services.Interface
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDTO>> GetAllAsync();
        Task<CategoryResponseDTO> GetByIdAsync(int categoryId);
        Task<int> AddAsync(CategoryRequestDTO categoryDto);
        Task UpdateAsync(int categoryId, CategoryRequestDTO categoryDto);
        Task<bool> DeleteAsync(int categoryId);
        Task<bool> ExistsAsync(int categoryId);
    }
}
