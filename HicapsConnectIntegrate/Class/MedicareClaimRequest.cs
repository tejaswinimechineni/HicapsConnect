using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Request object used to send the Medicare Claim Details down.
    /// The addMediClaimLine method will raise an Error if invalid field details are sent down.  
    /// You must adhere to the rules as set out in the Field definition.  
    /// You must ensure that your code will not attempt to send down invalid details.
    /// </summary>
    public class MedicareClaimRequest : BaseRequest
    {
        private readonly string STR_ForAPathologistDiagnosticClaimReferringDetailsMu = "For a Pathologist/Diagnostic Claim, Referring Details must be blank|";
        private readonly string STR_ForAPathologistDiagnosticClaimAndNoRequestingPro = "For a Pathologist/Diagnostic Claim, and no Requesting Provider details given, Requesting Override Type cde must be set|";
        private readonly string STR_AValidServicingProviderNumberMustBeFilledIn = "A valid servicing Provider Number must be filled in|";
        private readonly string STR_ClaimantIRNFieldMustBeFilledInIfClaimantMedicare = "Claimant IRN field must be filled in, if ClaimantMedicareCardNum is supplied|";
        private readonly string STR_PatientIRNFieldMustBeFilledIn = "Patient IRN field must be filled in|";
        private readonly string STR_ServiceTypeCdeIsCompulsaryValidValuesAreGGeneral = "ServiceTypeCde is compulsary, valid values are G (General), S (Specialist), O (Optical), P (Pathology), D (Diagnostic)|";
        private readonly string STR_ForBulkBillClaimsNoChargeAmountMayBeSpecified = "For Bulk Bill Claims, No Charge amount may be specified|";
        private readonly string STR_ClaimMustHaveAtLeastOneLine = "Claim must have at least one line";
        private readonly string STR_ClaimCanOnlyHaveAMaximumOf14Lines = "Claim can only have a maximum of 14 lines";
        private readonly string STR_ChargeAmountCanOnlyBeAPositiveValue = "Charge amount can only be a positive value|";
        private readonly string STR_PatientContributionAmountCanOnlyBeAPositiveValue = "Patient Contribution Amount can only be a positive value|";
        private readonly string STR_PatientContributionAmountMustBeLessThatCharge = "Patient Contribution amount must be less that Charge|";
        private readonly string STR_InvalidItemNumberItemNumberMustOnlyContainAlphaN = "Invalid Item Number, Item Number must only contain AlphaNumeric values only eg A-Z, a-z, 0-9, and spaces|";
        private readonly string STR_ForASpecialistClaimAndNoReferringProviderDetails = "For a specialist Claim, and no Referring Provider details given, Referral Override Type cde must be set|";
        private readonly string STR_ForASpecialistClaimRequestingDetailsMustBeBlank = "For a specialist Claim, Requesting Details must be blank|";
        private string _accountReferenceId;

        public string AccountReferenceId
        {
            get { return _accountReferenceId; }
            set { _accountReferenceId = value; }
        }
        // Required for Multi-Merchant
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }

        private string _claimantMedicareCardNum;

        public string ClaimantMedicareCardNum
        {
            get { return _claimantMedicareCardNum; }
            set { _claimantMedicareCardNum = value; }
        }

        private string _ClaimantIRN;

        public string ClaimantIRN
        {
            get { return _ClaimantIRN; }
            set { _ClaimantIRN = value; }
        }

        private string _patientMedicareCardNum;

        public string PatientMedicareCardNum
        {
            get { return _patientMedicareCardNum; }
            set { _patientMedicareCardNum = value; }
        }

        private string _PatientIRN;

        public string PatientIRN
        {
            get { return _PatientIRN; }
            set { _PatientIRN = value; }
        }

        private string _servicingProviderNum;

        public string ServicingProviderNum
        {
            get { return _servicingProviderNum; }
            set { _servicingProviderNum = value; }
        }
        private string _payeeProviderNum;

        public string PayeeProviderNum
        {
            get { return _payeeProviderNum; }
            set { _payeeProviderNum = value; }
        }
        //private string _psuedoProviderNum;

        //public string PsuedoProviderNum
        //{
        //    get { return _psuedoProviderNum; }
        //    set { _psuedoProviderNum = value; }
        //}

        private string _serviceTypeCde;

        public string ServiceTypeCde
        {
            get { return _serviceTypeCde; }
            set { _serviceTypeCde = value; }
        }

        private DateTime _referralIssueDate;

        public DateTime ReferralIssueDate
        {
            get { return _referralIssueDate; }
            set { _referralIssueDate = value; }
        }

        private string _referralPeriodTypeCde;

        public string ReferralPeriodTypeCde
        {
            get { return _referralPeriodTypeCde; }
            set { _referralPeriodTypeCde = value; }
        }

        private string _referralOverrideTypeCde;

        public string ReferralOverrideTypeCde
        {
            get { return _referralOverrideTypeCde; }
            set { _referralOverrideTypeCde = value; }
        }

        private string _referringProviderNum;

        public string ReferringProviderNum
        {
            get { return _referringProviderNum; }
            set { _referringProviderNum = value; }
        }


        private DateTime _requestIssueDate;

        public DateTime RequestIssueDate
        {
            get { return _requestIssueDate; }
            set { _requestIssueDate = value; }
        }

        private string _requestPeriodTypeCde;

        public string RequestPeriodTypeCde
        {
            get { return _requestPeriodTypeCde; }
            set { _requestPeriodTypeCde = value; }
        }

        private string _requestOverrideTypeCde;

        public string RequestOverrideTypeCde
        {
            get { return _requestOverrideTypeCde; }
            set { _requestOverrideTypeCde = value; }
        }

        private string _requestingProviderNum;

        public string RequestingProviderNum
        {
            get { return _requestingProviderNum; }
            set { _requestingProviderNum = value; }
        }
        private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }
        // SELECT MEDICARE TXN
        // 1. FULLY PAID
        //  2. PART PAID
        // 3. UN-PAID
        // 4. BULK BILL
        private int _claimType;

        public int ClaimType
        {
            get { return _claimType; }
            set { _claimType = value; }
        }

        //// These are per line.... shouldn't be here I think
        ////private decimal _chargeAmount;

        ////public decimal ChargeAmount
        ////{
        ////    get { return _chargeAmount; }
        ////    set { _chargeAmount = value; }
        ////}

        ////private string _dateOfService;

        ////public string DateOfService
        ////{
        ////    get { return _dateOfService; }
        ////    set { _dateOfService = value; }
        ////}

        ////private string _itemNum;

        ////public string ItemNum
        ////{
        ////    get { return _itemNum; }
        ////    set { _itemNum = value; }
        ////}

        ////private string _itemDescription;

        ////public string ItemDescription
        ////{
        ////    get { return _itemDescription; }
        ////    set { _itemDescription = value; }
        ////}

        ////private string _itemOverrideCode;

        ////public string ItemOverrideCode
        ////{
        ////    get { return _itemOverrideCode; }
        ////    set { _itemOverrideCode = value; }
        ////}

        ////private string _lspNum;

        ////public string LspNum
        ////{
        ////    get { return _lspNum; }
        ////    set { _lspNum = value; }
        ////}
        private string _cevRequestInd;

        public string CevRequestInd
        {
            get { return _cevRequestInd; }
            set { _cevRequestInd = value; }
        }
        //private decimal _benefitAssigned;

        //public decimal BenefitAssigned
        //{
        //    get { return _benefitAssigned; }
        //    set { _benefitAssigned = value; }
        //}
        private string _requestTypeCde;

        public string RequestTypeCde
        {
            get { return _requestTypeCde; }
            set { _requestTypeCde = value; }
        }

        // Claim Data format stored as a string array to help serialisation
        //Format is  + Separator
        //  ItemNumber + ItemDescription +  ChargeAmount + dateofService + ItemOverrideCode + LspNum )
        // 
        private List<string> _claimDetails;

        public List<string> ClaimDetails
        {
            get { return _claimDetails; }
            set { _claimDetails = value; }
        }

        public MedicareClaimRequest()
        {
            ClaimDetails = new List<string>();
            TransactionAmount = 0;
        }


        public void addMediClaimLine(string itemNum, decimal chargeAmount, DateTime dateOfService, string itemOverrideCde, string lspNum, string equipmentId, string selfDeemedCde, decimal contribPatientAmount, string spcId)
        {
            string data = "";
            string dateofservice = "";
            string feeamount = "00";
            string contribPatientAmountStr = "00";
            string zerofill = "00000000";

            TransactionAmount += chargeAmount;
            if (chargeAmount < 0)
            {
                throw new InvalidOperationException(STR_ChargeAmountCanOnlyBeAPositiveValue);
            }
            if (contribPatientAmount < 0)
            {
                throw new InvalidOperationException(STR_PatientContributionAmountCanOnlyBeAPositiveValue);
            }
            if (chargeAmount < contribPatientAmount)
            {
                throw new InvalidOperationException(STR_PatientContributionAmountMustBeLessThatCharge);
            }
            itemNum = Right(new String(' ', 6) + itemNum, 6);
            //bodyPart = Right(zerofill + bodyPart, 2);
            // 
            if (!isSpaceAlphaNumeric(itemNum)) { itemNum = "000000"; throw new IndexOutOfRangeException(STR_InvalidItemNumberItemNumberMustOnlyContainAlphaN); }
            //if (!isNumeric(bodyPart)) { itemNumber = "00"; throw new IndexOutOfRangeException("Invalid Bodypart id, Bodypart must contain numeric values only eg 11"); }
            dateofservice = Right(zerofill + dateOfService.Day.ToString(), 2) + Right(zerofill + dateOfService.Month.ToString(), 2) + Right(zerofill + dateOfService.Year.ToString(), 4);
            //    // Get fee into 012500" format for 125.00
            feeamount = (chargeAmount * System.Convert.ToDecimal(100)).ToString();
            if (feeamount.IndexOf('.') > 0) { feeamount = Left(feeamount, feeamount.IndexOf('.')); }
            feeamount = Right(zerofill + feeamount, 6);

            // Contrib Patient Amount
            contribPatientAmountStr = (contribPatientAmount * System.Convert.ToDecimal(100)).ToString();
            if (contribPatientAmountStr.IndexOf('.') > 0) { contribPatientAmountStr = Left(contribPatientAmountStr, contribPatientAmountStr.IndexOf('.')); }
            contribPatientAmountStr = Right(zerofill + contribPatientAmountStr, 6);

            lspNum = Right((new String(' ', 6) + lspNum), 6);
            itemOverrideCde = Right((new String(' ', 2) + itemOverrideCde), 2);
            equipmentId = Right((new String(' ', 5) + equipmentId), 5);
            spcId = Right((new String(' ', 4) + spcId), 4);
            selfDeemedCde = Right(("  " + selfDeemedCde), 2); //TODO fix size... should be 2

            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            //        6          6          8                  2           6           5            2                       6                  4
            data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr + spcId;
            ClaimDetails.Add(data);

            //ChargeAmount += fee;
        }
        private string validateClaim()
        {
            string validationMessage = "";
            switch (ClaimType)
            {
                case 1:
                case 2:
                case 3: validationMessage += validatePatientClaim(); break; // Patient Claim
                case 4: validationMessage += validateBulkBillClaim(); break;// Bulk Bill 
            }
            return validationMessage;
        }

        private string validateBulkBillClaim()
        {
            return "";
        }

        private string validatePatientClaim()
        {
            return "";
        }
        public override bool validateMessage(ref string validationMessage)
        {
           //decimal TransAmount = 0;
            string selfDeemedCdeCheck = "";
            validationMessage = "";
            validationMessage += validateClaim();
            // For Bulk Bill, No Charge AMount is sent
            if (ClaimType != 4)
            {
                validationMessage += validateTransactionAmount(TransactionAmount, "MedicareClaim");
            }
            else
            {
                if (TransactionAmount > 0)
                {
                    validationMessage = STR_ForBulkBillClaimsNoChargeAmountMayBeSpecified;
                }
            }

            if (ClaimDetails == null || ClaimDetails.Count <= 0)
            {
                validationMessage += STR_ClaimMustHaveAtLeastOneLine;
            }
            if (ClaimDetails.Count > 14)
            {
                validationMessage += STR_ClaimCanOnlyHaveAMaximumOf14Lines;
            }
            validationMessage += validateMedicareNumber(PatientMedicareCardNum);
            validationMessage += validateMedicareNumber(ClaimantMedicareCardNum);
            validationMessage += validateMedicarePatientIrn(PatientIRN);
            validationMessage += validateMedicareClaimType(ClaimType);
            validationMessage += validateMedicareProviderNumberId(ServicingProviderNum);
            validationMessage += validateMedicareProviderNumberId(ReferringProviderNum);
            validationMessage += validateMedicareProviderNumberId(PayeeProviderNum);
            validationMessage += validateMedicareServiceTypeCde(ServiceTypeCde);
            validationMessage += validateCevRequestInd(ClaimType, CevRequestInd, ServiceTypeCde);
            
            if (string.IsNullOrEmpty(ServiceTypeCde))
            {
                validationMessage += STR_ServiceTypeCdeIsCompulsaryValidValuesAreGGeneral;
            }
            if (string.IsNullOrEmpty(PatientIRN))
            {
                validationMessage += STR_PatientIRNFieldMustBeFilledIn;
            }
            //if (string.IsNullOrEmpty(MerchantId))
            //{
            //    validationMessage += "Merchant Id field must be filled in";
            //}
            //else
            //{
            //    validationMessage += validateMerchantId(MerchantId);
            //}

            if (!string.IsNullOrEmpty(ClaimantMedicareCardNum) && string.IsNullOrEmpty(ClaimantIRN))
            {
                validationMessage += STR_ClaimantIRNFieldMustBeFilledInIfClaimantMedicare;
            }
            if (string.IsNullOrEmpty(ServicingProviderNum))
            {
                validationMessage += STR_AValidServicingProviderNumberMustBeFilledIn;
            }
            if (ServiceTypeCde == "S")
            {
                // For a Specialist.  Referring Details must be provided
                if (string.IsNullOrEmpty(ReferringProviderNum))
                {
                    // No Referring Doctors input. so Referral Override code should be provided
                    if (string.IsNullOrEmpty(ReferralOverrideTypeCde))
                    {
                        validationMessage += STR_ForASpecialistClaimAndNoReferringProviderDetails;
                    }
                    else
                    {
                        validationMessage += validateMedicareReferralOverrideTypeCde(ReferralOverrideTypeCde);
                    }
                }
                if (!string.IsNullOrEmpty(RequestingProviderNum))
                {
                    validationMessage += STR_ForASpecialistClaimRequestingDetailsMustBeBlank;
                }
            }
            foreach (string medicareClaimDetails in ClaimDetails)
            {
                if (string.IsNullOrEmpty(selfDeemedCdeCheck))
                {
                    // Get first value for selfDeemedCdeCheck.
                    selfDeemedCdeCheck = extractMedicareClaimLineSelfDeemedCde(medicareClaimDetails);
                    if(selfDeemedCdeCheck != "SD" && selfDeemedCdeCheck != "SS")
                    {
                        selfDeemedCdeCheck = "";
                    }
                }
                validationMessage += validateMedicareClaimLine(this, medicareClaimDetails);
            }

            // Code moved into the validateMedicareClaimLine, to allow handling
            // of selfDeemedCde.
            if (ServiceTypeCde == "P" || ServiceTypeCde == "D")
            {
                // For a Pathologist.  Requesting details must be provided
                if (string.IsNullOrEmpty(RequestingProviderNum) && selfDeemedCdeCheck != "SD")
                {
                    // No Requesting doctos input, so Requesting overrid code should be set
                    if (string.IsNullOrEmpty(RequestOverrideTypeCde))
                    {
                        validationMessage += STR_ForAPathologistDiagnosticClaimAndNoRequestingPro;
                    }
                    else
                    {
                        validationMessage += validateMedicareRequestingOverrideTypeCde(ReferralOverrideTypeCde);
                    }
                }

                if (!string.IsNullOrEmpty(ReferringProviderNum))
                {
                    validationMessage += STR_ForAPathologistDiagnosticClaimReferringDetailsMu;
                }

            }
            if (ServiceTypeCde == "G")
            {
                validationMessage += validateMedicareAccountReference(AccountReferenceId);
            }

      
            validationMessage += validateMedicareIrn(ClaimantIRN);
            validationMessage += validateProviderNumberId(ServicingProviderNum);
            return checkValidationMessage(validationMessage);


        }

       


    }
}
