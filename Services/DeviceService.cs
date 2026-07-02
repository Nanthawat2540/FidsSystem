using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class DeviceService : BaseRepository, IDeviceService
    {
        public DeviceService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<DeviceItem>> GetAllAsync(string? search = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<DeviceItem>("sp_Device_GetAll",
                new { Search = search },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<DeviceItem?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<DeviceItem>("sp_Device_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(DeviceItem device)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Device_Create",
                new { device.ZoneId, device.ZoneName, device.DeviceName, device.DeviceType, device.IPAddress, device.Ratio },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdateAsync(DeviceItem device)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Device_Update",
                new { device.Id, device.ZoneId, device.ZoneName, device.DeviceName, device.DeviceType, device.IPAddress, device.Ratio },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Device_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
