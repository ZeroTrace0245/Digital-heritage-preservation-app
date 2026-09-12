namespace DigitalHeritageApp.Models;

// One shared, auditable record type for community conversations and calls to action.
public class CommunityItem
{
    public int Id { get; set; }
    public required string Type { get; set; } // Memory, Identification, Challenge, HelpRequest, Timeline
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? Tags { get; set; }
    public string Visibility { get; set; } = "Public";
    public bool ConsentConfirmed { get; set; }
    public bool IsSensitive { get; set; }
    public string Status { get; set; } = "Open";
    public int ContributorId { get; set; } = 1;
    public int? GroupId { get; set; }
    public int? ParentItemId { get; set; }
    public int? VerifiedById { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
