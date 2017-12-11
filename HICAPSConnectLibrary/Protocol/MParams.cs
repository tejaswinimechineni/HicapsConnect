using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HICAPSConnectLibrary.Protocol
{
    /// <summary>
    /// Medicare Claim structure
    /// </summary>
    public class MParams
    {
        public Dictionary<string, string> EncodeMedicare()
        {
            Dictionary<string, string> fields = new Dictionary<string, string>();
            return fields;
        }

        public Dictionary<string, string> GetM0Fields()
        {
            string m0Field = "";
            int sizm0Field = 0;
            var fieldsM0 = new Dictionary<string, string>();

            //DF 03 09 30 30 30 30 30 30 30 30 30
            if (!string.IsNullOrEmpty(AccountReferenceId))
            {
                fieldsM0.Add("DF-03",AccountReferenceId);
            }
            //DF 04 01 4E
            if (ClaimType != 4)
            {
                if (AccountPaidInd == "N")
                {
                    fieldsM0.Add("DF-04", "N");
                }
                else
                {
                    fieldsM0.Add("DF-04", "Y");
                }
            }
            // Do not send down claimant details for Bulk Bill
            if (ClaimType != 4)
            {
                //DF 05 10 12 34 56 78 90 12 34 56 78 90 ClaimantMedicareCardNum
                if (!string.IsNullOrEmpty(ClaimantMedicareCardNum))
                {
                    fieldsM0.Add("DF-05", ClaimantMedicareCardNum);
                }
                // DF 06 01 31 ClaimantIRN
                if (!string.IsNullOrEmpty(ClaimantIRN))
                {
                    fieldsM0.Add("DF-06", ClaimantIRN);
                }
            }
            //DF 07 10 12 34 56 78 90 12 34 56 78 90 PatientMedicareCardNum
            if (!string.IsNullOrEmpty(PatientMedicareCardNum))
            {
                fieldsM0.Add("DF-07", PatientMedicareCardNum);               
            }
            // DF 08 01 31 PatientIRN
            if (!string.IsNullOrEmpty(PatientIRN))
            {
                fieldsM0.Add("DF-08", PatientIRN); 
            }
            // DF 41 CEV Request indicator
            if (!string.IsNullOrEmpty(CevRequestInd))
            {
                CevRequestInd = CevRequestInd.ToUpper();
                fieldsM0.Add("DF-41", CevRequestInd); 
            }

            // DF 42 08 30 31 32 33 34 35 36 37  PayeeProviderNum
            //
            if (!string.IsNullOrEmpty(PayeeProviderNum))
            {
                fieldsM0.Add("DF-41", PayeeProviderNum); 
            }
            // DF 0A 08 30 31 32 33 34 35 36 37 ServicingProviderNum
            //
            if (!string.IsNullOrEmpty(ServicingProviderNum))
            {
                fieldsM0.Add("DF-0A", ServicingProviderNum);
            }
            // This is correct
            sizm0Field = m0Field.Replace(" ", "").Length / 2;
            // DF 0C  ?? PseudoProviderNum
            #region Second Level Data Elementa
            //Second Level Data Elements
            string secondLevelm0Field = "";

            //DF 0D 04 01 12 20 09
            if (ReferralIssueDate != null && ReferralIssueDate > new DateTime(2000, 1, 1))
            {
                string dateTimeField = ReferralIssueDate.Day.ToString("D2") + " " + ReferralIssueDate.Month.ToString("D2") + " " + ReferralIssueDate.Year.ToString().Substring(0, 2) + " " + ReferralIssueDate.Year.ToString().Substring(2, 2);
                fieldsM0.Add("DF-0D", dateTimeField.Replace(" ",""));
            }
            // DF 0E 01 53
            if (!string.IsNullOrEmpty(ReferralPeriodTypeCde))
            {
                fieldsM0.Add("DF-0E", ReferralPeriodTypeCde.Substring(0, 1));
            }
            // DF 0F 01 53
            if (!string.IsNullOrEmpty(ReferralOverrideTypeCde))
            {
                fieldsM0.Add("DF-0F", ReferralOverrideTypeCde.Substring(0, 1));
             }
            // DF 10 08 32 31 30 32 31 32 32 31
            if (!string.IsNullOrEmpty(ReferringProviderNum))
            {
                fieldsM0.Add("DF-10", ReferringProviderNum);
            }
            #region Requesting part
            //DF 11 04 01 12 20 09
            if (RequestIssueDate != null && RequestIssueDate > new DateTime(2000, 1, 1))
            {
                string dateTimeField = RequestIssueDate.Day.ToString("D2") + " " + RequestIssueDate.Month.ToString("D2") + " " + RequestIssueDate.Year.ToString().Substring(0, 2) + " " + RequestIssueDate.Year.ToString().Substring(2, 2);
                fieldsM0.Add("DF-11", dateTimeField.Replace(" ", ""));
                secondLevelm0Field = secondLevelm0Field + "DF 11 04 " + dateTimeField + " ";
            }
            // DF 12 01 53
            if (!string.IsNullOrEmpty(RequestOverrideTypeCde) && string.IsNullOrEmpty(RequestingProviderNum))
            {
                fieldsM0.Add("DF-12", RequestOverrideTypeCde.Substring(0, 1));
            }
            // DF 13 01 53
            if (!string.IsNullOrEmpty(RequestTypeCde) && !string.IsNullOrEmpty(RequestingProviderNum))
            {
                fieldsM0.Add("DF-13", RequestTypeCde.Substring(0, 1));
            }
            // DF 14 08 32 31 30 32 31 32 32 31
            if (!string.IsNullOrEmpty(RequestingProviderNum))
            {
                fieldsM0.Add("DF-14", RequestingProviderNum);
            }
            #endregion
            // DF 15 01 4F ServiceType Cde
            // ServiceTypeCde not valid for Bulk Bill
            if (ClaimType != 4)
            {
                if (!string.IsNullOrEmpty(ServiceTypeCde))
                {
                    //  secondLevelm0Field = secondLevelm0Field + "DF 15 01 " + HicapsParams.ServiceTypeCde.Substring(0, 1) + " ";
                    fieldsM0.Add("DF-15", ServiceTypeCde.Substring(0, 1));
                }
            }
            #endregion

            return fieldsM0;
        }
        public List<Dictionary<string, string>> GetM1Fields()
        {
            string m1Field = "";
            string m1SubField = "";
            //TODO extract details out of             HicapsParams.ClaimData 
            List<Dictionary<string, string>> listFieldsM1 = new List<Dictionary<string, string>>();
          
         ///   int linecount = 0;
          //  int m1FieldLen = 0;
            string[] ClaimData = ClaimDetails.ToArray();
            // Each Sub Claim Line
            for (int i = 0; i < ClaimData.Length; i++)
            {
                var fieldsM1 = new Dictionary<string, string>();
                //        6          6          8                  2            6           5            1                       6
                //data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr;
                //00002301000021072009        000000
                //00002301000021072009              000000
                string data = ClaimData[i];
                string itemNumber = data.Substring(0, 6).Trim(); //= "12345";
                string chargeAmount = data.Substring(6, 6).Trim(); ;//= "0000060";
                string dateofservice = data.Substring(12, 8).Trim(); ;//= "01072009";
                string itemOverrideCode = data.Substring(20, 2).Trim(); //= "AP"; //"AP"
                string lspn = data.Substring(22, 6).Trim();//= "123456"; //123456
                string equipmentId = data.Substring(28, 5).Trim();//= "12345"; //12345
                string selfDeemedCde = data.Substring(33, 2).Trim();//= "Y";  //Y
                string contribPatientAmount = data.Substring(35, 6).Trim();//= "000000"; // "0000000";
                string spcid = data.Substring(41, 4).Trim(); // = "    "

                // D1 = Charge Amount
                if (!string.IsNullOrEmpty(chargeAmount) && chargeAmount != "000000")
                {
                    // can only be size 7
                    int amount = Convert.ToInt32(chargeAmount);
                    string hexField = amount.ToString("X").PadLeft(6,'0');
                    fieldsM1.Add("D1",hexField);
                }
                // D9 = Date of Service
                if (!string.IsNullOrEmpty(dateofservice))
                {
                    // can only be size 8
                    fieldsM1.Add("D9", dateofservice);
                }
                // D2 = ItemNumber AlphsNumeric
                if (!string.IsNullOrEmpty(itemNumber))
                {
                    // Can only be size 6
                    fieldsM1.Add("D2", itemNumber);
                }
                // D3 = Item OVerride Code ?
                if (!string.IsNullOrEmpty(itemOverrideCode))
                {
                    // itemOverrideCode can only be 2.
                    fieldsM1.Add("D3", itemOverrideCode);
                    
                }
                // D4 = Patient Contributon amount
                if (!string.IsNullOrEmpty(contribPatientAmount) && contribPatientAmount != "000000")
                {
                    // Can only be most size 7, 9999999
                    int contribAmount = Convert.ToInt32(contribPatientAmount);
                    string hexField2 = contribAmount.ToString("X").PadLeft(6,'0');
                    fieldsM1.Add("D4", hexField2);
                }

                // D8 = LSPN
                if (!string.IsNullOrEmpty(lspn))
                {
                    // can only be len 6
                    lspn = lspn.PadLeft(6,'0');
                    fieldsM1.Add("D8", lspn);
                }

                // DA selfDeemedCde Id
                if (!string.IsNullOrEmpty(selfDeemedCde))
                {
                    // can only be len 2
                    fieldsM1.Add("DA", selfDeemedCde);
                }

                // DB Equipment Id
                if (!string.IsNullOrEmpty(equipmentId))
                {
                    // Can only be len 5
                    fieldsM1.Add("DB", equipmentId);
                }
                // DC spcid
                if (!string.IsNullOrEmpty(spcid))
                {
                    // Can only be len 4
                    fieldsM1.Add("DC", spcid);
                }

                listFieldsM1.Add(fieldsM1);
                // End of repeat line
            }
            return listFieldsM1;
        }
        private decimal _transactionAmount;

        public MParams()
        {
            _claimDetails = new List<string>();
            TransactionAmount = 0;
        }
        public string AccountPaidInd { get; set; }
        public string AccountReferenceId { get; set; }
        public decimal BenefitAssigned { get; set; }
        public string CevRequestInd { get; set; }
        public string ClaimantIRN { get; set; }
        public string ClaimantMedicareCardNum { get; set; }


        /// <summary>
        /// Claim Data format stored as a string array to help serialisation
        /// ItemNumber + ItemDescription +  ChargeAmount + dateofService + ItemOverrideCode + LspNum
        /// </summary>
        private List<string> _claimDetails;

        public List<string> ClaimDetails
        {
            get { return _claimDetails; }
            set { }
        }


        /// <summary>
        /// SELECT MEDICARE TXN
        /// 1. FULLY PAID
        /// 2. PART PAID
        /// 3. UN-PAID
        /// 4. BULK BILL 
        /// </summary>
        public int ClaimType { get; set; }

        public string PatientIRN { get; set; }

        public string PatientMedicareCardNum { get; set; }

        public string PayeeProviderNum { get; set; }

        public DateTime ReferralIssueDate { get; set; }

        public string ReferralOverrideTypeCde { get; set; }

        private string _referralPeriodTypeCde;
        public string ReferralPeriodTypeCde
        {
            get { return _referralPeriodTypeCde; }
            // Where we have Alpha indicators ie : ReferralPeriodTypeCde 
            // Can you please ensure that they are sent in upper case

            set { _referralPeriodTypeCde = (value ?? "").ToUpper(); }
        }

        public string ReferringProviderNum { get; set; }
        public string RequestTypeCde { get; set; }
        public string RequestingProviderNum { get; set; }

        public DateTime RequestIssueDate { get; set; }

        public string RequestOverrideTypeCde { get; set; }

        public string RequestPeriodTypeCde { get; set; }

        public string ServiceTypeCde { get; set; }

        public string ServicingProviderNum { get; set; }

        public decimal TransactionAmount
        {
            get { return _transactionAmount; }
            set { }
        }

        public void addMediClaimLine(string itemNum, decimal chargeAmount, DateTime dateOfService,
            string itemOverrideCde, string lspNum, string equipmentId, string selfDeemedCde,
            decimal contribPatientAmount, string spcId)
        {
            string data = "";
            string dateofservice = "";
            string feeamount = "00";
            string contribPatientAmountStr = "00";
            string zerofill = "00000000";

            _transactionAmount += chargeAmount;
            if (chargeAmount < 0)
            {
                throw new InvalidOperationException("Charge amount can only be a positive value");
            }
            if (contribPatientAmount < 0)
            {
                throw new InvalidOperationException("Patient Contribution Amount can only be a positive value");
            }
            if (chargeAmount < contribPatientAmount)
            {
                throw new InvalidOperationException("Patient Contribution amount must be less that Charge");
            }
            itemNum = itemNum.PadLeft(6,' ');
            //bodyPart = Right(zerofill + bodyPart, 2);
            //// 
            //if (!isSpaceAlphaNumeric(itemNum))
            //{
            //    itemNum = "000000";
            //    throw new IndexOutOfRangeException(
            //        "Invalid Item Number, Item Number must only contain AlphaNumeric values only eg A-Z, a-z, 0-9, and spaces");
            //}
            //if (!isNumeric(bodyPart)) { itemNumber = "00"; throw new IndexOutOfRangeException("Invalid Bodypart id, Bodypart must contain numeric values only eg 11"); }
            dateofservice = dateOfService.Day.ToString().PadLeft(2, '0') +
                            dateOfService.Month.ToString().PadLeft(2, '0')+
                            dateOfService.Year.ToString().PadLeft(2, '0');
            //    // Get fee into 012500" format for 125.00
            feeamount = (chargeAmount*System.Convert.ToDecimal(100)).ToString();
            if (feeamount.IndexOf('.') > 0)
            {
                feeamount = feeamount.Substring(0, feeamount.IndexOf('.'));//Left(feeamount, feeamount.IndexOf('.'));
            }
            feeamount = (zerofill + feeamount).Right(6);

            // Contrib Patient Amount
            contribPatientAmountStr = (contribPatientAmount*System.Convert.ToDecimal(100)).ToString();
            if (contribPatientAmountStr.IndexOf('.') > 0)
            {
                contribPatientAmountStr = contribPatientAmountStr.Substring(0, contribPatientAmountStr.IndexOf('.'));
            }
            contribPatientAmountStr = contribPatientAmountStr.PadLeft(6,'0');

            lspNum = lspNum.PadLeft(6, ' ');//(new String(' ', 6) + lspNum).Right(6);
            itemOverrideCde = itemOverrideCde.PadLeft(2, ' ');//((new String(' ', 2) + itemOverrideCde).Right(2);
            equipmentId = equipmentId.PadLeft(5);//Right((new String(' ', 5) + equipmentId), 5);
            spcId = spcId.PadLeft(4, ' ');//Right((new String(' ', 4) + spcId), 4);
            selfDeemedCde = selfDeemedCde.PadLeft(2, ' ');//Right(("  " + selfDeemedCde), 2);
            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            //        6          6          8                  2           6           5            2                       6                  4
            data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr + spcId;
            _claimDetails.Add(data);

        }
    }
}
