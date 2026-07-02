using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface ISystemService
    {
        // Airline Logos
        Task<IEnumerable<AirlineLogo>> GetAllLogosAsync(string? search = null);
        Task<IEnumerable<AirlineLogo>> GetAirlineLogosAsync() => GetAllLogosAsync();
        Task<AirlineLogo?> GetLogoByIdAsync(int id);
        Task<int> CreateLogoAsync(AirlineLogo logo);
        Task UpdateLogoAsync(AirlineLogo logo);
        Task DeleteLogoAsync(int id);

        // System Settings
        Task<Dictionary<string, string>> GetAllSettingsAsync();
        Task<string?> GetSettingAsync(string key);
        Task UpsertSettingAsync(string key, string value);
        Task UpsertSettingsAsync(Dictionary<string, string> settings);
    }
}
