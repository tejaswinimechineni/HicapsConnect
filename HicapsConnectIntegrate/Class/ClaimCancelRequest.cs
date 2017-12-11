using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to Cancel a previously paid HicapsClaim.
    /// </summary>
    public class ClaimCancelRequest : BaseRequest
    {
        private string _rrnNumber;

        public string RrnNumber
        {
            get { return _rrnNumber; }
            set { _rrnNumber = value; }
        }
        private string _providerNumberId;

        public string ProviderNumberId
        {
            get { return _providerNumberId; }
            set
            {
                _providerNumberId = Right("00000000" + value, 8);
            }
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
        private bool _printReceiptOnTerminal;
        public bool PrintReceiptOnTerminal
        {
            get { return _printReceiptOnTerminal; }
            set { _printReceiptOnTerminal = value; }
        }
        private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }
        // Patient Data

        private List<string> _patientNameDetails;

        public List<string> PatientNameDetails
        {
            get { return _patientNameDetails; }
            set { _patientNameDetails = value; }
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
       public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateCancelTransactionAmount(TransactionAmount, "Transaction");
            validationMessage += validateRRN(RrnNumber);
            validationMessage += validateProviderNumberId(ProviderNumberId);

            return checkValidationMessage(validationMessage);
        }
    }
}
