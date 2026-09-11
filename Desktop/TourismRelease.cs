using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow
{
    private const string ProjectUrl = "https://github.com/ZeroTrace0245/Digital-heritage-preservation-app";
    private string DataFolder => Path.GetDirectoryName(Path.GetFullPath(store.FilePath))!;
    private TextBlock syncStatus = new() { TextWrapping = TextWrapping.Wrap };
    private DevicePreferences device = new();
    private bool printing;
    private sealed class DevicePreferences
    {
        public string DeviceId { get; set; } = Guid.NewGuid().ToString("N");
        public string SyncFolder { get; set; } = "";
    }
    private static bool ValidMoney(double value) => double.IsFinite(value) && value >= 0 && value <= 1000000000;
    private Button AddAction(StackPanel panel, string title, Func<Task> action)
    {
        var button = new Button { Content = T(title), HorizontalAlignment = HorizontalAlignment.Left };
        button.Click += async (_, _) =>
        {
            button.IsEnabled = false;
            try { await action(); }
            catch (Exception ex) { Message(T("Action failed.") + " " + ex.Message, true); }
            finally { button.IsEnabled = true; }
        };
        panel.Children.Add(button);
        return button;
    }
    private void BuildReleaseControls()
    {
        var release = new StackPanel { Spacing = 12 };
        SettingsPanel.Children.Add(release);
        release.Children.Add(new TextBlock { Text = "Recovery and sync", FontSize = 22 });
        release.Children.Add(new TextBlock { Text = "The last 10 versions are kept automatically. Restore a snapshot to recover travel data or archive records.", TextWrapping = TextWrapping.Wrap });
        AddAction(release, "Recovery backups", ShowRecovery);
        release.Children.Add(syncStatus);
        AddAction(release, "Choose cloud folder", ChooseSyncFolder);
        AddAction(release, "Sync now", () => { PublishSyncSnapshot(true); return Task.CompletedTask; });
        AddAction(release, "Review incoming snapshots", ReviewSync);
        AddAction(release, "Disconnect sync", () => { device.SyncFolder = ""; SaveDevice(); UpdateSyncStatus(); return Task.CompletedTask; });
        release.Children.Add(new TextBlock { Text = "Browser and privacy", FontSize = 22, Margin = new Thickness(0, 12, 0, 0) });
        AddAction(release, "Clear browsing data", ClearBrowsingData);
        AddAction(release, "Privacy information", ShowPrivacy);
        AddAction(release, "About and support", ShowAbout);
        AddAction(TripsPanel, "Import shared trip", ImportSharedTrip);
        AddAction((StackPanel)WikiNotes.Child, "Open source", async () =>
        {
            if (wikiPlace is { SourceUrl.Length: > 0 }) await OpenWikiUri(new Uri(wikiPlace.SourceUrl));
            else Message(T("No source URL is recorded for this place."));
        });
        try
        {
            var preferences = Path.Combine(DataFolder, "device.json");
            if (File.Exists(preferences))
            {
                device = JsonSerializer.Deserialize<DevicePreferences>(File.ReadAllText(preferences)) ?? new();
                if (!Guid.TryParseExact(device.DeviceId, "N", out _) || device.SyncFolder is null) throw new InvalidDataException("Invalid device settings.");
            }
        }
        catch { device = new(); syncStatus.Text = "Device settings could not be read. Choose your cloud folder again."; }
        if (string.IsNullOrEmpty(syncStatus.Text)) UpdateSyncStatus();
    }

    private void SaveDevice() => RecoveryFiles.Write(Path.Combine(DataFolder, "device.json"), JsonSerializer.Serialize(device));
    private void UpdateSyncStatus() => syncStatus.Text = device.SyncFolder.Length == 0 ? T("Cloud sync is disconnected.") : T("Cloud folder") + ": " + device.SyncFolder;
    private async Task ChooseSyncFolder()
    {
        var picker = new FolderPicker(); picker.FileTypeFilter.Add("*");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
        var folder = await picker.PickSingleFolderAsync(); if (folder is null) return;
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = "Lorevia will write your trips, custom place notes and saved places to a Lorevia subfolder here. OneDrive or Dropbox must already sync this folder. Anyone with access can read these snapshots. Incoming changes are reviewed before replacing local plans. Offline downloads and archive records stay separate.", TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = folder.Path, TextWrapping = TextWrapping.Wrap });
        if (await ShowDialog(Dialog("Connect cloud folder?", panel, "Connect")) != ContentDialogResult.Primary) return;
        device.SyncFolder = Path.Combine(folder.Path, "Lorevia");
        SaveDevice(); UpdateSyncStatus(); PublishSyncSnapshot(true);
    }
    private void PublishSyncSnapshot(bool report = false)
    {
        if (!writable) { if (report) Message(T("Restore readable travel data before syncing."), true); return; }
        if (device.SyncFolder.Length == 0) { if (report) Message(T("Choose a cloud folder first."), true); return; }
        try
        {
            var target = Path.Combine(device.SyncFolder, device.DeviceId + ".lorevia-sync");
            RecoveryFiles.Write(target, TourismStore.Serialize(data));
            syncStatus.Text = T("Snapshot saved to cloud folder") + " · " + DateTime.Now.ToString("g");
            if (report) Message(T("Snapshot saved. Your cloud provider handles upload to other devices."));
        }
        catch (Exception ex) { syncStatus.Text = T("Local changes are saved; cloud sync needs retry.") + " " + ex.Message; }
    }
    private async Task ReviewSync()
    {
        if (!Directory.Exists(device.SyncFolder)) { Message(T("Choose an available cloud folder first."), true); return; }
        var files = Directory.GetFiles(device.SyncFolder, "*.lorevia-sync").Where(f => Path.GetFileNameWithoutExtension(f) != device.DeviceId).OrderByDescending(File.GetLastWriteTimeUtc).ToArray();
        if (files.Length == 0) { Message(T("No snapshots from other devices yet.")); return; }
        var chosen = await ChooseSnapshot(files, "Incoming snapshots");
        if (chosen is null) return;
        if (new FileInfo(chosen).Length > 25000000) throw new InvalidDataException("Snapshot is too large.");
        var incoming = TourismStore.Parse(await File.ReadAllTextAsync(chosen));
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = $"{incoming.Trips.Count} trips · {incoming.Saved.Count} saved places · {incoming.CustomDestinations.Count} custom places\nReplace this device’s travel data with this snapshot? A recovery copy is kept. This is a complete replacement, including deletions and preferences; it is not a merge.", TextWrapping = TextWrapping.Wrap });
        if (await ShowDialog(Dialog("Apply incoming snapshot?", panel, "Restore")) != ContentDialogResult.Primary) return;
        ApplyIncoming(incoming);
    }
    private void ApplyIncoming(TravelData incoming)
    {
        store.Save(incoming); data = incoming; writable = true;
        ApplyTheme(); InitializePreferences(); Refresh(); PublishSyncSnapshot();
        Message(T("Travel backup restored."));
    }
    private async Task<string?> ChooseSnapshot(string[] files, string title)
    {
        var list = new ListView { ItemsSource = files.Select(f => new SnapshotChoice(f)).ToArray(), DisplayMemberPath = "Label", MaxHeight = 300, SelectionMode = ListViewSelectionMode.Single };
        var panel = new StackPanel { Spacing = 12 }; panel.Children.Add(list);
        var dialog = Dialog(title, panel, "Review");
        dialog.IsPrimaryButtonEnabled = false;
        list.SelectionChanged += (_, _) => dialog.IsPrimaryButtonEnabled = list.SelectedItem is SnapshotChoice;
        return await ShowDialog(dialog) == ContentDialogResult.Primary ? (list.SelectedItem as SnapshotChoice)?.Path : null;
    }
    private sealed record SnapshotChoice(string Path)
    {
        public string Label => $"{File.GetLastWriteTime(Path):g} · {System.IO.Path.GetFileName(Path)}";
    }
    private async Task ShowRecovery()
    {
        var archivePath = new ArchiveStore().FilePath;
        var panel = new StackPanel { Spacing = 12 };
        var type = new ComboBox { Header = T("Recover"), ItemsSource = new[] { "Travel plans", "Heritage archive" }, SelectedIndex = 0 };
        panel.Children.Add(type);
        if (await ShowDialog(Dialog("Recovery backups", panel, "Review")) != ContentDialogResult.Primary) return;
        var path = type.SelectedIndex == 0 ? store.FilePath : archivePath;
        var files = RecoveryFiles.List(path);
        if (files.Length == 0) { Message(T("No recovery copies yet. Copies are created when existing data changes.")); return; }
        var selected = await ChooseSnapshot(files, "Recovery backups"); if (selected is null) return;
        var json = await File.ReadAllTextAsync(selected);
        // Validate before offering to replace anything, including damaged current files.
        if (type.SelectedIndex == 0) TourismStore.Parse(json); else ArchiveStore.Parse(json);
        var confirm = new StackPanel { Spacing = 12 };
        confirm.Children.Add(new TextBlock { Text = T("Restore this snapshot? The current file will be kept as another recovery copy."), TextWrapping = TextWrapping.Wrap });
        if (await ShowDialog(Dialog("Restore", confirm, "Restore")) != ContentDialogResult.Primary) return;
        if (type.SelectedIndex == 0) ApplyIncoming(TourismStore.Parse(json));
        else { archive?.Close(); archive = null; new ArchiveStore().Save(ArchiveStore.Parse(json)); Message(T("Archive restored.")); }
    }
    private async Task ImportSharedTrip()
    {
        var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".lorevia-trip");
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
        var file = await picker.PickSingleFileAsync(); if (file is null) return;
        if ((await file.GetBasicPropertiesAsync()).Size > 5000000) throw new InvalidDataException("Shared trip is too large.");
        var json = await Windows.Storage.FileIO.ReadTextAsync(file);
        var share = TravelExchange.Parse(json);
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = $"{share.Trip.Name}\n{share.Trip.Summary}\n{T("Import as a new trip? Existing trips and places will be preserved.")}", TextWrapping = TextWrapping.Wrap });
        if (await ShowDialog(Dialog("Import shared trip", panel, "Import")) != ContentDialogResult.Primary) return;
        Guid id = Guid.Empty;
        if (Change(next => id = TravelExchange.Import(next, json))) TripsList.SelectedItem = data.Trips.Single(t => t.Id == id);
    }

    private async Task ClearBrowsingData()
    {
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = T("Clear cookies, browsing history, cached pages and website sign-ins? Downloaded articles and travel plans are kept."), TextWrapping = TextWrapping.Wrap });
        if (await ShowDialog(Dialog("Clear browsing data", panel, "Clear")) != ContentDialogResult.Primary) return;
        await WikiBrowser.EnsureCoreWebView2Async();
        WikiBrowser.CoreWebView2.Stop();
        await WikiBrowser.CoreWebView2.Profile.ClearBrowsingDataAsync(CoreWebView2BrowsingDataKinds.AllProfile);
        Message(T("Browsing data cleared."));
    }
    private async Task ShowPrivacy()
    {
        var panel = new StackPanel { Spacing = 12, MaxWidth = 500 };
        panel.Children.Add(new TextBlock { Text = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "PRIVACY.md")), TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true });
        await ShowDialog(Dialog("Privacy information", panel, ""));
    }
    private async Task ShowAbout()
    {
        var panel = new StackPanel { Spacing = 14, MaxWidth = 500 };
        panel.Children.Add(new TextBlock { Text = "LOREVIA", FontSize = 28, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
        panel.Children.Add(new TextBlock { Text = "Version " + typeof(App).Assembly.GetName().Version + "\nDiscover places, preserve stories and plan your journey.", TextWrapping = TextWrapping.Wrap });
        var report = new TextBox { Header = T("Describe a problem or suggestion"), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 100, MaxLength = 4000 };
        panel.Children.Add(report);
        panel.Children.Add(new TextBlock { Text = T("Support opens the project issue tracker. Review your report before submitting; do not include private trip details."), TextWrapping = TextWrapping.Wrap });
        var dialog = Dialog("About and support", panel, "Report a problem");
        dialog.SecondaryButtonText = T("Check for updates");
        var result = await ShowDialog(dialog);
        if (result == ContentDialogResult.Primary)
            await OpenWikiUri(new Uri(ProjectUrl + "/issues/new?title=" + Uri.EscapeDataString("Lorevia feedback") + "&body=" + Uri.EscapeDataString("Version: " + typeof(App).Assembly.GetName().Version + "\n\n" + report.Text)));
        else if (result == ContentDialogResult.Secondary) await CheckUpdates();
    }
    private async Task CheckUpdates()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Lorevia/1.0");
        using var response = await http.GetAsync("https://api.github.com/repos/ZeroTrace0245/Digital-heritage-preservation-app/releases/latest");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) { Message(T("No public release has been published yet.")); return; }
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var tag = json.RootElement.GetProperty("tag_name").GetString() ?? "";
        if (!Version.TryParse(tag.TrimStart('v'), out var latest)) { await OpenWikiUri(new Uri(ProjectUrl + "/releases")); return; }
        if (latest <= typeof(App).Assembly.GetName().Version) { Message(T("You have the latest published version.")); return; }
        var panel = new StackPanel { Spacing = 12 }; panel.Children.Add(new TextBlock { Text = T("A newer version is available:") + " " + latest });
        if (await ShowDialog(Dialog("Check for updates", panel, "View release")) == ContentDialogResult.Primary) await OpenWikiUri(new Uri(ProjectUrl + "/releases/latest"));
    }

    private async Task PrintTrip(TravelTrip trip, bool pdf)
    {
        if (printing) return;
        printing = true;
        try
        {
            await PrintBrowser.EnsureCoreWebView2Async();
            var loaded = new TaskCompletionSource<bool>();
            void Completed(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args) => loaded.TrySetResult(args.IsSuccess);
            PrintBrowser.NavigationCompleted += Completed;
            try
            {
                PrintBrowser.NavigateToString(TravelExchange.Html(data, trip));
                if (!await loaded.Task.WaitAsync(TimeSpan.FromSeconds(20))) throw new IOException("Print preview could not load.");
            }
            finally { PrintBrowser.NavigationCompleted -= Completed; }
            if (!pdf) { PrintBrowser.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.System); return; }
            var picker = new FileSavePicker { SuggestedFileName = "lorevia-itinerary" }; picker.FileTypeChoices.Add("PDF itinerary", new[] { ".pdf" });
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSaveFileAsync(); if (file is null) return;
            if (!await PrintBrowser.CoreWebView2.PrintToPdfAsync(file.Path, null)) throw new IOException("PDF could not be saved.");
            Message(T("Export saved successfully."));
        }
        finally { printing = false; }
    }
}
