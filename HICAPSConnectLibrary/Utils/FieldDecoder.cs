using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HICAPSConnectLibrary.Encrypt;

namespace HICAPSConnectLibrary.Utils
{
    public static class FieldDecoder
    {
        /// <summary>
        /// Decrypt a field
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="KVC"></param>
        /// <returns></returns>
        internal static string decodeAlphaEncrypt(string fieldValue, byte[] KVC)
        {
            if (string.IsNullOrEmpty(fieldValue)) { return fieldValue; }
            if (KVC == null) { return fieldValue; }

            byte[] dataArray = Encoding.Default.GetBytes(fieldValue);
            byte[] dataArrayOutput = new byte[dataArray.Length]; // OFP = same length as input array
            dataArrayOutput = TripleDesOFB.OFBCrypt(dataArray, KVC);
            return Encoding.Default.GetString(dataArrayOutput);
        }
    }
}
