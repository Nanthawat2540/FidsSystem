namespace FidsSystem.Models
{
    public class TemplateItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Ratio { get; set; } = string.Empty;
    }

    public static class DeviceTypes
    {
        public static readonly string[] All = {
            "Baggage Claim", "Baggage Claim Information",
            "Arrival Information", "Check-in",
            "Departures Information", "Departures Gate", "Gate"
        };
    }
}
