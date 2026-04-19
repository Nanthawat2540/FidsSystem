namespace FidsSystem.Models
{
    public class FlightStatusHistory
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? OldGate { get; set; }
        public string? NewGate { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
