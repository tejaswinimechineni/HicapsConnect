using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HicapsTools
{
    public static class Extensions
    {

        public static bool isNumeric(this string strValue)
        {
            if (String.IsNullOrEmpty(strValue)) { return false; }
            string regExpression = @"/(^-?\d\d*\.\d*$)|(^-?\d\d*$)|(^-?\.\d\d*$)/";
            return Regex.IsMatch(strValue, regExpression);
        }

        public static bool isAlphaNumeric(this string strValue)
        {
            if (String.IsNullOrEmpty(strValue)) { return false; }

            string regExpression = @"^[a-zA-Z0-9]+$";
            return Regex.IsMatch(strValue, regExpression);
        }

        public static bool isSpaceAlphaNumeric(this string strValue)
        {
            if (String.IsNullOrEmpty(strValue)) { return false; }

            string regExpression = @"^[a-zA-Z0-9 ]+$";
            return Regex.IsMatch(strValue, regExpression);
        }
        public static string LeftSpacePad(this string field, int length)
        {
            return LeftPad(field, ' ', length);
        }
        public static string LeftPad(this string field, char character, int length)
        {
            field = field.Trim();
            field = new String(character, length) + field;
            return Right(field, length);
        }
        public static string Right(this string rightString, int length)
        {
            return rightString.Substring(rightString.Length - length);
        }
        public static string Left(this string leftString, int length)
        {
            return leftString.Substring(0, length);
        }
        public static string CalculateMD5(this string rawstring)
        {
            // step 1, calculate MD5 hash from input
            if (string.IsNullOrEmpty(rawstring))
            {
                return string.Empty;
            }
            try
            {
                System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.Default.GetBytes(rawstring);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString();
            }
            catch { }
            return string.Empty;
        }
    }
}
