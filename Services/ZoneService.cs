using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class ZoneService : BaseRepository, IZoneService
    {
        public ZoneService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<Zone>> GetAllAsync(string? search = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Zone>("sp_Zone_GetAll",
                new { Search = search },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Zone?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Zone>("sp_Zone_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(Zone zone)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Zone_Create",
                new { zone.ZoneName, zone.Remark },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdateAsync(Zone zone)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Zone_Update",
                new { zone.Id, zone.ZoneName, zone.Remark },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Zone_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
