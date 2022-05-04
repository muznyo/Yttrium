using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Yttrium;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yttrium_browser
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string OriginalUserAgent;
        string GoogleSignInUserAgent;
        public MainPage()
        {
            this.InitializeComponent();
            //creates settings file on app first launch
            SettingsData settings = new SettingsData();
            settings.CreateSettingsFile();
            WebBrowser.CoreWebView2Initialized += delegate
            {
                OriginalUserAgent = WebBrowser.CoreWebView2.Settings.UserAgent;
                GoogleSignInUserAgent = OriginalUserAgent.Substring(0, OriginalUserAgent.IndexOf("Edg/"))
                .Replace("Mozilla/5.0", "Mozilla/4.0");
            };
        }

        //back navigation
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
                TabContent.Content = WebBrowser;
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
        private void WebBrowser_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            //website load status
            try
            {
                WebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                //appTitle.Text = "Yttrium browser | " + WebBrowser.CoreWebView2.DocumentTitle;
                SearchBar.Text = WebBrowser.Source.AbsoluteUri;
                RefreshButton.Visibility = Visibility.Visible;
                StopRefreshButton.Visibility = Visibility.Collapsed;

                //history
                DataTransfer datatransfer = new DataTransfer();
                if (!string.IsNullOrEmpty(SearchBar.Text))
                {
                    datatransfer.SaveSearchTerm(SearchBar.Text, WebBrowser.CoreWebView2.DocumentTitle, WebBrowser.Source.AbsoluteUri);
                }
            }
            catch
            {

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


            //            await WebBrowser.EnsureCoreWebView2Async();
            //            await WebBrowser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
            //document.addEventListener('DOMContentLoaded', function() {
            //  const style = document.createElement('style');
            //  style.textContent = '/* width */ \
            //::-webkit-scrollbar { \
            //  width: 20px !important; \
            //} \
            // \
            //::-webkit-scrollbar-track { \
            //  background: red !important; \
            //}';
            //  document.head.append(style);
            //}, false);");
        }

        //if enter is pressed, it searches text in SearchBar or goes to web page
        private void SearchBar_KeyDown(object sender, KeyRoutedEventArgs e)
        {
                if (e.Key == Windows.System.VirtualKey.Enter && WebBrowser != null && WebBrowser.CoreWebView2 != null)
                {
                    Search();
                TabContent.Content = WebBrowser;
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
            WebBrowser.Source = new Uri("https://www.google.com/search?q=" + SearchBar.Text);
            //string link = "https://" + SearchBar.Text;
            //WebBrowser.CoreWebView2.Navigate(link);
            SearchBar.Text = WebBrowser.Source.AbsoluteUri;

        }

        //home button redirects to homepage
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            TabContent.Content = new HomePage();
        }

        //opens settings page
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }


        //handles progressring and refresh behavior
        private void WebBrowser_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            RefreshButton.Visibility = Visibility.Collapsed;
            StopRefreshButton.Visibility = Visibility.Visible;

            var isGoogleLogin = new Uri(args.Uri).Host.Contains("accounts.google.com");
            WebBrowser.CoreWebView2.Settings.UserAgent = isGoogleLogin ? GoogleSignInUserAgent : OriginalUserAgent;
        }

        //stops refreshing if clicked on progressbar
        private void StopRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.CoreWebView2.Stop();
        }

        private void DragArea_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(sender as Border);

        }

        private void Tabs_AddTabButtonClick(TabView sender, object args)
        {
            var newTab = new Microsoft.UI.Xaml.Controls.TabViewItem
            {
                IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Home },
                Header = "Home page"
            };
            //WebView2 webView = new WebView2();
            //await webView.EnsureCoreWebView2Async();
            //webView.CoreWebView2.Navigate("https://google.com");
            newTab.Content = new HomePage();
            sender.TabItems.Add(newTab);
            sender.SelectedItem = newTab;
        }

        private  void Tabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (sender.TabItems.Count <= 1)
                Tabs_AddTabButtonClick(sender, args);

            sender.TabItems.Remove(args.Tab);
        }
        
        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog aboutdialog = new AboutDialog();

            var result = await aboutdialog.ShowAsync();

        }
    }
}

