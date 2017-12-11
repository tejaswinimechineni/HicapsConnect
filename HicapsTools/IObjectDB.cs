using System;
namespace HicapsTools
{
    /// <summary>
    /// Interface for Simple Object Database
    /// </summary>
    interface IObjectDB
    {
        bool hasValue(string key);
        string getValue(string key);
        string setValue(string key, string value);
        void Load();
        void Save();
        void Clear();
    }
}
