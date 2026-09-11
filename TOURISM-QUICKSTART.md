# Serendib desktop travel planner

A native WinUI 3 app with Windows Mica Alt, light/dark/system appearance, offline destination photography, saved places and personal itineraries.

## Run

```powershell
dotnet build "digital heritage preservation app.csproj" -p:Platform=x64
& ".\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\digital heritage preservation app.exe"
```

Keep the complete build output folder together when copying the app. Windows 11 provides the Mica effect; Windows controls manage system fallback behavior. The heritage archive remains available in the left navigation.

## Locations and customization

- Use the top location bar to switch between Local, All, Sri Lanka, Japan, South Korea and Russia. Additional countries appear when you add your own destinations. The last selected tab is remembered.
- Local means the home country selected in Settings. It defaults to Sri Lanka and does not request GPS access. You can enter another country in the editable home-country selector.
- Choose English, Japanese, Korean or Russian in Settings. Main navigation, settings, filters and editor controls change immediately. Destination descriptions, place names and personal notes retain their original language. Some detailed status messages and the separate heritage archive remain English.
- Open a destination and choose Customize place to edit its name, country, region, category, description and highlight. Built-in edits can be reset to the original.
- Add a place supports countries beyond the included catalog. Custom places use a neutral visual when no bundled photo exists. A custom place used in an itinerary must be removed from those itineraries before it can be deleted.
- Saved places use the current country and search filters. Select All to see saved places across countries.

## Places wiki

Open **Places wiki** in the left navigation. Type to filter offline destination notes by name, region, country, category or description; the top country tabs apply to these notes. Select a result to read its description and use **Save or plan a visit** for trip and favorites controls. Custom destination edits appear here too.

Press Enter, the search icon, or **Search Wikipedia** to search worldwide in the selected app language. **Read on Wikipedia** searches for the selected place. Wikipedia pages and their article links open inside the app, with Back, Retry and Close article controls. The source address and Wikipedia’s own attribution remain visible. Links outside Wikipedia are blocked in this reader.

Online articles require internet and Microsoft Edge WebView2 Runtime; offline notes remain available when the reader cannot load. Articles are live web pages and are not saved in travel backups. The app language selects the Wikipedia edition; it does not translate local notes. Detailed wiki instructions and status messages currently use English.

Manual checks: search for Sigiriya, open its offline notes, save it, and read Wikipedia; follow an article link and go back; try a worldwide search and an empty query; switch country and language; disconnect internet and retry; verify custom notes update after editing a destination.

## Trips and storage

Create a trip with a start date and 1–60 days. Open destinations to save them or add them to a trip. A trip can contain stops in multiple countries. Select a stop to change its day or notes, or remove it. Export itineraries as text.

Travel data is stored at `%LOCALAPPDATA%\DigitalHeritage\travel.json`, separately from the heritage archive and web API. Existing travel files receive default location and language preferences automatically. Use one app instance at a time.

Export and restore JSON backups from Settings. Restore validates the complete file before replacing data and preserves a timestamped copy of the previous file. Invalid data does not overwrite the existing file. Startup exceptions are logged to `%LOCALAPPDATA%\DigitalHeritage\startup-errors.log`.

This is an offline personal planner. It does not provide reservations, payments, live availability, transport routing or automatic translation of destination content. Map links open in the browser. Photo attribution and licenses are bundled in `Assets/Travel/CREDITS.md` and available from Settings.

## Verification

```powershell
dotnet run --project Tests/ArchiveChecks.csproj
dotnet build "digital heritage preservation app.csproj" -p:Platform=x64
dotnet build digital-heritage-api.csproj
```

Manual checks: switch countries and combined search/category filters; change home country and choose Local; switch each language and theme; save a place; create a custom place in another country; customize and reset a built-in place; create a trip with stops from multiple countries; edit days and notes; restart to verify persistence; export/restore a backup and reject malformed JSON.
