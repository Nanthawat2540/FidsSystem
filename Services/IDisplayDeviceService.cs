using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IDisplayDeviceService
    {
        Task<IEnumerable<DisplayDevice>> GetAllAsync(string? search = null);
        Task<DisplayDevice?> GetByIdAsync(int id);
        Task<int> CreateAsync(DisplayDevice device);
        Task UpdateAsync(DisplayDevice device);
        Task<bool> ToggleOnAsync(int id);
        Task DeleteAsync(int id);
    }
}
