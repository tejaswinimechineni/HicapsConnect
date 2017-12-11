using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HICAPSConnectLibrary
{
    public class TerminalParameters
    {
        #region Properties
        /// <summary>
        /// Default Computer Name
        /// </summary>
        private string _computerName = "HICAPS Connect";
        public string ComputerName
        {
            get { return _computerName; }
            set { _computerName = value; }
        }

        /// <summary>
        /// Default Merchant Password
        /// </summary>
        private string _mRPwd;
        public string MRPwd
        {
            get { return _mRPwd ?? ""; }
            set { _mRPwd = value; }
        }

        /// <summary>
        /// Default Vendor Name
        /// </summary>
        private string _vendorName = "HICAPS Connect";
        public string VendorName
        {
            get { return _vendorName; }
            set { _vendorName = value; }
        }

        public string Acquirer { get; set; }
        /// <summary>
        /// Amount to Cancel
        /// </summary>
        public decimal CancelAmount { get; set; }

        /// <summary>
        /// Amount for Cashout
        /// </summary>
        public decimal CashOutAmount { get; set; }

        /// <summary>
        /// CCV Value, Credit-Card
        /// </summary>
        public string CCV { get; set; }

        /// <summary>
        /// Reason No CCV Provided, Credit Card
        /// </summary>
        public int CCVReason { get; set; }

        /// <summary>
        /// Source that CCV was obtained
        /// </summary>
        public int CCVSource { get; set; }

        // Temporary Claim Data in the format..... //Settings.Option.PatientID + Settings.Option.ItemNumber + Settings.Option.BodyPart + Settings.Option.DateOfService + (Settings.Option.ItemAmount * 100).ToString("000000");
        public string[] ClaimData { get; set; }

        public bool Disable54AFields { get; set; }

        public bool DisablePatientNameFields { get; set; }

        public string EncryptMerchantId { get; set; }

        public string EncryptTerminalId { get; set; }
        public bool EncryptionFlag { get; set; }
        public string ExpiryDate { get; set; }
        public byte[] KVC { get; set; }
        public string MerchantId { get; set; }
        public bool MultiMerchant { get; set; }
        public bool NonSettlementFlag { get; set; }

        public string[] PatientData { get; set; }

        public string PmsKey { get; set; }

        public string PrimaryAccountNumber { get; set; }

        public bool PrintCustomerReceipt { get; set; }

        public bool PrintCustomerReceiptPrompt { get; set; }

        public bool PrintMerchantReceipt { get; set; }

        public bool PrintReceiptOnTerminal { get; set; }

        public string ProviderNumberId { get; set; }

        public string RrnNumber { get; set; }

        public decimal SaleAmount { get; set; }

        public string SettlementType { get; set; }

        public string TerminalId { get; set; }

        public bool TodayTransactions { get; set; }

        public decimal TotalClaimAmount { get; set; }
        public string USExpiryDate
        {
            get
            {
                // Format is YYMM.  _expiryDate is formated MMYY
                return ExpiryDate.Substring(2, 2) + ExpiryDate.Substring(0, 2);
            }
        }

      
        #endregion
        #region Medicare Properties
        // SELECT MEDICARE TXN
        // 1. FULLY PAID
        //  2. PART PAID
        // 3. UN-PAID
        // 4. BULK BILL

        public string AccountPaidInd { get; set; }

        public string AccountReferenceId { get; set; }
        
        public decimal BenefitAssigned { get; set; }
        
        public string CevRequestInd { get; set; }
        
        public string ClaimantIRN { get; set; }

        public string ClaimantMedicareCardNum { get; set; }

        public int ClaimType { get; set; }
        public string PatientIRN { get; set; }

        public string PatientMedicareCardNum { get; set; }
        public string PayeeProviderNum { get; set; }
        public string PsuedoProviderNum { get; set; }

        public DateTime ReferralIssueDate { get; set; }

        public string ReferralOverrideTypeCde { get; set; }

        public string ReferralPeriodTypeCde { get; set; }

        public string ReferringProviderNum { get; set; }

        public string RequestingProviderNum { get; set; }

        public DateTime RequestIssueDate { get; set; }

        public string RequestOverrideTypeCde { get; set; }

        public string RequestPeriodTypeCde { get; set; }

        public string RequestTypeCde { get; set; }

        public string ServiceTypeCde { get; set; }

        public string ServicingProviderNum { get; set; }
        // I think this should be boolean ?
        #endregion


        public TerminalParameters()
        {
            TodayTransactions = true;
            DisablePatientNameFields = false;
            Disable54AFields = false;
            NonSettlementFlag = false;
            PrintReceiptOnTerminal = false;
            TotalClaimAmount = 0;
        }

        public bool PharmacyFlag { get; set; }

        public string getPatientDataAsFieldString()
        {
            string field;
            string result = "";
            for (int i = 0; i < PatientData.Length; i++)
            {
                field = "";
                field = PatientData[i].Trim();
                field = field + new String(' ', 32 - field.Length);
                result += field;
            }
            return result;
        }
    }
}


