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

    public sealed partial class SettingsPage_History : Page
    {
        List<string> timestamps = new List<string>();

        public SettingsPage_History()
        {
            this.InitializeComponent();
            timestamps.Clear();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            HistoryData.Source = await GetHistoryGroupedAsync();
        }   


        public async Task<ObservableCollection<GroupInfoList>> GetHistoryGroupedAsync()
        {
            var query = from item in await new DataTransfer().GetHistoryAsync(null, null)
                        group item by item.FormattedDate.Date.ToString("dd MMMM yyyy") into g
                        orderby DateTime.Parse(g.Key).Date descending
                        select new GroupInfoList(g)
                        {
                            Key = FormattedDate(DateTime.Parse(g.Key).Date)
                        };
            var collection = new ObservableCollection<GroupInfoList>(query);
            if (collection.Count > 0)
            {
                HistoryCard.Visibility = Windows.UI.Xaml.Visibility.Visible;
                NoHistoryCard.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            return collection;
        }

        public string FormattedDate(DateTime date)
        {
            var todayDate = DateTime.Now.ToString("dddd - dd MMMM yyyy");
            var targetDate = date.ToString("dddd - dd MMMM yyyy");
            switch (targetDate.CompareTo(todayDate))
            {
                case 0: return "Today " + date.ToString("- dd MMM yyyy");
            };
            todayDate = DateTime.Now.ToString("MMMM yyyy");
            targetDate = date.ToString("MMMM");
            switch (targetDate.CompareTo(todayDate))
            {
                case 0: return "Last Month " + date.ToString("- dd MMM yyyy");
            };
            return date.ToString("dddd - dd MMMM yyyy");
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is HistoryData item)
            {
                
            }
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

        public string ShortDate
        {
            get
            {
                return FormattedDate.ToString("HH:mm");
            }
        }

        public DateTime FormattedDate { get
            {
                return DataTransfer.UnixTimeStampToDateTime(double.Parse(Timestamp));
            } 
        }

        public bool isValid { get
            {
                return URL != "" && URLLabel != "" && Timestamp != "";
            }
        }

    }
}
