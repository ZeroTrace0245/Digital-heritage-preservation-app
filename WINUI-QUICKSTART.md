# WinUI 3 desktop app

The native desktop app provides four collections: stories, artifacts, language entries, and events.
Create, edit, and delete records; search all text fields; filter by collection; mark favorites;
track event dates and upcoming counts; export and merge JSON backups.
Required fields are validated before saving. Imports preserve a timestamped copy of the previous archive.

## Build and run

On Windows with the .NET SDK installed:

```powershell
dotnet build "digital heritage preservation app.csproj" -p:Platform=x64
& ".\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\digital heritage preservation app.exe"
```

The app is unpackaged and includes the Windows App SDK and .NET runtime in its output.
No signing certificate or running API server is required.
Keep the entire build output directory together when copying the app.

## Storage and backups

Records are saved in `%LOCALAPPDATA%\DigitalHeritage\archive.json`.
This is a separate offline desktop archive; it does not synchronize with the existing web API or SQLite database.
Use one app instance at a time. Export backups regularly.
Import accepts the desktop JSON backup format and merges by record ID, replacing matching records after confirmation.
Invalid files are rejected; an unreadable local archive is preserved and editing is disabled until a valid backup is imported.

## Verification

```powershell
dotnet run --project Tests/ArchiveChecks.csproj
dotnet build digital-heritage-api.csproj
```

Manual desktop checks: add each record type, test required fields, edit and favorite a record,
search and switch collections, cancel a deletion, confirm a deletion, export and import a backup,
then restart the app and verify saved records. Test file-picker cancellation and an invalid JSON import.

Media attachments, API synchronization, authentication, recording, and map views are not implemented in the desktop app.
