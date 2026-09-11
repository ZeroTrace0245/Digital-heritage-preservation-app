using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow
{
    private OfflineLibrary Library => new(Path.Combine(DataFolder, "offline-articles.json"));
    private void OfflineSearchChanged(object sender, TextChangedEventArgs e) { if (ready) RefreshOffline(); }
    private void RefreshOffline()
    {
        try
        {
            var selected = (OfflineArticles.SelectedItem as OfflineArticle)?.Url;
            var all = Library.Load();
            var matches = all.Where(a => (a.Title + " " + a.Text).Contains(OfflineSearch.Text.Trim(), StringComparison.OrdinalIgnoreCase)).ToArray();
            OfflineArticles.ItemsSource = matches;
            OfflineArticles.SelectedItem = matches.FirstOrDefault(a => a.Url == selected);
            OfflineStatus.Text = matches.Length == 0 ? T("No downloaded articles match. Open a Wikipedia article and choose Download article.") : $"{matches.Length} / {all.Count} · {T("Downloaded articles")}";
        }
        catch (Exception ex) { OfflineStatus.Text = T("Offline library could not be read.") + " " + ex.Message; }
    }
    private void SelectOfflineArticle(object sender, SelectionChangedEventArgs e)
    {
        var article = OfflineArticles.SelectedItem as OfflineArticle;
        OfflineActions.Visibility = article is null ? Visibility.Collapsed : Visibility.Visible;
        OfflineTitle.Text = article?.Title ?? "";
        OfflineSource.Text = article is null ? "" : $"{article.Url}\n{T("Downloaded")}: {article.DownloadedAt.LocalDateTime:g}\nWikipedia contributors · CC BY-SA 4.0 · https://creativecommons.org/licenses/by-sa/4.0/\n{T("Text-only snapshot; images and interactive content are not included.")}";
        OfflineBody.Text = article?.Text ?? "";
    }
    private async void OpenOfflineSource(object sender, RoutedEventArgs e)
    {
        if (OfflineArticles.SelectedItem is OfflineArticle article) await OpenWikiUri(new Uri(article.Url));
    }
    private async void DeleteOfflineArticle(object sender, RoutedEventArgs e)
    {
        if (OfflineArticles.SelectedItem is not OfflineArticle article) return;
        try
        {
            var panel = new StackPanel { Spacing = 12 }; panel.Children.Add(new TextBlock { Text = article.Title, TextWrapping = TextWrapping.Wrap });
            if (await ShowDialog(Dialog("Delete download", panel, "Delete")) != ContentDialogResult.Primary) return;
            var articles = Library.Load(); articles.RemoveAll(a => a.Url == article.Url); Library.Save(articles); RefreshOffline();
        }
        catch (Exception ex) { Message(T("Action failed.") + " " + ex.Message, true); }
    }
    private async void DownloadArticle(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender; button.IsEnabled = false;
        try
        {
            if (WikiLoading.Visibility == Visibility.Visible || WikiBrowser.CoreWebView2 is null) { WikiFailure(T("Wait for the article to finish loading.")); return; }
            // Capture URL, title and article text together, so a navigation cannot mix sources.
            var script = "(() => { const u = new URL(location.href); if (u.protocol !== 'https:' || !u.hostname.endsWith('.wikipedia.org') || !u.pathname.startsWith('/wiki/')) return null; const article = document.querySelector('#mw-content-text'); const heading = document.querySelector('#firstHeading'); if (!article || !heading) return null; return {Title:heading.innerText, Url:u.origin + u.pathname, Text:article.innerText}; })()";
            var result = await WikiBrowser.CoreWebView2.ExecuteScriptAsync(script);
            using var json = JsonDocument.Parse(result);
            if (json.RootElement.ValueKind == JsonValueKind.Null) { WikiFailure(T("Open a Wikipedia article to download its text.")); return; }
            var root = json.RootElement;
            var article = new OfflineArticle(root.GetProperty("Title").GetString()!, root.GetProperty("Url").GetString()!, root.GetProperty("Text").GetString()!, DateTimeOffset.UtcNow);
            var articles = Library.Load(); articles.RemoveAll(a => a.Url == article.Url); articles.Add(article); Library.Save(articles);
            RefreshOffline(); WikiError.Severity = InfoBarSeverity.Success; WikiError.Message = T("Article saved to Offline library."); WikiError.IsOpen = true;
        }
        catch (Exception ex) { WikiFailure(T("Download failed.") + " " + ex.Message); }
        finally { button.IsEnabled = true; }
    }
}
