using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Contains a  list of all Hicaps Transactions that have been processed through the Terminal.
    /// For .NET languages, you may directly iterate through the List Collections to get each object.
    /// For those using the ActiveX control, you must use the getTransactionCount and 
    /// getTransaction Methods using a for-next loop.  
    /// The getTransaction Method is a 1-based method.  i.e. First object sits at Index 1.
    /// </summary>
   public class HicapsTransListingResponse : ClaimResponse 
    {
        private string _subTransCode;

        public string SubTransCode
        {
            get { return _subTransCode; }
            set { _subTransCode = value; }
        }

        private List<ClaimResponse > _claimTransactionList;

        public List<ClaimResponse> ClaimTransactionList
        {
            get { return _claimTransactionList; }
            set { _claimTransactionList = value; }
        }

        private List<QuoteResponse> _quoteTransactionList;

        public List<QuoteResponse> QuoteTransactionList
        {
            get { return _quoteTransactionList; }
            set { _quoteTransactionList = value; }
        }
        private List<ClaimCancelResponse> _claimCancelTransactionList;

        public List<ClaimCancelResponse> ClaimCancelTransactionList
        {
            get { return _claimCancelTransactionList; }
            set { _claimCancelTransactionList = value; }
        }
        public HicapsTransListingResponse() {
            ClaimTransactionList = new List<ClaimResponse>();
            QuoteTransactionList = new List<QuoteResponse>();
            ClaimCancelTransactionList = new List<ClaimCancelResponse>();
        }
    }
}
