using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;


namespace HicapsConnectIntegrate.Class
{
    
   public class TerminalTestResponse : BaseResponse 
    {

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
        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
        public string IP { get; set; }
        public int Port { get; set; }

        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTerminalId(TerminalId);
            return checkValidationMessage(validationMessage);
        }


      
    }
}
