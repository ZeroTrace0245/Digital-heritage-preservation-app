namespace DigitalHeritageApp.Models;

public class CulturalEvent
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Culture { get; set; }
    public string? Type { get; set; } // e.g., "festival", "ceremony", "ritual"
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PhotoUrl { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; } // e.g., "yearly", "monthly"
    public int ContributorId { get; set; }
    public User? Contributor { get; set; }
    public int Attendees { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
