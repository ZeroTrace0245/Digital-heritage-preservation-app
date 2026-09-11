using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace digital_heritage_preservation_app;

public sealed class HeritageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Kind { get; set; } = "Story";
    public string Title { get; set; } = "";
    public string Culture { get; set; } = "";
    public string Location { get; set; } = "";
    public string Tags { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTimeOffset? EventDate { get; set; }
    public bool IsFavorite { get; set; }
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;
    public string Summary => $"{(IsFavorite ? "★ · " : "")}{Kind} · {Culture} · {Location}";
}

public sealed class ArchiveStore
{
    public static readonly string[] Kinds = { "Story", "Artifact", "Language", "Event" };
    public string FilePath { get; }
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public ArchiveStore(string? path = null) => FilePath = path ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigitalHeritage", "archive.json");
    public List<HeritageRecord> Load() => File.Exists(FilePath) ? Parse(File.ReadAllText(FilePath)) : new();
    public static List<HeritageRecord> Parse(string json)
    {
        var records = JsonSerializer.Deserialize<List<HeritageRecord>>(json, Options) ?? throw new InvalidDataException("Backup must contain an array of records.");
        if (records.Any(r => r is null || r.Id == Guid.Empty || !Kinds.Contains(r.Kind) || string.IsNullOrWhiteSpace(r.Title) || string.IsNullOrWhiteSpace(r.Description) || r.Culture is null || r.Location is null || r.Tags is null || r.Kind == "Event" && r.EventDate is null) || records.Select(r => r.Id).Distinct().Count() != records.Count)
            throw new InvalidDataException("Backup contains invalid records or duplicate IDs.");
        return records;
    }
    public static string Serialize(List<HeritageRecord> records) => JsonSerializer.Serialize(records, Options);
    public void Save(List<HeritageRecord> records)
    {
        Parse(Serialize(records));
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, Serialize(records));
        File.Move(temporary, FilePath, true);
    }
}
