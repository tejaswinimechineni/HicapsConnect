using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// The first row in the array returns a string which contains the details of the HicapsTotalsResponse:
    /// </summary>
    public class HicapsTotalsResponse : BaseResponse
    {
        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }

        private List<string> _hicapsTotalsListDetails;

        public List<string> HicapsTotalsListDetails
        {
            get { return _hicapsTotalsListDetails; }
            set { _hicapsTotalsListDetails = value; }
        }
        public HicapsTotalsResponse()
        {
            HicapsTotalsListDetails = new List<string>();
        }
    }
}
