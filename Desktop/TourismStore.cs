using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace digital_heritage_preservation_app;

public sealed record Destination(string Id, string Name, string Region, string Category, string Description, string Highlight, string Image)
{
    public string Country { get; init; } = "Sri Lanka";
    public string Subtitle => $"{Region} · {Country} · {Category}";
    public override string ToString() => $"{Name} · {Country}";
}

public static class Destinations
{
    public static readonly Destination[] All =
    {
        new("sigiriya", "Sigiriya", "Central Province", "Heritage", "Follow the gardens and stone pathways beneath Sri Lanka’s iconic rock fortress. Make time for the surrounding villages and landscapes as well as the climb.", "Ancient gardens. Extraordinary horizons.", "ms-appx:///Assets/Travel/sigiriya.jpg"),
        new("ella", "Ella", "Uva Province", "Nature", "Slow down in the hill country. Explore tea-covered slopes, walking trails and the landscape around the Nine Arch Bridge.", "Take the scenic route.", "ms-appx:///Assets/Travel/ella.jpg"),
        new("galle", "Galle Fort", "Southern Province", "Heritage", "Wander through the fort’s streets, discover small galleries and walk the ocean-facing ramparts. Leave room in your day for a quiet café stop.", "History meets the Indian Ocean.", "ms-appx:///Assets/Travel/galle.jpg"),
        new("mirissa", "Mirissa", "Southern Province", "Coast", "Build a slower day around sandy beaches and coastal viewpoints. Check local sea conditions before swimming and choose responsible wildlife operators.", "A little more ocean, a little less hurry.", "ms-appx:///Assets/Travel/mirissa.jpg"),
        new("kandy", "Kandy", "Central Province", "Culture", "Explore the lakeside city and its living cultural traditions. Dress respectfully at sacred places and allow time to discover the surrounding hills.", "Find the heart of the hill country.", "ms-appx:///Assets/Travel/kandy.jpg"),
        new("yala", "Yala", "Southern / Uva", "Nature", "Discover Sri Lanka’s dry-zone landscapes with a local safari guide. Keep your distance from wildlife and follow park rules throughout your visit.", "Leave space for the wild.", "ms-appx:///Assets/Travel/yala.jpg"),
        new("tokyo", "Tokyo", "Kantō", "Culture", "Explore Tokyo one neighborhood at a time, from Asakusa’s historic streets to the energy of Shibuya. Leave time for small shops, parks and local food.", "A thousand discoveries around every corner.", "ms-appx:///Assets/Travel/tokyo.jpg") { Country = "Japan" },
        new("kyoto", "Kyoto", "Kansai", "Heritage", "Walk through temple gardens and traditional streets. Explore early, respect private property and make room for a quieter route beyond the busiest sights.", "A quieter kind of wonder.", "ms-appx:///Assets/Travel/kyoto.jpg") { Country = "Japan" },
        new("fuji", "Mount Fuji", "Yamanashi / Shizuoka", "Nature", "Discover lakeside landscapes and views of Japan’s iconic mountain. Mountain access is seasonal; check official conditions before planning a climb.", "Find a new perspective.", "ms-appx:///Assets/Travel/fuji.jpg") { Country = "Japan" },
        new("seoul", "Seoul", "Seoul", "Culture", "Pair royal palaces with contemporary neighborhoods, riverside walks and markets. Build a day around a few nearby places to enjoy the city at your own pace.", "Tradition with a new rhythm.", "ms-appx:///Assets/Travel/seoul.jpg") { Country = "South Korea" },
        new("busan", "Busan", "Yeongnam", "Coast", "Explore a coastal city of beaches, hillside neighborhoods and seafood markets. Combine an urban walk with time beside the sea.", "City days. Ocean evenings.", "ms-appx:///Assets/Travel/busan.jpg") { Country = "South Korea" },
        new("jeju", "Jeju Island", "Jeju", "Nature", "Discover volcanic landscapes, coastal paths and island villages. Plan around changing weather and give each part of the island time to unfold.", "Follow the island’s own pace.", "ms-appx:///Assets/Travel/jeju.jpg") { Country = "South Korea" },
        new("moscow", "Moscow", "Central Russia", "Heritage", "Explore historic architecture, museums and the city’s public spaces. Group nearby sights into walkable days and check venue information before visiting.", "History on a monumental scale.", "ms-appx:///Assets/Travel/moscow.jpg") { Country = "Russia" },
        new("petersburg", "Saint Petersburg", "Northwestern Russia", "Culture", "Discover canals, broad avenues and art collections. Allow unhurried time for museum visits and walks through the historic center.", "A city reflected in water.", "ms-appx:///Assets/Travel/petersburg.jpg") { Country = "Russia" },
        new("baikal", "Lake Baikal", "Siberia", "Nature", "Explore the landscapes around the freshwater lake with local guidance. Conditions vary strongly by season; plan transport and outdoor activities carefully.", "Space to breathe, room to explore.", "ms-appx:///Assets/Travel/baikal.jpg") { Country = "Russia" }
    };
    public static readonly string[] Countries = { "Sri Lanka", "Japan", "South Korea", "Russia" };
    public static readonly string[] Categories = { "Heritage", "Nature", "Coast", "Culture" };
    public static Destination[] Catalog(TravelData data) => All.Where(d => !data.CustomDestinations.Any(c => c.Id == d.Id)).Concat(data.CustomDestinations).ToArray();
    public static Destination[] Filter(TravelData data, string location, string query, string category, bool savedOnly)
    {
        var country = location == "Local" ? data.HomeCountry : location;
        return Catalog(data).Where(d => country == "All" || d.Country.Equals(country, StringComparison.OrdinalIgnoreCase))
            .Where(d => !savedOnly || data.Saved.Contains(d.Id))
            .Where(d => category == "All experiences" || d.Category == category)
            .Where(d => $"{d.Name} {d.Region} {d.Country} {d.Category} {d.Description}".Contains(query.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
    }
}

public sealed class TripStop
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DestinationId { get; set; } = "";
    public int Day { get; set; } = 1;
    public string Notes { get; set; } = "";
}

public sealed class TravelTrip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public DateTime Start { get; set; } = DateTime.Today;
    public int Days { get; set; } = 3;
    public List<TripStop> Stops { get; set; } = new();
    public string Summary => $"{Start:dd MMM yyyy} · {Days} days · {Stops.Count} stops";
}

