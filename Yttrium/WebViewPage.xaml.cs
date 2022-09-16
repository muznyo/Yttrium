using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yttrium
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebViewTab : TabViewItem
    {
        public event Action<WebView2> CustomLaunch = null;
        public event Action<WebViewTab> NavigationCompleted = null;
        public event Action<WebViewTab> NavigationStarting = null;
        public event Action<WebViewTab> SourceChanged = null;
        public event Action<WebViewTab> ContentLoading = null;
        public event Action<WebViewTab, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs> NewTabRequested = null;

        string OriginalUserAgent;
        string GoogleSignInUserAgent;
        public WebViewTab()
        {
            Header = "New Tab";
            IconSource = new BitmapIconSource() { ShowAsMonochrome = false, UriSource = new Uri("ms-appx:///Assets/Square44x44Logo.altform-lightunplated_targetsize-48.png") };
            this.InitializeComponent();
            WebBrowser.CoreWebView2Initialized += delegate
            {
                WebBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "yttrium", "Assets/NewTabPage", Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);
                if (CustomLaunch == null)
                    WebBrowser.Source = new Uri(SettingsPage_General.ObtainHomepage);
                else
                    CustomLaunch?.Invoke(WebBrowser);
                WebBrowser.CacheMode = new BitmapCache();
                // Google login fix
                OriginalUserAgent = WebBrowser.CoreWebView2.Settings.UserAgent;
                GoogleSignInUserAgent = OriginalUserAgent.Substring(0, OriginalUserAgent.IndexOf("Edg/"))
                .Replace("Mozilla/5.0", "Mozilla/4.0");

                WebBrowser.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
                WebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                WebBrowser.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
                WebBrowser.CoreWebView2.ContentLoading += CoreWebView2_ContentLoading;
                WebBrowser.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            };
        }

        private void CoreWebView2_ContentLoading(Microsoft.Web.WebView2.Core.CoreWebView2 sender,
            Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs args) => ContentLoading?.Invoke(this);

        private void CoreWebView2_SourceChanged(Microsoft.Web.WebView2.Core.CoreWebView2 sender,
            Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs args) => SourceChanged?.Invoke(this);

        private void CoreWebView2_NewWindowRequested(object sender,
            Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e) => NewTabRequested?.Invoke(this, e);

        public void WebBrowser_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            // Update Tab Header
            try
            {
                if (WebBrowser.Source.AbsoluteUri != SettingsPage_General.NewTabHomepage)
                {
                    Uri icoURI = new Uri("https://www.google.com/s2/favicons?sz=64&domain_url=" + WebBrowser.Source);
                    IconSource = new BitmapIconSource() { UriSource = icoURI, ShowAsMonochrome = false };
                    Header = WebBrowser.CoreWebView2.DocumentTitle.ToString();
                }
            }
            catch { }
            NavigationCompleted?.Invoke(this);
        }

        // Handles progressing and refresh behavior
        public void WebBrowser_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            var isGoogleLogin = new Uri(args.Uri).Host.Contains("accounts.google.com");
            WebBrowser.CoreWebView2.Settings.UserAgent = isGoogleLogin ? GoogleSignInUserAgent : OriginalUserAgent;
            // Update Tab Header
            try
            {
                if (WebBrowser.Source.AbsoluteUri != SettingsPage_General.NewTabHomepage)
                {
                    Uri icoURI = new Uri("https://www.google.com/s2/favicons?sz=64&domain_url=" + WebBrowser.Source);
                    IconSource = new BitmapIconSource() { UriSource = icoURI, ShowAsMonochrome = false };
                    Header = WebBrowser.CoreWebView2.DocumentTitle.ToString();
                }
            }
            catch { }
            NavigationStarting?.Invoke(this);
        }
    }
}
