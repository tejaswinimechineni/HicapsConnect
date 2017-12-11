using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Request object used to return the Medicare Claim Response Details from Medicare.
    /// </summary>
    public class MedicareClaimResponse : BaseResponse 
    {
        private List<string> _medicareClaimDetails;

        public List<string> MedicareClaimDetails
        {
            get { return _medicareClaimDetails; }
            set { _medicareClaimDetails = value; }
        }
        public MedicareClaimResponse()
        {
            MedicareClaimDetails = new List<string>();
        }

        private string _accountReferenceId;

        public string AccountReferenceId
        {
            get { return _accountReferenceId; }
            set { _accountReferenceId = value; }
        }

        private string _accountPaidInd;

        public string AccountPaidInd
        {
            get { return _accountPaidInd; }
            set { _accountPaidInd = value; }
        }

        private string _acceptanceTypeCde;

        public string AcceptanceTypeCde
        {
            get { return _acceptanceTypeCde; }
            set { _acceptanceTypeCde = value; }
        }

        private string _assessmentErrorCde;

        public string AssessmentErrorCde
        {
            get { return _assessmentErrorCde; }
            set { _assessmentErrorCde = value; }
        }

        private string _claimantFirstName;

        public string ClaimantFirstName
        {
            get { return _claimantFirstName; }
            set { _claimantFirstName = value; }
        }

        private string _claimantLastName;

        public string ClaimantLastName
        {
            get { return _claimantLastName; }
            set { _claimantLastName = value; }
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
        private string _medicareError;

        public string MedicareError
        {
            get { return _medicareError; }
            set { _medicareError = value; }
        }
        private string _medicareEligibilityStatus;
        public string MedicareEligibilityStatus
        {
            get { return _medicareEligibilityStatus; }
            set { _medicareEligibilityStatus = value; }
        }
        private string _merchantId;

        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }

        private string _patientFirstName;

        public string PatientFirstName
        {
            get { return _patientFirstName; }
            set { _patientFirstName = value; }
        }

        private string _patientLastName;

        public string PatientLastName
        {
            get { return _patientLastName; }
            set { _patientLastName = value; }
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
        

        private string _referringProviderNum;

        public string ReferringProviderNum
        {
            get { return _referringProviderNum; }
            set { _referringProviderNum = value; }
        }
        private string _referralOverrideTypeCde;

        public string ReferralOverrideTypeCde
        {
            get { return _referralOverrideTypeCde; }
            set { _referralOverrideTypeCde = value; }
        }
        private string _referringProviderName;
      
        public string ReferringProviderName
        {
            get { return _referringProviderName; }
            set { _referringProviderName = value; }
        }
        private string _referralPeriodTypeCde;

        public string ReferralPeriodTypeCde
        {
            get { return _referralPeriodTypeCde; }
            set { _referralPeriodTypeCde = value; }
        }
        private string _requestingProviderNum;

        public string RequestingProviderNum
        {
            get { return _requestingProviderNum; }
            set { _requestingProviderNum = value; }
        }

        private string _requestingProviderName;

        public string RequestingProviderName
        {
            get { return _requestingProviderName; }
            set { _requestingProviderName = value; }
        }
        private string _requestOverrideTypeCde;

        public string RequestOverrideTypeCde
        {
            get { return _requestOverrideTypeCde; }
            set { _requestOverrideTypeCde = value; }
        }
        private DateTime _requestIssueDate;

        public DateTime RequestIssueDate
        {
            get { return _requestIssueDate; }
            set { _requestIssueDate = value; }
        }
        private string _requestTypeCde;

        public string RequestTypeCde
        {
            get { return _requestTypeCde; }
            set { _requestTypeCde = value; }
        }
        private DateTime _referralIssueDate;

        public DateTime ReferralIssueDate
        {
            get { return _referralIssueDate; }
            set { _referralIssueDate = value; }
        }
       private string _payeeProviderNum;

       public string PayeeProviderNum
        {
            get { return _payeeProviderNum; }
            set { _payeeProviderNum = value; }
        }

        private string _psuedoProviderNum;

        public string PsuedoProviderNum
        {
            get { return _psuedoProviderNum; }
            set { _psuedoProviderNum = value; }
        }

        private string _servicingProviderNum;

        public string ServicingProviderNum
        {
            get { return _servicingProviderNum; }
            set { _servicingProviderNum = value; }
        }
        private string _servicingProviderName;

        public string ServicingProviderName
        {
            get { return _servicingProviderName; }
            set { _servicingProviderName = value; }
        }
        private string _transactionId;

        public string TransactionId
        {
            get { return _transactionId; }
            set { _transactionId = value; }
        }

        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }
        private string _inConfidenceKey;

        public string InConfidenceKey
        {
            get { return _inConfidenceKey; }
            set { _inConfidenceKey = value; }
        }
        private string _serviceTypeCde;

        public string ServiceTypeCde
        {
            get { return _serviceTypeCde; }
            set { _serviceTypeCde = value; }
        }
        private string _terminalId;

        public string TerminalId
        {
            get { return _terminalId; }
            set { _terminalId = value; }
        }
        private decimal _transactionAmount;

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { _transactionAmount = value; }
        }
        public void addExplanationCode(int index, string explanationCode)
        {
            if (MedicareClaimDetails.Count > index)
            {
                MedicareClaimDetails[index] = Left(MedicareClaimDetails[index], 51) + explanationCode + MedicareClaimDetails[index].Substring(55);
            }
        }
        public void addMediClaimLineResponse(string itemNum, decimal chargeAmount, DateTime dateOfService, string itemOverrideCde, string lspNum, string equipmentId, string selfDeemedCde, decimal contribPatientAmount, string spcId,decimal benefitAmount,string explanationCode,decimal benefitAssignedAmount, decimal scheduleFeeAmount)
        {
            string data = "";
            string dateofservice = "";
            string feeamount = "00";
            string contribPatientAmountStr = "00";
            string benefitAmountStr = "00";
            string benefitAssignedAmountStr = "00";
            string scheduleFeeAmountStr = "00";
            string zerofill = "00000000";

            TransactionAmount += chargeAmount;
          
            itemNum = Right(new String(' ', 6) + itemNum, 6);
            dateofservice = Right(zerofill + dateOfService.Day.ToString(), 2) + Right(zerofill + dateOfService.Month.ToString(), 2) + Right(zerofill + dateOfService.Year.ToString(), 4);
            //    // Get fee into 012500" format for 125.00
            feeamount = (chargeAmount * System.Convert.ToDecimal(100)).ToString();
            if (feeamount.IndexOf('.') > 0) { feeamount = Left(feeamount, feeamount.IndexOf('.')); }
            feeamount = Right(zerofill + feeamount, 6);

            // Contrib Patient Amount
            contribPatientAmountStr = (contribPatientAmount * System.Convert.ToDecimal(100)).ToString();
            if (contribPatientAmountStr.IndexOf('.') > 0) { contribPatientAmountStr = Left(contribPatientAmountStr, contribPatientAmountStr.IndexOf('.')); }
            contribPatientAmountStr = Right(zerofill + contribPatientAmountStr, 6);

            // benefit amount
            benefitAmountStr = (benefitAmount * System.Convert.ToDecimal(100)).ToString();
            if (benefitAmountStr.IndexOf('.') > 0) { benefitAmountStr = Left(benefitAmountStr, benefitAmountStr.IndexOf('.')); }
            benefitAmountStr = Right(zerofill + benefitAmountStr, 6);

            //benefit assigned amount // bulk bill
              benefitAssignedAmountStr = (benefitAssignedAmount * System.Convert.ToDecimal(100)).ToString();
            if (benefitAssignedAmountStr.IndexOf('.') > 0) { benefitAssignedAmountStr = Left(benefitAssignedAmountStr, benefitAssignedAmountStr.IndexOf('.')); }
            benefitAssignedAmountStr = Right(zerofill + benefitAssignedAmountStr, 6);

            // ScheduleFee Amount
            //benefit assigned amount // bulk bill
            scheduleFeeAmountStr = (scheduleFeeAmount * System.Convert.ToDecimal(100)).ToString();
            if (scheduleFeeAmountStr.IndexOf('.') > 0) { scheduleFeeAmountStr = Left(scheduleFeeAmountStr, scheduleFeeAmountStr.IndexOf('.')); }
            scheduleFeeAmountStr = Right(zerofill + scheduleFeeAmountStr, 6);

            lspNum = Right((new String(' ', 6) + lspNum), 6);
            itemOverrideCde = Right((new String(' ', 2) + itemOverrideCde), 2);
            equipmentId = Right((new String(' ', 5) + equipmentId), 5);
            spcId = Right((new String(' ', 4) + spcId), 4);
            selfDeemedCde = Right(("  " + selfDeemedCde), 2); //TODO fix size... should be 2
            explanationCode = Right((new String(' ', 4) + explanationCode), 4);
            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            //        6          6          8                  2           6           5            2                       6                  4           6                 4                   6                        6
            data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr + spcId + benefitAmountStr + explanationCode + benefitAssignedAmountStr + scheduleFeeAmountStr;
            this.MedicareClaimDetails.Add(data);

            //ChargeAmount += fee;
        }
    }
}
