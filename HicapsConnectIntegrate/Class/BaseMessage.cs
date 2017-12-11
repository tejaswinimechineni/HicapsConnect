using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Reflection;

namespace HicapsConnectIntegrate.Class
{


    public class BaseMessage
    {
        private const string STR_ManualPANNotAllowedForEftposDepositTransactions = "Manual PAN not allowed for Eftpos Deposit Transactions";
        private readonly string STR_MedicareIRNCannotStartWithLeadingZeros = "Medicare IRN cannot start with leading zeros|";
        private readonly string STR_MerchantIdMustBeSupplied = "MerchantId must be supplied|";
        private readonly string STR_SuppliedMerchantIDIsInvalid = "Supplied Merchant ID is invalid|";
        private readonly string STR_ProviderNumberIdMustBeSupplied = "ProviderNumberId must be supplied|";
        private readonly string STR_ProviderNumberIdMustNotBeGreaterThan8Characters = "ProviderNumberId must not be greater than 8 characters|";
        private readonly string STR_ProviderNumberIdMustOnlyContain8AlphaNumericChar = "ProviderNumberId must only contain 8 AlphaNumeric Characters|";
        private readonly string STR_PatientIDMustBe2NumericCharactersEg01 = "Patient ID must be 2 numeric characters eg, 01|";
        private readonly string STR_ItemNumberMustBeNoMoreThan4NumericCharactersEg10 = "Item Number must be no more than 4 numeric characters eg, 1001|";
        private readonly string STR_BodypartIDMustBeNoMoreThan2ALphaNumericCharacter = "Bodypart ID must be no more than 2 ALphaNumeric characters eg, 11|";
        private readonly string STR_InvalidClaimDetails = "Invalid claim details|";
        private readonly string STR_InvalidMedicareProviderNumberCheckdigitFailed = "Invalid Medicare Provider Number (Checkdigit Failed)|";
        private readonly string STR_RequestingIssueDateCannotBeAfterDateOfService = "Requesting Issue Date cannot be after date of service|";
        private readonly string STR_ReferralIssueDateCannotBeAfterDateOfService = "Referral Issue Date cannot be after date of service|";
        private readonly string STR_ForBulkBillClaimsConsessionEntitlementFlagMustBe = "For Bulk Bill claims, Consession entitlement Flag must be set to either Y or N|";
        private readonly string STR_ForPartPaidClaimsAllLinesMustHavePatientContribu = "For Part Paid claims, all lines must have patient contribution amount > 0|";
        private readonly string STR_ForPathologyClaimsTheSPCidFieldMustNotBeBlank = "For Pathology claims, the SPCid field must not be blank|";
        private readonly string STR_InvalidSPCIdSPCIMustNotBeGreaterThanLength4 = "Invalid SPCId, SPCI must not be greater than length 4|";
        private readonly string STR_InvalidSelfDeemedCdeValidValuesAreYAndN = "Invalid selfDeemedCde, Valid values are 'SD', 'SS', 'N'|";
        private readonly string STR_InvalidLSPNLSPNMustBeA6DigitNumericNumber = "Invalid LSPN, LSPN must be a 6 digit numeric number|";
        private readonly string STR_InvalidEquipmentIdEquipmentIdMustBeA5DigitNumeri = "Invalid EquipmentId, EquipmentId must be a 5 digit numeric number|";
        private readonly string STR_InvalidItemOverrideCodeValidValuesAreBlankAPAONC = "Invalid item Override Code, valid values are blank, AP, AO, NC|";
        private readonly string STR_InvalidDatetimePassed = "Invalid Datetime passed|";
        private readonly string STR_InvalidDateMustBeDDMMYYYYEg01122009 = "Invalid Date, must be DDMMYYYY, eg 01122009|";
        private readonly string STR_InvalidDayNotAValidDay = "Invalid Day, not a valid day|";
        private readonly string STR_InvalidDateNotAValidMonth = "Invalid Date, not a valid month|";
        private readonly string STR_InvalidYearNotAValidYear = "Invalid year, not a valid year|";
        private readonly string STR_InvalidDateNotAValidDate = "Invalid date, not a valid Date|";
        private readonly string STR_InvalidMedicareAmountValueMustBeInCents = "Invalid medicare amount, value must be in cents|";
        private readonly string STR_InvalidMedicareChargeAmountValueMustBePositive = "Invalid medicare Charge amount, value must be positive|";
        private readonly string STR_AccountReferenceIdCanOnlyBeAtMost9AlphaNumericCh = "Account Reference Id can only be at most 9 AlphaNumeric characters long|";
        private readonly string STR_InvalidServiceTypeCodeValidValuesAreGGeneralSSpe = "Invalid Service Type Code, valid values are G (General), S (Specialist), O (Optical), P (Pathology), D (Diagnostic)|";
        private readonly string STR_InvalidReferralOverrideTypeCdeEnteredValidValues = "Invalid ReferralOverrideTypeCde entered, valid values are N (Not Applicable), L (Lost), E (Emergency)|";
        private readonly string STR_InvalidRequestingOverrideTypeCdeEnteredValidValu = "Invalid RequestingOverrideTypeCde entered, valid values are N (Not Applicable), L (Lost), E (Emergency)|";
        private readonly string STR_InvalidMedicareItemNumberItemNumberCanOnlyBeNume = "Invalid Medicare Item Number, item number can only be numerics, with length of at most 6|";
        private readonly string STR_InvalidPrimaryAccountNumberFieldFieldMustOnlyCon = "Invalid Primary Account Number field, Field must only contain Numeric digits|";
        private readonly string STR_InvalidPrimaryAccountNumberFieldFieldMustBeAtLea = "Invalid Primary Account Number field, Field must be at least 13 digits long|";
        private readonly string STR_InvalidPrimaryAccountNumberFieldFieldCanOnlyBeAt = "Invalid Primary Account Number field, Field can only be at most 19 digits long|";
        private readonly string STR_InvalidCCVFieldFieldMustOnlyContainNumericDigits = "Invalid CCV Field, Field must only contain Numeric digits|";
        private readonly string STR_InvalidCCVFieldFieldCanOnlyBeFrom0To6DigitsLong = "Invalid CCV Field, Field can only be from 0 to 6 digits long|";
        private readonly string STR_InvalidCCVSourceFieldValidValuesAre1234 = "Invalid CCV Source Field, valid values are 1,2,3,4|";
        private readonly string STR_CCVNotSuppliedMustAnswerCCVReasonCodeValidValues = "CCV not Supplied, Must answer CCVReason Code, valid values are 1,2,3|";
        private readonly string STR_CCVSuppliedCCVReasonMustBeBlank = "CCV Supplied, CCV Reason must be blank|";
        private readonly string STR_InvalidCCVReasonFieldValidValuesAre123 = "Invalid CCV Reason Field, valid values are 1,2,3|";
        private readonly string STR_ExpiryDateMustBeEnteredIfPrimaryAccountNumberIsS = "Expiry date must be entered if PrimaryAccountNumber is set|";
        private readonly string STR_InvalidExpiryDateMustBeMMYYEg0109 = "Invalid Expiry Date, must be MMYY, eg 0109|";
        private readonly string STR_InvalidExpiryDateMustBeMMYYMustBe4NumericNumbers = "Invalid Expiry Date, must be MMYY, must be 4 Numeric numbers|";
        private readonly string STR_InvalidExpiryDateNotAValidMonth = "Invalid Expiry Date, not a valid month|";
        private readonly string STR_FeeAmountMustBeGreaterThan0 = "Fee amount must be greater than 0|";
        private readonly string STR_FeeAmountMustBeLessThan1000000 = "Fee amount must be less than 10000.00|";
        private readonly string STR_PatientNameMustBeLessThan28Characters = "Patient name must be less than 28 characters|";
        private readonly string STR_ServiceDateCannotBeGreaterThanToday = "Service date cannot be greater than today|";
        private readonly string STR_RRNMustBeSupplied = "RRN must be supplied|";
        private readonly string STR_RRNMustBeNoMoreThan12NumericCharacters = "RRN must be no more than 12 numeric characters|";
        private readonly string STR_SettlementTypeValuesMayOnlyBeSPOrL = "SettlementType values may Only be S, P or L)|";
        private readonly string STR_AmountMustBeGreaterThan0 = "{0} amount must be greater than 0|";
        private readonly string STR_AmountMustBeLessThan1000000 = "{0} amount must be less than 10000.00|";
        private readonly string STR_NotAHicapsTerminal = "Not a Hicaps Terminal|";
        private readonly string STR_RefundPasswordMustBeA4DigitNumber = "Refund password must be a 4 digit number|";
        private readonly string STR_InvalidItemOverrideCodeSpecifiedValidValuesAreBl = "Invalid Item OverrideCode specified, valid values are blank, AP, AO, NC|";
        private readonly string STR_InvalidReferralOverrideCodeSpecifiedValidValuesA = "Invalid Referral OverrideCode specified, valid values are blank, N (Not Applicable), L (Lost) ,E (Emergency)|";
        private readonly string STR_InvalidReferralPeriodCodeSpecifiedValidValuesAre = "Invalid Referral Period Code specified, valid values are blank, S (Standard), I (Indeterminate)|";
        private readonly string STR_PatientMedicareIRNMustBeFilledId = "Patient Medicare IRN must be supplied";
        private readonly string STR_MedicareIRNCanOnlyBeNumeric = "Medicare IRN can only be numeric|";
        private readonly string STR_InvalidClaimTypeClaimTypeMustBe1FullyPaid2Partly = "Invalid Claim Type, Claim Type must be 1 (Fully Paid), 2(Partly Paid) 3 (UnPaid) 4 (Bulk Bill)|";
        private readonly string STR_InvalidMedicareNumberCheckTheNumberAndTryAgain = "Invalid Medicare Number,{0} check the number and try again|";
        private readonly string STR_CevRequestIndIsOnlyValidForBulkBillClaims = "CevRequestInd is only valid for Bulk Bill Claims|";
        private readonly string STR_InvalidCEVRequestIndValidValuesAreYOrN = "Invalid CEVRequestInd, Valid values are Y or N|";
        private byte _formatVersion;
        private bool _readonly = false;

