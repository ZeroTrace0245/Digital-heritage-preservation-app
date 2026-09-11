using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace digital_heritage_preservation_app;

public sealed class SharedTrip
{
    public int Version { get; set; } = 1;
    public TravelTrip Trip { get; set; } = new();
    public List<Destination> Places { get; set; } = new();
}

public static class TravelExchange
{
    public static string Export(TravelData data, TravelTrip trip) => JsonSerializer.Serialize(new SharedTrip
    {
        Trip = trip,
        Places = Destinations.Catalog(data).Where(d => trip.Stops.Any(s => s.DestinationId == d.Id)).ToList()
    }, new JsonSerializerOptions { WriteIndented = true });

    public static SharedTrip Parse(string json)
    {
        if (json.Length > 5000000) throw new InvalidDataException("Shared trip is too large.");
        var share = JsonSerializer.Deserialize<SharedTrip>(json) ?? throw new InvalidDataException("Empty shared trip.");
        if (share.Version != 1 || share.Trip is null || share.Places is null) throw new InvalidDataException("Unsupported shared trip.");
        TourismStore.Parse(TourismStore.Serialize(new TravelData { Trips = new() { share.Trip }, CustomDestinations = share.Places }));
        if (share.Trip.Stops.Any(s => !share.Places.Any(d => d.Id == s.DestinationId))) throw new InvalidDataException("Shared trip is missing its place details.");
        return share;
    }

    public static Guid Import(TravelData target, string json)
    {
        var share = Parse(json);
        var mapping = new Dictionary<string, string>();
        foreach (var place in share.Places)
        {
            var existing = Destinations.Catalog(target).FirstOrDefault(d => d.Id == place.Id);
            if (existing == place) { mapping[place.Id] = place.Id; continue; }
            var id = "custom-" + Guid.NewGuid();
            target.CustomDestinations.Add(place with { Id = id });
            mapping[place.Id] = id;
        }
        share.Trip.Id = Guid.NewGuid();
        foreach (var stop in share.Trip.Stops) { stop.Id = Guid.NewGuid(); stop.DestinationId = mapping[stop.DestinationId]; }
        target.Trips.Add(share.Trip);
        return share.Trip.Id;
    }

    public static void MoveStop(TravelTrip trip, Guid stopId, int offset)
    {
        if (offset is not (-1 or 1)) throw new ArgumentOutOfRangeException(nameof(offset));
        var index = trip.Stops.FindIndex(s => s.Id == stopId);
        if (index < 0) throw new InvalidDataException("Stop does not exist.");
        var sameDay = trip.Stops.Select((s, i) => (s, i)).Where(x => x.s.Day == trip.Stops[index].Day).Select(x => x.i).ToArray();
        var neighbor = Array.IndexOf(sameDay, index) + offset;
        if (neighbor < 0 || neighbor >= sameDay.Length) return;
        var other = sameDay[neighbor];
        (trip.Stops[index], trip.Stops[other]) = (trip.Stops[other], trip.Stops[index]);
    }

    public static string Html(TravelData data, TravelTrip trip)
    {
        string E(string value) => WebUtility.HtmlEncode(value);
        var html = new StringBuilder("<!doctype html><html><head><meta charset='utf-8'><meta http-equiv='Content-Security-Policy' content=\"default-src 'none'; style-src 'unsafe-inline'\"><style>body{font:16px 'Segoe UI',sans-serif;max-width:850px;margin:40px;color:#183e39}h1{font-size:32px}section{break-inside:avoid;border-top:1px solid #ccc;margin-top:24px}p{white-space:pre-wrap}small{color:#555}@media print{body{margin:0}}</style></head><body>");
        html.Append("<small>LOREVIA · PERSONAL ITINERARY</small><h1>").Append(E(trip.Name)).Append("</h1><p>").Append(E(trip.Summary)).Append("</p><p>")
            .Append(E($"Budget: {trip.Currency} {trip.Budget:N2} · Estimated stops: {trip.EstimatedTotal:N2}")).Append("</p>");
        for (var day = 1; day <= trip.Days; day++)
        {
            html.Append("<section><h2>").Append(E($"Day {day} · {trip.Start.AddDays(day - 1):dd MMM yyyy}")).Append("</h2>");
            foreach (var stop in trip.Stops.Where(s => s.Day == day))
            {
                var place = Destinations.Catalog(data).Single(d => d.Id == stop.DestinationId);
                html.Append("<h3>").Append(E(place.Name)).Append("</h3><small>").Append(E(place.Subtitle)).Append("</small><p>").Append(E(stop.Notes)).Append("</p><p>").Append(E($"Estimated cost: {trip.Currency} {stop.EstimatedCost:N2}")).Append("</p>");
            }
            html.Append("</section>");
        }
        return html.Append("<p><small>Estimates are entered by you; prices and exchange rates are not live.</small></p></body></html>").ToString();
    }
}
