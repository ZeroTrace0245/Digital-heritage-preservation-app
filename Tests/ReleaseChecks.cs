using digital_heritage_preservation_app;

public static class ReleaseChecks
{
    public static void Run(string directory)
    {
        var path = Path.Combine(directory, "recovery-test.json");
        var store = new TourismStore(path);
        for (var i = 0; i < 15; i++) store.Save(new TravelData { HomeCountry = "Country " + i });
        if (RecoveryFiles.List(path).Length != 10 || store.Load().HomeCountry != "Country 14") throw new Exception("Recovery retention failed.");
        var previous = TourismStore.Parse(File.ReadAllText(RecoveryFiles.List(path)[0]));
        if (previous.HomeCountry != "Country 13") throw new Exception("Recovery copy did not preserve previous state.");
        var before = File.ReadAllText(path);
        try { store.Save(new TravelData { Trips = new() { new TravelTrip { Name = "Bad", Budget = -1 } } }); throw new Exception("Negative budget accepted."); }
        catch (InvalidDataException) { }
        if (File.ReadAllText(path) != before) throw new Exception("Failed save changed current data.");
        File.WriteAllText(path, "broken");
        store.Save(previous);
        if (!RecoveryFiles.List(path).Any(f => File.ReadAllText(f) == "broken")) throw new Exception("Corrupt original was not preserved during restore.");

        var data = new TravelData { HasSeenUserGuide = true };
        var trip = new TravelTrip { Name = "日本 <script>alert(1)</script>", Currency = "LKR", Budget = 20000, Days = 3 };
        var first = new TripStop { DestinationId = "ella", Day = 1, EstimatedCost = 1200, Notes = "Tea & hills" };
        var second = new TripStop { DestinationId = "sigiriya", Day = 1, EstimatedCost = 3000 };
        var third = new TripStop { DestinationId = "galle", Day = 2 };
        trip.Stops.AddRange(new[] { first, third, second }); data.Trips.Add(trip);
        TravelExchange.MoveStop(trip, second.Id, -1);
        if (trip.Stops[0].Id != second.Id || trip.Stops[1].Id != third.Id || trip.Stops[2].Id != first.Id) throw new Exception("Reordering crossed day boundaries.");
        var json = TravelExchange.Export(data, trip);
        var target = new TravelData();
        target.CustomDestinations.Add(Destinations.All.Single(d => d.Id == "ella") with { Description = "Keep my version" });
        var imported = TravelExchange.Import(target, json);
        var copy = target.Trips.Single(t => t.Id == imported);
        if (copy.Id == trip.Id || copy.Stops.Any(s => trip.Stops.Any(o => o.Id == s.Id)) || copy.EstimatedTotal != 4200 || copy.Budget != 20000 || copy.Currency != "LKR") throw new Exception("Trip import lost budget or identity isolation.");
        if (Destinations.Catalog(target).Single(d => d.Id == "ella").Description != "Keep my version" || copy.Stops.Last().DestinationId == "ella") throw new Exception("Shared trip overwrote local place.");
        TourismStore.Parse(TourismStore.Serialize(target));
        var html = TravelExchange.Html(data, trip);
        if (html.Contains("<script>") || !html.Contains("&lt;script&gt;") || !html.Contains("Tea &amp; hills")) throw new Exception("Printable itinerary failed HTML escaping.");
        var roundTrip = TourismStore.Parse(TourismStore.Serialize(data));
        if (!roundTrip.HasSeenUserGuide || roundTrip.Trips[0].Stops[0].EstimatedCost != 3000) throw new Exception("New preferences did not persist.");
        var invalid = TravelExchange.Parse(json); invalid.Places.Clear();
        Reject(() => TravelExchange.Parse(System.Text.Json.JsonSerializer.Serialize(invalid)));

        var library = new OfflineLibrary(Path.Combine(directory, "offline.json"));
        var article = new OfflineArticle("京都", "https://ja.wikipedia.org/wiki/京都市", "A saved article with 日本語.", DateTimeOffset.UtcNow);
        library.Save(new() { article });
        if (library.Load().Single() != article) throw new Exception("Offline article did not survive restart.");
        Reject(() => library.Save(new() { article with { Url = "https://wikipedia.org.attacker.test/wiki/Example" } }));
        Reject(() => library.Save(new() { article, article }));
        if (library.Load().Single() != article) throw new Exception("Invalid download damaged library.");
        if (!Destinations.Filter(new(), "All", "Kassapa", "All experiences", false).Any(d => d.Id == "sigiriya")) throw new Exception("Guide history is not searchable.");
        Console.WriteLine("PASS: recovery retention and corrupt-file preservation, budget validation, same-day ordering, safe trip sharing, printable HTML escaping, offline Unicode downloads and guide search.");
    }
    private static void Reject(Action action)
    {
        try { action(); } catch (InvalidDataException) { return; }
        throw new Exception("Invalid release data accepted.");
    }
}