        public bool ReadOnly
        {
            get { return _readonly; }
            set { if (value) { _readonly = value; } }
        }

        protected string _validationOkMessage = "Validated Ok";
        /// <summary>
        /// Network Message Id.  For Future Use
        /// </summary>
        private string _msgId;
        public string MsgId
        {
            get { return _msgId; }
            set { _msgId = value; }
        }
        /// <summary>
        /// Always set to 1
        /// </summary>
        public byte FormatVersion
        {
            get { return _formatVersion; }
            set { _formatVersion = value; }
        }
        private byte _requestResponseIndicator;

        /// <summary>
        /// ‘0’ 	This message is a request message which requires a response. The terminal will print all receipts.
        /// If set to 1, on a request message, then response will not be cached. 
        /// </summary>
        public byte RequestResponseIndicator
        {
            get { return _requestResponseIndicator; }
            set { _requestResponseIndicator = value; }
        }
        private byte _moreIndicator;

        /// <summary>
        /// Set to 1 by terminal if there are more data to be returned in subsequent message
        /// </summary>
        public byte MoreIndicator
        {
            get { return _moreIndicator; }
            set { _moreIndicator = value; }
        }
        private string _transactionCode;

        /// <summary>
        /// Identifies the type of transaction this message is performing.
        /// </summary>
        public string TransactionCode
        {
            get { return _transactionCode; }
            set { _transactionCode = value; }
        }
        private string _computerName = Environment.MachineName;

