using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HICAPSConnectLibrary.Encrypt
{
    /// <summary>
    /// Secure File Write
    /// Companion of SFileRead class
    /// </summary>
   public class SFileWrite 
    {
       private readonly string _publicKey;
       private readonly int _keySize;
       
       /// <summary>
       /// ctor
       /// </summary>
       /// <param name="publicKey">Value of Public key in XML CSP format</param>
       public SFileWrite(string publicKey)
       {
           _publicKey = publicKey;
           _keySize = RSA.KeySize(_publicKey);

       }
       /// <summary>
       /// Writes all encrtyped data to file.
       /// If File already exists, existing data is overwritten
       /// </summary>
       /// <param name="filename">Name of file</param>
       /// <param name="data">Un-Encrypted Text</param>
       public void WriteAllText(string filename, string data)
       {
           if (File.Exists(filename))
           {
               File.WriteAllText(filename,"");
           }

           File.AppendAllText(filename,data);
       }

       /// <summary>
       /// Writes all encrtyped data to file.
       /// If File already exists, data is Appended
       /// </summary>
       /// <param name="filename">Name of file</param>
       /// <param name="data">Un-Encrypted Text</param>
       public void AppendAllText(string filename, string data)
       {
           /* 
            * RSA encryption is only meant to encrypt small blocks of data of up to
            * (KeySize - 384) / 8) + 7
            * 
            * The alternative is to instead RSA public encrypt and store a random generated key with the 
            * AES symmetric encrypted data using the random key
            * 
            * However for the purposes of this file, the data should be very small anyway and at least close 
            * to (KeySize - 384) / 8) + 7
            */

           var sData = SplitBy(data, (((_keySize - 384)/ 8) + 7) );
           foreach (var block in sData)
           {
               string eData = RSA.Encrypt(_publicKey, block);
               File.AppendAllText(filename, "~" + Environment.NewLine + eData + Environment.NewLine + "~" + Environment.NewLine);
           }
       }

       private IEnumerable<string> SplitBy(string str, int chunkLength)
       {
           if (String.IsNullOrEmpty(str)) throw new ArgumentException();
           if (chunkLength < 1) throw new ArgumentException();

           for (int i = 0; i < str.Length; i += chunkLength)
           {
               if (chunkLength + i > str.Length)
                   chunkLength = str.Length - i;

               yield return str.Substring(i, chunkLength);
           }
       }
    }
}
