using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IPermissionService
    {
        /// <summary>All permission keys enabled for a user (merges role defaults + overrides).</summary>
        Task<HashSet<string>> GetUserPermissionsAsync(int userId, string role);

        Task<bool> HasPermissionAsync(int userId, string role, string key);

        Task SetPermissionAsync(int userId, string key, bool enabled);

        Task ResetToDefaultAsync(int userId);

        /// <summary>Returns all override rows keyed by UserId for the panel grid.</summary>
        Task<Dictionary<int, Dictionary<string, bool>>> GetAllOverridesAsync();
    }
}
