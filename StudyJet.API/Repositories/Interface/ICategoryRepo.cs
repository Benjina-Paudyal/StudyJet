using StudyJet.API.Data.Entities;
using System.Threading.Tasks;

namespace StudyJet.API.Repositories.Interface
{
    public interface ICategoryRepo
    {
        Task<List<Category>> SelectAllAsync();
        Task<Category> SelectByIdAsync(int categoryId);
        Task<Category> InsertAsync(Category category);
        Task UpdateAsync(Category category);
        Task<bool> DeleteAsync(int categoryId);
        Task<bool> ExistsAsync(int categoryId);
    }
}
