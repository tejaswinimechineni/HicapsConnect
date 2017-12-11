using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    public class EftposDepositRequest : RefundRequest
    {
        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTransactionAmount(TransactionAmount, "Refund");
            validationMessage += validateMerchantId(MerchantId);
            validationMessage += validateEftposDepositPAN(PrimaryAccountNumber);
            validationMessage += validateExpiryDate(PrimaryAccountNumber, ExpiryDate);
            validationMessage += validateCCV(CCV);
            validationMessage += validateCCVReason(PrimaryAccountNumber, CCV, CCVReason);
            validationMessage += validateCCVSource(PrimaryAccountNumber, CCV, CCVSource);
            // validationMessage += validateRefundPassword(MerchantPassword);
            return checkValidationMessage(validationMessage);
        }
        public override bool isMessageAllowed()
        {
            return false;
        }
    }
}
