namespace FidsSystem.Models
{
    public class Advertisement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileType { get; set; } = "Video";   // Video | Image
        public string? FilePath { get; set; }
        public int? Duration { get; set; }
        public decimal? FileSizeMB { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public class AdPlaylist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int VideoCount { get; set; }
        public int ImageCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MatchDisplay
    {
        public int DisplayId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public string Ratio { get; set; } = string.Empty;
        public int? PlaylistId { get; set; }
        public string? PlaylistName { get; set; }
        public bool IsMatched { get; set; }
        public DateTime? MatchedAt { get; set; }
    }
}