public sealed class TravelData
{
    public List<string> Saved { get; set; } = new();
    public List<TravelTrip> Trips { get; set; } = new();
    public string Theme { get; set; } = "Default";
    public string Language { get; set; } = "en";
    public string HomeCountry { get; set; } = "Sri Lanka";
    public string Location { get; set; } = "Local";
    public List<Destination> CustomDestinations { get; set; } = new();
}

public sealed class TourismStore
{
    public string FilePath { get; }
    public TourismStore(string? path = null) => FilePath = path ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigitalHeritage", "travel.json");
    public TravelData Load() => File.Exists(FilePath) ? Parse(File.ReadAllText(FilePath)) : new();
    public static string Serialize(TravelData data) => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    public static TravelData Parse(string json)
    {
        var data = JsonSerializer.Deserialize<TravelData>(json) ?? throw new InvalidDataException("Travel data is empty.");
        if (data.CustomDestinations is null || data.CustomDestinations.Any(d => d is null || string.IsNullOrWhiteSpace(d.Id) || string.IsNullOrWhiteSpace(d.Name) || d.Name.Length > 120 || string.IsNullOrWhiteSpace(d.Country) || d.Country.Length > 80 || d.Region is null || !Destinations.Categories.Contains(d.Category) || string.IsNullOrWhiteSpace(d.Description) || d.Description.Length > 4000 || d.Highlight is null || d.Image is null || (d.Image.Length > 0 && !Destinations.All.Any(b => b.Image == d.Image))) || data.CustomDestinations.Select(d => d.Id).Distinct().Count() != data.CustomDestinations.Count)
            throw new InvalidDataException("Invalid custom destination.");
        if (!new[] { "en", "ja", "ko", "ru" }.Contains(data.Language) || string.IsNullOrWhiteSpace(data.HomeCountry) || data.HomeCountry.Length > 80 || string.IsNullOrWhiteSpace(data.Location) || data.Location.Length > 80)
            throw new InvalidDataException("Invalid language or location.");
        var ids = Destinations.Catalog(data).Select(d => d.Id).ToHashSet();
        if (data.Saved is null || data.Trips is null || data.Saved.Any(id => !ids.Contains(id)) || data.Saved.Distinct().Count() != data.Saved.Count || !new[] { "Default", "Light", "Dark" }.Contains(data.Theme))
            throw new InvalidDataException("Invalid travel preferences.");
        if (data.Trips.Any(t => t is null || t.Id == Guid.Empty || string.IsNullOrWhiteSpace(t.Name) || t.Name.Length > 120 || t.Days < 1 || t.Days > 60 || t.Start > DateTime.MaxValue.AddDays(-t.Days) || t.Stops is null || t.Stops.Any(s => s is null || s.Id == Guid.Empty || !ids.Contains(s.DestinationId) || s.Day < 1 || s.Day > t.Days || s.Notes is null || s.Notes.Length > 2000) || t.Stops.Select(s => s.Id).Distinct().Count() != t.Stops.Count) || data.Trips.Select(t => t.Id).Distinct().Count() != data.Trips.Count)
            throw new InvalidDataException("Invalid trip or itinerary.");
        return data;
    }
    public void Save(TravelData data)
    {
        var json = Serialize(data);
        Parse(json);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
        File.WriteAllText(FilePath + ".tmp", json);
        File.Move(FilePath + ".tmp", FilePath, true);
    }
}
