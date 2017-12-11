using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object containing the Card Details
    /// </summary>
    public class CardReadResponse : BaseResponse
    {
        

        private string _primaryAccountNumber;
        public string PrimaryAccountNumber
        {
            get
            {
                return _primaryAccountNumber;
            }
            set
            {
                _primaryAccountNumber = value;
            }
        }
        private string _expiryDate;
        public string ExpiryDate
        {
            get
            {
                return _expiryDate;
            }
            set
            {
                _expiryDate = value;
            }
        }
        private string _secondTrack;
        public string SecondTrack
        {
            get
            {
                return _secondTrack;
            }
            set
            {
                _secondTrack = value;
            }
        }
        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
        private string _trackdata;
        public string TrackData
        {
            get
            {
                return _trackdata;
            }
            set
            {
                _trackdata = value;
            }
        }
        public CardReadResponse() { }
       
        
    }
}
