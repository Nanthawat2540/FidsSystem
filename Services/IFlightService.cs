using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IFlightService
    {
        Task<IEnumerable<Flight>> GetAllFlightsAsync();
        Task<Flight?> GetFlightByIdAsync(int id);
        Task CreateFlightAsync(Flight flight, string? changedBy = null);
        Task UpdateFlightAsync(Flight flight, string? changedBy = null);
        Task DeleteFlightAsync(int id);
        Task UpdateStatusAsync(int id, string status, string? changedBy = null);
        Task BulkUpdateStatusAsync(IEnumerable<int> ids, string status, string? changedBy = null);
        Task<IEnumerable<FlightStatusHistory>> GetStatusHistoryAsync(int flightId);
    }
}
