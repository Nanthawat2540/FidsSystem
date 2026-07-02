using System.Data;
using Microsoft.Data.SqlClient;

namespace FidsSystem.Services
{
    public abstract class BaseRepository
    {
        private readonly string _connectionString;

        protected BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        protected IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
