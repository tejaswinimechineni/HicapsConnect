using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to send a Eftpos CashoutRequest to the terminal.
    /// Please note, sending the CardNumber and CCV fields down to the terminal requires a valid PmsKey.
    /// </summary>
    public class CashoutRequest : BaseRequest
    {
        // Required for Multi-Merchant
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }

        private decimal _cashoutAmount;

        public decimal CashoutAmount
        {
            get { return _cashoutAmount; }
            set { _cashoutAmount = value; }
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
        private string _CCV;

        public string CCV
        {
            get { return _CCV; }
            set { _CCV = value; }
        }
        private int _CCVReason;

        public int CCVReason
        {
            get { return _CCVReason; }
            set { _CCVReason = value; }
        }
        private int _CCVSource;
        public int CCVSource
        {
            get { return _CCVSource; }
            set { _CCVSource = value; }
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
        private bool _printMerchantReceipt;

        public bool PrintMerchantReceipt
        {
            get { return _printMerchantReceipt; }
            set { _printMerchantReceipt = value; }
        }
        private bool _printCustomerReceipt;

        public bool PrintCustomerReceipt
        {
            get { return _printCustomerReceipt; }
            set { _printCustomerReceipt = value; }
        }
        private bool _printCustomerReceiptPrompt;

        public bool PrintCustomerReceiptPrompt
        {
            get { return _printCustomerReceiptPrompt; }
            set { _printCustomerReceiptPrompt = value; }
        }
       public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTransactionAmount(CashoutAmount, "Cashout");
            validationMessage += validateMerchantId(MerchantId);
            validationMessage += validateEftposPAN(PrimaryAccountNumber);
            validationMessage += validateExpiryDate(PrimaryAccountNumber,ExpiryDate);
            validationMessage += validateCCV(CCV);
            validationMessage += validateCCVReason(PrimaryAccountNumber, CCV, CCVReason);
            validationMessage += validateCCVSource(PrimaryAccountNumber, CCV, CCVSource);
            return checkValidationMessage(validationMessage);
        }
       
    }
}
