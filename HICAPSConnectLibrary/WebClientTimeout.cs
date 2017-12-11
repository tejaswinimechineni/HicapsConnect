using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace HICAPSConnectLibrary
{
    public class WebClientTimeout : WebClient
    {
        // Timeout in milliseconds, default = 600,000 msec
        public int Timeout { get; set; }

        public WebClientTimeout()
        {
            this.Timeout = 10000;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var objWebRequest = base.GetWebRequest(address);
            objWebRequest.Timeout = this.Timeout;
            return objWebRequest;
        }
    }
}
