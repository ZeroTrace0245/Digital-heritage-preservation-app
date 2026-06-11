namespace DigitalHeritageApp.Models;

public class LanguageEntry
{
    public int Id { get; set; }
    public required string Language { get; set; }
    public required string Word { get; set; }
    public required string Translation { get; set; }
    public string? AudioUrl { get; set; } // Pronunciation audio
    public string? IPA { get; set; } // International Phonetic Alphabet
    public string? PartOfSpeech { get; set; } // noun, verb, adjective, etc.
    public string? Example { get; set; } // Example sentence
    public string? ExampleTranslation { get; set; }
    public string? Notes { get; set; }
    public string? Dialect { get; set; }
    public int ContributorId { get; set; }
    public User? Contributor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
