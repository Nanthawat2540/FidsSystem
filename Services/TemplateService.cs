using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class TemplateService : BaseRepository, ITemplateService
    {
        public TemplateService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<TemplateItem>> GetAllAsync(string? search = null, string? deviceType = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TemplateItem>("sp_Template_GetAll",
                new { Search = search, DeviceType = deviceType },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<TemplateItem?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<TemplateItem>("sp_Template_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(TemplateItem template)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Template_Create",
                new { template.Name, template.DeviceType, template.Ratio },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdateAsync(TemplateItem template)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Template_Update",
                new { template.Id, template.Name, template.DeviceType, template.Ratio },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Template_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
