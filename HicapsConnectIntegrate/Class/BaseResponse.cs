using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Identifies the result of the transaction in a response. Response code should be set to ‘00’ for all request messages.
    /// </summary>
    public class BaseResponse: BaseMessage
    {
  
        private string _comPort;

        public string ComPort
        {
            get { return _comPort; }
            set { _comPort = value; }
        }

        private DateTime _responseTime = DateTime.Now;

        /// <summary>
        /// Time of Response
        /// </summary>
        public DateTime ResponseTime
        {
            get { return _responseTime; }
            set { _responseTime = value; }
        }
        private string _responseText;

        /// <summary>
        /// Text equivilant of ResponseCode
        /// </summary>
        public string ResponseText
        {
            get { return _responseText; }
            set { _responseText = value; }
        }

        private string _responseCode;
        /// <summary>
        /// Identifies the result of the transaction in a response. Response code should be set to ‘00’ for all request messages.
        /// </summary>
        /// <remarks></remarks>
        public string ResponseCode
        {
            get { return _responseCode; }
            set { _responseCode = value; }
        }
        private string _serverVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string ServerVersion
        {
            get { return _serverVersion; }
            set {  }
        }
        /// <summary>
        /// Version of the Client.
        /// </summary>
        private string _clientVersion = "";
        public string ClientVersion
        {
            get { return _clientVersion; }
            set { _clientVersion = value; }
        }

    }
}
