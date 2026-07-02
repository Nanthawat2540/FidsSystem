using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class SystemService : BaseRepository, ISystemService
    {
        public SystemService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<AirlineLogo>> GetAllLogosAsync(string? search = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AirlineLogo>("sp_Logo_GetAll",
                new { Search = search },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<AirlineLogo?> GetLogoByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AirlineLogo>("sp_Logo_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateLogoAsync(AirlineLogo logo)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Logo_Create",
                new { logo.AirlineName, logo.IATACode, logo.BackgroundColor, logo.ImageUrl },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdateLogoAsync(AirlineLogo logo)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Logo_Update",
                new { logo.Id, logo.AirlineName, logo.IATACode, logo.BackgroundColor, logo.ImageUrl },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteLogoAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Logo_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            using var conn = CreateConnection();
            var rows = await conn.QueryAsync<(string SettingKey, string SettingValue)>(
                "SELECT SettingKey, SettingValue FROM SystemSettings ORDER BY SettingKey");
            return rows.ToDictionary(r => r.SettingKey, r => r.SettingValue);
        }

        public async Task<string?> GetSettingAsync(string key)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<string>("sp_Setting_GetByKey",
                new { Key = key },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task UpsertSettingAsync(string key, string value)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Setting_Upsert",
                new { Key = key, Value = value },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task UpsertSettingsAsync(Dictionary<string, string> settings)
        {
            using var conn = CreateConnection();
            foreach (var kv in settings)
            {
                await conn.ExecuteAsync("sp_Setting_Upsert",
                    new { Key = kv.Key, Value = kv.Value },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
