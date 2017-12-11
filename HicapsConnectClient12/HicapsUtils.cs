using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using HicapsConnectControl;
using System.Xml.Serialization;
using System.Security.Cryptography;

/* 
 * Some code copied from the original HicapsConnectClient project
 * because it is useful and can be used with little modification
 */

namespace HicapsConnectClient12
{
    /*
     * Some static methods that are useful for various tasks
     */
    public class Utils
    {
        // Generated on 1/10/2013
        internal static string StorageKey = "83558d0b310809d2acfa6421ef987e53";

        // Generated on 1/10/2013
        internal static string MachineSalt = "e2291abf0d57c870c1b2fe6b14fb45ff";

        // Generated on 1/10/2013
        internal static string StorageIV = "1acac541148794b343fe738f726f34b1";

        internal static string PMSKey = "8562026";
        internal static string VendorName = "HICAPSConnectClient";

        internal static byte[] HexToByte(string hex)
        {
            byte[] key = new byte[16];
            for (int i = 0; i < hex.Length; i += 2)
            {
                key[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return key;
        }

        internal static byte[] GetMachineSpecificKey()
        {
            var KeyDeriver = new Rfc2898DeriveBytes(Environment.MachineName, HexToByte(MachineSalt));
            byte[] machineKey = KeyDeriver.GetBytes(16);
            byte[] storageKey = HexToByte(StorageKey);
            byte[] finalKey = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                finalKey[i] = (byte) (machineKey[i] ^ storageKey[i]);
            }
            return finalKey;
        }

        // Serializer stolen from HicapsConnectControl
        internal static string Serialize<T>(T myBo)
            where T : HicapsConnectControl.HicapsConnectControl.BaseMessage
        {
            using (MemoryStream myMS = new MemoryStream())
            {
               
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(myMS, myBo);
                return Encoding.Default.GetString(myMS.ToArray());
            }
        }

        // Deserializer stolen from HicapsConnectControl
        internal static T Deserialize<T>(string xmlData)
            where T : HicapsConnectControl.HicapsConnectControl.BaseMessage
        {
            
            using (MemoryStream myResultMS = new MemoryStream(Encoding.Default.GetBytes(xmlData)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
               return (T)serializer.Deserialize(myResultMS);
            }
        }

        public static string stripCurrencyFormatting(string fieldText)
        {
            if (fieldText == null) { fieldText = ""; }
            fieldText = fieldText.Replace("-", "");
            fieldText = fieldText.Replace("-", "");
            fieldText = fieldText.Replace("+", "");
            fieldText = fieldText.Replace("%", "");
            fieldText = fieldText.Replace("$", "");
            fieldText = fieldText.Replace("_", "");
            fieldText = fieldText.Replace(" ", "").Trim();
            if (fieldText.Trim() == "" || fieldText.Trim() == ".") { fieldText = "0.00"; }
            return fieldText;
        }

        public static string getProviderNumberId (string provider)
        {
            try
            {
                return ((provider ?? "00000000").PadLeft(8, '0')).Substring(0, 8);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string getProviderNumberFromRow(string provider)
        {
            string strValue = provider;
            try
            {
                strValue = strValue.Split('|')[2];
            }
            catch { }

            if (!string.IsNullOrEmpty(strValue) && strValue.Length >= 8)
            {
                strValue = strValue.Substring(0, 8);
            }
            return strValue;
        }

        public static string getMerchantId(string merchant)
        {
            string strValue = merchant;
            if (!string.IsNullOrEmpty(strValue) && strValue.Length >= 8)
            {
                strValue = strValue.Substring(0, 8);
            }
            return strValue;
        }
    }



    /*
     * A class that encapsulates the disk caching behaviour of 
     * the original Hicaps connect client.
     * 
     * So far it caches:
     *  - Providers
     *  - Merchants
     *  
     * To do:
     *  - Card details?
     *  - Items?
     */
    public class HicapsCacheManager
    {

        public string[] syncMerchantCache(bool forceUpdate, string url, HicapsConnectControl.HicapsConnectControl hicaps)
        {
            if (forceUpdate)
            {
                HicapsConnectControl.HicapsConnectControl.AllMerchantListRequest myRequest = new HicapsConnectControl.HicapsConnectControl.AllMerchantListRequest();
                myRequest.ServerUrl = url;
                HicapsConnectControl.HicapsConnectControl.AllMerchantListResponse myBo = hicaps.sendAllMerchantList(myRequest);
                return syncMerchantCache(myBo, true, url);
            }
            else
            {
                HicapsConnectControl.HicapsConnectControl.AllMerchantListResponse myBo = new HicapsConnectControl.HicapsConnectControl.AllMerchantListResponse();
                return syncMerchantCache(myBo, false, url);
            }
        }
        public string[] syncMerchantCache(HicapsConnectControl.HicapsConnectControl.AllMerchantListResponse myBo, bool forceUpdate, string url)
        {
            Debug.WriteLine("syncMerchantCache: exists=" + File.Exists(getMerchantCacheFilename(url))
                + ", name=" + getMerchantCacheFilename(url));
            if (!File.Exists(getMerchantCacheFilename(url)) || forceUpdate)
            {
                List<string> list = myBo.MerchantListDetails;
                string[] MerchantCacheList = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                {

                    string[] fields = myBo.breakupLineFields(i);
                    MerchantCacheList[i] = fields[0] + "|" +
                                            fields[1] + "|" +
                                            fields[2] + "|";

                }
                createMerchantCacheFile(MerchantCacheList, url);
                return MerchantCacheList;
            }
            else
            {
                return getMerchantCacheFile(url);
            }

        }

        private string getMerchantCacheFilename(string url)
        {
            string terminalId = "";
            string[] serverParts = url.Split(':');
            // TODO put the Server url into an object
            if (serverParts.Count() > 3)
            {
                terminalId = serverParts[2];
            }
            else
            {
                terminalId = "";
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar + "MerchantList" + terminalId + ".dat";
        }

        private void createMerchantCacheFile(string[] MerchantList, string url)
        {
            // Provider list is a Pipe delimited file per line.
            string merchantFile = getMerchantCacheFilename(url);
            createCacheFile(merchantFile, MerchantList);
        }

        private string[] getMerchantCacheFile(string url)
        {
            return getCacheFile(getMerchantCacheFilename(url));
        }

        public string[] syncProviderCache(bool forceUpdate, string url, HicapsConnectControl.HicapsConnectControl hicaps)
        {
            if (forceUpdate)
            {
                HicapsConnectControl.HicapsConnectControl.AllProviderListRequest myRequest = new HicapsConnectControl.HicapsConnectControl.AllProviderListRequest();
                myRequest.ServerUrl = url;
                HicapsConnectControl.HicapsConnectControl.AllProviderListResponse myBo = hicaps.sendAllProviderList(myRequest);
                return syncProviderCache(myBo, true, url);
            }
            else
            {
                HicapsConnectControl.HicapsConnectControl.AllProviderListResponse myBo = new HicapsConnectControl.HicapsConnectControl.AllProviderListResponse();
                return syncProviderCache(myBo, false, url);
            }
        }

        public string[] syncCardCache(bool forceUpdate, string url, HicapsConnectControl.HicapsConnectControl hicaps)
        {
            if (forceUpdate)
            {
                HicapsConnectControl.HicapsConnectControl.CardListRequest myRequest = new HicapsConnectControl.HicapsConnectControl.CardListRequest();
                myRequest.ServerUrl = url;
                HicapsConnectControl.HicapsConnectControl.CardListResponse myBo = hicaps.sendCardList(myRequest);
                return syncCardCache(myBo, true, url);
            }
            else
            {
                HicapsConnectControl.HicapsConnectControl.CardListResponse myBo = new HicapsConnectControl.HicapsConnectControl.CardListResponse();
                return syncCardCache(myBo, false, url);
            }
        }

        public string[] syncProviderCache(HicapsConnectControl.HicapsConnectControl.AllProviderListResponse myBo, 
                                        bool forceUpdate, string url)
        {
            Debug.WriteLine("syncProviderCache: exists=" + File.Exists(getProviderCacheFilename(url))
                + ", name=" + getProviderCacheFilename(url));
            if (!File.Exists(getProviderCacheFilename(url)) || forceUpdate)
            {
                List<string> list = myBo.ProviderListDetails;
                string[] providerCacheList = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                {

                    string[] fields = myBo.breakupLineFields(i);
                    providerCacheList[i] = fields[0] + "|" +
                                            fields[1] + "|" +
                                            fields[2] + "|" +
                                            fields[3] + "|" +
                                            fields[4] + "|" +
                                            fields[5] + "|" +
                                            fields[6];


                }
                createProviderCacheFile(providerCacheList, url);
                return providerCacheList;
            }
            else
            {
                string[] s = getProviderCacheFile(url);
                return s;
            }
        }

        public string[] syncCardCache(HicapsConnectControl.HicapsConnectControl.CardListResponse myBo,
                                        bool forceUpdate, string url)
        {
            if (!File.Exists(getProviderCacheFilename(url)) || forceUpdate)
            {
                IEnumerable<string> list = myBo.CardFundDetails
                    .Where( line => line.Length > 6)
                    .Select(line => line.Substring(0, 6).Trim() + "|" + line.Substring(6));
                createCardCacheFile(list.ToArray(), url);
                return list.ToArray();
            }
            else
            {
                return getProviderCacheFile(url);
            }
        }

        private string[] getProviderCacheFile(string url)
        {
            return getCacheFile(getProviderCacheFilename(url));
        }

        private string[] getCardCacheFile(string url)
        {
            return getCacheFile(getCardCacheFilename(url));
        }

        private string[] getCacheFile(string filename)
        {
            StreamReader myFile;
            string lineData = "";
            List<string> providerList = new List<string>();
            if (File.Exists(filename))
            {
                try
                {
                    myFile = new StreamReader(filename, false);
                }
                catch (Exception)
                {
                    // Could not open so exit.
                    return null;
                }

                try
                {

                    while (lineData != null)
                    {
                        lineData = myFile.ReadLine();
                        if (lineData != null && lineData.IndexOf("|") > 0)
                        {
                            providerList.Add(lineData);
                        }
                    }

                }
                finally { myFile.Close(); }
            }
            return providerList.ToArray();
        }

        private string getProviderCacheFilename(string url)
        {
            string terminalId = "";
            string[] serverParts = url.Split(':');
            // TODO put the Server url into an object
            if (serverParts.Count() > 3)
            {
                terminalId = serverParts[2];
            }
            else
            {
                terminalId = "";
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar + "ProviderList" + terminalId + ".dat";
        }

        private string getCardCacheFilename(string url)
        {
            string terminalId = "";
            string[] serverParts = url.Split(':');
            // TODO put the Server url into an object
            if (serverParts.Count() > 3)
            {
                terminalId = serverParts[2];
            }
            else
            {
                terminalId = "";
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar + "CardList" + terminalId + ".dat";
        }

        private void createProviderCacheFile(string[] ProviderList, string url)
        {
            // Provider list is a Pipe delimited file per line.
            string ProviderFile = getProviderCacheFilename(url);
            createCacheFile(ProviderFile, ProviderList);
        }

        private void createCardCacheFile(string[] CardList, string url)
        {
            string CardFile = getCardCacheFilename(url);
            createCacheFile(CardFile, CardList);
        }

        private void createCacheFile(string filename, string[] dataList)
        {
            StreamWriter myFile;
            try
            {
                myFile = new StreamWriter(filename, false);
            }
            catch (Exception)
            {
                // Could not open so exit.
                return;
            }

            try
            {

                foreach (string myRow in dataList)
                {
                    myFile.WriteLine(myRow);
                }

            }
            finally { myFile.Close(); }
        }

    }
}
