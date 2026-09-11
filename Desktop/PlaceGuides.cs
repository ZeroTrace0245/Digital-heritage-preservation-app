using System;
using System.Collections.Generic;

namespace digital_heritage_preservation_app;

public static class PlaceGuides
{
    private static readonly Dictionary<string, (string History, string Source)> Notes = new()
    {
        ["sigiriya"] = ("Sigiriya preserves the remains of King Kassapa I’s fifth-century capital on and around a granite peak. Its surviving stairways and lion gateway connect the lower landscape with the summit.", "202"),
        ["galle"] = ("Galle’s fortified town developed from Portuguese foundations in the sixteenth century. Its architecture reflects exchanges between European building traditions and South Asian life.", "451"),
        ["kandy"] = ("Kandy was the last capital of the Sinhala kings. The Temple of the Tooth remains a major Buddhist place of pilgrimage and an important part of the city’s cultural identity.", "450"),
        ["kyoto"] = ("Kyoto served as Japan’s imperial capital for more than a thousand years. The UNESCO property includes historic monuments in Kyoto, Uji and Otsu, with temples, shrines and gardens illustrating Japanese architecture and garden design.", "688"),
        ["fuji"] = ("Mount Fuji has long been a focus of pilgrimage and artistic inspiration. UNESCO recognizes a cultural landscape that includes the mountain and associated religious sites, lakes and springs.", "1418"),
        ["jeju"] = ("Jeju’s volcanic heritage includes Hallasan, the Geomunoreum lava-tube system and Seongsan Ilchulbong. These landscapes preserve different expressions of the island’s volcanic formation.", "1264"),
        ["baikal"] = ("Lake Baikal is an ancient, exceptionally deep freshwater lake. Its long isolation has supported distinctive freshwater ecosystems and many species found nowhere else.", "754"),
        ["petersburg"] = ("Saint Petersburg’s historic center combines canals, monumental ensembles and architecture associated with the city’s development from the time of Peter the Great. Its UNESCO property also includes related groups of monuments.", "540"),
        ["moscow"] = ("The Kremlin and Red Square form a historic center of political and religious life in Moscow. The UNESCO property includes the Kremlin’s architectural ensemble and Red Square with Saint Basil’s Cathedral.", "545")
    };

    public static Destination Enrich(Destination destination)
    {
        var city = destination.Id switch
        {
            "tokyo" => ("Tokyo was formerly called Edo. The city developed as a political and cultural center during the Edo period and was renamed Tokyo in 1868. Its neighborhoods preserve different layers of that history.", "https://www.gotokyo.org/en/see-and-do/history/index.html"),
            "seoul" => ("Seoul’s history spans ancient settlements and successive Korean kingdoms. Its role as the Joseon capital helped shape the royal palaces and historic urban landscape visible today.", "https://english.seoul.go.kr/seoul-views/meaning-of-seoul/1-history/"),
            "busan" => ("Busan’s port has long connected Korea with overseas trade. During the Korean War the city served as a temporary capital and a place of refuge, an important part of its modern identity.", "https://www.busan.go.kr/eng/history-of-busan"),
            _ => ("", "")
        };
        if (city.Item1.Length > 0) return destination with { History = city.Item1, SourceUrl = city.Item2, CheckedAt = new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.Zero) };
        if (!Notes.TryGetValue(destination.Id, out var note)) return destination;
        return destination with
        {
            History = note.History,
            SourceUrl = "https://whc.unesco.org/en/list/" + note.Source + "/",
            CheckedAt = new DateTimeOffset(2026, 9, 11, 0, 0, 0, TimeSpan.Zero)
        };
    }

    public static string Details(Destination place) =>
        place.Description + "\n\n" + place.Highlight +
        (string.IsNullOrWhiteSpace(place.History) ? "" : "\n\nHistory and context\n" + place.History) +
        "\n\nAccessibility\n" + (string.IsNullOrWhiteSpace(place.Accessibility) ? "Access details have not been verified. Ask the venue about step-free routes, accessible toilets and assistance before visiting." : place.Accessibility) +
        (string.IsNullOrWhiteSpace(place.SourceUrl) ? "\n\nSource: local destination notes." : "\n\nSource: " + place.SourceUrl) +
        (place.CheckedAt is { } date ? "\nSource checked: " + date.ToString("yyyy-MM-dd") : "\nSource check date not recorded.");
}
