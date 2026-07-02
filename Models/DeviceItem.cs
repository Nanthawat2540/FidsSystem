namespace FidsSystem.Models
{
    public class DeviceItem
    {
        public int Id { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? IPAddress { get; set; }
        public string? Ratio { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
