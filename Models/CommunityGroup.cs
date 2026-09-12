namespace DigitalHeritageApp.Models;

public class CommunityGroup
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Region { get; set; }
    public string Visibility { get; set; } = "Community";
    public int CreatedById { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
