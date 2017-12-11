using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HICAPSConnectLibrary.Utils
{
    public static class FileRepo
    {
        public static string CardSamples = "CardSamples.txt";
        public static string Merchants = "Merchants.txt";
        public static string Items = "Items.txt";
        public static string Providers = "Providers.txt";
        public static string CardList = "CardList.txt";
        public static string ItemResponseCodes = "ItemResponseCodes.txt";
        public static string TransCodeList = "TransCodeList.txt";
        public static string AS2805_HICAPS = "AS2805_HICAPS.txt";
        public static Dictionary<string, string[]> _data = new Dictionary<string, string[]>();

        static FileRepo()
        {

        }
        /// <summary>
        /// Get file data split by line feed.  If file doesn't exist use one stored in Resources
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string[] getFileData(string filename)
        {
            if (_data != null && _data.ContainsKey(filename))
            {
                return _data[filename];
            }

            try
            {
                if (File.Exists(filename))
                {
                    var data = removeBlankLines(File.ReadAllLines(filename));

                    if (_data != null && _data.ContainsKey(filename))
                    {
                        _data[filename] = data;
                    }
                    return data;
                }
            }
            catch (Exception e)
            { }


            
            // Try application exe assembly first
            var assembly = Assembly.GetEntryAssembly();
            var files = assembly.GetManifestResourceNames().FirstOrDefault(c => c.EndsWith(filename));

            // If not found, try current assembly.
            if ((files.IsNullOrWhiteSpace()))
            {
                assembly = Assembly.GetExecutingAssembly();
                files = assembly.GetManifestResourceNames().FirstOrDefault(c => c.EndsWith(filename));
            }
           
           // var resourceName = "HicapsConnectSimulator." + filename;

            using (Stream stream = assembly.GetManifestResourceStream(files))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                var data = result.Replace("\r", "").Split('\n');
                data = removeBlankLines(data);
                writeFileData(filename, data);
                if (_data != null && _data.ContainsKey(filename))
                {
                    _data[filename] = data;
                }
                return data;
            }
        }

        public static void setFileData(string filename, string[] data)
        {
            data = removeBlankLines(data);
            if (_data != null && _data.ContainsKey(filename))
            {
                _data[filename] = data;
            }
            writeFileData(filename, data);
        }

        private static void writeFileData(string filename, string[] data)
        {
            try
            {

                File.WriteAllLines(filename, data);
            }
            catch { }
        }

        private static string[] removeBlankLines(string[] data)
        {
            var _data = new List<string>();
            if (data != null)
            {
                //   _data.AddRange(data.Where(line => !string.IsNullOrEmpty(line) && !string.IsNullOrEmpty(line.Trim())));
                foreach (var line in data)
                {

                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        _data.Add(line);
                    }
                }
            }
            return _data.ToArray();
        }
    }
}
