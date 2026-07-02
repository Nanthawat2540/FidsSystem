using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IZoneService
    {
        Task<IEnumerable<Zone>> GetAllAsync(string? search = null);
        Task<Zone?> GetByIdAsync(int id);
        Task<int> CreateAsync(Zone zone);
        Task UpdateAsync(Zone zone);
        Task DeleteAsync(int id);
    }
}
