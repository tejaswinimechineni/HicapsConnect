using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace HicapsTools
{

    public enum ConfigFileType
    {
        WebConfig,
        AppConfig
    }

    public static class StringCipher
    {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly string initVector = "03lifdhg235476df";

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }
    public class AppConfig : System.Configuration.AppSettingsReader
    {
        public string docName = String.Empty;
        private XmlNode node = null;
        private int _configType;
        private const string STR_DefaultEmptyConfigSettings = "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><appSettings></appSettings></configuration>";

        public int ConfigType
        {
            get
            {
                return _configType;
            }
            set
            {
                _configType = value;
            }
        }

        private void CreatePathMissing(string path)
        {
            try
            {
                path = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(path))
                {
                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);
                }
            }
            catch
            {
            }
        }

        public AppConfig(string startPath, bool createEmptyFile = true)
        {
            docName = startPath;
            try
            {
                CreatePathMissing(startPath);
                if (!File.Exists(startPath))
                {
                    File.WriteAllText(startPath, STR_DefaultEmptyConfigSettings);
                }
            }
            catch (Exception) { }
        }

        public bool SetEncryptedValue(string key, string value)
        {
            value = StringCipher.Encrypt(value, SystemInformation.UserName);
            return SetValue(key, value);
        }
        public string GetEncryptedValue(string key, string defaultvalue)
        {
            string value = GetValue(key, defaultvalue);
            if (value != defaultvalue)
            {
                value = StringCipher.Decrypt(value, SystemInformation.UserName);
            }
            return value;
        }
        public bool SetValue(string key, string value)
        {
            XmlDocument cfgDoc = new XmlDocument();
            try
            {
                loadConfigDoc(cfgDoc);
                // retrieve the appSettings node 
                node = cfgDoc.SelectSingleNode("//appSettings");
            }
            catch { return false; }
            if (node == null)
            {
                //throw new System.InvalidOperationException("appSettings section not found");
                return false;
            }

            try
            {
                // XPath select setting "add" element that contains this key 	  
                XmlElement addElem = (XmlElement)node.SelectSingleNode("//add[@key='" + key + "']");
                if (addElem != null)
                {
                    addElem.SetAttribute("value", value);
                }
                // not found, so we need to add the element, key and value
                else
                {
                    XmlElement entry = cfgDoc.CreateElement("add");
                    entry.SetAttribute("key", key);
                    entry.SetAttribute("value", value);
                    node.AppendChild(entry);
                }
                //save it
                saveConfigDoc(cfgDoc, docName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetValue(string key, string defaultvalue)
        {
            XmlDocument cfgDoc = new XmlDocument();
            try { 
            loadConfigDoc(cfgDoc);
            // retrieve the appSettings node 
            node = cfgDoc.SelectSingleNode("//appSettings");
            }
            catch { return defaultvalue; }
            if (node == null)
            {
              //  throw new System.InvalidOperationException("appSettings section not found");
                return defaultvalue;
            }

            try
            {
                // XPath select setting "add" element that contains this key 	  
                XmlElement addElem = (XmlElement)node.SelectSingleNode("//add[@key='" + key + "']");
                if (addElem != null)
                {
                    return addElem.GetAttribute("value");
                }
                // not found, so we need to add the element, key and value
                else
                {
                    return defaultvalue;
                }
            }
            catch
            {
                return defaultvalue;
            }
        }

        public bool removeElement(string elementKey)
        {
            try
            {
                XmlDocument cfgDoc = new XmlDocument();
                loadConfigDoc(cfgDoc);
                // retrieve the appSettings node 
                node = cfgDoc.SelectSingleNode("//appSettings");
                if (node == null)
                {
                    throw new System.InvalidOperationException("appSettings section not found");
                }
                // XPath select setting "add" element that contains this key to remove	  
                node.RemoveChild(node.SelectSingleNode("//add[@key='" + elementKey + "']"));

                saveConfigDoc(cfgDoc, docName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void saveConfigDoc(XmlDocument cfgDoc, string cfgDocPath)
        {
            try
            {
                XmlTextWriter writer = new XmlTextWriter(cfgDocPath, null);
                writer.Formatting = Formatting.Indented;
                cfgDoc.WriteTo(writer);
                writer.Flush();
                writer.Close();
                return;
            }
            catch
            {
                throw;
            }
        }
        private XmlDocument loadConfigDoc(XmlDocument cfgDoc)
        {
            //// load the config file 
            //if (Convert.ToInt32(ConfigType) == Convert.ToInt32(ConfigFileType.AppConfig))
            //{

            //    docName = ((Assembly.GetEntryAssembly()).GetName()).Name;
            //    docName += ".exe.config";
            //}
            //else
            //{
            //    docName = System.Web.HttpContext.Current.Server.MapPath("web.config");
            //}
            //docName = pathFilename;
            string xmlDoc = File.ReadAllText(docName);
            cfgDoc.LoadXml(xmlDoc);
            return cfgDoc;
        }

    }

}