        /// <summary>
        /// Name of the computer source creating the message
        /// </summary>
        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; }
        }
        /// <summary>
        /// Name of the Software vendor
        /// </summary>
        private string _softwareVendorName;

        public string SoftwareVendorName
        {
            get { return _softwareVendorName; }
            set { _softwareVendorName = value; }
        }
        private string _serverUrl;
        /// <summary>
        /// Destination of Server to go to format is IP:PORT:TERMINALID:COMX
        /// </summary>

        public string ServerUrl
        {
            get { return _serverUrl; }
            set { _serverUrl = value; }
        }
        
        public string NetworkName { get; set; }
        public BaseMessage() { MsgId = System.Guid.NewGuid().ToString(); }
        public static string extractComPort(string url)
        {
            try
            {
                string[] serverParts = url.Split(':');
                // TODO put the Server url into an object
                if (serverParts.Count() > 3)
                {
                    return serverParts[3];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        public static string extractIpAddress(string url)
        {
            try
            {
                string[] serverParts = url.Split(':');
                // TODO put the Server url into an object
                if (serverParts.Count() > 3)
                {
                    return serverParts[0];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        public static string extractIpPort(string url)
        {
            try
            {
                string[] serverParts = url.Split(':');
                // TODO put the Server url into an object
                if (serverParts.Count() > 3)
                {
                    return serverParts[1];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        public static string extractTerminalId(string url)
        {
            try
            {
                string[] serverParts = url.Split(':');
                // TODO put the Server url into an object
                if (serverParts.Count() > 3)
                {
                    return serverParts[2];
                }
            }
            catch
            {
                return "";
            }
            return "";
        }
        public static string extractClassName(string xmlMessage)
        {
            string myClassName = "";
            int tagpos;
            int endtagpos;
            string tag;
            string[] tags;
            try
            {
            int pos = xmlMessage.IndexOf("?>");

                if (pos > 0)
                {
                     tagpos = xmlMessage.IndexOf('<', pos);
                     endtagpos = xmlMessage.IndexOf('>', tagpos);
                     tag = xmlMessage.Substring(tagpos + 1, endtagpos);
                     tags = tag.Split(' ');
                    if (tags.Count() > 0)
                        myClassName = tags[0];
                }
                else{
                // Run check for DataCOntractSerialiser
                pos = xmlMessage.IndexOf("<HicapsConnectControl.");
                    tagpos = pos + "<HicapsConnectControl".Length;
                    endtagpos = xmlMessage.IndexOf('>', tagpos);
                    tag = xmlMessage.Substring(tagpos + 1, endtagpos);
                    tags = tag.Split(' ');
                    if (tags.Count() > 0)
                        myClassName = tags[0];
                }
            }
            catch
            {
                myClassName = "BaseMessage";
            }
            return myClassName;
        }
        public virtual string[] breakupLineFields(int Index)
        {
            throw new NotImplementedException();
        }

        protected string Right(string rightString, int length)
        {
            return rightString.Substring(rightString.Length - length);
        }
        protected string Left(string leftString, int length)
        {
            return leftString.Substring(0, length);
        }
        public virtual bool validateMessage(ref string validationMessage)
        {
            // Implement in decendant
            //throw new NotImplementedException("Implement in decendant");
            validationMessage = _validationOkMessage;
            return true;
        }
      
        public virtual bool isMessageAllowed()
        {
            return true;
        }
        protected bool checkValidationMessage(string validationMessage)
        {
            if (validationMessage == null || validationMessage.Trim().Length == 0)
            {
                validationMessage = _validationOkMessage;
                return true;
            }
            else
            {
                return false;
            }
        }
        protected string validateMerchantId(string MerchantId)
        {
            if (MerchantId == null || MerchantId.Trim().Length <= 0)
            {
                return STR_MerchantIdMustBeSupplied;
            }
            if (MerchantId.Trim().Length > 8 || MerchantId.Trim().Length < 8)
            {
                return STR_SuppliedMerchantIDIsInvalid;
            }
            return "";
        }

        protected string validateProviderNumberId(string ProviderNumberId)
        {
            if (ProviderNumberId == null || ProviderNumberId.Trim().Length <= 0)
            {
                return STR_ProviderNumberIdMustBeSupplied;
            }

            if (ProviderNumberId.Trim().Length > 8)
            {
                return STR_ProviderNumberIdMustNotBeGreaterThan8Characters;
            }
            if (!isAlphaNumeric(ProviderNumberId.Trim()))
            {
                return STR_ProviderNumberIdMustOnlyContain8AlphaNumericChar;
            }
            return "";
        }

        protected string validatePatientId(string patientId)
        {
            if (patientId.Trim().Length > 2 || !isNumeric(patientId))
            {
                return STR_PatientIDMustBe2NumericCharactersEg01;
            }
            return "";
        }
        protected string validateItemNumber(string itemNumber)
        {

            if (itemNumber.Trim().Length > 4 || !isSpaceAlphaNumeric(itemNumber))
            {
                return STR_ItemNumberMustBeNoMoreThan4NumericCharactersEg10;
            }
            return "";
        }
        protected string validateBodyPart(string bodypartId)
        {
            // Bodypart id can be blank
            if (bodypartId.Trim().Length == 0)
            {
                return "";
            }
            if (!isAlphaNumeric(bodypartId) || bodypartId.Trim().Length > 2)
            {
                return STR_BodypartIDMustBeNoMoreThan2ALphaNumericCharacter;
            }
            return "";
        }
        protected string validateClaimLine(string claimDetailsLine)
        {
            string validationMessage = "";

            if (claimDetailsLine.Length != 18)
            {
                return STR_InvalidClaimDetails;
            }

            validationMessage += validatePatientId(claimDetailsLine.Substring(0, 2));
            validationMessage += validateItemNumber(claimDetailsLine.Substring(2, 4));
            validationMessage += validateBodyPart(claimDetailsLine.Substring(6, 2));

            return validationMessage;
        }
        #region Medicare Validation Code
        protected string validateMedicareProviderNumberId(string ProviderNumberId)
        {
            string errorMessage = "";
            if (!string.IsNullOrEmpty(ProviderNumberId))
            {
                errorMessage = validateProviderNumberId(ProviderNumberId);
                if (!isValidProviderNumberId(ProviderNumberId))
                {
                    errorMessage += STR_InvalidMedicareProviderNumberCheckdigitFailed;
                }
            }
            return errorMessage;
        }

        private bool isValidProviderNumberId(string ProviderNumberId)
        {
            IDictionary<char, int> _PLVTbl = new Dictionary<char, int>();
            IDictionary<int, char> _CDTbl = new Dictionary<int, char>();

            _PLVTbl.Add('0', 0);
            _PLVTbl.Add('1', 1);
            _PLVTbl.Add('2', 2);
            _PLVTbl.Add('3', 3);
            _PLVTbl.Add('4', 4);
            _PLVTbl.Add('5', 5);
            _PLVTbl.Add('6', 6);
            _PLVTbl.Add('7', 7);
            _PLVTbl.Add('8', 8);
            _PLVTbl.Add('9', 9);
            _PLVTbl.Add('A', 10);
            _PLVTbl.Add('B', 11);
            _PLVTbl.Add('C', 12);
            _PLVTbl.Add('D', 13);
            _PLVTbl.Add('E', 14);
            _PLVTbl.Add('F', 15);
            _PLVTbl.Add('G', 16);
            _PLVTbl.Add('H', 17);
            _PLVTbl.Add('J', 18);
            _PLVTbl.Add('K', 19);
            _PLVTbl.Add('L', 20);
            _PLVTbl.Add('M', 21);
            _PLVTbl.Add('N', 22);
            _PLVTbl.Add('P', 23);
            _PLVTbl.Add('Q', 24);
            _PLVTbl.Add('R', 25);
            _PLVTbl.Add('T', 26);
            _PLVTbl.Add('U', 27);
            _PLVTbl.Add('V', 28);
            _PLVTbl.Add('W', 29);
            _PLVTbl.Add('X', 30);
            _PLVTbl.Add('Y', 31);

            _CDTbl.Add(0, 'Y');
            _CDTbl.Add(1, 'X');
            _CDTbl.Add(2, 'W');
            _CDTbl.Add(3, 'T');
            _CDTbl.Add(4, 'L');
            _CDTbl.Add(5, 'K');
            _CDTbl.Add(6, 'J');
            _CDTbl.Add(7, 'H');
            _CDTbl.Add(8, 'F');
            _CDTbl.Add(9, 'B');
            _CDTbl.Add(10, 'A');

            // TODO add in check digit routine.
            if (ProviderNumberId.Length != 8)
            {
                // Provider should be 8 digits
                return false;
            }

            if (!isAlphaNumeric(ProviderNumberId))
            {
                return false;
            }


            int digit1 = Convert.ToInt32(ProviderNumberId.Substring(0, 1));
            int digit2 = Convert.ToInt32(ProviderNumberId.Substring(1, 1));
            int digit3 = Convert.ToInt32(ProviderNumberId.Substring(2, 1));
            int digit4 = Convert.ToInt32(ProviderNumberId.Substring(3, 1));
            int digit5 = Convert.ToInt32(ProviderNumberId.Substring(4, 1));
            int digit6 = Convert.ToInt32(ProviderNumberId.Substring(5, 1));
            char digit7 = Convert.ToChar(ProviderNumberId.Substring(6, 1));
            char checkDigit = Convert.ToChar(ProviderNumberId.Substring(7, 1));

            int PLV = 0;
            _PLVTbl.TryGetValue(digit7, out PLV);

            int sum = ((digit1 * 3) + (digit2 * 5) + (digit3 * 8) + (digit4 * 4) + (digit5 * 2) + digit6 + (PLV * 6));

            int remainder = 0;
            Math.DivRem(sum, 11, out remainder);

            char finalCheckDigit;
            _CDTbl.TryGetValue(remainder, out finalCheckDigit);
            if (finalCheckDigit == checkDigit)
                return true;
            else
                return false;

            return false;
        }
        protected string extractMedicareClaimLineSelfDeemedCde(string claimDetailsLine)
        {
            return claimDetailsLine.Substring(33, 2).Trim();
        }
        protected string extractMedicareClaimLineEquipmentId(string claimDetailsLine)
        {
            return claimDetailsLine.Substring(28, 5).Trim();
        }
        protected string validateMedicareClaimLine(MedicareClaimRequest medicareClaimRequest, string claimDetailsLine)
        {
            string validationMessage = "";
            if (medicareClaimRequest.ClaimType > 0 && medicareClaimRequest.ClaimType <= 4)
            {
                string itemNumber = claimDetailsLine.Substring(0, 6).Trim(); //= "12345";
                string chargeAmount = claimDetailsLine.Substring(6, 6).Trim(); ;//= "0000060";
                string dateofservice = claimDetailsLine.Substring(12, 8).Trim(); ;//= "01072009";
                string itemOverrideCode = claimDetailsLine.Substring(20, 2).Trim(); //= "AP"; //"AP"
                string lspn = claimDetailsLine.Substring(22, 6).Trim();//= "123456"; //123456
                string equipmentId = extractMedicareClaimLineEquipmentId(claimDetailsLine);//= "12345"; //12345
                string selfDeemedCde = extractMedicareClaimLineSelfDeemedCde(claimDetailsLine);//= "Y";  //Y
                string contribPatientAmount = claimDetailsLine.Substring(35, 6);//= "000000"; // "0000000";
                string spcid = claimDetailsLine.Substring(41, 4); // = "    "

                validationMessage += validateMedicareItemNumber(itemNumber);
                if (medicareClaimRequest.ClaimType != 4)
                {
                    validationMessage += validateMedicareTransactionAmount(chargeAmount);
                }
                validationMessage += validateMedicareDateStr(dateofservice);
                validationMessage += validateMedicareItemOverrideCode(itemOverrideCode);
                validationMessage += validateMedicareLspn(lspn);
                validationMessage += validateMedicareSelfDeemedCde(medicareClaimRequest,selfDeemedCde);
                validationMessage += validateMedicareTransactionAmount(contribPatientAmount);
                try
                {
                    /* Taken from Medicare EasyClaim Guide... if selfDeemedCde = 'SD' no requesting details
                     * can be set
                     * 
                     */ 
                   
                    if (medicareClaimRequest.RequestIssueDate != null && medicareClaimRequest.RequestIssueDate.Year > 2000)
                    {
                        if (medicareClaimRequest.RequestIssueDate > ConvertMedicareDateStr(dateofservice))
                        {
                            validationMessage += STR_RequestingIssueDateCannotBeAfterDateOfService;
                        }
                    }
                    if (medicareClaimRequest.ReferralIssueDate != null && medicareClaimRequest.ReferralIssueDate.Year > 2000)
                    {
                        if (medicareClaimRequest.ReferralIssueDate > ConvertMedicareDateStr(dateofservice))
                        {
                            validationMessage += STR_ReferralIssueDateCannotBeAfterDateOfService;
                        }
                    }

                }
                catch (Exception)
                { // Means something wrong with the dates... should never really get here.
                }
                //
                if (medicareClaimRequest.ClaimType == 4)
                {
                    if (string.IsNullOrEmpty(medicareClaimRequest.CevRequestInd) && medicareClaimRequest.ServiceTypeCde != "S")
                    {
                        validationMessage += STR_ForBulkBillClaimsConsessionEntitlementFlagMustBe;
                    }
                }
                if (medicareClaimRequest.ClaimType == 2)
                {
                    if (isNumeric(contribPatientAmount) && Convert.ToInt32(contribPatientAmount) == 0)
                    {
                        validationMessage += STR_ForPartPaidClaimsAllLinesMustHavePatientContribu;
                    }
                }
                if (medicareClaimRequest.ServiceTypeCde == "P" && string.IsNullOrEmpty(spcid))
                {
                    validationMessage += STR_ForPathologyClaimsTheSPCidFieldMustNotBeBlank;
                }

                validationMessage += validateMedicareSpcId(spcid);


                // TODO add in Medicare Claim Line Validation
            }
            return validationMessage;
        }

        private string validateMedicareSpcId(string spcid)
        {
            // TODO write validation.
            if (!string.IsNullOrEmpty(spcid))
            {
                if (spcid.Length > 4)
                {
                    return STR_InvalidSPCIdSPCIMustNotBeGreaterThanLength4;
                }
            }
            return "";
        }

        private string validateMedicareSelfDeemedCde(MedicareClaimRequest medicareClaimRequest,string selfDeemedCde)
        {
            if (!string.IsNullOrEmpty(selfDeemedCde))
            {
                if (selfDeemedCde != "SD" && selfDeemedCde != "SS" && selfDeemedCde != "N")
                {
                    return STR_InvalidSelfDeemedCdeValidValuesAreYAndN;
                
                }
                if (medicareClaimRequest.ServiceTypeCde == "S" || medicareClaimRequest.ServiceTypeCde == "G")
                {
                    return "Only Pathology and Diagnostic claims may have a Self Deemed Code";
                }

                if (medicareClaimRequest.ServiceTypeCde == "P")
                {
                    if (selfDeemedCde == "SS")
                    {
                        return "Pathology claims may only have a self deemed code of SD";
                    }
                    if (selfDeemedCde == "SD")
                    {
                        if (!string.IsNullOrEmpty(medicareClaimRequest.RequestingProviderNum))
                        {
                            return "If service is Self deemed, No requesting details are to be sent";
                        }
                    }
                }
                if (medicareClaimRequest.ServiceTypeCde == "D")
                {
                    if (selfDeemedCde == "SD")
                    {
                        return "Diagnostic claims may only have a self deemed code of " + "SS";
                    }

                    if (selfDeemedCde == "SS")
                    {
                        if (string.IsNullOrEmpty(medicareClaimRequest.RequestingProviderNum))
                        {
                            return "If service is substituted service, Requesting details must be sent";
                        }
                    }
                }

            }

            return "";
        }

        private string validateMedicareLspn(string lspn)
        {
            if (!string.IsNullOrEmpty(lspn))
            {
                if (!isNumeric(lspn) || lspn.Length > 6)
                {
                    return STR_InvalidLSPNLSPNMustBeA6DigitNumericNumber;
                }
            }
            return "";
        }
        private string validateMedicareEquipmentId(string EquipmentId)
        {
            if (!string.IsNullOrEmpty(EquipmentId))
            {
                if (!isNumeric(EquipmentId))
                {
                    return STR_InvalidEquipmentIdEquipmentIdMustBeA5DigitNumeri;
                }
            }
            return "";
        }
        private string validateMedicareItemOverrideCode(string itemOverrideCode)
        {
            string validationMessage = "";
            if (!string.IsNullOrEmpty(itemOverrideCode))
            {
                if ("AP,AO,NC,".IndexOf(itemOverrideCode + ",") < 0)
                {
                    return STR_InvalidItemOverrideCodeValidValuesAreBlankAPAONC;
                }
            }
            return "";
        }
        public static DateTime ConvertMedicareDateStr(string dateofservice)
        {
            DateTime myDate;
            string day = dateofservice.Substring(0, 2);
            int iDay = Convert.ToInt32(day);
            string month = dateofservice.Substring(2, 2);
            int iMonth = Convert.ToInt32(month);
            string year = dateofservice.Substring(4, 4);
            int iYear = Convert.ToInt32(year);
            try
            {
                myDate = new DateTime(iYear, iMonth, iDay);
            }
            catch (Exception )
            {
                throw new ArgumentOutOfRangeException("Invalid DateTime Passed");
            }
            return myDate;
        }
        private string validateMedicareDateStr(string dateofservice)
        {
            if (dateofservice.Length != 8 || !isNumeric(dateofservice))
            {
                return STR_InvalidDateMustBeDDMMYYYYEg01122009;
            }
            string day = dateofservice.Substring(0, 2);
            int iDay = Convert.ToInt32(day);
            if (iDay > 31 || iDay < 0)
            {
                return STR_InvalidDayNotAValidDay;
            }

            string month = dateofservice.Substring(2, 2);
            int iMonth = Convert.ToInt32(month);
            if ("01,02,03,04,05,06,07,08,09,10,11,12,".IndexOf(month + ",") < 0)
            {
                return STR_InvalidDateNotAValidMonth;
            }
            string year = dateofservice.Substring(4, 4);
            int iYear = Convert.ToInt32(year);

            if (iYear < 2000)
            {
                return STR_InvalidYearNotAValidYear;
            }
            try
            {
                DateTime myDate = new DateTime(iYear, iMonth, iDay);
            }
            catch (Exception )
            {
                return STR_InvalidDateNotAValidDate;
            }
            return "";
        }

        private string validateMedicareTransactionAmount(string chargeAmount)
        {
            // in theory this shouldn't happen.... but in case it does
            if (string.IsNullOrEmpty(chargeAmount) || !isNumeric(chargeAmount) || chargeAmount.Length != 6)
            {
                return STR_InvalidMedicareAmountValueMustBeInCents;
            }
            int amount = Convert.ToInt32(chargeAmount);
            if (amount < 0)
            {
                return STR_InvalidMedicareChargeAmountValueMustBePositive;
            }
            return "";
        }
        protected string validateMedicareAccountReference(string AccountReferenceId)
        {
            if (!string.IsNullOrEmpty(AccountReferenceId))
            {
                if (AccountReferenceId.Length > 9)
                {
                    return STR_AccountReferenceIdCanOnlyBeAtMost9AlphaNumericCh;
                }
            }
            return "";
        }
        protected string validateMedicareServiceTypeCde(string serviceTypeCde)
        {
            if (!string.IsNullOrEmpty(serviceTypeCde))
            {
                if ("G,S,O,P,D,".IndexOf(serviceTypeCde + ",") < 0)
                {
                    return STR_InvalidServiceTypeCodeValidValuesAreGGeneralSSpe;
                }
            }
            return "";
        }
        protected string validateMedicareReferralOverrideTypeCde(string ReferralOverrideTypeCde)
        {
            if (!string.IsNullOrEmpty(ReferralOverrideTypeCde))
            {
                if ("N,L,E,".IndexOf(ReferralOverrideTypeCde + ",") < 0)
                {
                    return STR_InvalidReferralOverrideTypeCdeEnteredValidValues;
                }
            }
            return "";
        }
        protected string validateMedicareRequestingOverrideTypeCde(string RequestingOverrideTypeCde)
        {
            if (!string.IsNullOrEmpty(RequestingOverrideTypeCde))
            {
                if ("N,L,E,".IndexOf(RequestingOverrideTypeCde + ",") < 0)
                {
                    return STR_InvalidRequestingOverrideTypeCdeEnteredValidValu;
                }
            }
            return "";
        }
        protected string validateMedicareItemNumber(string itemNumber)
        {
            if (string.IsNullOrEmpty(itemNumber) || itemNumber.Length >= 6 || !isNumeric(itemNumber))
            {
                return STR_InvalidMedicareItemNumberItemNumberCanOnlyBeNume;
            }
            return "";
        }
        #endregion
        protected string validateEftposPAN(string PrimaryAccountNumber)
        {

            if (string.IsNullOrEmpty(PrimaryAccountNumber))
            {
                return "";
            }
            if (!isNumeric(PrimaryAccountNumber))
            {
                return STR_InvalidPrimaryAccountNumberFieldFieldMustOnlyCon;
            }
            if (PrimaryAccountNumber.Length > 0 && PrimaryAccountNumber.Length < 13)
            {
                return STR_InvalidPrimaryAccountNumberFieldFieldMustBeAtLea;
            }
            if (PrimaryAccountNumber.Length > 19)
            {
                return STR_InvalidPrimaryAccountNumberFieldFieldCanOnlyBeAt;
            }
            return "";
        }
        protected string validateEftposDepositPAN(string PrimaryAccountNumber)
        {
            if (PrimaryAccountNumber != null)
            {
                PrimaryAccountNumber = PrimaryAccountNumber.Trim();
            }

            if (!string.IsNullOrEmpty(PrimaryAccountNumber))
            {
                return STR_ManualPANNotAllowedForEftposDepositTransactions;
            }
            return "";
        }
        protected string validateCCV(string CCV)
        {
            if (string.IsNullOrEmpty(CCV))
            {
                return "";
            }
            if (!isNumeric(CCV))
            {
                return STR_InvalidCCVFieldFieldMustOnlyContainNumericDigits;
            }
            if (CCV.Length > 6)
            {
                return STR_InvalidCCVFieldFieldCanOnlyBeFrom0To6DigitsLong;
            }
            return "";
        }
        protected string validateCCVSource(string PrimaryAccountNumber, string CCV, int CCVSource)
        {
            if (string.IsNullOrEmpty(PrimaryAccountNumber))
            {
                return "";
            }
            if (string.IsNullOrEmpty(CCV))
            {
                return "";
            }
            if (CCVSource > 4 || CCVSource < 1)
            {
                return STR_InvalidCCVSourceFieldValidValuesAre1234;
            }
            return "";
        }
        protected string validateCCVReason(string PrimaryAccountNumber, string CCV, int CCVReason)
        {
            if (string.IsNullOrEmpty(PrimaryAccountNumber))
            {
                return "";
            }
            if (string.IsNullOrEmpty(CCV) && ((CCVReason > 4 || CCVReason < 1)))
            {
                return STR_CCVNotSuppliedMustAnswerCCVReasonCodeValidValues;
            }
            if (!string.IsNullOrEmpty(CCV) && (CCVReason != 0))
            {
                return STR_CCVSuppliedCCVReasonMustBeBlank;
            }

            if (CCVReason > 4 || CCVReason < 0)
            {
                return STR_InvalidCCVReasonFieldValidValuesAre123;
            }
            return "";
        }
        public void CopyObject<T>(T sourceObject)
        where T : BaseMessage
        {
            //  If either the source, or destination is null, return   
            if (sourceObject == null)
                return;


            //  Get the type of each object   
            Type sourceType = sourceObject.GetType();
            Type targetType = this.GetType();

            //  Loop through the source properties   
            foreach (PropertyInfo p in sourceType.GetProperties())
            {
                //  Get the matching property in the destination object   
                PropertyInfo targetObj = targetType.GetProperty(p.Name);
                //  If there is none, skip   
                if (targetObj == null)
                    continue;

                //  Set the value in the destination  
                // Ensure copy can succeed, and types must match
                if ((targetObj.CanWrite) && (p.CanRead) && (targetObj.GetType() == p.GetType()))
                {

                    targetObj.SetValue(this, p.GetValue(sourceObject, null), null);
                }
            }
        }
        protected string validateExpiryDate(string PrimaryAccountNumber, string expiryDate)
        {
            if (string.IsNullOrEmpty(expiryDate))
            {
                // No Expiry Date is fine..
                if (string.IsNullOrEmpty(PrimaryAccountNumber))
                {
                    return "";
                }
                else
                {
                    return STR_ExpiryDateMustBeEnteredIfPrimaryAccountNumberIsS;
                }
            }

            if (expiryDate.Length != 4)
            {
                return STR_InvalidExpiryDateMustBeMMYYEg0109;
            }
            if (!isNumeric(expiryDate))
            {
                return STR_InvalidExpiryDateMustBeMMYYMustBe4NumericNumbers;
            }
            string month = Left(expiryDate, 2);
            if ("01,02,03,04,05,06,07,08,09,10,11,12,".IndexOf(month + ",") < 0)
            {
                return STR_InvalidExpiryDateNotAValidMonth;
            }

            return "";
        }
        protected string validatePatientNameDetails(string patientName)
        {
            string validationMessage = "";
            if (patientName.Length > 32)
            {
                return "Invalid patient Details";
            }
            validationMessage += validatePatientId(patientName.Substring(0, 2));
            validationMessage += validatePatientName(patientName.Substring(2));
            return validationMessage;
        }
        protected string validateFeeAmount(decimal feeAmount)
        {

            if (feeAmount <= 0)
            {
                return STR_FeeAmountMustBeGreaterThan0;
            }
            if (feeAmount >= 10000)
            {
                return STR_FeeAmountMustBeLessThan1000000;
            }
            return "";
        }
        protected string validatePatientName(string patientName)
        {
            if (patientName.Trim().Length > 28)
            {
                return STR_PatientNameMustBeLessThan28Characters;
            }
            return "";
        }
        protected string validateServiceDate(DateTime serviceDate)
        {
            if (serviceDate.Date > System.DateTime.Now.Date)
            {
                return STR_ServiceDateCannotBeGreaterThanToday;
            }

            return "";
        }
        protected string validateRRN(string RRN)
        {
            if (RRN == null || RRN.Trim().Length <= 0)
            {
                return STR_RRNMustBeSupplied;
            }
            if (RRN.Trim().Length > 12)
            {
                return STR_RRNMustBeNoMoreThan12NumericCharacters;
            }
            return "";
        }
        protected string validateSettlementType(string settlementType)
        {
            if (string.IsNullOrEmpty(settlementType))
            {
                return "";
            }
            if (settlementType == "S" || settlementType == "P" || settlementType == "L")
            {
                return "";
            }
            return STR_SettlementTypeValuesMayOnlyBeSPOrL;
        }
        protected string validateTransactionAmount(decimal transAmount, string transType)
        {
            if (transType == null || transType == "") transType = "Transaction";

            if (transAmount <= 0)
            {
                return String.Format(STR_AmountMustBeGreaterThan0, transType);
            }
            if (transAmount >= 10000)
            {
                return String.Format(STR_AmountMustBeLessThan1000000, transType);
            }
            return "";
        }
        protected string validateCancelTransactionAmount(decimal transAmount, string transType)
        {
            /* Cancel Transaction Amount can be 0 */
            if (transType == null || transType == "") transType = "Transaction";

            if (transAmount < 0)
            {
                return String.Format(STR_AmountMustBeGreaterThan0, transType);
            }
            if (transAmount >= 10000)
            {
                return String.Format(STR_AmountMustBeLessThan1000000, transType);
            }
            return "";
        }
        protected string validateTerminalId(string terminalId)
        {
            if (terminalId != null && terminalId.Trim() != "" && !terminalId.StartsWith("S"))
            {
                return STR_NotAHicapsTerminal;
            }
            return "";
        }
        protected string validateServerUrl(string serverUrl)
        {

            return "";
        }
        protected string validateRefundPassword(string refundPassword)
        {
            if (!isNumeric(refundPassword) || refundPassword.Trim().Length != 4)
            {
                return STR_RefundPasswordMustBeA4DigitNumber;
            }
            return "";
        }
        #region Medicare Validation
        protected string validateItemOverrideCode(string itemOverrideCode)
        {
            if (!string.IsNullOrEmpty(itemOverrideCode))
            {
                if ((itemOverrideCode.Length != 3) || ("AP,AO,NC,".IndexOf(itemOverrideCode + ",") < 0))
                {
                    return STR_InvalidItemOverrideCodeSpecifiedValidValuesAreBl;
                }
            }
            return "";
        }
        protected string validateReferralOverrideCode(string referralOverrideCode)
        {
            if (!string.IsNullOrEmpty(referralOverrideCode))
            {
                if ((referralOverrideCode.Length != 1) || ("N,L,E,".IndexOf(referralOverrideCode + ",") < 0))
                {
                    return STR_InvalidReferralOverrideCodeSpecifiedValidValuesA;
                }
            }
            return "";
        }
        protected string validateReferralPeriodCode(string referralPeriodCode)
        {
            if (!string.IsNullOrEmpty(referralPeriodCode))
            {
                if ((referralPeriodCode.Length != 1) || ("S,I,".IndexOf(referralPeriodCode + ",") < 0))
                {
                    return STR_InvalidReferralPeriodCodeSpecifiedValidValuesAre;
                }
            }
            return "";
        }
        protected string validateMedicareIrn(string medicareIrn)
        {
            if (!string.IsNullOrEmpty(medicareIrn))
            {
                if (!isNumeric(medicareIrn))
                {
                    return STR_MedicareIRNCanOnlyBeNumeric;
                }
                if (medicareIrn.StartsWith("0"))
                {
                    return STR_MedicareIRNCannotStartWithLeadingZeros;
                }
            }
            return "";
        }
        protected string validateMedicarePatientIrn(string medicareIrn)
        {
            if (!string.IsNullOrEmpty(medicareIrn))
            {
                if (!isNumeric(medicareIrn))
                {
                    return STR_MedicareIRNCanOnlyBeNumeric;
                }
            }
            else
            {
                return STR_PatientMedicareIRNMustBeFilledId;
            }
            return "";
        }
        protected string validateMedicareClaimType(int claimType)
        {
            if (claimType > 4 || claimType < 0)
            {
                return STR_InvalidClaimTypeClaimTypeMustBe1FullyPaid2Partly;
            }
            return "";

        }
        protected string validateMedicareNumber(string medicareNumber)
        {
            // add check digit routine.
            if (!string.IsNullOrEmpty(medicareNumber))
            {
                if (!isMedicareCardValid(medicareNumber))
                {
                    return String.Format(STR_InvalidMedicareNumberCheckTheNumberAndTryAgain, medicareNumber);
                }
            }
            return "";
        }
        protected string validateCevRequestInd(int ClaimType, string CevRequestInd,string ServiceTypeCde)
        {
            if (!string.IsNullOrEmpty(CevRequestInd) && ClaimType != 4)
            {
                return STR_CevRequestIndIsOnlyValidForBulkBillClaims;
            }
            if (!string.IsNullOrEmpty(CevRequestInd) && ClaimType == 4 && CevRequestInd != "Y" && ServiceTypeCde != "S" && CevRequestInd != "N")
            {
                return STR_InvalidCEVRequestIndValidValuesAreYOrN;
            }
            return "";
        }
        protected string validatePrintReceiptFlag(string PrintReceiptFlag)
        {
            if (!string.IsNullOrEmpty(PrintReceiptFlag))
            {
                if ((PrintReceiptFlag.Length != 1) || ("M,C,P,".IndexOf(PrintReceiptFlag + ",") < 0))
                {
                    return "Valid PrintReceiptFlag values are 'M', 'C', 'P'";
                }
            }
            return "";
        }
        private static bool isMedicareCardValid(string MedicareNumber)
        {
            /******************************************************************************************************
             * p_check_medicare_no.sql :  							
             *
             *	Checks person's 10 digit medicare numbers.
             *	Digit 1 must be in the range 2 to 6
             *	Digit 9 is a check digit and must be the remainder of:
             *	(digit 1 + (digit 2 * 3) + (digit 3 * 7) + (digit 4 * 9) + digit 5  + (digit 6 * 3) + (digit 7 * 7) + (digit 8 * 9)) / 10
             *
             * INPUTS :									
             *		@a_c_medicareno
             *
             * OUTPUTS :								
             *		@a_c_result  (either "True" or "False")	
             * MODS:									
             *		GL 29/10/99
             *		JP Rewrote to C# 2008
             ******************************************************************************************************/
            char[] charArray;
            int digit1;
            int digit2;
            int digit3;
            int digit4;
            int digit5;
            int digit6;
            int digit7;
            int digit8;
            int digit9;
            int remainderDigit;
            bool returnValue;


            if (MedicareNumber == null) { return true; }
            MedicareNumber = MedicareNumber.Trim();
            if (MedicareNumber == "") { return true; }

            charArray = MedicareNumber.ToCharArray();

            if (MedicareNumber.Length < 10) { return false; }

            digit1 = System.Convert.ToInt16(charArray[0]) - 48;
            digit2 = System.Convert.ToInt16(charArray[1]) - 48;
            digit3 = System.Convert.ToInt16(charArray[2]) - 48;
            digit4 = System.Convert.ToInt16(charArray[3]) - 48;
            digit5 = System.Convert.ToInt16(charArray[4]) - 48;
            digit6 = System.Convert.ToInt16(charArray[5]) - 48;
            digit7 = System.Convert.ToInt16(charArray[6]) - 48;
            digit8 = System.Convert.ToInt16(charArray[7]) - 48;
            digit9 = System.Convert.ToInt16(charArray[8]) - 48;

            if ((digit1 >= 2) && (digit1 <= 6))
            {
                remainderDigit = (digit1 + (digit2 * 3) + (digit3 * 7) + (digit4 * 9) + digit5 + (digit6 * 3) + (digit7 * 7) + (digit8 * 9)) % 10;

                if (digit9 == remainderDigit)
                {
                    returnValue = true;

                }
                else
                {
                    returnValue = false;
                }
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }
        #endregion

        protected bool isNumeric(string strValue)
        {
            string regExpression = @"/(^-?\d\d*\.\d*$)|(^-?\d\d*$)|(^-?\.\d\d*$)/";
            return Regex.IsMatch(strValue, regExpression);
        }

        protected bool isAlphaNumeric(string strValue)
        {
            string regExpression = @"^[a-zA-Z0-9]+$";
            return Regex.IsMatch(strValue, regExpression);
        }

        protected bool isSpaceAlphaNumeric(string strValue)
        {
            string regExpression = @"^[a-zA-Z0-9 ]+$";
            return Regex.IsMatch(strValue, regExpression);
        }

        protected void validateScriptNumber(string scriptNumber)
        {
            if (string.IsNullOrEmpty(scriptNumber) || scriptNumber == string.Empty || !isNumeric(scriptNumber) || scriptNumber.Length > 10 || scriptNumber.Length < 1)
            {
                throw new InvalidOperationException("Pharmacy Script number is missing or invalid");
            }
        }
        private string _terminalVersion = "";

        public string TerminalVersion
        {
            get { return _terminalVersion; }
            set { _terminalVersion = value; }
        }
        public string ToDebugString()
        {
            string myoutput = "";
            try
            {
                PropertyInfo[] properties = this.GetType().GetProperties();
                List<string> myList = new List<string>();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(this, null) != null && property.GetValue(this, null).GetType() == myList.GetType())
                    {
                        List<string> myArray = (List<string>)property.GetValue(this, null);
                        foreach (string subRow in myArray)
                        {
                            myList.Add(String.Format("{0} = {1}\r\n", property.Name, subRow ?? ""));
                        }
                    }
                    else
                    {
                        myList.Add(String.Format("{0} = {1}\r\n", property.Name, property.GetValue(this, null) ?? ""));
                    }
                }
                // Sort the list;
                string[] myOutputArray = myList.ToArray();
                Array.Sort(myOutputArray);
                foreach (var myRow in myOutputArray)
                {
                    myoutput += myRow;
                }
            }
            catch (Exception )
            {
                myoutput = "Error creating debug string";
            }
            return myoutput;
        }

    }
}
