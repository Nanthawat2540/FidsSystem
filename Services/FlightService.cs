using Dapper;
using FidsSystem.Hubs;
using FidsSystem.Models;
using Microsoft.AspNetCore.SignalR;

namespace FidsSystem.Services
{
    public class FlightService : BaseRepository, IFlightService
    {
        private readonly IHubContext<FlightHub> _hub;

        public FlightService(IConfiguration configuration, IHubContext<FlightHub> hub)
            : base(configuration)
        {
            _hub = hub;
        }

        public async Task<IEnumerable<Flight>> GetAllFlightsAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Flight>(
                "SELECT * FROM Flights ORDER BY ScheduledTime");
        }

        public async Task<Flight?> GetFlightByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Flight>(
                "SELECT * FROM Flights WHERE Id = @Id", new { Id = id });
        }

        public async Task CreateFlightAsync(Flight flight, string? changedBy = null)
        {
            using var conn = CreateConnection();
            var newId = await conn.QueryFirstOrDefaultAsync<int>(
                @"INSERT INTO Flights (FlightNumber, Airline, Origin, Destination,
                    ScheduledTime, EstimatedTime, Status, Gate, Belt, FlightType, Remark)
                  OUTPUT INSERTED.Id
                  VALUES (@FlightNumber, @Airline, @Origin, @Destination,
                    @ScheduledTime, @EstimatedTime, @Status, @Gate, @Belt, @FlightType, @Remark)",
                flight);

            await LogHistoryAsync(conn, newId, flight.FlightNumber,
                null, flight.Status, null, flight.Gate, changedBy);

            await _hub.Clients.Group("display").SendAsync("FlightUpdated");
        }

        public async Task UpdateFlightAsync(Flight flight, string? changedBy = null)
        {
            using var conn = CreateConnection();

            var old = await conn.QueryFirstOrDefaultAsync<Flight>(
                "SELECT * FROM Flights WHERE Id = @Id", new { flight.Id });

            await conn.ExecuteAsync(
                @"UPDATE Flights SET
                    FlightNumber   = @FlightNumber,
                    Airline        = @Airline,
                    Origin         = @Origin,
                    Destination    = @Destination,
                    ScheduledTime  = @ScheduledTime,
                    EstimatedTime  = @EstimatedTime,
                    Status         = @Status,
                    Gate           = @Gate,
                    Belt           = @Belt,
                    FlightType     = @FlightType,
                    Remark         = @Remark,
                    UpdatedAt      = GETDATE()
                  WHERE Id = @Id",
                flight);

            if (old != null && (old.Status != flight.Status || old.Gate != flight.Gate))
            {
                await LogHistoryAsync(conn, flight.Id, flight.FlightNumber,
                    old.Status, flight.Status,
                    old.Gate,   flight.Gate,
                    changedBy);
            }

            await _hub.Clients.Group("display").SendAsync("FlightUpdated");
        }

        public async Task DeleteFlightAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("DELETE FROM Flights WHERE Id = @Id", new { Id = id });
            await _hub.Clients.Group("display").SendAsync("FlightUpdated");
        }

        public async Task UpdateStatusAsync(int id, string status, string? changedBy = null)
        {
            using var conn = CreateConnection();

            var old = await conn.QueryFirstOrDefaultAsync<Flight>(
                "SELECT * FROM Flights WHERE Id = @Id", new { Id = id });

            await conn.ExecuteAsync(
                "UPDATE Flights SET Status = @Status, UpdatedAt = GETDATE() WHERE Id = @Id",
                new { Status = status, Id = id });

            if (old != null && old.Status != status)
            {
                await LogHistoryAsync(conn, id, old.FlightNumber,
                    old.Status, status, old.Gate, old.Gate, changedBy);
            }

            await _hub.Clients.Group("display").SendAsync("FlightUpdated");
        }

        public async Task BulkUpdateStatusAsync(IEnumerable<int> ids, string status, string? changedBy = null)
        {
            var idList = string.Join(",", ids);
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Flight_BulkUpdateStatus",
                new { Ids = idList, Status = status },
                commandType: System.Data.CommandType.StoredProcedure);

            await _hub.Clients.Group("display").SendAsync("FlightUpdated");
        }

        public async Task<IEnumerable<FlightStatusHistory>> GetStatusHistoryAsync(int flightId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<FlightStatusHistory>("sp_FlightHistory_GetByFlight",
                new { FlightId = flightId },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        private static async Task LogHistoryAsync(
            System.Data.IDbConnection conn,
            int flightId, string flightNumber,
            string? oldStatus, string? newStatus,
            string? oldGate, string? newGate,
            string? changedBy)
        {
            try
            {
                await conn.ExecuteAsync("sp_FlightHistory_Add",
                    new { FlightId = flightId, FlightNumber = flightNumber,
                          OldStatus = oldStatus, NewStatus = newStatus,
                          OldGate = oldGate, NewGate = newGate,
                          ChangedBy = changedBy },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
            catch { /* non-fatal */ }
        }
    }
}
