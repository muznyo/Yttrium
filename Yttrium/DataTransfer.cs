using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace Yttrium
{
    public class DataTransfer
    {
        //the file name
        string filename = "settings.xml";

        //saves history
        public async void SaveSearchTerm(string searchterm, string title, string url)
        {
            //result from documentload method is stored in doc
            var doc = await DocumentLoad().AsAsyncOperation(); //load xml file

            var history = doc.GetElementsByTagName("history");

            XmlElement elsearchterm = doc.CreateElement("searchterm");
            XmlElement elsitename = doc.CreateElement("sitename");
            XmlElement elurl = doc.CreateElement("url");
            XmlElement eldate = doc.CreateElement("time");

            var historyitem = history[0].AppendChild(doc.CreateElement("historyitem"));

            historyitem.AppendChild(elsearchterm);
            historyitem.AppendChild(elsitename);
            historyitem.AppendChild(elurl);
            historyitem.AppendChild(eldate);

            elsearchterm.InnerText = searchterm;
            elsitename.InnerText = title;
            elurl.InnerText = url;
            eldate.InnerText = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            //saves history to settings.xml
            SaveDocument(doc);

        }

        // Parse History
        public async Task<ObservableCollection<HistoryData>> GetHistoryAsync(string millisStart, string millisEnd)
        {
            var arrayList = new ObservableCollection<HistoryData>();
            //result from documentload method is stored in doc
            var doc = await DocumentLoad().AsAsyncOperation(); //load xml file

            var history = doc.GetElementsByTagName("historyitem");
            if (history.Count > 0)
            {
                for (int i = 0; i < history.Length; i++)
                {
                    var historyitem = history[i];
                    var historyData = new HistoryData();
                    for (int j = 0; j < historyitem.ChildNodes.Length; j++) 
                    {
                        var child = historyitem.ChildNodes[j];
                        switch (child.NodeName)
                        {
                            case "url":
                                historyData.URL = child.InnerText.ToString();
                                break;
                            case "sitename":
                                historyData.URLLabel = child.InnerText.ToString();
                                break;
                            case "time":
                                historyData.Timestamp = child.InnerText.ToString();
                                break;
                        }
                    }
                    if (historyData.isValid)
                    {
                        if ((millisStart == null && millisEnd == null) || 
                        (long.Parse(historyData.Timestamp) <= long.Parse(millisEnd) 
                        && long.Parse(historyData.Timestamp) >= long.Parse(millisStart)))
                            arrayList.Add(historyData);
                    }
                }
            }
            var sortableList = new List<HistoryData>(arrayList);
            sortableList.Sort((a, b) => { return b.Timestamp.CompareTo(a.Timestamp); });

            for (int i = 0; i < sortableList.Count; i++)
            {
                arrayList.Move(arrayList.IndexOf(sortableList[i]), i);
            }
            return arrayList;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp / 1000).ToLocalTime();
            return dateTime;
        }

        private async Task<XmlDocument> DocumentLoad()
        {
            XmlDocument result = null;

            await Task.Run(async () =>
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                XmlDocument doc = await XmlDocument.LoadFromFileAsync(file);
                result = doc;
            });
            return result;
        }

        //saves history to settings.xml
        private async void SaveDocument(XmlDocument doc)
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
            await doc.SaveToFileAsync(file);
        }
    }
}
