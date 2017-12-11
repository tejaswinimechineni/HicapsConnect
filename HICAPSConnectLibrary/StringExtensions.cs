using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HICAPSConnectLibrary
{
    public static class StringExtensions
    {
        // If you go to .NET 4.5  this can be replaced with string.IsNullOrWhiteSpace()
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null) return true;
            return value.Trim().Length <= 0;

        }
        public static string SHA256(this string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                value = "";
            }
           
            string hash = String.Empty;
            byte[] crypto;
            using (var crypt = new SHA256Managed())
            {
                crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(value), 0, Encoding.UTF8.GetByteCount(value));
            }
            return crypto.Aggregate(hash, (current, bit) => current + bit.ToString("x2"));
        }

        public static int ToInt(this string value)
        {
            int iValue = 0;
            if (int.TryParse(value, out iValue))
            {
                return iValue;
            }
            return 0;
        }
        public static string Right(this string rightString, int length)
        {
            return rightString.Substring(rightString.Length - length);
        }

        public static string Left(this string leftString, int length)
        {
            return leftString.Substring(0, length);
        }
    }
}
