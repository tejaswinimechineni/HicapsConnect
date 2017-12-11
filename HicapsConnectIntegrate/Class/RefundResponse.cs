using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object containing the Result of the Refund Transaction.
    /// </summary>
    public class RefundResponse : BaseResponse
    {
        private string _authResponseCode;

        public string AuthResponseCode
        {
            get { return _authResponseCode; }
            set { _authResponseCode = value; }
        }
        private string _approvalCode;

        public string ApprovalCode
        {
            get { return _approvalCode; }
            set { _approvalCode = value; }
        }

        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
       
        private string _printReceiptData;

        public string PrintReceiptData
        {
            get { return _printReceiptData; }
            set { _printReceiptData = value; }
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

        private string _invoiceNumber;

        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set { _invoiceNumber = value; }
        }
        private string _rrnNumber;

        public string RrnNumber
        {
            get { return _rrnNumber; }
            set { _rrnNumber = value; }
        }
        private string _terminalSwipe;

        public string TerminalSwipe
        {
            get { return _terminalSwipe; }
            set { _terminalSwipe = value; }
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
        private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }
        private decimal _surchargeAmount;

        public decimal SurchargeAmount
        {
            get { return _surchargeAmount; }
            set { _surchargeAmount = value; }
        }
        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTerminalId(TerminalId);
            return checkValidationMessage(validationMessage);
        }

    }
}
