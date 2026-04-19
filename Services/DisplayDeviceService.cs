using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class DisplayDeviceService : BaseRepository, IDisplayDeviceService
    {
        public DisplayDeviceService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<DisplayDevice>> GetAllAsync(string? search = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<DisplayDevice>("sp_Display_GetAll",
                new { Search = search },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<DisplayDevice?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<DisplayDevice>("sp_Display_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(DisplayDevice device)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Display_Create",
                new { device.ZoneId, device.ZoneName, device.DeviceName, device.TemplateId, device.TemplateName, device.Ratio, device.DataSet, device.IsDisplayOn },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdateAsync(DisplayDevice device)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Display_Update",
                new { device.Id, device.ZoneId, device.ZoneName, device.DeviceName, device.TemplateId, device.TemplateName, device.Ratio, device.DataSet, device.IsDisplayOn },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<bool> ToggleOnAsync(int id)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Display_ToggleOn",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
            return result?.IsDisplayOn == true;
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Display_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
