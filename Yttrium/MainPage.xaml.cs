using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yttrium
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public WebView2 WebBrowser
        {
            get
            {
                return Tabs.SelectedItem is WebViewTab webViewPage ? webViewPage.WebBrowser : null;
            }
        }

        public Frame TabContent
        {
            get
            {
                return Tabs.SelectedItem is WebViewTab webViewPage ? webViewPage.TabContent : null;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            // Enables Navigation Cache
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            LoadSettings();
        }

        private void LoadSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings != null)
            {

            }
        }

        //back navigation
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.CanGoBack) WebBrowser.GoBack();
        }

        //forward navigation
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.CanGoForward) WebBrowser.GoForward();
        }

        //refresh 
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.Reload();
        }

        //navigation completed

        //if enter is pressed, it searches text in SearchBar or goes to web page
        private void SearchBar_KeyDown(object sender, KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter && WebBrowser != null && WebBrowser.CoreWebView2 != null)
            {
                Search();
                this.Focus(FocusState.Programmatic);
            }

        }

        //if clicked on SearchBar, the text will be selected
        private void SearchBar_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchBar.SelectAll();
        }

        //method for search engine + updates link text in SearchBar
        private void Search()
        {
            String searchText = SearchBar.Text;
            if (searchText != "")
            {
                LoadWebsite(searchText);
            }
        }

        //home button redirects to homepage
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LoadWebsite(SettingsPage_General.ObtainHomepage);
        }

        //opens settings page
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        //stops refreshing if clicked on progressbar
        private void StopRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.CoreWebView2.Stop();
        }

        //titlebar
        private void DragArea_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(sender as Border);

        }

        //add new tab
        private void Tabs_AddTabButtonClick(TabView sender, object args)
        {
            sender.TabItems.Add(new WebViewTab());
            sender.SelectedItem = Tabs.TabItems.Last();
        }

        //close tab
        private async void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (sender.TabItems.Count <= 1)
                await ApplicationView.GetForCurrentView().TryConsolidateAsync();
            sender.TabItems.Remove(args.Tab);
        }

        //opens about app dialog
        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutdialog = new AboutDialog();
            await aboutdialog.ShowAsync();
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WebBrowser != null) UpdateComponents();
        }

        private void Tab_NavigationCompleted(WebViewTab sender)
        {
            UpdateComponents();
            WebBrowser.CoreWebView2.ContainsFullScreenElementChanged += (obj, args) =>
            {
                Boolean fullScreen = WebBrowser.CoreWebView2.ContainsFullScreenElement;
                var view = ApplicationView.GetForCurrentView();
                if (fullScreen)
                {
                    if (view.TryEnterFullScreenMode())
                    {
                        ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                        /** 
                         * TODO: HIDE TOOLBAR ON FULLSCREEN
                        **/
                        // The SizeChanged event will be raised when the entry to full-screen mode is complete.
                    }
                }
                else
                {
                    view.ExitFullScreenMode();
                    ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
                    /** 
                     * TODO: SHOW TOOLBAR ON FULLSCREEN
                    **/
                    // The SizeChanged event will be raised when the exit from full-screen mode is complete.
                }
                Tabs.IsAddTabButtonVisible = !fullScreen;
            };
        }

        private void Tab_NavigationStarting(WebViewTab obj)
        {
            RefreshButton.Visibility = Visibility.Collapsed;
            StopRefreshButton.Visibility = Visibility.Visible;
        }

        private void LoadWebsite(string url)
        {
            // Loads the website URL
            // Or searches for it for validation fails
            WebBrowser.Source = ValidateUrl(url);

            // Updates the UI
            UpdateComponents();

            // Create Settings file if needed
            if (!File.Exists(ApplicationData.Current.LocalFolder.Path + "settings.xml"))
            {
                SettingsData.CreateSettingsFile();
            }

            // Saves the search history
            DataTransfer datatransfer = new DataTransfer();
            if (!string.IsNullOrEmpty(SearchBar.Text) && SearchBar.Text != SettingsPage_General.NewTabHomepage)
            {
                datatransfer.SaveSearchTerm(SearchBar.Text, WebBrowser.CoreWebView2.DocumentTitle, WebBrowser.Source.AbsoluteUri);
            }
        }

        private Uri ValidateUrl(string url)
        {
            String searchCopy = url;
            searchCopy = searchCopy.Split('?')[0];
            searchCopy = searchCopy.Split('/').Last();
            Boolean hasValidPrefix = searchCopy.Contains('.');
            Uri searchQuery;
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                searchQuery = new Uri(url);
            }
            else if (!url.Contains("://") && hasValidPrefix)
            {
                url = "https://" + url;
                searchQuery = new Uri(url);
            }
            else
            {
                searchQuery = new Uri("https://www.google.com/search?q=" + url);
            }
            return searchQuery;
        }

        public static bool ValidateUrl(string url, out string validatedUrl)
        {
            String searchCopy = url;
            searchCopy = searchCopy.Split('?')[0];
            searchCopy = searchCopy.Split('/').Last();
            Boolean hasValidPrefix = searchCopy.Contains('.');
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                validatedUrl = url;
            }
            else if (!url.Contains("://") && hasValidPrefix)
            {
                url = "https://" + url;
                validatedUrl = url;
            }
            else
            {
                validatedUrl = "https://www.google.com/search?q=" + url;
            }
            return validatedUrl != "";
        }

        private void UpdateComponents()
        {
            SearchBar.Text = WebBrowser.Source.AbsoluteUri ==
                SettingsPage_General.NewTabHomepage ? "" : WebBrowser.Source.AbsoluteUri;
            RefreshButton.Visibility = Visibility.Visible;
            StopRefreshButton.Visibility = Visibility.Collapsed;

            SSLIcon.Foreground = null;
            SSLIcon.FontFamily = new FontFamily("Segoe Fluent Icons");
            String tooltipMessage = "";
            ToolTip tooltip = new ToolTip
            {
                Content = tooltipMessage
            };

            if (WebBrowser.Source.AbsoluteUri.Contains("https"))
            {
                //change icon to lock
                SSLIcon.Glyph = "\xE72E";
                tooltipMessage = "This website has a SSL certificate";
                SSLIcon.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 68, 210, 78));

            }
            else
            {
                //change icon to warning
                SSLIcon.Glyph = "\xE7BA";
                tooltipMessage = "This website is unsafe and doesn't have a SSL certificate";
                SSLIcon.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 191, 0));
            }

            ToolTipService.SetToolTip(SSLButton, tooltip);

        }


    }

}

