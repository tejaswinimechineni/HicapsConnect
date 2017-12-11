using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class AllItemListResponse : BaseResponse 
    {
        private List<string> _itemListDescription;

        public List<string> ItemListDescription
        {
            get { return _itemListDescription; }
            set { _itemListDescription = value; }
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
        public AllItemListResponse()
        {
            _itemListDescription = new List<string>();
        }
    }
}
