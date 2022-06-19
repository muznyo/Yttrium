﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Yttrium
{
    public class SettingsData
    {
        public static async void CreateSettingsFile()
        {
            try
            {
                //creates a settings.xml file for storing settings
                var storagefile = await ApplicationData.Current.LocalFolder.CreateFileAsync("settings.xml");

                using (IRandomAccessStream writestream = await storagefile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    Stream s = writestream.AsStreamForWrite();
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Async = true;
                    settings.Indent = true;
                    using (XmlWriter writer = XmlWriter.Create(s, settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("settings");
                        writer.WriteStartElement("history");
                        writer.WriteEndElement();
                        writer.WriteStartElement("favorites");
                        writer.WriteEndElement();
                        writer.WriteStartElement("searchengine");
                        writer.WriteStartElement("google");
                        writer.WriteAttributeString("prefix", "https://www.google.com/search?q=");
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        writer.Flush();
                        await writer.FlushAsync();
                    }
                }

                await Windows.System.Launcher.LaunchFileAsync(storagefile);
            }
            catch
            {


            }
        }
    }
}
