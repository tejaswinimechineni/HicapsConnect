using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class AllItemResponseCodeListResponse : BaseResponse
    {
        private List<string> _itemResponseCodeDescription;

        public List<string> ItemResponseCodeDescription
        {
            get { return _itemResponseCodeDescription; }
            set { _itemResponseCodeDescription = value; }
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
        public AllItemResponseCodeListResponse()
        {
            _itemResponseCodeDescription = new List<string>();
        }
    }
}
