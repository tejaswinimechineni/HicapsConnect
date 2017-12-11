using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object containing a string array which is used to list the MerchantListDetails.
    /// </summary>
   public class AllMerchantListResponse : BaseResponse 
    {
               private List<string> _merchantListDetails;

        public List<string> MerchantListDetails
        {
            get { return _merchantListDetails; }
            set { _merchantListDetails = value; }
        }
        public AllMerchantListResponse()
        {
            MerchantListDetails = new List<string>();
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
                string fieldData = MerchantListDetails[Index];
                myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                myData[2] = fieldData.Substring(23, 8).Trim(); // Provider Id
                myData[3] = fieldData.Substring(31, 32).Trim(); //Description
            }
            catch (Exception )
            {
                // throw new IndexOutOfRangeException();
            }

            return myData;
        }
    }
}
