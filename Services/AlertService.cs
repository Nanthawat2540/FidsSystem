using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class AlertService : BaseRepository, IAlertService
    {
        public AlertService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Alert>("sp_Alert_GetActive",
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Alert>> GetAllAlertsAsync(int topN = 100)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Alert>("sp_Alert_GetAll",
                new { TopN = topN },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task AddAlertAsync(AlertType type, AlertSeverity severity, string title, string message, string? flightNumber = null)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Alert_Add",
                new { Type = type.ToString(), Severity = severity.ToString(),
                      Title = title, Message = message, FlightNumber = flightNumber },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task AcknowledgeAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Alert_Acknowledge",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task AcknowledgeAllAsync()
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Alert_AcknowledgeAll",
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                using var conn = CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Alerts WHERE IsAcknowledged = 0");
            }
            catch { return 0; }
        }

        public async Task CheckAndRaiseAlertsAsync()
        {
            try
            {
                using var conn = CreateConnection();

                // Delayed flights (EstimatedTime > ScheduledTime + 15 min)
                var delayed = await conn.QueryAsync<Flight>(
                    @"SELECT * FROM Flights
                      WHERE EstimatedTime IS NOT NULL
                        AND EstimatedTime > DATEADD(MINUTE, 15, ScheduledTime)
                        AND Status NOT IN ('Cancelled','Departed','Landed')
                        AND CAST(ScheduledTime AS DATE) = CAST(GETDATE() AS DATE)");

                foreach (var f in delayed)
                {
                    var delay = (int)(f.EstimatedTime!.Value - f.ScheduledTime).TotalMinutes;
                    await AddAlertAsync(
                        AlertType.FlightDelay, AlertSeverity.Warning,
                        $"Flight {f.FlightNumber} Delayed {delay} min",
                        $"{f.FlightNumber} ({f.Airline}) to {f.Destination} delayed by {delay} minutes.",
                        f.FlightNumber);
                }

                // Cancelled flights today
                var cancelled = await conn.QueryAsync<Flight>(
                    @"SELECT * FROM Flights
                      WHERE Status = 'Cancelled'
                        AND CAST(ScheduledTime AS DATE) = CAST(GETDATE() AS DATE)");

                foreach (var f in cancelled)
                {
                    await AddAlertAsync(
                        AlertType.FlightCancelled, AlertSeverity.Critical,
                        $"Flight {f.FlightNumber} Cancelled",
                        $"{f.FlightNumber} ({f.Airline}) to {f.Destination} has been cancelled.",
                        f.FlightNumber);
                }

                // Offline screens (no heartbeat in 5 minutes)
                var offlineScreens = await conn.QueryAsync<dynamic>(
                    @"SELECT ScreenKey, LastPing FROM ScreenHeartbeat
                      WHERE LastPing < DATEADD(MINUTE, -5, GETDATE())");

                foreach (var s in offlineScreens)
                {
                    await AddAlertAsync(
                        AlertType.ScreenOffline, AlertSeverity.Warning,
                        $"Screen Offline: {s.ScreenKey}",
                        $"Display screen '{s.ScreenKey}' has not responded for more than 5 minutes.");
                }
            }
            catch { /* background job — swallow errors */ }
        }
    }
}
