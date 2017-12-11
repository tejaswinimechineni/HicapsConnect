using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object detailing the success or failure of the Claim Transaction.
    /// The Claim Line Assessment Details are set out in the String array ClaimListDetails.  
    /// Refer to the ClaimDetails definition for the individual field breakup.
    /// </summary>
    public class ClaimResponse : BaseResponse
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

         private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }

        private decimal _benefitAmount;

        public decimal BenefitAmount
        {
            get { return _benefitAmount; }
            set { _benefitAmount = value; }
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
        private string _rrnNumber;

        public string RrnNumber
        {
            get { return _rrnNumber; }
            set { _rrnNumber = value; }
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

        public ClaimResponse()
        {
            ClaimDetails = new List<string>();
            PatientNameDetails = new List<string>();
            TransactionAmount = 0;
        }
        private string _providerName;

        public string ProviderName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }

       private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }
        public void addClaimResultLine(string patientId, string itemNumber, string bodyPart, DateTime serviceDate, decimal fee, decimal benefit,string responseCode)
        {
            string data = "";
            string dateservice = "";
            string feeamount = "";
            string benefitamount = "";
            string zerofill = "00000000";

            patientId = Right((zerofill + patientId), 2);
            itemNumber = Right(zerofill + itemNumber, 4);
            bodyPart = Right(zerofill + bodyPart, 2);
            // 
            dateservice = serviceDate.ToString("DDMM");
            
            // Get fee into 012500" format for 125.00
            feeamount = "00000000" + (fee * System.Convert.ToDecimal(100)).ToString();
            feeamount = Left(feeamount, feeamount.IndexOf('.'));
            feeamount = Right(feeamount, 6);

            benefitamount = "00000000" + (benefit * System.Convert.ToDecimal(100)).ToString();
            benefitamount = Left(benefitamount, benefitamount.IndexOf('.'));
            benefitamount = Right(benefitamount, 6);

            responseCode = Right((zerofill + responseCode), 2);

            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            data = patientId + itemNumber + bodyPart + dateservice + feeamount + benefitamount + responseCode;
            ClaimDetails.Add(data);

            TransactionAmount += fee;
        }
        // Patient Data

        private List<string> _patientNameDetails;

        public List<string> PatientNameDetails
        {
            get { return _patientNameDetails; }
            set { _patientNameDetails = value; }
        }
        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTerminalId(TerminalId);
            return checkValidationMessage(validationMessage);
        }

    }
}
