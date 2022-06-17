using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                if (Tabs.SelectedItem is WebViewTab webViewPage)
                {
                    return webViewPage.WebBrowser;
                }
                else
                {
                    return null;
                }
            }
        }

        public Frame TabContent
        {
            get
            {
                if (Tabs.SelectedItem is WebViewTab webViewPage)
                {
                    return webViewPage.TabContent;
                }
                else
                {
                    return null;
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            //creates settings file on app first launch
            SettingsData settings = new SettingsData();
            settings.CreateSettingsFile();
        }


        //back navigation
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.CanGoBack)
            {
                WebBrowser.GoBack();
            }
        }

        //forward navigation
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {

            if (WebBrowser.CanGoForward)
            {
                WebBrowser.GoForward();
            }

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
                SearchBar.Text = WebBrowser.Source.AbsoluteUri;
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
            //if (SearchBar.Text.Contains("https://") || SearchBar.Text.Contains("http://"))
            //{
            //    WebBrowser.Source = new Uri(SearchBar.Text);
            //}
            //else
            //{
            //    WebBrowser.Source = new Uri("https://www.google.com/search?q=" + SearchBar.Text);
            //}
            //string link = "https://" + SearchBar.Text;
            //WebBrowser.CoreWebView2.Navigate(link);
            String searchText = SearchBar.Text;
            String searchCopy = searchText;
            searchCopy = searchCopy.Split('?')[0];
            searchCopy = searchCopy.Split('/').Last();
            Boolean hasValidPrefix = searchCopy.Contains('.');
            Uri uriResult;
            if (Uri.TryCreate(searchText, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp)
            {
                WebBrowser.Source = new Uri(searchText);
            } 
            else if (!searchText.Contains("http") && hasValidPrefix)
            {
                searchText = "https://" + searchText;
                WebBrowser.Source = new Uri(searchText);
            }
            else
            {
                WebBrowser.Source = new Uri("https://www.google.com/search?q=" + searchText);
            }

            WebBrowser.Source = new Uri("https://www.google.com/search?q=" + SearchBar.Text);
            //SearchBar.Text = newTab.Content == new HomePage() ? "Home page" : WebBrowser.Source.AbsoluteUri;
            //TabContent.Content = WebBrowser;
        }

        //home button redirects to homepage
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.CoreWebView2.Navigate("ms-appx-web:///Assets/HomePage/index.html");
        }

        public async Task HomePage()
        {
            string fname = @"Assets/HomePage/index.html";
            var contents = "";
            StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await InstallationFolder.GetFileAsync(fname);
            if (File.Exists(file.Path))
            {
                contents = File.ReadAllText(file.Path);
            }
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
            //WebView2 webView = new WebView2();
            //await webView.EnsureCoreWebView2Async();
            //webView.CoreWebView2.Navigate("https://google.com");
            //newTab.Content = new HomePage();
            //sender.TabItems.Add(new TabViewItem() { Content = newTab });
            //sender.SelectedItem = newTab ;
            //SearchBar.Text = newTab.Header.ToString();
            //TabContent.Content = WebBrowser.Source = new Uri("File://Homepage.html");
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
            if (WebBrowser != null)
            {
                SearchBar.Text = WebBrowser.Source.AbsoluteUri;
            }
        }

        private void Tabs_Loaded(object sender, RoutedEventArgs e)
        {
            // not really needed. we already have a new tab page and also new TabViewItem here does nothing. Also, if this code is used again it should be new WebViewTab()
            //Uri icoURI = new Uri("https://www.google.com/s2/favicons?sz=64&domain_url=" + WebBrowser.Source);
            //new TabViewItem()
            //{

            //    Content = new WebView2(),
            //    IconSource = new Microsoft.UI.Xaml.Controls.BitmapIconSource() { UriSource = icoURI, ShowAsMonochrome = false },
            //    Header = WebBrowser.CoreWebView2.DocumentTitle.ToString()
            //};
        }

        private void Tab_NavigationCompleted(WebViewTab sender)
        {
            var WebBrowser = sender.WebBrowser;
            SearchBar.Text = WebBrowser.Source.AbsoluteUri;
            RefreshButton.Visibility = Visibility.Visible;
            StopRefreshButton.Visibility = Visibility.Collapsed;

            //history
            DataTransfer datatransfer = new DataTransfer();
            if (!string.IsNullOrEmpty(SearchBar.Text))
            {
                datatransfer.SaveSearchTerm(SearchBar.Text, WebBrowser.CoreWebView2.DocumentTitle, WebBrowser.Source.AbsoluteUri);
            }
            if (WebBrowser.Source.AbsoluteUri.Contains("https"))
            {
                //change icon to lock
                SSLIcon.FontFamily = new FontFamily("Segoe Fluent Icons");
                SSLIcon.Glyph = "\xE72E";

                ToolTip tooltip = new ToolTip
                {
                    Content = "This website has a SSL certificate"
                };
                ToolTipService.SetToolTip(SSLButton, tooltip);

            }
            else
            {
                //change icon to warning
                SSLIcon.FontFamily = new FontFamily("Segoe Fluent Icons");
                SSLIcon.Glyph = "\xE7BA";
                ToolTip tooltip = new ToolTip
                {
                    Content = "This website is unsafe and doesn't have a SSL certificate"
                };
                ToolTipService.SetToolTip(SSLButton, tooltip);

            }
        }

        private void Tab_NavigationStarting(WebViewTab obj)
        {
            RefreshButton.Visibility = Visibility.Collapsed;
            StopRefreshButton.Visibility = Visibility.Visible;
        }

    }
}

