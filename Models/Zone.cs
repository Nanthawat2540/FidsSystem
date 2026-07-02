namespace FidsSystem.Models
{
    public class Zone
    {
        public int Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
