using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class PermissionService : BaseRepository, IPermissionService
    {
        public PermissionService(IConfiguration cfg) : base(cfg) { }

        public async Task<HashSet<string>> GetUserPermissionsAsync(int userId, string role)
        {
            using var conn = CreateConnection();

            // Load override rows from DB
            var rows = await conn.QueryAsync<(string PermKey, bool IsEnabled)>(
                "sp_Perm_GetByUser",
                new { UserId = userId },
                commandType: System.Data.CommandType.StoredProcedure);

            var overrides = rows.ToDictionary(r => r.PermKey, r => r.IsEnabled);

            var result = new HashSet<string>();
            foreach (var key in PermKeys.All)
            {
                bool enabled = overrides.TryGetValue(key, out var ov)
                    ? ov
                    : PermKeys.RoleDefault(key, role);

                if (enabled) result.Add(key);
            }
            return result;
        }

        public async Task<bool> HasPermissionAsync(int userId, string role, string key)
        {
            var perms = await GetUserPermissionsAsync(userId, role);
            return perms.Contains(key);
        }

        public async Task SetPermissionAsync(int userId, string key, bool enabled)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Perm_Upsert",
                new { UserId = userId, PermKey = key, IsEnabled = enabled },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task ResetToDefaultAsync(int userId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Perm_ResetUser",
                new { UserId = userId },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Dictionary<int, Dictionary<string, bool>>> GetAllOverridesAsync()
        {
            using var conn = CreateConnection();
            var rows = await conn.QueryAsync<(int UserId, string PermKey, bool IsEnabled)>(
                "sp_Perm_GetAll",
                commandType: System.Data.CommandType.StoredProcedure);

            var result = new Dictionary<int, Dictionary<string, bool>>();
            foreach (var r in rows)
            {
                if (!result.ContainsKey(r.UserId))
                    result[r.UserId] = [];
                result[r.UserId][r.PermKey] = r.IsEnabled;
            }
            return result;
        }
    }
}
