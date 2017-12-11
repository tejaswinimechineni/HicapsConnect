using System;
namespace HicapsConnectClient12
{
    /// <summary>
    /// Interface for Simple Object Database
    /// </summary>
    interface IObjectDB
    {
        string getValue(string key);
        string setValue(string key, string value);
        void Load();
        void Save();
        void Clear();
    }
}
