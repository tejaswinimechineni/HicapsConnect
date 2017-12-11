using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Request object used to send the Quote Claim Details down.
    /// The addClaimLine method will raise an Error if invalid field details are sent down.  
    /// You must adhere to the rules as set out in the Field definition.  
    /// You must ensure that your code will not attempt to send down invalid details.
    /// </summary>
    public class QuoteRequest : ClaimRequest
    {
       public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateTransactionAmount(TransactionAmount, "Quote");
            if (ClaimDetails.Count <= 0)
            {
                validationMessage += "Quote must have at least one line";
            }

            validationMessage += validateProviderNumberId(ProviderNumberId);
            return checkValidationMessage(validationMessage);


        }
    }
}
