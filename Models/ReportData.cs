namespace FidsSystem.Models
{
    public class FlightReportData
    {
        public int TotalDepartures { get; set; }
        public int TotalArrivals { get; set; }
        public int OnTime { get; set; }
        public int Delayed { get; set; }
        public int Cancelled { get; set; }
        public int Boarded { get; set; }
        public IEnumerable<AirlineStat> ByAirline { get; set; } = [];
        public IEnumerable<StatusStat> ByStatus { get; set; } = [];
        public IEnumerable<Flight> RecentDelays { get; set; } = [];
        public DateTime ReportDate { get; set; } = DateTime.Today;
    }

    public class AirlineStat
    {
        public string Airline { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusStat
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
