using System;
using System.Collections.Generic;

namespace HICAPSConnectLibrary.Protocol
{
    public class CParams
    {
        private List<string> _patientNameDetails = new List<string>(); 
        public List<string> PatientNameDetails
        {
            get { return _patientNameDetails; }
            set { _patientNameDetails = value; }
        }

        public decimal ClaimTransactionAmount = 0.00M;
        public string PatientName;


        public CParams()
        {
            VendorName = "HICAPSConnectClient";
            PmsKey = "8562026";
            MedicareClaim = new MParams();
        }

        /// <summary>
        ///     Medicare Claim Details structure.
        /// </summary>
        public MParams MedicareClaim { get; set; }

        /// <summary>
        ///     IP Address of Client
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        ///     IP Port for Address of Client
        /// </summary>
        public string IPPort { get; set; }

        /// <summary>
        ///     Name of Computer generating transaction
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        ///     Cashout amount for EFTPOS transactions
        /// </summary>
        public decimal CashOutAmount { get; set; }

        /// <summary>
        ///     Sale amount for EFTPOS transactions
        /// </summary>
        public decimal SaleAmount { get; set; }

        /// <summary>
        ///     Cancel/Refund amount for HICAPS/EFTPOS transactions
        /// </summary>
        public decimal CancelAmount { get; set; }

        /// <summary>
        ///     Merchant Password for Refund EFTPOS Transaction
        /// </summary>
        public string MRPwd { get; set; }

        /// <summary>
        ///     Merchant ID for EFTPOS transactions
        /// </summary>
        public string MerchantId { get; set; }

        /// <summary>
        ///     Provider Number ID for All transactions
        /// </summary>
        public string ProviderNumberId { get; set; }

        /// <summary>
        ///     Print Today Transactions for HICAPS Totals Transaction
        /// </summary>
        public bool TodayTransactions { get; set; }

        /// <summary>
        ///     Print Receipts on Terminal for HICAPS Transactions
        /// </summary>
        public bool PrintReceiptOnTerminal { get; set; }

        /// <summary>
        ///     Print Customer Receipt for EFTPOS Transactions
        /// </summary>
        public bool PrintCustomerReceipt { get; set; }

        /// <summary>
        ///     Show Print Customer Receipt Prompt on Terminal for EFTPOS Transactions
        /// </summary>
        public bool PrintCustomerReceiptPrompt { get; set; }

        /// <summary>
        ///     Print Merchant Receipt for EFTPOS Transactions
        /// </summary>
        public bool PrintMerchantReceipt { get; set; }

        /// <summary>
        ///     Choose what last receipt type to print, for Print Last Receipt
        /// </summary>
        public string Acquirer { get; set; }

        /// <summary>
        ///     Type of Settlement to be performed for Settlement Transaction
        /// </summary>
        public string SettlementType { get; set; }

        public string TerminalId { get; set; }

        public string NetworkName { get; set; }

        public string PmsKey { get; set; }

        public string VendorName { get; set; }

        public bool PharmacyFlag { get; set; }

        public bool NonSettlementFlag { get; set; }

        /// <summary>
        ///  Principle Merchant ID for Terminal, used for Encryption 
        /// </summary>
        public string EncryptMerchantId { get; set; }

        /// <summary>
        ///  Principle Terminal ID for Terminal, used for Encryption 
        /// </summary>
        public string EncryptTerminalId { get; set; }

        /// PCI Settings
        
        /// <summary>
        /// Primary Account Number
        /// </summary>
        public string PrimaryAccountNumber { get; set; }

        /// <summary>
        /// CCV Field for CC EFTPos Transactions
        /// </summary>
        public string CCV { get; set; }

        /// <summary>
        /// CCVReason Field for CC EFTPos Transactions
        /// </summary>
        public int CCVReason { get; set; }

        /// <summary>
        /// CCVSource Field for CC EFTPos Transactions
        /// </summary>
        public int CCVSource { get; set; }

        /// <summary>
        /// ExpiryDate Field for CC EFTPos Transactions
        /// </summary>
        public string ExpiryDate { get; set; }

        private List<string> _claimDetails = new List<string>();
        public List<string> ClaimDetails
        {
            get { return _claimDetails; }
            set { _claimDetails = value; }
        }

        public string ClaimData
        {
            get
            {
                string result = "";
                foreach (string claimDetail in _claimDetails)
                {
                    result += claimDetail;
                }
                return result;
            }
        }

        public int ClaimLines
        {
            get { return _claimDetails.Count; }
            set { }
        }

        public string RRN { get; set; }

       

        public void ClaimLinesClear()
        {
            _claimDetails.Clear();
            ClaimTransactionAmount = 0.00M;
        }

        public void ClaimLinesAdd(string patientId, string itemNumber, string bodyPart, DateTime serviceDate,
            decimal fee)
        {
            string data = "";
            string dateservice = "";
            string feeamount = "00";
            // string zerofill = "00000000";
            //  string errorMessage = "";
            if (bodyPart == null)
            {
                bodyPart = "";
            }
            if (patientId == null)
            {
                patientId = "";
            }
            if (itemNumber == null)
            {
                itemNumber = "";
            }

            patientId = patientId.PadLeft(2, '0');
            itemNumber = itemNumber.PadLeft(4, ' ');
            bodyPart = bodyPart.PadLeft(2, '0');
            dateservice = serviceDate.Day.ToString().PadLeft(2, '0') + serviceDate.Month.ToString().PadLeft(2, '0');
            // Get fee into 012500" format for 125.00
            feeamount = "00000000" + (fee*Convert.ToDecimal(100));
            if (feeamount.IndexOf('.') > 0)
            {
                feeamount = Left(feeamount, feeamount.IndexOf('.'));
            }
            feeamount = Right(feeamount, 6);

            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            data = patientId + itemNumber + bodyPart + dateservice + feeamount;
            _claimDetails.Add(data);
            ClaimTransactionAmount += fee;
        }

        private string Left(string leftString, int length)
        {
            return leftString.Substring(0, length);
        }

        private string Right(string rightString, int length)
        {
            return rightString.Substring(rightString.Length - length);
        }
    }
}