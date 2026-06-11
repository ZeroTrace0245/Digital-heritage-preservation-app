namespace DigitalHeritageApp.Models;

public class Artifact
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Category { get; set; } // e.g., "clothing", "tools", "art"
    public string? PhotoUrl { get; set; }
    public string? HistoricalContext { get; set; }
    public string? Culture { get; set; }
    public string? EstimatedAge { get; set; }
    public string? Materials { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }
    public int ContributorId { get; set; }
    public User? Contributor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
