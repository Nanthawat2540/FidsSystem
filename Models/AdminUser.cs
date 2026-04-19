namespace FidsSystem.Models
{
    public static class Roles
    {
        public const string Admin   = "ADMIN";
        public const string Staff   = "STAFF";
        public const string Airline = "AIRLINE";

        public static readonly string[] All = [Admin, Staff, Airline];
    }

    public class AdminUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.Staff;
        public string Status { get; set; } = "ACTIVE";
        public string? AirlineCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AirlineLogo
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? BackgroundColor { get; set; }
        public string AirlineName { get; set; } = string.Empty;
        public string IATACode { get; set; } = string.Empty;
    }
}
