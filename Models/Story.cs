namespace DigitalHeritageApp.Models;

public class Story
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? TranscriptionText { get; set; }
    public string? MediaUrl { get; set; } // URL to audio/video
    public string MediaType { get; set; } = "audio"; // "audio" or "video"
    public string? Culture { get; set; }
    public string? Language { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }
    public int ContributorId { get; set; }
    public User? Contributor { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
