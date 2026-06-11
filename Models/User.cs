namespace DigitalHeritageApp.Models;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Community { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Story> Stories { get; set; } = new List<Story>();
    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();
    public ICollection<LanguageEntry> LanguageEntries { get; set; } = new List<LanguageEntry>();
    public ICollection<CulturalEvent> Events { get; set; } = new List<CulturalEvent>();
}
