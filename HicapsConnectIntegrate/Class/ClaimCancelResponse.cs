using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object detailing the success of failure of the transaction.
    /// </summary>
    public class ClaimCancelResponse : BaseResponse
    {
        private string _rrnNumber;
        public string RrnNumber
        {
            get { return _rrnNumber; }
            set { _rrnNumber = value; }
        }

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
        private DateTime _transactionDate;
        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }

        private string _terminalId;
        public string TerminalId
        {
            get { return _terminalId; }
            set { _terminalId = value; }
        }
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }

        private string _providerName;

        public string ProviderName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }
        private string _providerNumberId;

        public string ProviderNumberId
        {
            get { return _providerNumberId; }
            set { _providerNumberId = value; }
        }
        private string _membershipId;

        public string MembershipId
        {
            get { return _membershipId; }
            set { _membershipId = value; }
        }
        private decimal _benefitAmount;

        public decimal BenefitAmount
        {
            get { return _benefitAmount; }
            set { _benefitAmount = value; }
        }

        // Claim Data format stored as a string array to help serialisation
        //Format is  + Separator
        // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
        // 
        private List<string> _claimDetails;

        public List<string> ClaimDetails
        {
            get { return _claimDetails; }
            set { _claimDetails = value; }
        }
        public ClaimCancelResponse() 
        {
            ClaimDetails = new List<string>();
        }
        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTerminalId(TerminalId);
            return checkValidationMessage(validationMessage);
        }

    }
}
