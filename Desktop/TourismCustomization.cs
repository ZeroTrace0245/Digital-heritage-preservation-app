using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow
{
    private bool settingPreferences;
    private Destination[] Catalog => Destinations.Catalog(data);
    private string T(string key) => TravelText.Get(key, data.Language);
    private sealed record DestinationCard(Destination Destination, string Language)
    {
        public string Name => Destination.Name;
        public string Region => Destination.Region;
        public string Image => Destination.Image;
        public string Highlight => Destination.Highlight;
        public string Category => TravelText.Get(Destination.Category, Language);
        public string ExploreLabel => TravelText.Get("Explore destination  ↗", Language);
        public override string ToString() => Destination.ToString();
    }

    private void InitializePreferences()
    {
        settingPreferences = true;
        LanguageChoice.SelectedItem = LanguageChoice.Items.OfType<ComboBoxItem>().Single(i => i.Tag?.ToString() == data.Language);
        HomeChoice.ItemsSource = Destinations.Countries.Concat(Catalog.Select(d => d.Country)).Append(data.HomeCountry).Distinct().ToArray();
        HomeChoice.Text = data.HomeCountry;
        settingPreferences = false;
    }

    private void RefreshLocation()
    {
        LocationTabs.Children.Clear();
        var countries = new[] { "Local", "All" }.Concat(Destinations.Countries).Concat(Catalog.Select(d => d.Country)).Append(data.Location).Distinct();
        foreach (var location in countries)
        {
            var button = new ToggleButton { Content = T(location), IsChecked = data.Location == location, Padding = new Thickness(18, 9, 18, 9), MinWidth = 86 };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(button, T(location));
            ToolTipService.SetToolTip(button, location == "Local" ? T(data.HomeCountry) : T(location));
            button.Click += (_, _) =>
            {
                if (!Change(next => next.Location = location)) RefreshLocation();
            };
            LocationTabs.Children.Add(button);
        }
        var featured = Destinations.Filter(data, data.Location, "", "All experiences", false).FirstOrDefault();
        HeroImage.Source = featured is null || string.IsNullOrEmpty(featured.Image) ? null : new BitmapImage(new Uri(featured.Image));
        HeroTitle.Text = T("A world of wonder.") + "\n" + T(data.Location == "Local" ? data.HomeCountry : data.Location);
        HeroDescription.Text = T("Choose a country. Find your next chapter.");
        HeroButton.IsEnabled = featured is not null;
        PageSubtitle.Text = page switch
        {
            "Discover" => T(data.Location == "Local" ? data.HomeCountry : data.Location) + " · " + T("Choose a country. Find your next chapter."),
            "Saved" => T("Keep a little inspiration for later."),
            "Trips" => T("Less organizing. More exploring."),
            "Wiki" => T("Explore places, history and culture without leaving the app."),
            _ => T("A travel companion that feels like yours.")
        };
    }

    private void LanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ready || settingPreferences || LanguageChoice.SelectedItem is not ComboBoxItem selected) return;
        var language = selected.Tag?.ToString() ?? "en";
        if (language != data.Language) Change(next => next.Language = language);
        InitializePreferences();
    }

    private void SaveHome(object sender, RoutedEventArgs e)
    {
        var country = HomeChoice.Text.Trim();
        if (country.Length is 0 or > 80) { Message("Enter a country name (up to 80 characters).", true); return; }
        country = Destinations.Countries.Concat(Catalog.Select(d => d.Country)).FirstOrDefault(c => c.Equals(country, StringComparison.OrdinalIgnoreCase)) ?? country;
        if (Change(next => next.HomeCountry = country)) { InitializePreferences(); Message("Home country saved."); }
    }

    // Translate UI labels only; never modify destination content or user-entered text.
    private void Localize(DependencyObject node)
    {
        if (node == WikiPlaces || node == WikiNotes || node == WikiBrowser || node == Places || node == TripsList || node == Stops || node == HomeChoice || node == LanguageChoice || node == HeroTitle || node == HeroDescription) return;
        string Translate(string value) => TravelText.TranslateDisplayed(value, data.Language);
        if (node is TextBlock text && text.GetBindingExpression(TextBlock.TextProperty) is null) text.Text = Translate(text.Text);
        if (node is ContentControl control && control.Content is string content) control.Content = Translate(content);
        if (node is TextBox input)
        {
            if (input.Header is string header) input.Header = Translate(header);
            input.PlaceholderText = Translate(input.PlaceholderText);
            return;
        }
        if (node is ComboBox combo)
        {
            if (combo.Header is string header) combo.Header = Translate(header);
            var changed = false;
            foreach (var item in combo.Items.OfType<ComboBoxItem>())
                if (item.Content is string value && value != Translate(value)) { item.Content = Translate(value); changed = true; }
            // WinUI caches the selected item's string in its selection presenter.
            if (changed && combo.SelectedIndex >= 0)
            {
                var index = combo.SelectedIndex;
                var wasReady = ready;
                ready = false;
                try { combo.SelectedIndex = -1; combo.SelectedIndex = index; }
                finally { ready = wasReady; }
            }
            return;
        }
        if (node is NumberBox number) { if (number.Header is string header) number.Header = Translate(header); return; }
        if (node is CalendarDatePicker calendar) { if (calendar.Header is string header) calendar.Header = Translate(header); return; }
        if (node is Panel panel) { foreach (var child in panel.Children) Localize(child); return; }
        if (node is Border border && border.Child is { } borderChild) { Localize(borderChild); return; }
        if (node is NavigationView navigation)
        {
            foreach (var item in navigation.MenuItems.OfType<DependencyObject>()) Localize(item);
            if (navigation.PaneHeader is DependencyObject paneHeader) Localize(paneHeader);
            if (navigation.PaneFooter is DependencyObject paneFooter) Localize(paneFooter);
            if (navigation.SettingsItem is NavigationViewItem settings)
            {
                settings.Content = T("Settings");
                Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(settings, T("Settings"));
            }
        }
        if (node is ContentControl container && container.Content is DependencyObject childContent) { Localize(childContent); return; }
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) Localize(VisualTreeHelper.GetChild(node, i));
    }

    private async Task<ContentDialogResult> ShowDialog(ContentDialog dialog)
    {
        if (dialog.Title is string title) dialog.Title = TravelText.TranslateDisplayed(title, data.Language);
        dialog.PrimaryButtonText = TravelText.TranslateDisplayed(dialog.PrimaryButtonText, data.Language);
        dialog.SecondaryButtonText = TravelText.TranslateDisplayed(dialog.SecondaryButtonText, data.Language);
        dialog.CloseButtonText = TravelText.TranslateDisplayed(dialog.CloseButtonText, data.Language);
        if (dialog.Content is DependencyObject content) Localize(content);
        return await dialog.ShowAsync();
    }

    private async void AddPlace(object sender, RoutedEventArgs e) => await PlaceEditor(null);
    private async Task PlaceEditor(Destination? existing)
    {
        var name = new TextBox { Header = T("Place name"), Text = existing?.Name ?? "", MaxLength = 120 };
        var country = new ComboBox { Header = T("Country"), IsEditable = true, ItemsSource = Destinations.Countries.Concat(Catalog.Select(d => d.Country)).Distinct().ToArray(), Text = existing?.Country ?? (data.Location is "Local" or "All" ? data.HomeCountry : data.Location), HorizontalAlignment = HorizontalAlignment.Stretch };
        var region = new TextBox { Header = T("Region / city"), Text = existing?.Region ?? "", MaxLength = 120 };
        var category = new ComboBox { Header = T("Experience"), HorizontalAlignment = HorizontalAlignment.Stretch };
        foreach (var kind in Destinations.Categories) category.Items.Add(new ComboBoxItem { Content = T(kind), Tag = kind });
        category.SelectedIndex = Array.IndexOf(Destinations.Categories, existing?.Category ?? "Culture");
        var description = new TextBox { Header = T("Description"), Text = existing?.Description ?? "", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 120, MaxLength = 4000 };
        var highlight = new TextBox { Header = T("Short highlight"), Text = existing?.Highlight ?? "", MaxLength = 160 };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var panel = new StackPanel { Width = 420, Spacing = 12 };
        foreach (var item in new FrameworkElement[] { name, country, region, category, description, highlight, error }) panel.Children.Add(item);
        var dialog = Dialog(existing is null ? "Add your own place" : "Customize place", panel, "Save place");
        dialog.PrimaryButtonClick += (_, args) =>
        {
            var countryName = country.Text.Trim();
            if (string.IsNullOrWhiteSpace(name.Text) || countryName.Length is 0 or > 80 || string.IsNullOrWhiteSpace(description.Text)) { error.Text = T("Enter a place name, country and description."); args.Cancel = true; return; }
            countryName = Destinations.Countries.Concat(Catalog.Select(d => d.Country)).FirstOrDefault(c => c.Equals(countryName, StringComparison.OrdinalIgnoreCase)) ?? countryName;
            var destination = new Destination(existing?.Id ?? "custom-" + Guid.NewGuid(), name.Text.Trim(), region.Text.Trim(), (category.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Culture", description.Text.Trim(), highlight.Text.Trim(), existing?.Image ?? "") { Country = countryName };
            if (!Change(next => { next.CustomDestinations.RemoveAll(d => d.Id == destination.Id); next.CustomDestinations.Add(destination); next.Location = countryName; })) args.Cancel = true;
            else InitializePreferences();
        };
        if (existing is not null && data.CustomDestinations.Any(d => d.Id == existing.Id))
        {
            var builtIn = Destinations.All.Any(d => d.Id == existing.Id);
            dialog.SecondaryButtonText = T(builtIn ? "Reset to original" : "Delete");
            dialog.SecondaryButtonClick += (_, args) =>
            {
                if (!builtIn && data.Trips.Any(t => t.Stops.Any(s => s.DestinationId == existing.Id)))
                {
                    error.Text = data.Language switch { "ja" => "削除する前に、この場所を旅程から削除してください。", "ko" => "삭제하기 전에 여행 일정에서 이 장소를 제거하세요.", "ru" => "Сначала удалите это место из маршрутов.", _ => "Remove this place from your trip itineraries before deleting it." };
                    args.Cancel = true; return;
                }
                if (!Change(next => { next.CustomDestinations.RemoveAll(d => d.Id == existing.Id); if (!builtIn) next.Saved.Remove(existing.Id); })) args.Cancel = true;
                else InitializePreferences();
            };
        }
        await ShowDialog(dialog);
    }

    private async void ShowCredits(object sender, RoutedEventArgs e)
    {
        try
        {
            var text = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Assets", "Travel", "CREDITS.md"));
            var panel = new StackPanel { MaxWidth = 480 };
            panel.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, IsTextSelectionEnabled = true });
            var dialog = Dialog("Photo credits", panel, "");
            await ShowDialog(dialog);
        }
        catch (Exception ex) { Message(ex.Message, true); }
    }
}
