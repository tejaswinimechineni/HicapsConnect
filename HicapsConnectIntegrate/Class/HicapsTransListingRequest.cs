using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Send a request to the terminal to return a list of all Hicaps Transactions that have been send to the terminal
    /// </summary>
   public class HicapsTransListingRequest : BaseRequest 
    {
        private string _providerNumberId;

        public string ProviderNumberId
        {
            get { return _providerNumberId; }
            set
            {
                _providerNumberId = Right("00000000" + value, 8);
            }
        }
    }
}
