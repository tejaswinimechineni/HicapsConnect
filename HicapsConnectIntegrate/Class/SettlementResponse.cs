using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object describing the result of the transaction
    /// </summary>
   public class SettlementResponse : BaseResponse 
    {
        private string _authResponseCode;

        public string AuthResponseCode
        {
            get { return _authResponseCode; }
            set { _authResponseCode = value; }
        }
        private string _acquirerId;

        public string AcquirerId
        {
            get { return _acquirerId; }
            set { _acquirerId = value; }
        }

        private string _terminalId;

        public string TerminalId
        {
            get { return _terminalId; }
            set { _terminalId = value; }
        }
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }
        private string _providerName;

        public string ProviderName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }
        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTerminalId(TerminalId);
            return checkValidationMessage(validationMessage);
        }

    }
}
