using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class AllTransCodeListResponse : BaseResponse 
    {
        private List<string> _transCodeDescription;

        public List<string> TransCodeDescription
        {
            get { return _transCodeDescription; }
            set { _transCodeDescription = value; }
        }
        public AllTransCodeListResponse()
        {
            _transCodeDescription = new List<string>();
        }
        private DateTime _transactionDate;
        public DateTime TransactionDate
        {
            get
            {
                return _transactionDate;
            }
            set
            {
                _transactionDate = value;
            }
        }
    }
}
