using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IAuthService
    {
        Task<AdminUser?> AuthenticateAsync(string username, string password);
    }
}
