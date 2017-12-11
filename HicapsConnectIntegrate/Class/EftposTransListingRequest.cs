using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to request that the terminal return a list of all Eftpos Transactions that have been processed through the Terminal.
    /// </summary>
   public class EftposTransListingRequest : BaseRequest 
    {
        // Required for Multi-Merchant
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }
    }
}
