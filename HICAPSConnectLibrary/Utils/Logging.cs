using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HICAPSConnectLibrary.Utils
{
    public class Logging :BaseLogging, ILogging
    {
        private readonly string _filename = "logfile.txt";
        public Logging(string filename)
        {
            _filename = filename;
        }

        public Logging()
        {
            
        }
        public void LogData(byte[] data, string headerString)
        {
            try
            {
               headerString = GenerateHeader(data, headerString);

                if (!Environment.UserInteractive)
                {
                    File.AppendAllText(_filename, headerString);
                }
                else
                {
                    Console.WriteLine(headerString);
                }
            }
            catch (Exception) { }

        }
 
    }
}
