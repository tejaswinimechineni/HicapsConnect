using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to send a Eftpos Refund Request to the terminal.
    /// </summary>
    public class RefundRequest : BaseRequest
    {
        // Required for Multi-Merchant
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }
        private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }
        // Required for Multi-Merchant
        private string _merchantPassword;

        public string MerchantPassword
        {
            get { return _merchantPassword; }
            set { _merchantPassword = value; }
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
        private bool _printMerchantReceipt = true;

        public bool PrintMerchantReceipt
        {
            get { return _printMerchantReceipt; }
            set { _printMerchantReceipt = value; }
        }
        private bool _printCustomerReceipt = true;

        public bool PrintCustomerReceipt
        {
            get { return _printCustomerReceipt; }
            set { _printCustomerReceipt = value; }
        }
        private bool _printCustomerReceiptPrompt = true;

        public bool PrintCustomerReceiptPrompt
        {
            get { return _printCustomerReceiptPrompt; }
            set { _printCustomerReceiptPrompt = value; }
        }
       public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTransactionAmount(TransactionAmount, "Refund");
            validationMessage += validateMerchantId(MerchantId);
            validationMessage += validateEftposPAN(PrimaryAccountNumber);

            validationMessage += validateExpiryDate(PrimaryAccountNumber, ExpiryDate);
            validationMessage += validateCCV(CCV);
            validationMessage += validateCCVReason(PrimaryAccountNumber, CCV, CCVReason);
            validationMessage += validateCCVSource(PrimaryAccountNumber, CCV, CCVSource);
           // validationMessage += validateRefundPassword(MerchantPassword);
            return checkValidationMessage(validationMessage);
        }
        
    }
}
