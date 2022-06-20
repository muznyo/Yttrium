using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Yttrium
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage_History : Page
    {
        public SettingsPage_History()
        {
            this.InitializeComponent();

            
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            HistoryData.Source = await GetHistoryGroupedAsync();
        }   


        public static async Task<ObservableCollection<GroupInfoList>> GetHistoryGroupedAsync()
        {
            // Grab Contact objects from pre-existing list (list is returned from function GetContactsAsync())
            var query = from item in await new DataTransfer().GetHistoryAsync(null, null)

                            // Group the items returned from the query, sort and select the ones you want to keep
                        group item by item.FormattedDate into g
                        orderby g.Key

                        // GroupInfoList is a simple custom class that has an IEnumerable type attribute, and
                        // a key attribute. The IGrouping-typed variable g now holds the Contact objects,
                        // and these objects will be used to create a new GroupInfoList object.
                        select new GroupInfoList(g) { Key = g.Key };

            return new ObservableCollection<GroupInfoList>(query);
        }


    }


    // GroupInfoList class definition:
    public class GroupInfoList : List<object>
    {
        public GroupInfoList(IEnumerable<object> items) : base(items)
        {
        }
        public object Key { get; set; }
    }


    public class HistoryData
    {

        public Uri ICON { 
            get
            {
                return new Uri("https://www.google.com/s2/favicons?sz=64&domain_url=" + URL);
            } 
        }
        public string URL { get; set; }
        public string URLLabel { get; set; }
        public string Timestamp { get; set; }

        public string FormattedDate { get
            {
                var todayDate = DateTime.Now.ToString("dddd (dd MMMM yyyy)");
                var targetDate = DataTransfer.UnixTimeStampToDateTime(double.Parse(Timestamp)).ToString("dddd (dd MMMM yyyy)");
                switch (targetDate.CompareTo(todayDate))
                {
                    case 0: return "Today";
                    case 1: return "Yesterday";
                    case 7: return "Last Week";
                };
                todayDate = DateTime.Now.ToString("MMMM yyyy");
                targetDate = DataTransfer.UnixTimeStampToDateTime(double.Parse(Timestamp)).ToString("MMMM");
                switch (targetDate.CompareTo(todayDate))
                {
                    case 0: return "Last Month";
                    case 1: return "Two Months Ago";
                };
                return DataTransfer.UnixTimeStampToDateTime(double.Parse(Timestamp)).ToString("dddd (dd MMMM yyyy)");
            } 
        }

        public bool isValid { get
            {
                return URL != "" && URLLabel != "" && Timestamp != "";
            }
        }

    }
}
