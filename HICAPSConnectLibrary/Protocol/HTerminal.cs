using System;
using System.Collections.Generic;
using HICAPSConnectLibrary.Utils;

namespace HICAPSConnectLibrary.Protocol
{
    public class HTerminal
    {
        private static readonly string STR_HICAPSNONSETTLENOTALLOWED = "HICAPS NON-SETTLE NOT ALLOWED";
        private static readonly string STR_PROVIDERNUMBERNOTFOUND = "PROVIDER NUMBER NOT FOUND";
        private static readonly string STR_Yymmdd = "yyMMdd";
        private static readonly string STR_HhMMss = "hhmmss";
    
        public static HMessage getHicapsTotalsResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            foreach (var myRow in TerminalData.TransactionList)
            {
                switch (myRow.TransTypeCode)
                {
                    case "20":
                    case "26":
                    case "DP":
                    case "E7":
                    case "E8": 
                        break;
                    default: // do nothing
                        break;

                }
            }
            myResponse.ResponseCode = "NT";
            myResponse.FieldValues.Add("02", "NO TOTALS");
            myResponse.disableEdit = true;
            return myResponse;
        }
        public static HMessage getSaleResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.Key = myMessage.Key;
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            //check to see if merchant number exists within the Merchant.Txt file, if not, fail the response 
      //      string[] MerchantList;
            bool boolFoundMerchantNumInTxtFile = false;
            if (TerminalData.MerchantId != "")
            {
                boolFoundMerchantNumInTxtFile = DataQuery.isValidMerchant(TerminalData.MerchantId);
             
            }
            if (TerminalData.CardType != "E")
            {
                myResponse.ResponseCode = "IE";
                myResponse.FieldValues.Add("02", "TRANSACTION CANCELLED");
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12));  //Transaction Amount
                myResponse.FieldValues.Add("30", TerminalData.CardNumber.Left(6) + " " + TerminalData.CardNumber.Right(3)); ///PAN #
                myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
                myResponse.FieldValues.Add("79", "".PadLeft(12,'0')); /// RRN left Padded 0 12 chars
                myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
                myResponse.FieldValues.Add("TS", "S");
                return myResponse;
            }
            if (boolFoundMerchantNumInTxtFile == false)
            {
                myResponse.ResponseCode = "05";
             //   myResponse.FieldValues.Add("00", "03"); // Not Approved
                myResponse.FieldValues.Add("01", "03"); // Approval Code
                myResponse.FieldValues.Add("02", "INVALID MERCHANT -"); // Response Text
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero("0", 12)); /// Cashout Amount left Padded 0 12 chars   
            }
            else
            {
                myResponse.ResponseCode = "00";
              //  myResponse.FieldValues.Add("00", "00"); // Approved
                myResponse.FieldValues.Add("01", HMessage.generateInvoiceNumber()); // Approval Code
                myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12)); /// Cashout Amount left Padded 0 12 chars
            }
            //myResponse.FieldValues.Add("00", "00"); // Approved
            //myResponse.FieldValues.Add("01", TerminalProtocolMessage.generateInvoiceNumber()); // Approval Code
            //myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd)); // Trans Date
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss)); /// Trans Time
            myResponse.FieldValues.Add("30", TerminalData.CardNumber.Left(6) + " " + TerminalData.CardNumber.Right(3)); ///PAN #
            myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
            //myResponse.FieldValues.Add("40", TerminalProtocolMessage.PadLeftZero (myMessage.FieldValues["40"],12)); /// Transaction Amount left Padded 0 12 chars
            myResponse.FieldValues.Add("44", HMessage.GenerateSurcharge(myMessage.FieldValues["40"], TerminalData.SurchargePercent)); /// Surcharge Amount left Padded 0 12 chars
            myResponse.FieldValues.Add("65", HMessage.generateInvoiceNumber()); /// Invoice Number left Padded 0 6 chars
            myResponse.FieldValues.Add("79", HMessage.generateRRN()); /// RRN left Padded 0 12 chars
            myResponse.FieldValues.Add("16", TerminalData.TerminalId.Trim() + "  "); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("TS", "S"); /// Terminal Swipe

            return myResponse;
        }

        public static HMessage getRefundResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            //check to see if merchant number exists within the Merchant.Txt file, if not, fail the response 
            string[] MerchantList;
            bool boolFoundMerchantNumInTxtFile = false;
            if (TerminalData.MerchantId != "")
            {

                    MerchantList = FileRepo.getFileData(FileRepo.Merchants);
                    for (int intProvCntr = 0; intProvCntr < MerchantList.Length; )
                    {
                        if (MerchantList[intProvCntr].Contains(TerminalData.MerchantId.ToString()))
                        {
                            boolFoundMerchantNumInTxtFile = true;
                        }
                        intProvCntr++;
                    }
                
            }
            if (TerminalData.CardType != "E")
            {
                myResponse.ResponseCode = "IE";
                myResponse.FieldValues.Add("02", "TRANSACTION CANCELLED");
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12));  //Transaction Amount
                myResponse.FieldValues.Add("42", HMessage.PadLeftZero(myMessage.FieldValues["42"], 12)); // Cashout Amount
                myResponse.FieldValues.Add("44", HMessage.GenerateSurcharge(myMessage.FieldValues["42"], TerminalData.SurchargePercent));
                myResponse.FieldValues.Add("30", TerminalData.CardNumber.Left(6) + " " + TerminalData.CardNumber.Right(3)); ///PAN #
                myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
                myResponse.FieldValues.Add("79", "".PadLeft(12, '0')); /// RRN left Padded 0 12 chars
                myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
                myResponse.FieldValues.Add("TS", "S");
                return myResponse;
            }

            if (boolFoundMerchantNumInTxtFile == false)
            {
                myResponse.ResponseCode = "05";
                myResponse.FieldValues.Add("00", "03"); // Not Approved
                myResponse.FieldValues.Add("01", "03"); // Approval Code
                myResponse.FieldValues.Add("02", "INVALID MERCHANT -"); // Response Text
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero("0", 12)); /// Cashout Amount left Padded 0 12 chars   
            }
            else
            {
                myResponse.FieldValues.Add("00", "00"); // Approved
                myResponse.FieldValues.Add("01", HMessage.generateInvoiceNumber()); // Approval Code
                myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12)); /// Cashout Amount left Padded 0 12 chars
            }
            //myResponse.FieldValues.Add("00", "00"); // Approved
            //myResponse.FieldValues.Add("01", TerminalProtocolMessage.generateInvoiceNumber()); // Approval Code
            //myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd)); // Trans Date
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss)); /// Trans Time
            myResponse.FieldValues.Add("30", TerminalData.CardNumber); ///PAN #
            myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
            // myResponse.FieldValues.Add("40", TerminalProtocolMessage.PadLeftZero(myMessage.FieldValues["40"], 12)); /// Transaction Amount left Padded 0 12 chars
            myResponse.FieldValues.Add("44", HMessage.GenerateSurcharge(myMessage.FieldValues["40"], TerminalData.SurchargePercent)); /// Surcharge Amount left Padded 0 12 chars
            myResponse.FieldValues.Add("65", HMessage.generateInvoiceNumber()); /// Invoice Number left Padded 0 6 chars
            myResponse.FieldValues.Add("79", HMessage.generateRRN()); /// RRN left Padded 0 12 chars
            myResponse.FieldValues.Add("16", TerminalData.TerminalId.Trim() + "  "); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("TS", "S"); /// Terminal Swipe

            return myResponse;
        }
        public static HMessage getEftposDepositResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            myResponse.Key = myMessage.Key;
            //check to see if merchant number exists within the Merchant.Txt file, if not, fail the response 
            string[] MerchantList;
            bool boolFoundMerchantNumInTxtFile = false;
            if (TerminalData.MerchantId != "")
            {
                
                    MerchantList = FileRepo.getFileData(FileRepo.Merchants);
                    for (int intProvCntr = 0; intProvCntr < MerchantList.Length; )
                    {
                        if (MerchantList[intProvCntr].Contains(TerminalData.MerchantId.ToString()))
                        {
                            boolFoundMerchantNumInTxtFile = true;
                        }
                        intProvCntr++;
                    }
                
            }

            if (boolFoundMerchantNumInTxtFile == false)
            {
                myResponse.ResponseCode = "05";
                myResponse.FieldValues.Add("00", "03"); // Not Approved
                myResponse.FieldValues.Add("01", "03"); // Approval Code
                myResponse.FieldValues.Add("02", "INVALID MERCHANT -"); // Response Text
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero("0", 12)); /// Cashout Amount left Padded 0 12 chars   
            }
            else
            {
                myResponse.FieldValues.Add("00", "00"); // Approved
                myResponse.FieldValues.Add("01", HMessage.generateInvoiceNumber()); // Approval Code
                myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12)); /// Cashout Amount left Padded 0 12 chars
            }
            //myResponse.FieldValues.Add("00", "00"); // Approved
            //myResponse.FieldValues.Add("01", TerminalProtocolMessage.generateInvoiceNumber()); // Approval Code
            //myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd)); // Trans Date
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss)); /// Trans Time
            myResponse.FieldValues.Add("30", TerminalData.CardNumber); ///PAN #
            myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
            //myResponse.FieldValues.Add("40", TerminalProtocolMessage.PadLeftZero(myMessage.FieldValues["40"], 12)); /// Transaction Amount left Padded 0 12 chars
            myResponse.FieldValues.Add("44", HMessage.GenerateSurcharge(myMessage.FieldValues["40"], TerminalData.SurchargePercent)); /// Surcharge Amount left Padded 0 12 chars
            myResponse.FieldValues.Add("65", HMessage.generateInvoiceNumber()); /// Invoice Number left Padded 0 6 chars
            myResponse.FieldValues.Add("79", HMessage.generateRRN()); /// RRN left Padded 0 12 chars
            myResponse.FieldValues.Add("16", TerminalData.TerminalId.Trim() + "  "); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("TS", "S"); /// Terminal Swipe

            return myResponse;
        }
        public static HMessage getCashoutRequest(CParams terminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.TransTypeCode = HTransaction.Cashout;
            myRequest.RequestResponseIndicator = (byte)'0';
            // TURN on Encryption
            myRequest.Key = HMessage.GenerateKey(terminalData.TerminalId, terminalData.MerchantId);

            myRequest.FieldValues.Add("MN", terminalData.MerchantId.PadRight(8)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", terminalData.VendorName); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("PK", terminalData.PmsKey); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("42", HMessage.PadLeftZero("4000", 12)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("MX", "D0");
            if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        public static HMessage getCashoutResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            myResponse.Key = myMessage.Key;
            //check to see if merchant number exists within the Merchant.Txt file, if not, fail the response 
            string[] MerchantList;
            bool boolFoundMerchantNumInTxtFile = false;
            if (TerminalData.MerchantId != "")
            {
               
                    MerchantList = FileRepo.getFileData(FileRepo.Merchants);
                    for (int intProvCntr = 0; intProvCntr < MerchantList.Length; )
                    {
                        if (MerchantList[intProvCntr].Contains(TerminalData.MerchantId.ToString()))
                        {
                            boolFoundMerchantNumInTxtFile = true;
                        }
                        intProvCntr++;
                    }
                
            }
            if (TerminalData.CardType != "E")
            {
                myResponse.ResponseCode = "IE";
                myResponse.FieldValues.Add("02", "TRANSACTION CANCELLED");
                myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12));  //Transaction Amount
                myResponse.FieldValues.Add("42", HMessage.PadLeftZero(myMessage.FieldValues["42"], 12)); // Cashout Amount
                myResponse.FieldValues.Add("44", HMessage.GenerateSurcharge(myMessage.FieldValues["42"], TerminalData.SurchargePercent));
                myResponse.FieldValues.Add("30", TerminalData.CardNumber.Left(6) + " " + TerminalData.CardNumber.Right(3)); ///PAN #
                myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
                myResponse.FieldValues.Add("79", "".PadLeft(12, '0')); /// RRN left Padded 0 12 chars
                myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
                myResponse.FieldValues.Add("TS", "S");
                return myResponse;
            }

            if (boolFoundMerchantNumInTxtFile == false)
            {
                myResponse.ResponseCode = "05";
                myResponse.FieldValues.Add("00", "03"); // Not Approved
                myResponse.FieldValues.Add("01", "03"); // Approval Code
                myResponse.FieldValues.Add("02", "INVALID MERCHANT -"); // Response Text
                myResponse.FieldValues.Add("42", HMessage.PadLeftZero("0", 12)); /// Cashout Amount left Padded 0 12 chars   
            }
            else
            {
                myResponse.FieldValues.Add("00", "00"); // Approved
                myResponse.FieldValues.Add("01", HMessage.generateInvoiceNumber()); // Approval Code
                myResponse.FieldValues.Add("02", "      APPROVED"); // Response Text
                myResponse.FieldValues.Add("42", HMessage.PadLeftZero(myMessage.FieldValues["42"], 12)); /// Cashout Amount left Padded 0 12 chars
            }

            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd)); // Trans Date
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss)); /// Trans Time
            myResponse.FieldValues.Add("30", TerminalData.CardNumber.Left(6) + " " + TerminalData.CardNumber.Right(3)); ///PAN #
            myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
            myResponse.FieldValues.Add("44", HMessage.GenerateSurcharge(myMessage.FieldValues["42"], TerminalData.SurchargePercent)); /// Surcharge Amount left Padded 0 12 chars
            //myResponse.FieldValues.Add("44", TerminalData.
            myResponse.FieldValues.Add("65", HMessage.generateInvoiceNumber()); /// Invoice Number left Padded 0 6 chars
            myResponse.FieldValues.Add("79", HMessage.generateRRN()); /// RRN left Padded 0 12 chars
            myResponse.FieldValues.Add("16", TerminalData.TerminalId.Trim() + "  "); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("TS", "S"); /// Terminal Swipe

            return myResponse;
        }
        public static HMessage getSaleRequest(CParams terminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.Key = HMessage.GenerateKey(terminalData.TerminalId, terminalData.MerchantId);
            myRequest.TransTypeCode = HTransaction.Sale;
            myRequest.RequestResponseIndicator = (byte)'0';
            myRequest.FieldValues.Add("MX", "D0");
            myRequest.FieldValues.Add("MN", terminalData.MerchantId.PadRight(8)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", terminalData.VendorName); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("40", HMessage.PadLeftZero("4000", 6)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("PK", terminalData.PmsKey); /// SAle Amount left Padded 0 12 chars

            if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        public static HMessage getSaleCashoutRequest(CParams terminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.TransTypeCode = HTransaction.SaleCashout;
            myRequest.RequestResponseIndicator = (byte)'0';

            myRequest.FieldValues.Add("MN", terminalData.MerchantId.PadRight(8)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", terminalData.VendorName); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("PK", terminalData.PmsKey); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("40", HMessage.PadLeftZero("4000", 6)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("42", HMessage.PadLeftZero("1500", 6)); /// Cashout Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("MX", "D0");
            if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        public static HMessage getSaleCashoutResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = getCashoutResponse(myMessage, TerminalData);
            myResponse.FieldValues.Add("40", HMessage.PadLeftZero(myMessage.FieldValues["40"], 12)); /// Transaction Amount left Padded 0 12 chars
            return myResponse;
        }
        public static HMessage getClaimRequestSettle(CParams terminalData)
        {
            // Quote is same as a claim on code is different
            HMessage myRequest = getClaimRequest(terminalData);
            myRequest.TransTypeCode = HTransaction.SettlementClaims;
            return myRequest;
        }
        public static HMessage getClaimResponseSettle(HMessage myMessage, HParams terminalData)
        {
            // Quote is same as a claim on code is different
            HMessage myRequest = getClaimResponse(myMessage, terminalData);
            myRequest.TransTypeCode = HTransaction.SettlementClaims;
            return myRequest;
        }
        public static HMessage getQuoteRequest(CParams terminalData)
        {
            // Quote is same as a claim on code is different
            HMessage myRequest = getClaimRequest(terminalData);
            myRequest.TransTypeCode = HTransaction.Quotes;
            return myRequest;
        }
        public static HMessage getQuoteResponse(HMessage myMessage, HParams terminalData)
        {
            // Quote is same as a claim on code is different
            HMessage myRequest = getClaimResponse(myMessage,terminalData);
            myRequest.TransTypeCode = HTransaction.Quotes;
            return myRequest;
        }

        public static HMessage getClaimRequest(CParams terminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.RequestResponseIndicator = (byte)'0';
            // TURN on Encryption
            myRequest.Key = HMessage.GenerateKey(terminalData.TerminalId, terminalData.MerchantId);

            myRequest.TransTypeCode = HTransaction.Claims;
             myRequest.FieldValues.Add("LB","1");
            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", terminalData.VendorName); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("PK", terminalData.PmsKey); /// SAle Amount left Padded 0 12 chars
            
            myRequest.FieldValues.Add("40", HMessage.encodeDecimal(terminalData.ClaimTransactionAmount)); /// Transaction Amount left Padded 0 12 chars
            string eaData = terminalData.ProviderNumberId + terminalData.ClaimData;
            myRequest.FieldValues.Add("EA", eaData);
            if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            if (!(terminalData.PatientName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("PN", terminalData.PatientName);
            }
            return myRequest;

        }
        public static HMessage getClaimResponse(HMessage myMessage, HParams TerminalData)
        {
            decimal Percent100 = 100.00M;
            string claimData = "";
            string strProviderNumber = "";
          //  string[] ProviderList;
            bool boolFoundProviderNumInTxtFile = false;
            HMessage myResponse = new HMessage();
            myResponse.Key = myMessage.Key;
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            string ProviderName = "";
            if (myMessage.FieldValues.ContainsKey("EA"))
            {
                claimData = myMessage.FieldValues["EA"];
                if (claimData.Length >= 8)
                {
                    strProviderNumber = claimData.Substring(0, 8);
                    ProviderName = DataQuery.getProviderName(strProviderNumber);
                }
            }

            //check to see if provider number exists within the Provider.Txt file, if not, fail it unless 
            //the software vendor name is set to Hicaps Processing
            if (myMessage.TransTypeCode == HTransaction.SettlementClaims)
            {
                // Non Settlement code
                if (!myMessage.FieldValues.ContainsKey("VN") ||
                    !myMessage.FieldValues.ContainsKey("PK") ||
                    (myMessage.FieldValues["VN"] != "HICAPS Processing") ||
                    (myMessage.FieldValues["PK"] != "8166258"))
                {
                    myResponse.ResponseCode = "NS";
                    myResponse.FieldValues.Add("02", STR_HICAPSNONSETTLENOTALLOWED);
                    myResponse.disableEdit = true;
                    return myResponse;
                }
                else
                {
                    boolFoundProviderNumInTxtFile = true;
                }
            }
            else
            {
                // Check Provider Exists
                boolFoundProviderNumInTxtFile = DataQuery.isValidProvider(strProviderNumber);
            }


            if (boolFoundProviderNumInTxtFile == false)
            {
                myResponse.ResponseCode = "NP";
                myResponse.FieldValues.Add(HFields.ResponseText.Key, STR_PROVIDERNUMBERNOTFOUND); // Response Text

                // Disable Message Edit function
                myResponse.disableEdit = true;
                return myResponse;
            }
            else if (TerminalData.BenefitPercent > 0)
            {
                myResponse.FieldValues.Add(HFields.ResponseText.Key, "      APPROVED"); // Response Text
                Decimal benefitValue = ((Convert.ToDecimal(myMessage.FieldValues["40"].ToString()) * TerminalData.BenefitPercent) / Percent100);
                myResponse.FieldValues.Add("43", HMessage.encodeDecimal(benefitValue)); /// Benfit Amount left Padded 0 12 chars
                string responseData = HMessage.encodeClaimResponseText(myMessage.FieldValues["EA"], TerminalData.BenefitPercent, TerminalData.MembershipId);
                myResponse.FieldValues.Add("EA", responseData); // Claim Data... include response codes
                string patientData = HMessage.encodeClaimPatientData(myMessage.FieldValues["EA"],
                    TerminalData.PatientNames);
                if (!(patientData.IsNullOrWhiteSpace()))
                {
                    myResponse.FieldValues.Add("PN", patientData);
                }

            }
            else
            {
                myResponse.FieldValues.Add(HFields.ResponseText.Key, "      DECLINED"); // Response Text
                Decimal benefitValue = TerminalData.BenefitPercent;
                myResponse.FieldValues.Add("43", HMessage.encodeDecimal(benefitValue)); /// Benfit Amount left Padded 0 12 chars
                string responseData = HMessage.encodeClaimResponseText(myMessage.FieldValues["EA"], 0, TerminalData.MembershipId);
                myResponse.FieldValues.Add("EA", responseData); // Claim Data...TODO Fix this to include response codes

            }
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd)); // Trans Date
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss)); /// Trans Time
            myResponse.FieldValues.Add("16", TerminalData.TerminalId.Trim() + "  "); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("30", TerminalData.CardNumber); ///PAN #
            myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); ///Expiry US format YYMM
            myResponse.FieldValues.Add("40", myMessage.FieldValues["40"]); /// Transaction Amount left Padded 0 12 chars

            myResponse.FieldValues.Add("79", HMessage.generateRRN()); /// RRN left Padded 0 12 chars
            myResponse.FieldValues.Add("MN", TerminalData.MerchantId); /// Terminal ID 0 8 chars left padded
            myResponse.FieldValues.Add("MP", ProviderName); /// Provider Name 0 16 chars left padded
            myResponse.FieldValues.Add("TS", "S"); /// Terminal Swipe

            return myResponse;
        }
        public static HMessage getCardReadRequest(CParams terminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.RequestResponseIndicator = (byte)'0';
            // TURN on Encryption
            myRequest.Key = HMessage.GenerateKey(terminalData.TerminalId, terminalData.MerchantId);

            myRequest.TransTypeCode = HTransaction.CardRead;
            myRequest.FieldValues.Add("CN", Environment.MachineName);
            myRequest.FieldValues.Add("VN", terminalData.VendorName);
             if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        public static HMessage getCardReadResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            // TURN on Encryption
            myResponse.Key = myMessage.Key;
            // HF Test Card
            myResponse.TransTypeCode = HTransaction.CardRead;
            myResponse.FieldValues.Add(HFields.ResponseText.Key, "CARD READ - OK       ");
            // Do Card Hash.
            string cardNumber = TerminalData.CardNumber;
            string trackData = TerminalData.TrackData;
            if (!cardNumber.StartsWith("9036") && cardNumber.Length != 10)
            {
                cardNumber = cardNumber.Left(6) + " " + cardNumber.Right(3);
                trackData = string.Empty;
            }
            myResponse.FieldValues.Add("30", cardNumber); // Card Number
            myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); // Expiry
            myResponse.FieldValues.Add("32", TerminalData.TrackData2); // Track 2
            myResponse.FieldValues.Add("33", trackData); // Track 1 Raw Data
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));

            return myResponse;
        }
        /// <summary>
        /// This will generate a default response for each message
        /// </summary>
        /// <param name="myMessage"></param>
        /// <returns></returns>
        public static HMessage getDefaultResponseMessage(HMessage myMessage, HParams TerminalData)
        {
            if (!myMessage.isPartOfNetwork(TerminalData.NetworkName))
            {
                return getInvalidNetworkResponse(myMessage.TransTypeCode);
            }

            switch (myMessage.TransTypeCode)
            {
                case HTransaction.TerminalTest: return getTerminalTestResponse(TerminalData);
                case HTransaction.KeyCheck: return getKCResponse(myMessage, TerminalData);
                case HTransaction.EftposDeposit: return getEftposDepositResponse(myMessage, TerminalData);
                case HTransaction.Sale: return getSaleResponse(myMessage, TerminalData);
                case HTransaction.Claims: return getClaimResponse(myMessage, TerminalData);
                case HTransaction.Quotes: return getQuoteResponse(myMessage, TerminalData);
                case HTransaction.SettlementClaims: return getClaimResponse(myMessage, TerminalData);
                case HTransaction.MedicareClaims: return getMedicareClaimResponse(myMessage, TerminalData);
                case HTransaction.CancelClaims: return getClaimCancelResponse(myMessage, TerminalData);
                case HTransaction.Refund: return getRefundResponse(myMessage, TerminalData);
                case HTransaction.ClaimTotals: return getHicapsTotalsResponse(myMessage, TerminalData);
                case HTransaction.CardRead: return getCardReadResponse(myMessage, TerminalData);
                case HTransaction.Cashout: return getCashoutResponse(myMessage, TerminalData);
                case HTransaction.SaleCashout: return getSaleCashoutResponse(myMessage, TerminalData);
                case HTransaction.PrintLastReceipt: return getPrintLastReceiptResponse(myMessage, TerminalData);
                default: return getInvalidEcrResponse(myMessage.TransTypeCode);
            }

        }
        private static HMessage getPrintLastReceiptRequest(CParams TerminalData)
        {
            HMessage myRequest = new HMessage();

            myRequest.TransTypeCode = HTransaction.PrintLastReceipt;
            myRequest.RequestResponseIndicator = (byte)'0';
            if (!(TerminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", TerminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        private static HMessage getPrintLastReceiptResponse(HMessage myMessage, HParams TerminalData)
        {
             string searchClaimsTransList = "21,22,EF";
            string searchEftposTransList = "20,26,DP,E7,E8";

           HMessage lastMessage;
            HMessage myResponse = new HMessage();
            // TODO Get last messageData.
      
            for (int i = TerminalData.TransactionList.Count -1 ; i >=0 ; i--)
            {
                string transCode = TerminalData.TransactionList[i].TransTypeCode;
                if (searchClaimsTransList.Contains(transCode))
                {
                    // CLaim
                    lastMessage = CopyMessageItem("A1", TerminalData.TransactionList[i]);
                    lastMessage.FieldValues.Add("TC", transCode);
                    lastMessage.disableEdit = false;
                    return lastMessage;
                }
                if (searchEftposTransList.Contains(TerminalData.TransactionList[i].TransTypeCode))
                {
                    // Eftpos
                    myResponse.TransTypeCode = HTransaction.PrintLastReceipt;
                    lastMessage = CopyMessageItem("A0", TerminalData.TransactionList[i]);
                    lastMessage.FieldValues.Add("TC", transCode);
                    lastMessage.disableEdit = false;
                    return lastMessage;
                }
            }
           // No data.
            myResponse.TransTypeCode = HTransaction.PrintLastReceipt;
            myResponse.ResponseCode = "00";
            myResponse.FieldValues.Add("02", "RECEIPT - OK");
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));
       
            //myResponse.FieldValues.Add("VR", STR_SIMVERSION);
            return myResponse;
        }

        public static HMessage getMedicareClaimRequest(CParams TerminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.Key = HMessage.GenerateKey(TerminalData.TerminalId, TerminalData.MerchantId);
            myRequest.RequestResponseIndicator = (byte)'0';
            myRequest.TransTypeCode = HTransaction.MedicareClaims;
            myRequest.FieldValues.Add("MX", "D0");
            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", TerminalData.VendorName); /// SAle Amount left Padded 0 12 chars
         //   myRequest.FieldValues.Add("PK", TerminalData.PmsKey); /// SAle Amount left Padded 0 12 chars

            var m0fields =  TerminalData.MedicareClaim.GetM0Fields();
            var m1fields = TerminalData.MedicareClaim.GetM1Fields();
            myRequest.FieldValues.Add("CT",TerminalData.MedicareClaim.ClaimType.ToString()); //1/2/3/4

            myRequest.FieldValues.Add("M0", HMessage.ConvertToHex2(HMessage.encodeM0Field(m0fields, TerminalData.MedicareClaim.ClaimType,false)));
            myRequest.FieldValues.Add("M1", HMessage.ConvertToHex2(HMessage.encodeM1Field(m1fields, TerminalData.MedicareClaim.ClaimType)));
            if (!(TerminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", TerminalData.NetworkName.SHA256());
            }
            return myRequest;
        }

        private static HMessage getMedicareClaimResponse(HMessage myMessage, HParams TerminalData)
        {
            string claimType;
            string M0Data = "";
            string M1Data = "";
            string M2Data = "";
            string M3Data = "";

            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            myResponse.Key = myMessage.Key;
            claimType = "0";
            if (myMessage.FieldValues.ContainsKey("M0")) { M0Data = myMessage.FieldValues["M0"]; }
            if (myMessage.FieldValues.ContainsKey("M1")) { M1Data = myMessage.FieldValues["M1"]; }
            if (myMessage.FieldValues.ContainsKey("M2")) { M2Data = myMessage.FieldValues["M2"]; }
            if (myMessage.FieldValues.ContainsKey("M3")) { M3Data = myMessage.FieldValues["M3"]; }


            // TODO fix this.
            if (myMessage.FieldValues.ContainsKey("CT"))
            {
                claimType = myMessage.FieldValues["CT"];
            }
            switch (claimType)
            {
                case "1": break; // Fully Paid
                case "2": // Part Paid
                    if (!string.IsNullOrEmpty(M0Data)) { myResponse.FieldValues.Add("M0", HMessage.GenerateM0Response(M0Data, 2,TerminalData.CardNumber)); }
                    if (!string.IsNullOrEmpty(M1Data)) { myResponse.FieldValues.Add("M1", HMessage.GenerateM1Response(M1Data, 2)); }
                    if (!string.IsNullOrEmpty(M2Data)) { myResponse.FieldValues.Add("M2", HMessage.GenerateM2Response(M2Data, 2)); }
                    myResponse.FieldValues.Add("M3", HMessage.GenerateM3Response(3)); 
                    break;

                case "3": // UnPaid
                    if (!string.IsNullOrEmpty(M0Data)) { myResponse.FieldValues.Add("M0", HMessage.GenerateM0Response(M0Data, 3,TerminalData.CardNumber)); }
                    if (!string.IsNullOrEmpty(M1Data)) { myResponse.FieldValues.Add("M1", HMessage.GenerateM1Response(M1Data, 3)); }
                    if (!string.IsNullOrEmpty(M2Data)) { myResponse.FieldValues.Add("M2", HMessage.GenerateM2Response(M2Data, 3)); }
                    myResponse.FieldValues.Add("M3", HMessage.GenerateM3Response(3)); 
                    break;
                case "4": // Bulk Bill
                    if (!string.IsNullOrEmpty(M0Data)) { myResponse.FieldValues.Add("M0", HMessage.GenerateM0Response(M0Data,4,TerminalData.CardNumber)); }
                    if (!string.IsNullOrEmpty(M1Data)) { myResponse.FieldValues.Add("M1", HMessage.GenerateM1Response(M1Data,4)); }
                     myResponse.FieldValues.Add("M3", HMessage.GenerateM3Response(4)); 
                    break;
            }
            myResponse.ResponseCode = "00";
            myResponse.FieldValues.Add("02", "Medicare Claim Approved");
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));
            myResponse.FieldValues.Add("16", TerminalData.TerminalId.Trim() + "  ");
            myResponse.FieldValues.Add("30", TerminalData.CardNumber);
            myResponse.FieldValues.Add("MN", TerminalData.MerchantId);
            myResponse.FieldValues.Add("ME", "0000");
            myResponse.disableEdit = true;
            return myResponse;
        }

        public static HMessage getClaimCancelSettleRequest(CParams terminalData)
        {
            HMessage myRequest = getClaimCancelRequest(terminalData);
            myRequest.TransTypeCode = HTransaction.CancelSettlementClaims;

            return myRequest;
        }
        public static HMessage getClaimCancelSettleResponse(HMessage myMessage, HParams terminalData)
        {
            HMessage myResponse = getClaimCancelSettleResponse(myMessage, terminalData);
            myResponse.TransTypeCode = HTransaction.CancelSettlementClaims;
            return myResponse;
        }
        public static HMessage getClaimCancelRequest(CParams terminalData)
        {
            var myRequest = new HMessage();
            myRequest.TransTypeCode = HTransaction.CancelClaims;
            myRequest.RequestResponseIndicator = (byte)'0';
           // TURN on Encryption
            myRequest.Key = HMessage.GenerateKey(terminalData.TerminalId, terminalData.MerchantId);

            myRequest.FieldValues.Add("LB", "1");
            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", terminalData.VendorName); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("PK", terminalData.PmsKey); /// SAle Amount left Padded 0 12 chars

            myRequest.FieldValues.Add("40", HMessage.encodeDecimal(terminalData.ClaimTransactionAmount)); /// Transaction Amount left Padded 0 12 chars
            string eaData = terminalData.ProviderNumberId;
            myRequest.FieldValues.Add("EA", eaData);
            myRequest.FieldValues.Add("79", terminalData.RRN);

            if (!(terminalData.PatientName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("PN", terminalData.PatientName);
            }

            if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        private static HMessage getClaimCancelResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage originClaim = new HMessage();
            //string originalFile = Path.GetTempPath() + Path.DirectorySeparatorChar + myMessage.FieldValues["79"] + ".hic";
            foreach (var myResponseMessage in TerminalData.TransactionList)
            {
                if (myResponseMessage.TransTypeCode == HTransaction.Claims)
                {
                    if (myMessage.FieldValues["79"] == myResponseMessage.FieldValues["79"])
                    {
                        originClaim = myResponseMessage;
                        break;
                    }
                }
            }

            HMessage myResponse = new HMessage();
            if (originClaim.TransTypeCode == HTransaction.Claims)
            {

                myResponse.TransTypeCode = myMessage.TransTypeCode;
                myResponse.ResponseCode = "00";
                myResponse.FieldValues.Add("02", "APPROVED");
                myResponse.FieldValues.Add("EA", originClaim.FieldValues["EA"]);
                myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
                myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));
                myResponse.FieldValues.Add("16", TerminalData.TerminalId); /// Terminal ID 0 8 chars left padded
                myResponse.FieldValues.Add("MN", originClaim.FieldValues["MN"]); /// Merchant ID 0 8 chars left padded
                myResponse.FieldValues.Add("MP", DataQuery.getProviderNameFromMerchantId(originClaim.FieldValues["MN"])); /// Provider Name 0 8 chars left padded
                myResponse.FieldValues.Add("30", TerminalData.CardNumber); // Card Number
                myResponse.FieldValues.Add("31", TerminalData.ExpiryDate); // Expiry
                myResponse.FieldValues.Add("40", originClaim.FieldValues["40"]); /// Transaction Amount left Padded 0 12 chars
                myResponse.FieldValues.Add("43", originClaim.FieldValues["43"]); /// Transaction Amount left Padded 0 12 chars
                myResponse.FieldValues.Add("79", HMessage.generateRRN()); /// RRN left Padded 0 12 chars


            }
            else
            {
                myResponse.TransTypeCode = myMessage.TransTypeCode;
                myResponse.ResponseCode = "81";
                myResponse.FieldValues.Add("02", "UNMATCHED CANCEL");
                myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
                myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));
            }
            return myResponse;

        }

        public static HMessage getTerminalTestRequest(CParams TerminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.TransTypeCode = HTransaction.TerminalTest;
            myRequest.RequestResponseIndicator = (byte)'0';
            myRequest.FieldValues.Add("MX", "");
            myRequest.FieldValues.Add("D0", "");
            if (!(TerminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", TerminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        public static HMessage getTerminalTestResponse(HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = HTransaction.TerminalTest;
            myResponse.FieldValues.Add("02", "ECR COMMS - OK       ");
            //myResponse.FieldValues.Add("EA", "SIMTER  33123433       ");
            myResponse.FieldValues.Add("EA", TerminalData.TerminalId.Trim() + "  " + TerminalData.MerchantId);
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));
            myResponse.FieldValues.Add("VR", TerminalData.Version);


            return myResponse;
        }
        public static HMessage getTerminalIPTestResponse(HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = HTransaction.TerminalIPTest;
            myResponse.FieldValues.Add("02", "ECR COMMS - OK       ");
            //myResponse.FieldValues.Add("EA", "SIMTER  33123433       ");
            myResponse.FieldValues.Add("EA", TerminalData.TerminalId.Trim() + "  " + TerminalData.MerchantId);
            myResponse.FieldValues.Add("IA", TerminalData.IPAddress);
            myResponse.FieldValues.Add("IP", TerminalData.IPPort);
            myResponse.FieldValues.Add("03", DateTime.Now.Date.ToString(STR_Yymmdd));
            myResponse.FieldValues.Add("04", DateTime.Now.ToString(STR_HhMMss));
            myResponse.FieldValues.Add("VR", TerminalData.Version );


            return myResponse;
        }
        public static HMessage getTerminalIPTestRequest(CParams TerminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.TransTypeCode = HTransaction.TerminalIPTest;
            // Mark transaction as Request.
            myRequest.RequestResponseIndicator = (byte) '0';
            myRequest.FieldValues.Add("IA", TerminalData.IPAddress);
            myRequest.FieldValues.Add("IP", TerminalData.IPPort);
            if (!(TerminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", TerminalData.NetworkName.SHA256());
            }
            myRequest.FieldValues.Add("MX", "");

            return myRequest;
        }
        public static HMessage getInvalidEcrResponse(string transTypeCode)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = transTypeCode;
            myResponse.ResponseCode = "IE";
            myResponse.FieldValues.Add("02", "INVALID ECR MESSAGE");
            return myResponse;
        }
        public static HMessage getInvalidNetworkResponse(string transTypeCode)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = transTypeCode;
            myResponse.ResponseCode = "IK";
            myResponse.FieldValues.Add("02", "INVALID NETWORK");
            return myResponse;
        }
        public static HMessage getTerminalBusyResponse(string transTypeCode, HParams terminalData)
        {
            HMessage myResponse = getTerminalTestResponse(terminalData);
            myResponse.TransTypeCode = transTypeCode;
            myResponse.ResponseCode = "TB";
            if (myResponse.FieldValues.ContainsKey("02"))
            {
                myResponse.FieldValues.Remove("02");
            }
            myResponse.FieldValues.Add("02", "TERMINAL BUSY");
            return myResponse;
        }
        public static HMessage getStatusMessageResponse(string messageText)
        {
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = HTransaction.TerminalStatus;
            myResponse.RequestResponseIndicator = (byte)'2';
            myResponse.FieldValues.Add("SM", messageText);
            return myResponse;
        }
        private static HMessage CopyMessageItem(string transCode, HMessage myRow)
        {
            HMessage myItem = new HMessage();
            myItem.TransTypeCode = transCode;
            // Deep copy.
            foreach (var myFieldRow in myRow.FieldValues)
            {
                myItem.FieldValues.Add(myFieldRow.Key, myFieldRow.Value);
            }

            return myItem;
        }

        public static List<HMessage> getListResponse(HMessage myMessage)
        {
            List<HMessage> myResponseList = new List<HMessage>();
            string[] items;
            string messageData = "", lineData = "";
            int sizeLimit = 798;
            int linePadding = 19;
            int lineCount = 0;
            int lineLimit;
            string fieldCode = "20"; // Some messages use Field 20 others use EA Field. 

            switch (myMessage.TransTypeCode)
            {
                case "A8": items = FileRepo.getFileData(FileRepo.Items); linePadding = 19; break;
                case "A4": items = FileRepo.getFileData(FileRepo.Providers); FormatOldProviders(ref items); linePadding = 47; fieldCode = "EA"; break;  // Old Merchant list
                case "A5": items = FileRepo.getFileData(FileRepo.CardList); linePadding = 16; fieldCode = "EA"; break;
                case "A6": items = FileRepo.getFileData(FileRepo.ItemResponseCodes); linePadding = 17; break;
                case "A7": items = FileRepo.getFileData(FileRepo.TransCodeList); linePadding = 21; break;
                case "AP": items = FileRepo.getFileData(FileRepo.Providers); linePadding = 58; fieldCode = "EA"; break;
                case "AM": items = FileRepo.getFileData(FileRepo.Merchants); linePadding = 63; fieldCode = "EA"; break; // New Merchant List
                case "MP": items = FileRepo.getFileData(FileRepo.Providers); FormatMedicareProvider(ref items); linePadding = 58; fieldCode = "EA"; break; // New Merchant List

                default: items = new string[1]; break;
            }

            lineLimit = sizeLimit / linePadding;

            foreach (var itemRow in items)
            {
                if (lineCount == lineLimit)
                {
                    HMessage myItem = new HMessage();
                    myItem.TransTypeCode = myMessage.TransTypeCode;
                    myItem.FieldValues.Add(fieldCode, messageData); // Add field Value
                    myResponseList.Add(myItem); // Add to return List.
                    lineCount = 0; //Start again
                    messageData = ""; // Clear
                }

                lineData = itemRow.Trim();
                lineData += new String(' ', linePadding - lineData.Length);
                messageData += lineData;
                lineCount++;
            }
            // Add in trailer leftover if one exists.
            if (!string.IsNullOrEmpty(messageData))
            {
                HMessage myItem = new HMessage();
                myItem.TransTypeCode = myMessage.TransTypeCode;
                myItem.FieldValues.Add(fieldCode, messageData); // Add field Value
                myResponseList.Add(myItem); // Add to return List.
            }
            // incase file is missing or empty
            if (myResponseList.Count == 0)
            {
                HMessage myItem = new HMessage();
                myItem.TransTypeCode = myMessage.TransTypeCode;
            }
            return myResponseList;
        }

        public static List<HMessage> getListResponse(HMessage myMessage, HParams TerminalData)
        {
            List<HMessage> myResponseList = new List<HMessage>();
            string searchClaimsTransList = "21,22,EF";
            string searchEftposTransList = "20,26,DP,E7,E8";

            try
            {
                foreach (var myRow in TerminalData.TransactionList)
                {
                    if ((myMessage.TransTypeCode == HTransaction.ClaimTotals) && (searchClaimsTransList.Contains(myRow.TransTypeCode)))
                    {
                        HMessage myItem = CopyMessageItem(myMessage.TransTypeCode, myRow);
                        myItem.FieldValues.Add("TC", myRow.TransTypeCode);
                        myResponseList.Add(myItem);
                    }
                    if ((myMessage.TransTypeCode == HTransaction.EftposListing) && (searchEftposTransList.Contains(myRow.TransTypeCode)))
                    {
                        HMessage myItem = CopyMessageItem(myMessage.TransTypeCode, myRow);
                        myItem.FieldValues.Add("TC", myRow.TransTypeCode);
                        myResponseList.Add(myItem);

                    }
                }
            }
            catch (Exception )
            { }
            // Add final header to message.
            HMessage myResponse = new HMessage();
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            if (myResponseList.Count == 0)
            {
                myResponse.FieldValues.Add("02", "NO TRANSACTIONS");
                myResponse.ResponseCode = "NR";
            }
            else
            {
                myResponse.FieldValues.Add("02", "TXN LISTING OK");
                myResponse.ResponseCode = "00";
            }
            // Fields 03 and 04 added in actuall sendProtocol list function in Simulator.cs
            myResponseList.Add(myResponse);

            return myResponseList;
        }
        public static HMessage getKCRequest(CParams terminalData)
        {
            HMessage myRequest = new HMessage();
            myRequest.TransTypeCode = HTransaction.KeyCheck;
            myRequest.RequestResponseIndicator = (byte)'0';

            myRequest.FieldValues.Add("KV", HMessage.GenerateKVC(HMessage.GenerateKey(terminalData.TerminalId, terminalData.MerchantId)));
            if (!(terminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", terminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
        public static HMessage getKCResponse(HMessage myMessage, HParams TerminalData)
        {
            HMessage myResponse = new HMessage();
            myResponse.Key = myMessage.Key;
            myResponse.TransTypeCode = myMessage.TransTypeCode;
            myResponse.RequestResponseIndicator = (byte)'1';
            string kvc = myMessage.FieldValues["KV"];

            // Figure out KVC. 
            string tKvc = HMessage.GenerateKVC(myMessage.Key);
            if (kvc != tKvc)
            {
                myResponse.ResponseCode = "KE"; 
                myResponse.FieldValues.Add("02", "KVC Error");
            }
            else
            {
                myResponse.ResponseCode = "00";
                myResponse.FieldValues.Add("02", "KVC OK");
            }
            return myResponse;
        }
        public static HMessage getListRequest(string transCode,CParams TerminalData)
        {
            HMessage myRequest = new HMessage();
            
            myRequest.TransTypeCode = transCode;
            myRequest.RequestResponseIndicator = (byte)'0';

            myRequest.FieldValues.Add("CN", Environment.MachineName.PadRight(10)); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("VN", TerminalData.VendorName); /// SAle Amount left Padded 0 12 chars
            myRequest.FieldValues.Add("PK", TerminalData.PmsKey); /// SAle Amount left Padded 0 12 chars
       
            if (!(TerminalData.NetworkName.IsNullOrWhiteSpace()))
            {
                myRequest.FieldValues.Add("NN", TerminalData.NetworkName.SHA256());
            }
            return myRequest;
        }
    

      
        private static void FormatOldProviders(ref string[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Length >= 47)
                {
                    items[i] = items[i].Substring(0, 47);
                }
                else
                {
                    // Pad out string.  Strictly speaking this should not happen
                    // But if someone mis-edits the file, this will attempt to fix the problem
                    items[i] = items[i] + new String(' ', 47 - items[i].Length);
                }
            }
        }
        private static void FormatMedicareProvider(ref string[] items)
        {
            //    throw new NotImplementedException();
        }
        internal HMessage processMessage(byte[] hexArray)
        {
            return new HMessage(hexArray);
        }

        public static HMessage CreateTransaction(string requestType, CParams _params)
        {
            switch (requestType)
            {
                case HTransaction.AllItemList:
                case HTransaction.AllMedicareProviderList:
                case HTransaction.AllMerchantList :
                case HTransaction.AllProviderList:
                case HTransaction.CardList:
                case HTransaction.ItemResponseList:
                case HTransaction.TransactionResponseList:
                case HTransaction.ProviderList:
                case HTransaction.ClaimListing: // not sure this is right...
                case HTransaction.EftposListing: // not sure this is right...
                case HTransaction.ClaimTotals:
                    return getListRequest(requestType, _params);
                case HTransaction.Claims:
                    return getClaimRequest(_params);
                case HTransaction.SettlementClaims:
                    return getClaimRequestSettle(_params);
                case HTransaction.CancelClaims:
                    return getClaimCancelRequest(_params);
                case HTransaction.CancelSettlementClaims:
                    return getClaimCancelSettleRequest(_params);
                case HTransaction.CardRead:
                    return getCardReadRequest(_params);
                case HTransaction.Cashout:
                    return getCashoutRequest(_params);
                case HTransaction.EftposDeposit:
                    return getEftposDepositRequest(_params);
                case HTransaction.KeyCheck:
                    return getKCRequest(_params);
                case HTransaction.MedicareClaims:
                    return getMedicareClaimRequest(_params);
                case HTransaction.PrintLastReceipt:
                    return getPrintLastReceiptRequest(_params);
                case HTransaction.TerminalTest:
                    return getTerminalTestRequest(_params);
                case HTransaction.TerminalIPTest:
                    return getTerminalIPTestRequest(_params);                 
            
                default : throw new ArgumentOutOfRangeException(requestType);
            }
        }



        private static HMessage getEftposDepositRequest(CParams _params)
        {
            throw new NotImplementedException();
        }
    }
}
