using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Simple Key/Value Object Database Class.  Initialise Constructor with name of DB
    /// Written by John P.
    /// </summary>
    internal class ObjectDB : IObjectDB
    {
        Dictionary<string, string> keyValuePair = new Dictionary<string, string>();
        string DatabaseName = "defaultDB";

        /// <summary>
        /// Constructor, set name of DB as parameter
        /// </summary>
        /// <param name="_databaseName">Name of Database</param>
        public ObjectDB(string _databaseName)
        {
            if (!string.IsNullOrEmpty(_databaseName)) { DatabaseName = _databaseName; }
            Load();
        }

        /// <summary>
        /// Initialises and Loads DB
        /// </summary>
        public void Load()
        {
            try
            {
                using (Stream stream = new FileStream(getCachePath(), FileMode.Open))
                {
                    try
                    {
                        keyValuePair = (Dictionary<string, string>)(new BinaryFormatter()).Deserialize(stream);
                    }
                    catch
                    {
                        Clear();
                    }
                    finally
                    {
                        stream.Close();
                    }
                }

            }
            catch (Exception e)
            {
                Clear();
            }
        }

        /// <summary>
        /// Path to where application should be stored.  Hardcoded for HICAPSConnect, but should come off assembly variables
        /// </summary>
        /// <returns></returns>
        private string getCachePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar + DatabaseName + ".hcd";
        }

        /// <summary>
        /// Method to restore value from Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string getValue(string key)
        {
            try
            {
                if (keyValuePair.ContainsKey(key))
                {
                    return keyValuePair[key];
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Method to Update or Create Key/Value.  Only persist if different to disk
        /// </summary>
        /// <param name="key">Key (Unique Id)</param>
        /// <returns></returns>

        public string setValue(string key, string value)
        {
            try
            {
                if (keyValuePair.ContainsKey(key))
                {
                    if (keyValuePair[key] != value)
                    {
                        keyValuePair[key] = value;
                        Save();
                    }
                }
                else
                {
                    keyValuePair.Add(key, value);
                    Save();
                }
            }
            catch { }
            return null;
        }
        /// <summary>
        /// Persist Entire Database to disk.
        /// </summary>
        public void Save()
        {
            try
            {
                using (Stream stream = new FileStream(getCachePath(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    try
                    {
                        (new BinaryFormatter()).Serialize(stream, keyValuePair);
                        stream.Flush();
                    }
                    catch { }
                    finally
                    {
                        stream.Close();
                    }
                }

            }
            catch { }
        }
        /// <summary>
        /// Init/Wipe/Reset entire database
        /// </summary>
        public void Clear()
        {
            keyValuePair = new Dictionary<string, string>();
            Save();
        }
    }
}
