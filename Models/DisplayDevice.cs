namespace FidsSystem.Models
{
    public class DisplayDevice
    {
        public int Id { get; set; }
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public int? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? Ratio { get; set; }
        public string? DataSet { get; set; }
        public bool IsDisplayOn { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
