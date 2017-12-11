using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to request that the terminal perform a settlement of all Transactions that have been processed through the Terminal.
    /// AcquirerId must be set.  Normally this is set to 1.
    /// </summary>
    public class SettlementRequest : BaseRequest 
    {
        private string _acquirerId;

        public string AcquirerId
        {
            get { return _acquirerId; }
            set { _acquirerId = value; }
        }
        private string _settlementType;

        public string SettlementType
        {
            get { return _settlementType; }
            set { _settlementType = value; }
        }
        private bool _printTxnListing;

        public bool PrintTxnListing
        {
            get { return _printTxnListing; }
            set { _printTxnListing = value; }
        }
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }
    }
}
