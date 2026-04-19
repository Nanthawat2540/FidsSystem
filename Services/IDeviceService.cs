using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<DeviceItem>> GetAllAsync(string? search = null);
        Task<DeviceItem?> GetByIdAsync(int id);
        Task<int> CreateAsync(DeviceItem device);
        Task UpdateAsync(DeviceItem device);
        Task DeleteAsync(int id);
    }
}
