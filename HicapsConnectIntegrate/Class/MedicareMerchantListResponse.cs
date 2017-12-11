using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HicapsConnectIntegrate.Class
{
    public class MedicareMerchantListResponse : BaseResponse
    {

        private List<string> _medicareMerchantListDetails;

        public List<string> MedicareMerchantListDetails
        {
            get { return _medicareMerchantListDetails; }
            set { _medicareMerchantListDetails = value; }
        }
        public MedicareMerchantListResponse()
        {
            MedicareMerchantListDetails = new List<string>();
        }
        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
        public override string[] breakupLineFields(int Index)
        {
            string[] myData = new string[4];
            try
            {
                string fieldData = MedicareMerchantListDetails[Index];
                myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                myData[2] = fieldData.Substring(23, 8).Trim(); // Provider Id
                myData[3] = fieldData.Substring(31, 16).Trim(); //Provider Name
            }
            catch (Exception )
            {
                // throw new IndexOutOfRangeException();
            }

            return myData;
        }
    }
}
