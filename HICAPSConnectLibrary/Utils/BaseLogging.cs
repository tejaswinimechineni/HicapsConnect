using System;
using System.Collections.Generic;
using System.Reflection;


namespace HICAPSConnectLibrary.Utils
{
    public abstract class BaseLogging 
    {
        protected string GenerateHeader(string message, string headerString)
        {
            byte[] data = new byte[]{};
            headerString = Assembly.GetExecutingAssembly().GetName().Name;
            headerString = GenerateHeader(data, headerString);
            headerString = headerString + Environment.NewLine + WrapString(message.Trim(), 40) + Environment.NewLine;
            return headerString;
        }

        protected string GenerateHeader(byte[] data,string headerString)
        {

            headerString = String.Format("{0} {1:dd MMM yyyy H:mm:ss:fff} Version {2}", headerString, DateTime.Now, Assembly.GetExecutingAssembly().GetName().Version);
            headerString = String.Format("{0}{2}{1}", headerString, new String('-', headerString.Length), Environment.NewLine);
            if (data != null && data.Length > 0)
            {
                headerString = String.Format("{0}{2}{1}{2}", headerString, WrapString(BitConverter.ToString(data), 46), Environment.NewLine);
            }
            else
            {
                headerString += Environment.NewLine;
            }
            return headerString;
        }
        private string WrapString(string text, int maxLength)
        {
            string returnString = "";

            foreach (string inputString in Wrap(text, maxLength))
            {
                returnString += String.Format("{0}{1}     {2}{3}", inputString, new String(' ', 48 - inputString.Length), ConvertToByteString(inputString),Environment.NewLine);
            }

            return returnString;
        }

        private string ConvertToByteString(string hexValues)
        {
            string[] hexValuesSplit = hexValues.Split(' ');
            string returnString = "";
            foreach (var hex in hexValuesSplit)
            {
                // Convert the number expressed in base-16 to an integer.
                int value = Convert.ToInt32(hex, 16);
                // Any non-alpha show as full stop.
                if (value < 32 || value > 127) { value = 46; }
                // Get the character corresponding to the integral value.
                returnString += Char.ConvertFromUtf32(value);
            }
            return returnString;
        }
        private IEnumerable<string> Wrap(string text, int maxLength)
        {

            // Return empty list of strings if the text was empty
            if (text.Length == 0) return new List<string>();

            var words = text.Split('-');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var currentWord in words)
            {

                if ((currentLine.Length > maxLength) ||
                    ((currentLine.Length + currentWord.Length) > maxLength))
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }

                if (currentLine.Length > 0)
                    currentLine += " " + currentWord;
                else
                    currentLine += currentWord;

            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);


            return lines;
        }
    }
}
