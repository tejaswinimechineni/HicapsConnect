using System;
using HICAPSConnectLibrary.Encrypt;
using System.Reflection;
using System.IO;

namespace HICAPSConnectLibrary.Utils
{
    /// <summary>
    /// Provides encrypted Logging parameters.
    /// </summary>
    public class SLogging : BaseLogging, ILogging
    {
        private readonly string _filename = "logfile.txt";
        private string _publicKey = "";
        private string _privateKey = "";

        #region Private Properties

        private String PublicKey
        {
            get
            {
                if (String.IsNullOrEmpty(_publicKey))
                {
                    _publicKey = LoadPublicKey();
                }

                return _publicKey;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// ctor.  Must set Public key using SetPublicKey Method
        /// </summary>
        /// <param name="filename"></param>
        public SLogging(string filename)
        {
            _filename = filename;
            _publicKey = LoadPublicKey();
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="publicKey"></param>
        public SLogging(string filename, string publicKey)
        {
            _filename = filename;
            _publicKey = publicKey;
        }

        #endregion

        /// <summary>
       /// Logs data to file.
       /// </summary>
       /// <param name="data"></param>
       /// <param name="headerString"></param>
        public void LogData(byte[] data, string headerString)
        {
            if (_publicKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentOutOfRangeException("Public key is not set");
            }
            try
            {
                headerString = GenerateHeader(data, headerString);

                var fileWrite = new SFileWrite(PublicKey);
                fileWrite.AppendAllText(_filename, headerString);

            }
            catch (Exception) { }
        }

        public void LogMessage(String message, String header)
        {
            var fileWrite = new SFileWrite(PublicKey);

            fileWrite.AppendAllText(_filename, header + message + "\r\n");
        }

        public string ReadLog(bool useNewMethod)
        {
            string logText = String.Empty;

            if (_privateKey.IsNullOrWhiteSpace())
            {
                throw new ArgumentOutOfRangeException("Private key is not set");
            }
            try
            {                
                var fileRead = new SFileRead(_privateKey);

                if (useNewMethod)
                    logText = fileRead.ReadAllText(_filename, true);
                else
                    logText = fileRead.ReadAllText(_filename);
            }
            catch (Exception) { }

            return logText;
        }

        /// <summary>
        /// Sets or overrides public key defined through constructor.
        /// </summary>
        /// <param name="publicKey"></param>
        public void SetPublicKey(string publicKey)
        {
            _publicKey = publicKey;
        }

        /// <summary>
        /// Sets or overrides private key defined through constructor.
        /// </summary>
        /// <param name="privateKey"></param>
        public void SetPrivateKey(string privateKey)
        {
            _privateKey = privateKey;
        }

        /// <summary>
        /// Loads the public key from project resource public.xml
        /// </summary>
        /// <returns>String</returns>
        private String LoadPublicKey()
        {
            String publicKey = String.Empty;
            String resourceName = "HICAPSConnectLibrary.public.xml";
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        publicKey = reader.ReadToEnd();
                    }
                }
                else
                    throw new Exception("Could not load public key.");
            }

            return publicKey;
        }
    }
}
