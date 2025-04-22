using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;

namespace StudyJet.API.Repositories.Implementation
{
    public interface IUserRepo
    {
        Task<User> SelectByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<List<UserAdminDTO>> SelectUsersByRoleAsync(string role);
        Task<int> CountUsersByRoleAsync(string role);
    }
}
