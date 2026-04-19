using Dapper;
using FidsSystem.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FidsSystem.Services
{
    public class AutoStatusService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<FlightHub> _hub;
        private readonly ILogger<AutoStatusService> _logger;

        public AutoStatusService(
            IServiceScopeFactory scopeFactory,
            IHubContext<FlightHub> hub,
            ILogger<AutoStatusService> logger)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunChecksAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task RunChecksAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connStr = config.GetConnectionString("DefaultConnection")!;
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);

                var now = DateTime.Now;
                bool changed = false;

                // Auto-mark departed: departure flight past 30 min, still active
                var departed = await conn.ExecuteAsync(
                    @"UPDATE Flights SET Status='Departed', UpdatedAt=GETDATE()
                      WHERE FlightType='Departure'
                        AND Status NOT IN ('Cancelled','Departed','Delayed')
                        AND ScheduledTime < DATEADD(MINUTE,-30,@Now)
                        AND CAST(ScheduledTime AS DATE) = CAST(@Now AS DATE)",
                    new { Now = now });

                if (departed > 0) { changed = true; _logger.LogInformation("Auto-departed {Count} flights", departed); }

                // Auto-mark landed: arrival flight past 30 min, still active
                var landed = await conn.ExecuteAsync(
                    @"UPDATE Flights SET Status='Landed', UpdatedAt=GETDATE()
                      WHERE FlightType='Arrival'
                        AND Status NOT IN ('Cancelled','Landed','Baggage','Baggage Claim')
                        AND ScheduledTime < DATEADD(MINUTE,-30,@Now)
                        AND CAST(ScheduledTime AS DATE) = CAST(@Now AS DATE)",
                    new { Now = now });

                if (landed > 0) { changed = true; _logger.LogInformation("Auto-landed {Count} flights", landed); }

                // Auto-delay: estimated time > scheduled + 15 min, still "On Time"
                var delayed = await conn.ExecuteAsync(
                    @"UPDATE Flights SET Status='Delayed', UpdatedAt=GETDATE()
                      WHERE Status = 'On Time'
                        AND EstimatedTime IS NOT NULL
                        AND EstimatedTime > DATEADD(MINUTE,15,ScheduledTime)",
                    new { });

                if (delayed > 0) { changed = true; _logger.LogInformation("Auto-delayed {Count} flights", delayed); }

                // Raise alerts (scoped service)
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                await alertService.CheckAndRaiseAlertsAsync();

                if (changed)
                    await _hub.Clients.Group("display").SendAsync("FlightUpdated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoStatusService error");
            }
        }
    }
}
