using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace digital_heritage_preservation_app;

public sealed record OfflineArticle(string Title, string Url, string Text, DateTimeOffset DownloadedAt)
{
    public string Summary => $"{Title} · {DownloadedAt.LocalDateTime:d}";
}

public sealed class OfflineLibrary
{
    public string FilePath { get; }
    public OfflineLibrary(string path) => FilePath = path;
    public List<OfflineArticle> Load() => File.Exists(FilePath) ? Parse(File.ReadAllText(FilePath)) : new();
    public static List<OfflineArticle> Parse(string json)
    {
        if (json.Length > 25000000) throw new InvalidDataException("Offline library is too large.");
        var articles = JsonSerializer.Deserialize<List<OfflineArticle>>(json) ?? throw new InvalidDataException("Invalid offline library.");
        if (articles.Count > 100 || articles.Any(a => a is null || string.IsNullOrWhiteSpace(a.Title) || a.Title.Length > 500 || string.IsNullOrWhiteSpace(a.Text) || a.Text.Length > 200000 || !Uri.TryCreate(a.Url, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !uri.Host.EndsWith(".wikipedia.org", StringComparison.OrdinalIgnoreCase) || !uri.AbsolutePath.StartsWith("/wiki/", StringComparison.Ordinal)) || articles.Select(a => a.Url).Distinct().Count() != articles.Count)
            throw new InvalidDataException("Invalid offline article.");
        return articles;
    }
    public void Save(List<OfflineArticle> articles)
    {
        var json = JsonSerializer.Serialize(articles);
        Parse(json);
        RecoveryFiles.Write(FilePath, json);
    }
}
