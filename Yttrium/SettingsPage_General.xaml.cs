using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yttrium
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage_General : Page
    {

        private ApplicationDataContainer localSettings;
        public SettingsPage_General()
        {
            this.InitializeComponent();
            localSettings = ApplicationData.Current.LocalSettings;
            CustomTabGroup.SelectedIndex = 0;
            //    localSettings.Values["useCustomHomePage"] as string == "0" && 
            //    localSettings.Values["useCustomHomePage"] as string != "" ? 0 : 1;
            SearchEngine.Text = CustomHomepage;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["useCustomHomePage"] = (sender as RadioButton).Content.ToString();
        }

        private List<string> protocolSuggestions = new List<string>()
        {
            "https://",
            "https://wwww.",
            "https://google.com",
            "https://google.ro",
        };

        // Handle text change and present suitable items
        private void SearchEngine_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Since selecting an item will also change the text,
            // only listen to changes caused by user entering text.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<string>();
                var splitText = sender.Text.ToLower().Split(" ");
                foreach (var suggestion in protocolSuggestions)
                {
                    var found = splitText.All((key) =>
                    {
                        return suggestion.ToLower().Contains(key);
                    });
                    if (found)
                    {
                        suitableItems.Add(suggestion);
                    }
                }
                sender.ItemsSource = suitableItems;
            }
        }

        // Handle user selecting an item, in our case just output the selected item.
        private void SearchEngine_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            SearchEngine.Text = args.SelectedItem.ToString();
            if (MainPage.ValidateUrl(SearchEngine.Text, out string validatedUrl))
            {
                CustomHomepage = validatedUrl;
            }
        }

        private void SearchEngine_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (MainPage.ValidateUrl(SearchEngine.Text, out string validatedUrl))
            {
                CustomHomepage = validatedUrl;
            }
        }

        public static String ObtainHomepage
        {
            get
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                //if (localSettings != null && localSettings.Values["useCustomHomePage"] as string != "")
                //{
                //    return localSettings.Values["useCustomHomePage"] as string == "1" ? CustomHomepage : NewTabHomepage;
                //}
                return NewTabHomepage;
            }
        } 

        public static String NewTabHomepage = "https://yttrium/index.html";

        public static String CustomHomepage
        {
            get
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings != null)
                {
                    if (localSettings.Values["customHomePage"] != null)
                        return localSettings.Values["customHomePage"] as string;
                    else return NewTabHomepage;
                }
                else return NewTabHomepage;
            }

            set
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings != null)
                {
                    localSettings.Values["customHomePage"] = value;
                }
            }
        }

    }

}
