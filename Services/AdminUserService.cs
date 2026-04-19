using Dapper;
using FidsSystem.Models;
using System.Security.Cryptography;
using System.Text;

namespace FidsSystem.Services
{
    public class AdminUserService : BaseRepository, IAdminUserService
    {
        public AdminUserService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<AdminUser>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AdminUser>("sp_Admin_GetAll",
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<AdminUser?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AdminUser>("sp_Admin_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateAsync(AdminUser user, string password)
        {
            using var conn = CreateConnection();
            var hash = HashPassword(password);
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Admin_Create",
                new { user.Username, PasswordHash = hash, user.Role, user.AirlineCode },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdateAsync(AdminUser user, string? newPassword = null)
        {
            using var conn = CreateConnection();
            string? hash = newPassword != null ? HashPassword(newPassword) : null;
            await conn.ExecuteAsync("sp_Admin_Update",
                new { user.Id, user.Role, user.Status, user.AirlineCode, PasswordHash = hash },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Admin_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
