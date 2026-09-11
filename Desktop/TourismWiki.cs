using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow
{
    private Destination? wikiPlace;
    private Uri? wikiRequestedUri;
    private bool wikiInitialized;
    private bool wikiOpening;

    private void RefreshWiki()
    {
        if (!ready) return;
        var places = Destinations.Filter(data, data.Location, WikiSearch.Text, "All experiences", false);
        WikiPlaces.ItemsSource = places;
        WikiCount.Text = places.Length == 0
            ? "No offline notes match this search in the selected country. Try All above, or search Wikipedia worldwide."
            : $"{places.Length} offline destination notes · select a place to learn more";
        if (wikiPlace is not null)
        {
            wikiPlace = places.FirstOrDefault(d => d.Id == wikiPlace.Id);
            if (wikiPlace is null) WikiNotes.Visibility = Visibility.Collapsed;
            else RenderWikiNotes();
        }
    }

    private void WikiSearchChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => RefreshWiki();
    private async void WikiQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) => await SearchWiki(sender.Text);
    private async void SearchWikipedia(object sender, RoutedEventArgs e) => await SearchWiki(WikiSearch.Text);

    private async Task SearchWiki(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            WikiCount.Text = "Enter a place or landmark to search Wikipedia.";
            WikiSearch.Focus(FocusState.Programmatic);
            return;
        }
        await OpenWikiUri(new Uri($"https://{data.Language}.wikipedia.org/w/index.php?search={Uri.EscapeDataString(query.Trim())}"));
    }

    private void OpenWikiPlace(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Destination destination) return;
        wikiPlace = destination;
        CloseWikiReader(sender, new RoutedEventArgs());
        RenderWikiNotes();
    }

    private void RenderWikiNotes()
    {
        if (wikiPlace is null) return;
        WikiNotes.Visibility = Visibility.Visible;
        WikiPlaceTitle.Text = wikiPlace.Name;
        WikiPlaceFacts.Text = wikiPlace.Subtitle;
        WikiPlaceDescription.Text = wikiPlace.Description;
    }

    private async void ReadWikiPlace(object sender, RoutedEventArgs e)
    {
        if (wikiPlace is not null) await SearchWiki(wikiPlace.Name);
    }

    private async void PlanWikiPlace(object sender, RoutedEventArgs e)
    {
        if (wikiPlace is not null) await ShowDestination(wikiPlace);
    }

    private static bool IsWikiUri(Uri uri) => uri.Scheme == "https" &&
        (uri.Host.Equals("wikipedia.org", StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith(".wikipedia.org", StringComparison.OrdinalIgnoreCase));

    private async Task OpenWikiUri(Uri uri)
    {
        wikiRequestedUri = uri;
        WikiReaderPanel.Visibility = Visibility.Visible;
        WikiError.IsOpen = false;
        WikiLoading.Visibility = Visibility.Visible;
        if (wikiOpening) return;
        wikiOpening = true;
        try
        {
            await WikiBrowser.EnsureCoreWebView2Async();
            if (!wikiInitialized)
            {
                WikiBrowser.CoreWebView2.NewWindowRequested += (_, args) =>
                {
                    args.Handled = true;
                    if (Uri.TryCreate(args.Uri, UriKind.Absolute, out var target) && IsWikiUri(target))
                        WikiBrowser.CoreWebView2.Navigate(target.AbsoluteUri);
                    else WikiFailure("This link is outside Wikipedia. Continue reading with the links inside this article.");
                };
                wikiInitialized = true;
            }
            if (WikiReaderPanel.Visibility == Visibility.Visible)
                WikiBrowser.CoreWebView2.Navigate(wikiRequestedUri.AbsoluteUri);
        }
        catch (Exception)
        {
            WikiFailure("The reader could not start. Check that Microsoft Edge WebView2 Runtime is installed, then retry. Offline destination notes are still available.");
        }
        finally { wikiOpening = false; }
    }

    private void WikiNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out var uri) || !IsWikiUri(uri))
        {
            args.Cancel = true;
            WikiFailure("This link is outside Wikipedia. Continue reading with the links inside this article.");
            return;
        }
        wikiRequestedUri = uri;
        WikiSource.Text = "Source: Wikipedia · " + uri.AbsoluteUri;
        WikiError.IsOpen = false;
        WikiLoading.Visibility = Visibility.Visible;
    }

    private void WikiNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        WikiLoading.Visibility = Visibility.Collapsed;
        WikiBack.IsEnabled = sender.CanGoBack;
        if (!args.IsSuccess && args.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
            WikiFailure("Wikipedia could not be loaded. Check your internet connection and retry, or browse the offline destination notes above.");
    }

    private void WikiFailure(string message)
    {
        WikiLoading.Visibility = Visibility.Collapsed;
        WikiError.Message = message;
        WikiError.IsOpen = true;
    }

    private void WikiGoBack(object sender, RoutedEventArgs e) { if (WikiBrowser.CanGoBack) WikiBrowser.GoBack(); }
    private async void WikiRetry(object sender, RoutedEventArgs e) { if (wikiRequestedUri is not null) await OpenWikiUri(wikiRequestedUri); }
    private void CloseWikiReader(object sender, RoutedEventArgs e)
    {
        WikiBrowser.CoreWebView2?.Stop();
        WikiReaderPanel.Visibility = Visibility.Collapsed;
        WikiLoading.Visibility = Visibility.Collapsed;
    }
}
