using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

namespace HicapsConnectIntegrate.Class
{
    public class BaseRequest : BaseMessage
    {

        private string _pmsKey;

        public string PmsKey
        {
            get { return _pmsKey; }
            set { _pmsKey = value; }
        }

        private string _encryptMerchantId;

        public string EncryptMerchantId
        {
            get { return _encryptMerchantId; }
            set { _encryptMerchantId = value; }
        }
        private string _encryptTerminalId;

        public string EncryptTerminalId
        {
            get { return _encryptTerminalId; }
            set { _encryptTerminalId = value; }
        }
        private string _clientVersion;
        /// <summary>
        /// Version of the Client.
        /// </summary>
        public string ClientVersion
        {
            get { return _clientVersion; }
            set { _clientVersion = value; }
        }
        // Help Generic Methods...
        public string getPropertyValueAsString(string propertyName)
        {
            PropertyInfo[] properties = this.GetType().GetProperties();
            List<string> myList = new List<string>();
            foreach (PropertyInfo property in properties)
            {

                if (property.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (property.GetValue(this, null) != null && property.GetValue(this, null).GetType() == myList.GetType())
                    {
                        return "";
                    }
                    else
                    {
                        return String.Format("{0}", property.GetValue(this, null) ?? "");
                    }
                }
            }
            throw new ArgumentOutOfRangeException(String.Format("Property [{0}] does not exist", propertyName));
            return "";
        }
    }
}
