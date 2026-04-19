using System.ComponentModel.DataAnnotations;

namespace FidsSystem.Models
{
    public class Flight
    {
        public int Id { get; set; }
        [Required]
        public string FlightNumber { get; set; } = string.Empty;
        public string? Airline { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? EstimatedTime { get; set; }
        public string? Status { get; set; }
        public string? Gate { get; set; }
        public string? Belt { get; set; }
        public string? FlightType { get; set; }
        public string? Remark { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
