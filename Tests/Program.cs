using digital_heritage_preservation_app;

var directory = Path.Combine(Path.GetTempPath(), "HeritageChecks-" + Guid.NewGuid());
try
{
    ReleaseChecks.Run(directory);
    var store = new ArchiveStore(Path.Combine(directory, "archive.json"));
    if (store.Load().Count != 0) throw new Exception("Fresh archive is not empty.");
    var record = new HeritageRecord { Title = "ආයුබෝවන්", Kind = "Language", Description = "A greeting", Culture = "Sinhala", IsFavorite = true };
    store.Save(new() { record });
    var loaded = store.Load().Single();
    if (loaded.Id != record.Id || loaded.Title != record.Title || !loaded.IsFavorite) throw new Exception("Round trip lost data.");
    foreach (var invalid in new[] { "null", "{}", "[null]", ArchiveStore.Serialize(new() { record, record }), "[{\"Kind\":\"Unknown\",\"Title\":\"Invalid\"}]" })
    {
        var rejected = false;
        try { ArchiveStore.Parse(invalid); } catch { rejected = true; }
        if (!rejected) throw new Exception("Invalid backup was accepted.");
    }
    try { store.Save(new() { new HeritageRecord() }); } catch (InvalidDataException) { }
    if (store.Load().Single().Id != record.Id) throw new Exception("Invalid save damaged existing archive.");
    record.Description = "Updated translation";
    store.Save(new() { record });
    if (store.Load().Single().Description != record.Description) throw new Exception("Update did not persist.");
    store.Save(new());
    if (store.Load().Count != 0) throw new Exception("Deletion did not persist.");
    Console.WriteLine("PASS: empty archive, Unicode round trip, favorites, invalid imports, safe failed save, update and deletion.");
    var travelStore = new TourismStore(Path.Combine(directory, "travel.json"));
    if (travelStore.Load().Trips.Count != 0) throw new Exception("Fresh travel data is not empty.");
    var travel = new TravelData { Saved = new() { "ella" }, Theme = "Dark", Trips = new() { new TravelTrip { Name = "කන්ද උඩරට", Days = 4, Stops = new() { new TripStop { DestinationId = "ella", Day = 3, Notes = "Morning walk" } } } } };
    travelStore.Save(travel);
    var restored = travelStore.Load();
    if (restored.Saved.Single() != "ella" || restored.Theme != "Dark" || restored.Trips.Single().Stops.Single().Day != 3 || restored.Trips.Single().Name != "කන්ද උඩරට") throw new Exception("Travel round trip lost data.");
    var original = File.ReadAllText(travelStore.FilePath);
    Action<TravelData>[] invalidChanges = {
        d => d.Trips[0].Days = 2,
        d => d.Trips[0].Days = 61,
        d => d.Trips[0].Start = DateTime.MaxValue,
        d => d.Trips[0].Stops[0].DestinationId = "missing",
        d => d.Trips[0].Stops[0].Day = 0,
        d => d.Trips.Add(d.Trips[0]),
        d => d.Trips[0].Stops.Add(d.Trips[0].Stops[0]),
        d => d.Saved.Add("unknown"),
        d => d.Theme = "invalid"
    };
    foreach (var change in invalidChanges)
    {
        var invalid = TourismStore.Parse(original); change(invalid);
        var rejected = false;
        try { travelStore.Save(invalid); } catch (InvalidDataException) { rejected = true; }
        if (!rejected || File.ReadAllText(travelStore.FilePath) != original) throw new Exception("Invalid travel save was accepted or damaged data.");
    }
    foreach (var invalid in new[] { "null", "[]", "{\"Trips\":[null]}", "{\"Saved\":null}" })
    {
        var rejected = false;
        try { TourismStore.Parse(invalid); } catch { rejected = true; }
        if (!rejected) throw new Exception("Malformed travel import accepted.");
    }
    restored.Trips[0].Stops.Clear(); restored.Saved.Clear(); travelStore.Save(restored);
    if (travelStore.Load().Trips[0].Stops.Count != 0 || travelStore.Load().Saved.Count != 0) throw new Exception("Travel removals did not persist.");
    Console.WriteLine("PASS: travel persistence, Unicode, saved places, theme, itinerary validation, malformed backups, safe failed saves and removals.");
    var legacy = TourismStore.Parse("{\"Saved\":[\"ella\"],\"Trips\":[],\"Theme\":\"Light\"}");
    if (legacy.HomeCountry != "Sri Lanka" || legacy.Language != "en" || legacy.CustomDestinations.Count != 0) throw new Exception("Legacy travel data did not migrate.");
    foreach (var country in Destinations.Countries)
    {
        var matches = Destinations.Filter(legacy, country, "", "All experiences", false);
        if (matches.Length < 3 || matches.Any(d => d.Country != country)) throw new Exception("Country filtering failed: " + country);
    }
    legacy.HomeCountry = "Japan"; legacy.Location = "Local"; legacy.Language = "ja";
    if (Destinations.Filter(legacy, "Local", "", "All experiences", false).Any(d => d.Country != "Japan")) throw new Exception("Local did not use home country.");
    if (Destinations.Filter(legacy, "Japan", "Kyoto", "Heritage", false).Single().Id != "kyoto") throw new Exception("Combined filters failed.");
    if (Destinations.Filter(legacy, "Japan", "", "All experiences", true).Length != 0 || Destinations.Filter(legacy, "All", "", "All experiences", true).Single().Id != "ella") throw new Exception("Country and saved filters failed.");
    var custom = new Destination("custom-test", "우리 동네", "Paris", "Culture", "My own description", "A personal stop", "") { Country = "France" };
    legacy.CustomDestinations.Add(custom); legacy.Saved.Add(custom.Id);
    legacy.Trips.Add(new TravelTrip { Name = "World trip", Stops = new() { new TripStop { DestinationId = custom.Id } } });
    legacy.CustomDestinations.Add(Destinations.All.Single(d => d.Id == "kyoto") with { Name = "私の京都" });
    travelStore.Save(legacy);
    var customized = travelStore.Load();
    if (customized.Language != "ja" || customized.HomeCountry != "Japan" || customized.Location != "Local" || Destinations.Catalog(customized).Single(d => d.Id == "kyoto").Name != "私の京都" || Destinations.Filter(customized, "France", "", "All experiences", true).Single().Id != custom.Id) throw new Exception("Customization did not persist.");
    var preserved = File.ReadAllText(travelStore.FilePath);
    customized.CustomDestinations.RemoveAll(d => d.Id == custom.Id);
    var orphanRejected = false;
    try { travelStore.Save(customized); } catch (InvalidDataException) { orphanRejected = true; }
    if (!orphanRejected || File.ReadAllText(travelStore.FilePath) != preserved) throw new Exception("Orphaned custom stop damaged data.");
    foreach (var language in new[] { "ja", "ko", "ru" })
    {
        if (TravelText.Get("Discover", language) == "Discover" || TravelText.TranslateDisplayed(TravelText.Get("Discover", language), "en") != "Discover") throw new Exception("Language switching failed.");
    }
    Console.WriteLine("PASS: legacy migration, country/local/search/saved filters, custom destinations, cross-country trips, overrides, language switching and preferences.");
}
finally
{
    if (Directory.Exists(directory)) Directory.Delete(directory, true);
}
