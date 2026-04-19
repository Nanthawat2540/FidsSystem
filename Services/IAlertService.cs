using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IAlertService
    {
        Task<IEnumerable<Alert>> GetActiveAlertsAsync();
        Task<IEnumerable<Alert>> GetAllAlertsAsync(int topN = 100);
        Task AddAlertAsync(AlertType type, AlertSeverity severity, string title, string message, string? flightNumber = null);
        Task AcknowledgeAsync(int id);
        Task AcknowledgeAllAsync();
        Task<int> GetUnreadCountAsync();
        Task CheckAndRaiseAlertsAsync();
    }
}
