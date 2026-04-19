using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class AuthService : BaseRepository, IAuthService
    {
        public AuthService(IConfiguration configuration) : base(configuration) { }

        public async Task<AdminUser?> AuthenticateAsync(string username, string password)
        {
            var hash = AdminUserService.HashPassword(password);
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AdminUser>("sp_User_Authenticate",
                new { Username = username, PasswordHash = hash },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
