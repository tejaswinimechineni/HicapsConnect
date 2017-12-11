using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Request object used to send the Claim Details down.
    /// The addClaimLine method will raise an Error if invalid field details are sent down.  
    /// You must adhere to the rules as set out in the Field definition.  
    /// You must ensure that your code will not attempt to send down invalid details.
    /// </summary>
    public class ClaimRequest : BaseRequest
    {
        protected List<string> _itemList = new List<string>();
        public void setItemList(List<string> itemList)
        {
            _itemList.Clear();
            _itemList.AddRange(itemList);
        }
        private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }

        private string _providerNumberId;

        /// <summary>
        /// A 8 Alpha Numeric Field matching a provider number stored inside the terminal
        /// (Used in most request and response objects.)
        /// </summary>
        public string ProviderNumberId
        {
            get { return _providerNumberId; }
            set {
                _providerNumberId = Right ("00000000" + value,8);
                }
        }
        private string _membershipId;

        public string MembershipId
        {
            get { return _membershipId; }
            set { _membershipId = value; }
        }

        private bool _printReceiptOnTerminal;
        public bool PrintReceiptOnTerminal
        {
            get { return _printReceiptOnTerminal; }
            set { _printReceiptOnTerminal = value; }
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
        // Claim Data format stored as a string array to help serialisation
        // Format is  + Separator
        // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
        // 
        private List<string> _claimDetails;

        public List<string> ClaimDetails
        {
            get { return _claimDetails; }
            set { _claimDetails = value; }
        }

        // Patient Data

        private List<string> _patientNameDetails;

        public List<string> PatientNameDetails
        {
            get { return _patientNameDetails; }
            set { _patientNameDetails = value; }
        }
        public ClaimRequest()
        {
            ClaimDetails = new List<string>();
            PatientNameDetails = new List<string>();
            TransactionAmount = 0;
            
        }
        // This is to Flag for Non-Settlement Claims
        // PMS do not use this normally, and is restricted
        // via PMSKey on the Terminal.
        private bool _nonSettlementFlag = false;

        public bool NonSettlementFlag
        {
            get { return _nonSettlementFlag; }
            set { _nonSettlementFlag = value; }
        }

        public void addPatientName(string patientId, string patientName)
        {
            string zerofill = "00000000";
            string errorMessage = "";
            errorMessage += validatePatientName(patientName);
            errorMessage += validatePatientId(patientId);
            if (errorMessage.Length > 0) { throw new InvalidCastException(errorMessage); }
            if (PatientNameDetails.Count == 10) { throw new InvalidCastException("Only 10 different patients may be added"); };

            patientId = Right((zerofill + patientId ?? "00".Trim()), 2);
            patientName = patientName.Trim();
            PatientNameDetails.Add(patientId + patientName.Trim());
        }
        public void addClaimLine(string patientId, string itemNumber, string bodyPart, DateTime serviceDate, decimal fee)
        {
            string data = "";
            string dateservice = "";
            string feeamount = "00";
            string zerofill = "00000000";
            string errorMessage = "";

            errorMessage += validatePatientId(patientId);
            errorMessage += validateItemNumber(itemNumber);
            errorMessage += validateBodyPart(bodyPart);
            errorMessage += validateFeeAmount(fee);
            errorMessage += validateServiceDate(serviceDate);
            if (errorMessage.Length > 0) { throw new InvalidCastException(errorMessage); }

            patientId = Right((zerofill + patientId ?? "00".Trim()), 2);
            //itemNumber = Right(zerofill + itemNumber ?? "0000".Trim(), 4);
            itemNumber = Left(itemNumber + new string(' ', 4), 4);
            bodyPart = Right(zerofill + bodyPart ?? "00".Trim(), 2);
            // 
            if (!isNumeric(patientId)) { patientId = "00"; throw new InvalidCastException("Invalid patient id, PatientID must contain numeric values only eg 00"); }
            if (!isSpaceAlphaNumeric(itemNumber)) { itemNumber = "0000"; throw new InvalidCastException("Invalid Item Number, Item Number must only contain AlphaNumeric values only eg A-Z, a-z, 0-9, and spaces"); }
            if (!isAlphaNumeric(bodyPart)) { itemNumber = "00"; throw new InvalidCastException("Invalid Bodypart id, Bodypart must contain numeric values only eg 11"); }
            dateservice = Right(zerofill + serviceDate.Day.ToString(), 2) + Right(zerofill + serviceDate.Month.ToString(), 2);
            // Get fee into 012500" format for 125.00
            feeamount = "00000000" + (fee * System.Convert.ToDecimal(100)).ToString();
            if (feeamount.IndexOf('.') > 0) { feeamount = Left(feeamount, feeamount.IndexOf('.')); }
            feeamount = Right(feeamount, 6);

            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            data = patientId + itemNumber + bodyPart + dateservice + feeamount;
            ClaimDetails.Add(data);

            TransactionAmount += fee;
        }


       public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTransactionAmount(TransactionAmount, "Claim");
            if (ClaimDetails == null || ClaimDetails.Count <= 0)
            {
                validationMessage += "Claim must have at least one line";
            }
            if (ClaimDetails.Count > 24 )
            {
                validationMessage += "Claim can only have a maximum of 24 lines";
            }
            
           validationMessage += validateProviderNumberId(ProviderNumberId);

           foreach(string myRow in ClaimDetails)
           {
               validationMessage += validateClaimLine(myRow);
           }
            return checkValidationMessage(validationMessage);
           
          
        }
    
    }
}
