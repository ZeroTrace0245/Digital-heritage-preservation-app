using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow
{
    private string CommunityCountry => data.Location == "Local" ? data.HomeCountry : data.Location;

    private void RefreshCommunity()
    {
        var selectedCountry = CommunityCountry;
        var posts = data.CommunityPosts
            .Where(post => data.Location == "All" || post.Country.Equals(selectedCountry, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(post => post.CreatedAt)
            .ToArray();
        CommunityLocationLabel.Text = data.Location == "All" ? "Posts from all locations" : $"Posts for {selectedCountry}";
        CommunityPosts.ItemsSource = posts;
        CommunityEmpty.Visibility = posts.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void AddCommunityPost(object sender, RoutedEventArgs e)
    {
        var country = new ComboBox { Header = "Country", IsEditable = true, ItemsSource = Destinations.Countries, Text = CommunityCountry == "All" ? data.HomeCountry : CommunityCountry };
        var place = new TextBox { Header = "Town, neighbourhood or region", MaxLength = 120 };
        var type = new ComboBox { Header = "Post type", ItemsSource = new[] { "Local tip", "Story", "Help request" }, SelectedIndex = 0 };
        var author = new TextBox { Header = "Your name or community", PlaceholderText = "e.g. Nadeesha / Galle Fort residents", MaxLength = 80 };
        var title = new TextBox { Header = "Title", PlaceholderText = "What would you like visitors to know?", MaxLength = 160 };
        var body = new TextBox { Header = "Post", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 150, MaxLength = 4000 };
        var consent = new CheckBox { Content = "I have permission to share this information and any people mentioned.", IsChecked = false };
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var panel = new StackPanel { Spacing = 12, Width = 460 };
        foreach (var field in new FrameworkElement[] { country, place, type, author, title, body, consent, error }) panel.Children.Add(field);
        var dialog = Dialog("Share with the community", panel, "Publish post");
        dialog.PrimaryButtonClick += (_, args) =>
        {
            var countryName = country.Text.Trim();
            if (string.IsNullOrWhiteSpace(countryName) || string.IsNullOrWhiteSpace(author.Text) || string.IsNullOrWhiteSpace(title.Text) || string.IsNullOrWhiteSpace(body.Text) || consent.IsChecked != true)
            {
                error.Text = "Add the country, your name, title and post, then confirm that you have permission to share it.";
                args.Cancel = true;
                return;
            }
            if (!Change(next => next.CommunityPosts.Add(new CommunityPost { Country = countryName, Place = place.Text.Trim(), Type = type.SelectedItem?.ToString() ?? "Local tip", Author = author.Text.Trim(), Title = title.Text.Trim(), Body = body.Text.Trim(), ConsentConfirmed = true }))) args.Cancel = true;
        };
        await ShowDialog(dialog);
    }
}
