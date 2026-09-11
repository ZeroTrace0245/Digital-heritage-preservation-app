using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace digital_heritage_preservation_app;

public sealed partial class MainWindow : Window
{
    private readonly ArchiveStore store = new();
    private List<HeritageRecord> records = new();
    private string category = "All";
    private bool ready;
    private bool writable = true;
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
    }
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ready) return;
        try { records = store.Load(); }
        catch (Exception ex) { writable = false; Message("Archive could not be loaded. Your file is preserved. Restore a valid backup to continue. " + ex.Message, true); }
        ready = true;
        Refresh();
    }
    private void Message(string text, bool error = false) { Notice.Message = text; Notice.Severity = error ? InfoBarSeverity.Error : InfoBarSeverity.Success; Notice.IsOpen = true; }
    private void Navigate(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item) { category = item.Tag?.ToString() ?? "All"; if (ready) { Heading.Text = item.Content.ToString(); Refresh(); } }
    }
    private void FilterChanged(object sender, TextChangedEventArgs e) { if (ready) Refresh(); }
    private void Refresh(Guid? selected = null)
    {
        var query = Search.Text.Trim();
        var visible = records.Where(r => category == "All" || category == "Favorites" && r.IsFavorite || r.Kind == category)
            .Where(r => string.Join(" ", r.Title, r.Culture, r.Location, r.Tags, r.Description).Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Updated).ToList();
        Records.ItemsSource = visible;
        Records.SelectedItem = visible.FirstOrDefault(r => r.Id == selected);
        Empty.Visibility = visible.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        Stats.Text = $"{records.Count} preserved records  ·  {records.Count(r => r.IsFavorite)} favorites  ·  {records.Count(r => r.Kind == "Event" && r.EventDate >= DateTimeOffset.Now.Date)} upcoming events  ·  {visible.Count} shown";
        ShowDetails();
    }
    private void SelectRecord(object sender, SelectionChangedEventArgs e) { if (ready) ShowDetails(); }
    private void ShowDetails()
    {
        var record = Records.SelectedItem as HeritageRecord;
        DetailTitle.Text = record?.Title ?? "Select a record to explore its history.";
        DetailMeta.Text = record is null ? "" : $"{record.Summary}\nTags: {record.Tags}\nUpdated: {record.Updated.LocalDateTime:g}" + (record.EventDate is { } date ? $"\nEvent date: {date:d}" : "");
        DetailBody.Text = record?.Description ?? "";
        Actions.Visibility = record is null ? Visibility.Collapsed : Visibility.Visible;
        FavoriteButton.Content = record?.IsFavorite == true ? "Unfavorite" : "Favorite";
    }
    private bool Commit(List<HeritageRecord> next, Guid? selected = null, bool restore = false)
    {
        if (!writable && !restore) { Message("Restore a valid backup before changing the archive.", true); return false; }
        try { store.Save(next); records = next; writable = true; Refresh(selected); return true; }
        catch (Exception ex) { Message("Could not save: " + ex.Message, true); return false; }
    }
    private async void Add(object sender, RoutedEventArgs e) => await Editor(null);
    private async void Edit(object sender, RoutedEventArgs e) { if (Records.SelectedItem is HeritageRecord record) await Editor(record); }
    private async Task Editor(HeritageRecord? existing)
    {
        var kind = new ComboBox { Header = "Collection", ItemsSource = ArchiveStore.Kinds, SelectedItem = existing?.Kind ?? (ArchiveStore.Kinds.Contains(category) ? category : "Story"), HorizontalAlignment = HorizontalAlignment.Stretch };
        var title = new TextBox { Header = "Title / word *", Text = existing?.Title ?? "", MaxLength = 200 };
        var culture = new TextBox { Header = "Culture / language", Text = existing?.Culture ?? "" };
        var location = new TextBox { Header = "Location", Text = existing?.Location ?? "" };
        var tags = new TextBox { Header = "Tags (comma separated)", Text = existing?.Tags ?? "" };
        var body = new TextBox { Header = "Story, historical context, or translation *", Text = existing?.Description ?? "", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 150 };
        var date = new CalendarDatePicker { Header = "Event date (required for events)", Date = existing?.EventDate };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var panel = new StackPanel { Spacing = 12, Width = 440 };
        foreach (var element in new FrameworkElement[] { kind, title, culture, location, tags, body, date, error }) panel.Children.Add(element);
        var dialog = new ContentDialog { XamlRoot = Content.XamlRoot, Title = existing is null ? "Preserve a new record" : "Edit record", Content = new ScrollViewer { Content = panel, MaxHeight = 500 }, PrimaryButtonText = "Save", CloseButtonText = "Cancel" };
        dialog.PrimaryButtonClick += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(title.Text) || string.IsNullOrWhiteSpace(body.Text) || kind.SelectedItem?.ToString() == "Event" && date.Date is null)
            { error.Text = "Enter a title and description, and a date for events."; args.Cancel = true; return; }
            var record = new HeritageRecord { Id = existing?.Id ?? Guid.NewGuid(), Kind = kind.SelectedItem as string ?? "Story", Title = title.Text.Trim(), Culture = culture.Text.Trim(), Location = location.Text.Trim(), Tags = tags.Text.Trim(), Description = body.Text.Trim(), EventDate = kind.SelectedItem as string == "Event" ? date.Date : null, IsFavorite = existing?.IsFavorite ?? false };
            if (!Commit(records.Where(r => r.Id != record.Id).Append(record).ToList(), record.Id)) { error.Text = "Save failed. Check the archive status message."; args.Cancel = true; }
        };
        await dialog.ShowAsync();
    }
    private void Favorite(object sender, RoutedEventArgs e)
    {
        if (Records.SelectedItem is not HeritageRecord record) return;
        var copy = ArchiveStore.Parse(ArchiveStore.Serialize(records));
        copy.Single(r => r.Id == record.Id).IsFavorite = !record.IsFavorite;
        Commit(copy, record.Id);
    }
    private async void Delete(object sender, RoutedEventArgs e)
    {
        if (Records.SelectedItem is not HeritageRecord record) return;
        var dialog = new ContentDialog { XamlRoot = Content.XamlRoot, Title = "Delete record?", Content = $"Delete “{record.Title}”? This cannot be undone without a backup.", PrimaryButtonText = "Delete", CloseButtonText = "Cancel", DefaultButton = ContentDialogButton.Close };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) Commit(records.Where(r => r.Id != record.Id).ToList());
    }
    private async void Export(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!writable) { Message("Archive is unreadable. Export is disabled to avoid exporting an empty archive.", true); return; }
            var picker = new FileSavePicker { SuggestedFileName = $"heritage-backup-{DateTime.Now:yyyy-MM-dd}" };
            picker.FileTypeChoices.Add("JSON backup", new List<string> { ".json" });
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSaveFileAsync();
            if (file is null) return;
            await Windows.Storage.FileIO.WriteTextAsync(file, ArchiveStore.Serialize(records));
            Message($"Exported {records.Count} records.");
        }
        catch (Exception ex) { Message("Export failed: " + ex.Message, true); }
    }
    private async void Import(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker(); picker.FileTypeFilter.Add(".json");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            var file = await picker.PickSingleFileAsync(); if (file is null) return;
            var incoming = ArchiveStore.Parse(await Windows.Storage.FileIO.ReadTextAsync(file));
            var dialog = new ContentDialog { XamlRoot = Content.XamlRoot, Title = "Merge backup?", Content = $"Import {incoming.Count} records? Matching IDs will be replaced by the backup. Other records will remain.", PrimaryButtonText = "Import", CloseButtonText = "Cancel" };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
            if (File.Exists(store.FilePath)) File.Copy(store.FilePath, store.FilePath + $".{DateTime.UtcNow:yyyyMMddHHmmssfff}.bak");
            var ids = incoming.Select(r => r.Id).ToHashSet();
            if (Commit(records.Where(r => !ids.Contains(r.Id)).Concat(incoming).ToList(), restore: true)) Message($"Imported {incoming.Count} records. Previous archive backed up beside archive.json.");
        }
        catch (Exception ex) { Message("Import failed: " + ex.Message, true); }
    }
}

