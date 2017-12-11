using System;
using System.IO;
using System.Text;

namespace HICAPSConnectLibrary.Encrypt
{
    /// <summary>
    /// Secure File Reader
    /// Companion of SFileWrite class
    /// </summary>
    public class SFileRead 
    {
        private readonly string _privateKey;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="privateKey">Value of Private key in XML CSP format</param>
        public SFileRead(string privateKey)
        {
            _privateKey = privateKey;
        }
        /// <summary>
        /// Reads all data in a File encrypted using the SFileWrite class
        /// </summary>
        /// <param name="filename">Name of the file</param>
        /// <returns></returns>
        public string ReadAllText(string filename)
        {
            string[] data = File.ReadAllLines(filename);
            var eBuffer = new StringBuilder(1024);
            var dBuffer = new StringBuilder(1024);

            foreach (var line in data)
            {
                if (line.StartsWith("~"))
                {
                    if (eBuffer.Length > 0)
                    {
                        // End of encryption data-block
                        try
                        {
                            dBuffer.Append(RSA.Decrypt(_privateKey, eBuffer.ToString()));
                        }
                        catch
                        {
                            dBuffer.AppendLine("{ Decryption-Failed, Bad Data-Block }");
                        }
                        //wipe buffer to start again
                        //4.5 Method can be used here, and more effecient
                        //eBuffer.Clear();

                        eBuffer = new StringBuilder(1024);
                    }
                    
                }
                else
                {
                    eBuffer.AppendLine(line);
                }
            }
            return dBuffer.ToString();
        }

        public string ReadAllText(string filename, Boolean newMethod)
        {
            string[] data = File.ReadAllLines(filename);
            var eBuffer = new StringBuilder(1024);
            var dBuffer = new StringBuilder(1024);
            RSA rsa = new RSA(_privateKey);

            foreach (var line in data)
            {
                if (line.StartsWith("~"))
                {
                    if (eBuffer.Length > 0)
                    {
                        // End of encryption data-block
                        try
                        {
                            dBuffer.Append(rsa.Decrypt(eBuffer.ToString()));
                        }
                        catch
                        {
                            dBuffer.AppendLine("{ Decryption-Failed, Bad Data-Block }");
                        }
                        //wipe buffer to start again
                        //4.5 Method can be used here, and more effecient
                        //eBuffer.Clear();

                        eBuffer = new StringBuilder(1024);
                    }

                }
                else
                {
                    eBuffer.AppendLine(line);
                }
            }
            return dBuffer.ToString();
        }
    }
}
