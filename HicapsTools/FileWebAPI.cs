using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace HicapsTools
{
    static class FileWebAPI
    {
        private class HICAPSWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 10 * 1000; // 10 seconds
                return w;
            }
        }

        static string addressURL = "www.hicapsconnect.com.au/API/";
        static void CheckForUpdatesAndDownload(string url, string destination)
        {
            try
            {
                HICAPSWebClient myClient = new HICAPSWebClient();
                myClient.DownloadFile(addressURL, destination);
            }
            catch (Exception) { }
        }

    }
}
