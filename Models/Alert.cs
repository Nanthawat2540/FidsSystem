namespace FidsSystem.Models
{
    public enum AlertSeverity { Info, Warning, Critical }
    public enum AlertType { FlightDelay, GateChange, FlightCancelled, ScreenOffline }

    public class Alert
    {
        public int Id { get; set; }
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? FlightNumber { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
