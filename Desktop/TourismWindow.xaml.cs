using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow : Window
{
    private readonly TourismStore store = new(Environment.GetEnvironmentVariable("SERENDIB_TRAVEL_DATA_PATH"));
    private TravelData data = new();
    private bool ready;
    private bool writable = true;
    private string page = "Discover";
    private MainWindow? archive;
    public TourismWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBar);
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1320, 900));
    }
    private void LoadedWindow(object sender, RoutedEventArgs e)
    {
        if (ready) return;
        try { data = store.Load(); }
        catch (Exception ex) { writable = false; Message("Your travel file could not be read. Restore a backup in Settings to continue. The original file is preserved. " + ex.Message, true); }
        ApplyTheme();
        InitializePreferences();
        ready = true;
        Refresh();
    }
    private void Message(string message, bool error = false)
    {
        Notice.Message = T(message);
        Notice.Severity = error ? InfoBarSeverity.Error : InfoBarSeverity.Success;
        Notice.IsOpen = true;
    }
    private bool Change(Action<TravelData> change)
    {
        if (!writable) { Message("Restore a valid travel backup in Settings before making changes.", true); return false; }
        try
        {
            var next = TourismStore.Parse(TourismStore.Serialize(data));
            change(next);
            store.Save(next);
            data = next;
            Refresh();
            return true;
        }
        catch (Exception ex) { Message("Could not save your changes: " + ex.Message, true); return false; }
    }
    private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (!ready) return;
        if (args.IsSettingsSelected) page = "Settings";
        else if (args.SelectedItem is NavigationViewItem item)
        {
            if (item.Tag?.ToString() == "Archive")
            {
                if (archive is null) { archive = new MainWindow(); archive.Closed += (_, _) => archive = null; }
                archive.Activate();
                return;
            }
            page = item.Tag?.ToString() ?? "Discover";
        }
        Refresh();
    }
    private void Refresh()
    {
        if (!ready) return;
        ExplorePanel.Visibility = page is "Discover" or "Saved" ? Visibility.Visible : Visibility.Collapsed;
        TripsPanel.Visibility = page == "Trips" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPanel.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        WikiPanel.Visibility = page == "Wiki" ? Visibility.Visible : Visibility.Collapsed;
        RefreshWiki();
        Hero.Visibility = page == "Discover" ? Visibility.Visible : Visibility.Collapsed;
        PageTitle.Text = page == "Wiki" ? "Places wiki" : page == "Saved" ? "Saved places" : page == "Trips" ? "My trips" : page;
        PageSubtitle.Text = page switch { "Saved" => "Keep a little inspiration for later.", "Trips" => "Less organizing. More exploring.", "Settings" => "A travel companion that feels like yours.", _ => "Small island. Endless possibilities." };
        SectionTitle.Text = page == "Saved" ? "Your personal shortlist" : "Find your kind of escape";
        var query = Search.Text.Trim();
        var category = (Category.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "All experiences";
        var places = Destinations.Filter(data, data.Location, query, category, page == "Saved");
        Places.ItemsSource = places.Select(d => new DestinationCard(d, data.Language)).ToArray();
        ResultCount.Text = data.Language switch { "ja" => $"{places.Length} 件", "ko" => $"{places.Length}개 장소", "ru" => $"Мест: {places.Length}", _ => $"{places.Length} places" };
        EmptyPlaces.Visibility = places.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        var selected = (TripsList.SelectedItem as TravelTrip)?.Id;
        TripsList.ItemsSource = data.Trips;
        TripsList.SelectedItem = data.Trips.FirstOrDefault(t => t.Id == selected) ?? data.Trips.FirstOrDefault();
        EmptyTrips.Visibility = data.Trips.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        RenderStops();
        RefreshLocation();
        Localize(Root);
    }
    private void SearchChanged(object sender, TextChangedEventArgs e) => Refresh();
    private void CategoryChanged(object sender, SelectionChangedEventArgs e) => Refresh();
    private async void OpenPlace(object sender, ItemClickEventArgs e) { if (e.ClickedItem is DestinationCard card) await ShowDestination(card.Destination); }
    private async void ExploreHill(object sender, RoutedEventArgs e)
    {
        var featured = Destinations.Filter(data, data.Location, "", "All experiences", false).FirstOrDefault();
        if (featured is not null) await ShowDestination(featured);
    }
    private async Task ShowDestination(Destination destination)
    {
        var panel = new StackPanel { Spacing = 14, MaxWidth = 480 };
        if (!string.IsNullOrEmpty(destination.Image)) panel.Children.Add(new Image { Source = new BitmapImage(new Uri(destination.Image)), Height = 200, Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill });
        panel.Children.Add(new TextBlock { Text = destination.Subtitle, Opacity = 0.65 });
        panel.Children.Add(new TextBlock { Text = destination.Description, TextWrapping = TextWrapping.Wrap });
        var saved = new Button { Content = data.Saved.Contains(destination.Id) ? "♥ Saved to your places" : "♡ Save this place" };
        saved.Click += (_, _) =>
        {
            if (Change(next => { if (!next.Saved.Remove(destination.Id)) next.Saved.Add(destination.Id); })) saved.Content = data.Saved.Contains(destination.Id) ? "♥ Saved to your places" : "♡ Save this place";
        };
        panel.Children.Add(saved);
        var map = new HyperlinkButton { Content = "Open location in maps ↗", NavigateUri = new Uri("https://www.bing.com/maps?q=" + Uri.EscapeDataString(destination.Name + " " + destination.Country)) };
        panel.Children.Add(map);
        var customize = new Button { Content = T("Customize place") };
        panel.Children.Add(customize);
        var trip = new ComboBox { Header = "Add to a trip", ItemsSource = data.Trips, DisplayMemberPath = "Name", HorizontalAlignment = HorizontalAlignment.Stretch, SelectedIndex = data.Trips.Count > 0 ? 0 : -1 };
        var day = new NumberBox { Header = "Day", Minimum = 1, Maximum = 60, Value = 1, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        var notes = new TextBox { Header = "Notes (optional)", PlaceholderText = "A café to try, a trail to walk…", MaxLength = 2000 };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        if (data.Trips.Count == 0) panel.Children.Add(new TextBlock { Text = "Create a trip using ‘Plan a trip’ to start an itinerary.", TextWrapping = TextWrapping.Wrap });
        else { panel.Children.Add(trip); panel.Children.Add(day); panel.Children.Add(notes); }
        panel.Children.Add(error);
        var dialog = Dialog(destination.Name, panel, "Add to itinerary");
        var editRequested = false;
        customize.Click += (_, _) => { editRequested = true; dialog.Hide(); };
        dialog.IsPrimaryButtonEnabled = data.Trips.Count > 0 && writable;
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (trip.SelectedItem is not TravelTrip chosen || !ValidDay(day.Value, chosen.Days)) { error.Text = T("Choose a trip and a whole day within its duration."); args.Cancel = true; return; }
            if (!Change(next => next.Trips.Single(t => t.Id == chosen.Id).Stops.Add(new TripStop { DestinationId = destination.Id, Day = (int)day.Value, Notes = notes.Text.Trim() }))) args.Cancel = true;
            else Message($"{destination.Name} added to {chosen.Name}, day {(int)day.Value}.");
        };
        await ShowDialog(dialog);
        if (editRequested) await PlaceEditor(destination);
    }
    private ContentDialog Dialog(string title, StackPanel content, string primary) => new()
    {
        XamlRoot = Root.XamlRoot, RequestedTheme = Root.RequestedTheme, Title = T(title),
        Content = new ScrollViewer { Content = content, MaxHeight = 520 },
        PrimaryButtonText = T(primary), CloseButtonText = T("Close"), DefaultButton = ContentDialogButton.Primary
    };
    private static bool ValidDay(double value, int max) => double.IsFinite(value) && value >= 1 && value <= max && value == Math.Truncate(value);
    private async void CreateTrip(object sender, RoutedEventArgs e) => await TripEditor(null);
    private async void EditTrip(object sender, RoutedEventArgs e) { if (TripsList.SelectedItem is TravelTrip trip) await TripEditor(trip); }
    private async Task TripEditor(TravelTrip? existing)
    {
        var name = new TextBox { Header = "Trip name", Text = existing?.Name ?? "", PlaceholderText = "My next adventure", MaxLength = 120 };
        var start = new CalendarDatePicker { Header = "Start date", Date = new DateTimeOffset(existing?.Start ?? DateTime.Today) };
        var days = new NumberBox { Header = "Number of days (1–60)", Value = existing?.Days ?? 5, Minimum = 1, Maximum = 60, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var panel = new StackPanel { Spacing = 14, Width = 400 };
        panel.Children.Add(name); panel.Children.Add(start); panel.Children.Add(days); panel.Children.Add(error);
        var dialog = Dialog(existing is null ? "Your next chapter" : "Edit trip", panel, "Save trip");
        Guid tripId = existing?.Id ?? Guid.NewGuid();
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) || start.Date is null || !ValidDay(days.Value, 60)) { error.Text = T("Enter a name, start date and a whole number of days from 1 to 60."); args.Cancel = true; return; }
            if (existing?.Stops.Any(s => s.Day > days.Value) == true) { error.Text = T("Move or remove later stops before shortening this trip."); args.Cancel = true; return; }
            if (!Change(next =>
            {
                var trip = next.Trips.FirstOrDefault(t => t.Id == tripId);
                if (trip is null) { trip = new TravelTrip { Id = tripId }; next.Trips.Add(trip); }
                trip.Name = name.Text.Trim(); trip.Start = start.Date.Value.Date; trip.Days = (int)days.Value;
            })) args.Cancel = true;
        };
        if (await ShowDialog(dialog) == ContentDialogResult.Primary)
        {
            Nav.SelectedItem = Nav.MenuItems.OfType<NavigationViewItem>().Single(i => i.Tag?.ToString() == "Trips");
            TripsList.SelectedItem = data.Trips.Single(t => t.Id == tripId);
        }
    }
    private void SelectTrip(object sender, SelectionChangedEventArgs e) { if (ready) RenderStops(); }
    private void RenderStops()
    {
        Stops.Children.Clear();
        if (TripsList.SelectedItem is not TravelTrip trip) { ItineraryPanel.Visibility = Visibility.Collapsed; return; }
        ItineraryPanel.Visibility = Visibility.Visible;
        TripTitle.Text = "Your itinerary";
        for (var day = 1; day <= trip.Days; day++)
        {
            Stops.Children.Add(new TextBlock { Text = $"DAY {day:00}  ·  {trip.Start.AddDays(day - 1):ddd, dd MMM}", FontSize = 12, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 16, 0, 4) });
            var entries = trip.Stops.Where(s => s.Day == day).ToList();
            if (entries.Count == 0) Stops.Children.Add(new TextBlock { Text = T("An open day. Add a destination or leave room to wander."), Opacity = 0.55, TextWrapping = TextWrapping.Wrap });
            foreach (var stop in entries)
            {
                var destination = Catalog.Single(d => d.Id == stop.DestinationId);
                var content = new StackPanel { Spacing = 5 };
                content.Children.Add(new TextBlock { Text = destination.Name + "  ↗", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                content.Children.Add(new TextBlock { Text = string.IsNullOrWhiteSpace(stop.Notes) ? destination.Subtitle : stop.Notes, TextWrapping = TextWrapping.Wrap, Opacity = 0.65 });
                var button = new Button { Content = content, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(16) };
                button.Click += async (_, _) => await EditStop(trip.Id, stop);
                Stops.Children.Add(button);
            }
        }
    }
    private async Task EditStop(Guid tripId, TripStop stop)
    {
        var trip = data.Trips.Single(t => t.Id == tripId);
        var day = new NumberBox { Header = "Day", Value = stop.Day, Minimum = 1, Maximum = trip.Days, SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline };
        var notes = new TextBox { Header = "Notes", Text = stop.Notes, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, MaxLength = 2000, Height = 120 };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var panel = new StackPanel { Spacing = 14, Width = 400 }; panel.Children.Add(day); panel.Children.Add(notes); panel.Children.Add(error);
        var dialog = Dialog(Catalog.Single(d => d.Id == stop.DestinationId).Name, panel, "Save stop");
        dialog.SecondaryButtonText = "Remove stop";
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (!ValidDay(day.Value, trip.Days)) { error.Text = T("Enter a whole day within this trip."); args.Cancel = true; return; }
            if (!Change(next => { var item = next.Trips.Single(t => t.Id == tripId).Stops.Single(s => s.Id == stop.Id); item.Day = (int)day.Value; item.Notes = notes.Text.Trim(); })) args.Cancel = true;
        };
        dialog.SecondaryButtonClick += (_, args) => { if (!Change(next => next.Trips.Single(t => t.Id == tripId).Stops.RemoveAll(s => s.Id == stop.Id))) args.Cancel = true; };
        await ShowDialog(dialog);
    }
    private async void DeleteTrip(object sender, RoutedEventArgs e)
    {
        if (TripsList.SelectedItem is not TravelTrip trip) return;
        var dialog = new ContentDialog { XamlRoot = Root.XamlRoot, RequestedTheme = Root.RequestedTheme, Title = "Delete this trip?", Content = $"“{trip.Name}” and all its stops will be removed.", PrimaryButtonText = "Delete", CloseButtonText = "Cancel", DefaultButton = ContentDialogButton.Close };
        if (await ShowDialog(dialog) == ContentDialogResult.Primary) Change(next => next.Trips.RemoveAll(t => t.Id == trip.Id));
    }
    private void ApplyTheme()
    {
        Root.RequestedTheme = Enum.Parse<ElementTheme>(data.Theme);
        ThemeChoice.SelectedIndex = data.Theme == "Light" ? 1 : data.Theme == "Dark" ? 2 : 0;
    }
    private void ThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ready) return;
        var theme = ThemeChoice.SelectedIndex == 1 ? "Light" : ThemeChoice.SelectedIndex == 2 ? "Dark" : "Default";
        if (theme == data.Theme) return;
        Change(next => next.Theme = theme);
        ApplyTheme();
    }
    private async Task SaveFile(string name, string extension, string content)
    {
        try
        {
            var picker = new FileSavePicker { SuggestedFileName = name };
            picker.FileTypeChoices.Add(extension == ".json" ? "Travel backup" : "Text itinerary", new[] { extension });
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSaveFileAsync();
            if (file is null) return;
            await Windows.Storage.FileIO.WriteTextAsync(file, content);
            Message("Export saved successfully.");
        }
        catch (Exception ex) { Message("Export failed: " + ex.Message, true); }
    }
    private async void ExportTrip(object sender, RoutedEventArgs e)
    {
        if (TripsList.SelectedItem is not TravelTrip trip) return;
        var text = new StringBuilder().AppendLine(trip.Name).AppendLine(trip.Summary).AppendLine();
        for (var day = 1; day <= trip.Days; day++)
        {
            text.AppendLine($"Day {day} — {trip.Start.AddDays(day - 1):dd MMM yyyy}");
            foreach (var stop in trip.Stops.Where(s => s.Day == day)) text.AppendLine("  " + Catalog.Single(d => d.Id == stop.DestinationId).Name).AppendLine("  " + stop.Notes);
            text.AppendLine();
        }
        await SaveFile("serendib-itinerary", ".txt", text.ToString());
    }
    private async void ExportBackup(object sender, RoutedEventArgs e)
    {
        if (!writable) { Message("Restore readable travel data before exporting a backup.", true); return; }
        await SaveFile("serendib-backup", ".json", TourismStore.Serialize(data));
    }
    private async void ImportBackup(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".json");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSingleFileAsync(); if (file is null) return;
            var incoming = TourismStore.Parse(await Windows.Storage.FileIO.ReadTextAsync(file));
            var dialog = new ContentDialog { XamlRoot = Root.XamlRoot, RequestedTheme = Root.RequestedTheme, Title = "Restore travel backup?", Content = $"Replace your travel plans with {incoming.Trips.Count} trips and {incoming.Saved.Count} saved places? A safety copy of the existing file will be kept.", PrimaryButtonText = "Restore", CloseButtonText = "Cancel", DefaultButton = ContentDialogButton.Close };
            if (await ShowDialog(dialog) != ContentDialogResult.Primary) return;
            if (File.Exists(store.FilePath)) File.Copy(store.FilePath, store.FilePath + $".{DateTime.UtcNow:yyyyMMddHHmmssfff}.bak");
            store.Save(incoming); data = incoming; writable = true; ApplyTheme(); InitializePreferences(); Refresh(); Message("Travel backup restored.");
        }
        catch (Exception ex) { Message("Restore failed: " + ex.Message, true); }
    }
}


