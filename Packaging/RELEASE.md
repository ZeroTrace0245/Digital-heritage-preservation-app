# Lorevia release workflow

The first release target is a downloadable Windows x64 installer. The app bundles .NET and Windows App SDK and supports Windows 10 build 19041 or later. Destination photographs are included. The per-user Microsoft Edge WebView2 Evergreen bootstrapper needs internet when the runtime is absent.

## Build and verify

Install .NET 8 SDK, the project's Windows SDK/build tools and Inno Setup 6.

```powershell
dotnet run --project Tests/ArchiveChecks.csproj
dotnet build "digital heritage preservation app.csproj" -p:Platform=x64
./Tests/InspectRelease.ps1 -Online
./Packaging/Build-Installer.ps1 -Version 1.0.0
./Packaging/Test-Installer.ps1
```

Outputs: `artifacts/installer/Lorevia-Setup-1.0.0-x64.exe` and `.sha256`. The installer uses a stable AppId for upgrades, installs per user and preserves personal data on uninstall. The user has chosen to skip signing: this is an **unsigned beta installer**.

The installer test installs into a workspace directory, reinstalls to exercise the upgrade path, runs the UI walkthrough and uninstalls. It refuses to run over an existing Lorevia installation. It temporarily changes per-user installer registration and runs Microsoft's runtime bootstrapper.

The GitHub workflow builds/tests and uploads a development installer artifact. It does not publish a public release. Update checks use this repository's latest public release, with tags in `vMAJOR.MINOR.PATCH` format. Newer release pages open in the embedded browser. Downloaded installers are not executed automatically.

## Clean-PC release checks

Before broad distribution, run the installer on a separate clean Windows PC or Windows Sandbox without Visual Studio or the .NET SDK. Verify first launch with/without WebView2; interrupted runtime installation; keyboard navigation and Narrator; high contrast and 100%/150%/200% scaling; small windows; each language; maps and wiki downloads; offline use; browser-data clearing; PDF and a real printer; budget/order and sharing; recovery from corruption; upgrade and uninstall.

Also test two PCs with the same OneDrive/Dropbox folder, including provider outages and conflicting edits. Each device writes its own snapshot and incoming data requires review; there is no automatic merge. Local checks do not prove clean-PC compatibility or actual cloud transfer.

## Feature notes

- Local files stay in `%LOCALAPPDATA%\DigitalHeritage`; the last 10 replaced versions are retained in adjacent `.recovery` directories.
- Cloud travel snapshots include trips, favorites, custom places and travel preferences. Archive records and offline articles remain separate.
- Offline downloads save article text, source and date; images and interactive material are omitted. Limit: 100 articles, 200,000 characters each.
- Budgets use user-entered estimates in one three-letter currency per trip; there is no exchange-rate conversion.
- Shared-trip imports use new IDs and preserve existing place edits. Exports contain only the selected trip and its referenced places.
- The guide and release controls support English, Japanese, Korean and Russian. Source content, privacy text, the legacy archive and some detailed diagnostics retain English or their original language.
- Signing, Store submission and public upload are not performed. Reservations, payments, live prices and transport routing are outside this personal planner.

See PRIVACY.md and CONTENT-CREDITS.md for storage, network behavior and source attribution. The original geometric Lorevia icon can be regenerated with Packaging/New-BrandAssets.ps1.
