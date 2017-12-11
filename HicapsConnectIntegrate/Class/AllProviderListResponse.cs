using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object containing a  string array which is used to list the ProviderListDetails.
    /// </summary>
    public class AllProviderListResponse : BaseResponse
    {
        private List<string> _providerListDetails;

        public List<string> ProviderListDetails
        {
            get { return _providerListDetails; }
            set { _providerListDetails = value; }
        }
        public AllProviderListResponse()
        {
            ProviderListDetails = new List<string>();
        }
        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
        public string[] ProviderListDetailsStr
        {
            get { return _providerListDetails.ToArray(); }
            set { }
        }
        public override string[] breakupLineFields(int Index)
        {
            string[] myData = new string[6];
            try
            {
                string fieldData = ProviderListDetails[Index];
                myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                myData[2] = fieldData.Substring(23, 8).Trim(); // Provider Id
                myData[3] = fieldData.Substring(31, 16).Trim(); //Provider Name
                myData[4] = fieldData.Substring(47, 1).Trim(); // Provider Type
                myData[5] = fieldData.Substring(48, 8).Trim(); // Payee Provider Id
                myData[6] = fieldData.Substring(50, 2).Trim(); // Provider List Num

            }
            catch (Exception )
            {
                // throw new IndexOutOfRangeException();
            }

            return myData;
        }
    }
}
