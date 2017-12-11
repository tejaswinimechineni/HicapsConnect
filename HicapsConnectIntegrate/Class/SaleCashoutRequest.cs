using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to send a Eftpos Sale Cashout Request to the terminal.
    /// Please note, sending the CardNumber and CCV fields down to the terminal requires a valid PmsKey.
    /// </summary>
    public class SaleCashoutRequest : SaleRequest
    {
        private decimal _cashoutAmount;

        public decimal CashoutAmount
        {
            get { return _cashoutAmount; }
            set { _cashoutAmount = value; }
        }

       public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTransactionAmount(CashoutAmount, "Cashout");
            validationMessage += validateTransactionAmount(TransactionAmount, "Transaction");
            validationMessage += validateMerchantId(MerchantId);

            validationMessage += validateEftposPAN(PrimaryAccountNumber);
            validationMessage += validateExpiryDate(PrimaryAccountNumber,ExpiryDate);
            validationMessage += validateCCV(CCV);
            validationMessage += validateCCVReason(PrimaryAccountNumber ,CCV, CCVReason);
            validationMessage += validateCCVSource(PrimaryAccountNumber, CCV, CCVSource);

            return checkValidationMessage(validationMessage);
        }
    }
}
