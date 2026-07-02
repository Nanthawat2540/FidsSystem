using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IAdminUserService
    {
        Task<IEnumerable<AdminUser>> GetAllAsync();
        Task<AdminUser?> GetByIdAsync(int id);
        Task<int> CreateAsync(AdminUser user, string password);
        Task UpdateAsync(AdminUser user, string? newPassword = null);
        Task DeleteAsync(int id);
    }
}
