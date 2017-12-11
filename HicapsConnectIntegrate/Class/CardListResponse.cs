using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object containing a  string array which is used to list the CardListDetails.
    /// </summary>
    public class CardListResponse : BaseResponse
    {
       
        private List<string> _cardFundDetails;

        public List<string> CardFundDetails
        {
            get { return _cardFundDetails; }
            set { _cardFundDetails = value; }
        }
        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
        public CardListResponse()
        {
            CardFundDetails = new List<string>();
        }

        public override string[] breakupLineFields(int Index)
        {
            string[] myData = new string[3];
            try
            {
                string fieldData = CardFundDetails[Index];
                myData[0] = fieldData.Substring(0, 6).Trim();  //Fund Name
                myData[1] = fieldData.Substring(6, 14).Trim(); // Card Prefix
                myData[2] = fieldData.Substring(20, 2).Trim(); // Line Count
             }
            catch (Exception )
            {
                // throw new IndexOutOfRangeException();
            }
            return myData;
        }

    }
}
