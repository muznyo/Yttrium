using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yttrium
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebViewTab : TabViewItem
    {
        public event Action<WebViewTab> NavigationCompleted = null;
        public event Action<WebViewTab> NavigationStarting = null;

        string OriginalUserAgent;
        string GoogleSignInUserAgent;
        public WebViewTab()
        {
            Header = null;
            this.InitializeComponent();
            //google login fix
            WebBrowser.CoreWebView2Initialized += delegate
            {
                OriginalUserAgent = WebBrowser.CoreWebView2.Settings.UserAgent;
                GoogleSignInUserAgent = OriginalUserAgent.Substring(0, OriginalUserAgent.IndexOf("Edg/"))
                .Replace("Mozilla/5.0", "Mozilla/4.0");
            };
        }
        private void WebBrowser_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            //website load status
            try
            {
                WebBrowser.CoreWebView2.Settings.IsStatusBarEnabled = false;
                Uri icoURI = new Uri("https://www.google.com/s2/favicons?sz=64&domain_url=" + WebBrowser.Source);
                IconSource = new Microsoft.UI.Xaml.Controls.BitmapIconSource() { UriSource = icoURI, ShowAsMonochrome = false };
                Header = WebBrowser.CoreWebView2.DocumentTitle.ToString();
            }
            catch
            {

            }
            NavigationCompleted?.Invoke(this);

            


            //await WebBrowser.EnsureCoreWebView2Async();
            //await WebBrowser.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
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

        //handles progressring and refresh behavior
        private void WebBrowser_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
        {
            NavigationStarting?.Invoke(this);
            
            var isGoogleLogin = new Uri(args.Uri).Host.Contains("accounts.google.com");
            WebBrowser.CoreWebView2.Settings.UserAgent = isGoogleLogin ? GoogleSignInUserAgent : OriginalUserAgent;
        }
    }
}
