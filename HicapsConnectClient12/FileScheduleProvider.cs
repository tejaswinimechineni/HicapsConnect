using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Security.Cryptography;

namespace HicapsConnectClient12
{
    /* FileScheduleProvider
     * 
     * An Schedule provider implementation that saves an XML 
     * file on every write and reads the XML file on every 
     * read.
     * 
     * This is modified to the version in the Web Api as 
     * it needs to provide encryption.
     */
    public class FileScheduleProvider : IScheduleProvider
    {
        private Dictionary<string, Item> items;
        private string path;

        private DateTime expiry;
        public DateTime Expiry 
        {
            get 
            {
                return expiry;
            }
        }

        private string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                        + Path.DirectorySeparatorChar
                        + "HicapsConnect"
                        + Path.DirectorySeparatorChar
                        + "ItemCache.txt";

        public FileScheduleProvider()
        {
            path = filename;
            items = new Dictionary<string, Item>();
            ReadItemFile();           
            
        }

        private void ReadItemFile()
        {
            try
            {
                Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                XmlSerializer x = new XmlSerializer(typeof(Schedule));
                
                // create crypto stream
                var transform = new AesCryptoServiceProvider();
                using (var cryptoStream = new CryptoStream(stream,
                    transform.CreateDecryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                    CryptoStreamMode.Read))
                {
                    Schedule schedule = (Schedule) x.Deserialize(cryptoStream);
                    stream.Close();
                    expiry = schedule.Expiry;
                    items = new Dictionary<string, Item>();
                    foreach (Item i in schedule.Items)
                    {
                        if (i.Number != null && i.Number != "")
                        {
                            items[i.Number] = i;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("couldn't read item cache file: " + ex.Message);
            }
        }

        private void WriteItemFile()
        {
            try
            {
                Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                XmlSerializer x = new XmlSerializer(typeof(Schedule));

                // create crypto stream
                var transform = new AesCryptoServiceProvider();
                using (var cryptoStream = new CryptoStream(stream, 
                        transform.CreateEncryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)), 
                        CryptoStreamMode.Write))
                {
                    x.Serialize(cryptoStream, new Schedule(Expiry, items.Values.ToList<Item>()));
                }
                stream.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Couldn't write item file: " + e.Message);
            }
        }

        public Schedule GetCurrentSchedule()
        {
            ReadItemFile();
            return new Schedule(Expiry, items.Values.ToList());
        }

        public Item Get(string number)
        {
            ReadItemFile();
            return items.ContainsKey(number) ? items[number] : null;
        }

        public Item Add(Item item)
        {
            ReadItemFile();
            if (item == null) throw new ArgumentNullException("item");
            items[item.Number] = item;
            WriteItemFile();
            return item;
        }

        public void Remove(string number)
        {
            ReadItemFile();
            items.Remove(number);
            WriteItemFile();
        }

        public bool Update(Item item)
        {
            ReadItemFile();
            if (items.ContainsKey(item.Number))
            {
                items[item.Number] = item;
                WriteItemFile();
                return true;
            }
            else
            {
                return false;
            }
        }


        public string GetCurrentScheduleAsXml()
        {
            ReadItemFile();
            string ret = "";
            try
            {
                MemoryStream stream = new MemoryStream();
                XmlSerializer x = new XmlSerializer(typeof(Schedule));
                x.Serialize(stream, new Schedule(Expiry, items.Values.ToList<Item>()));
                stream.Position = 0;
                ret = new StreamReader(stream).ReadToEnd();
                stream.Close();
            }
            catch 
            {
                return "<!-- failed -->";
            }
            return ret;
        }

        public void SetCurrentScheduleAsXml(string xml)
        {

            MemoryStream stream = new MemoryStream();
            new StreamWriter(stream).Write(xml);
            XmlSerializer x = new XmlSerializer(typeof(Schedule));
            stream.Position = 0;
            Schedule schedule = (Schedule) x.Deserialize(stream);
            stream.Close();
            items = new Dictionary<string, Item>();
            foreach (Item i in schedule.Items)
            {
                if (i.Number != null && i.Number != "")
                {
                    items[i.Number] = i;
                }
            }
            WriteItemFile();
        }


        public void SetCurrentSchedule(Schedule s)
        {
            items = new Dictionary<string,Item>();
            s.Items.ForEach(it => items.Add(it.Number, it));
            expiry = s.Expiry;
            WriteItemFile();
        }
    }
}