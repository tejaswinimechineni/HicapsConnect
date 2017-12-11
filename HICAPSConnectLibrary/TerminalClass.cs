using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HICAPSConnectLibrary.Utils;

namespace HICAPSConnectLibrary
{
   static class TerminalClass
    {
        /// <summary>
        /// Sends the MSG.
        /// </summary>
        /// <param name="RequestType">Type of the request.</param>
        /// <param name="HicapsParams">The hicaps params.</param>
        /// <param name="bRequestMsg">The b request MSG.</param>
        /// <returns></returns>
        internal static string ToTerminalMessage(string RequestType, TerminalParameters HicapsParams, out byte[] bRequestMsg)
        {

            // John P locals
            string strHexin = "", strHexout = "";
            //  string strSalesAmnt, strCashOut;

            // Original
            string[] TransCodes = { "20", "21", "22", "26", "42", "50", "55", "61", "A4", "A5", "D0", "E7", "E8", "EF", "MP", "MC", "DP", "AP", "AM", "A0", "HT", "HL", "EL", "KC", "A6", "A7", "A8" };
            string[] ReqTypes = { "Sale", "Claim", "Quote", "Refund", "Void", "Card Read", "55", "Settlement", "Providers Info", "Card Tables", "Test", "Cashout", "Plus Cash", "Cancel", "Medicare Providers", "Medicare Claim", "Eftpos Deposit", "All Providers", "All Merchants", "Print Last Receipt", "Hicaps Totals", "Hicaps Txn Listing", "Eftpos Txn Listing", "Key Check", "Item Response Code", "Txn Response Code", "Item List" };
            string strRequestMsg = "";
         
           // int pos = 0;
            var enc = new UTF8Encoding();

            // Check for valid Request Type
            int index = Array.IndexOf(ReqTypes, RequestType);

            string strAddInfo = "4D 58 00 02 44 30 1C ";
            string strAddInfoEncrypt = "4D 58 00 02 46 30 1C ";
          
            string strAddPrintReceiptInfo = "4D 58 00 02 44 31 1C ";
            string strAddPrintReceiptInfoEncrypt = "4D 58 00 02 46 31 1C ";
            string strAddPrintYesterdayTransactionsPms = "4D 58 00 02 44 31 1C ";
            string strAddPrintYesterdayTransactionsTerm = "4D 58 00 02 44 32 1C ";
            string strAddPrintReceiptInfoPms = "4D 58 00 02 44 30 1C ";

            string strTransCode = TransCodes[index];
            // Setup Encryption
           
            switch (strTransCode)
            {
                case "20":      // Sale
                    strHexin = "02 00 34 36 30 30 30 30 30 30 30 30 30 31 30 32 30 30 30 30 1C 4D 58 00 02 46 30 1C 03 00";
                    string amount = FieldEncoder.encodeHexString(HicapsParams.SaleAmount);

                    if (HicapsParams.MultiMerchant)
                    {
                        // strHexin = strHexin.Replace(strMultiMerchant, "");
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MN", HicapsParams.MerchantId.Trim()), strHexin);
                    }
                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    if (HicapsParams.SaleAmount > 0)
                    {
                        //34 30 00 05 31 32 35 30 30 1C
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.SaleAmount)), strHexin);
                    }
                    else
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.SaleAmount)), strHexin);
                    }
                    // Sending down PAN
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("31", HicapsParams.USExpiryDate), strHexin);

                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C1", HicapsParams.CCV), strHexin);
                        if (HicapsParams.CCVReason > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C2", HicapsParams.CCVReason.ToString()), strHexin);
                        }
                        if (HicapsParams.CCVSource > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C3", HicapsParams.CCVSource.ToString()), strHexin);
                        }
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                        strHexin = FieldEncoder.encodePrintOptions(HicapsParams, strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;

                case "21":      // Claim
                    // Jp Changed... New field called 'LB' = Loyalty Bonus [4C 42 00 01 31 1C]

                    if (!HicapsParams.NonSettlementFlag)
                    {
                        strHexin = "02 00 66 36 30 30 30 30 30 30 30 30 30 31 30 32 31 30 30 30 1C 4D 58 00 02 46 30 1C 4C 42 00 01 31 1C 03 4A";
                    }
                    else
                    {
                        strHexin = "02 00 66 36 30 30 30 30 30 30 30 30 30 31 30 32 33 30 30 30 1C 4D 58 00 02 46 30 1C 4C 42 00 01 31 1C 03 4A";
                    }

                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    // Print receipt on terminal functionality

                    if (HicapsParams.PrintReceiptOnTerminal)
                    {
                        if (!HicapsParams.EncryptionFlag)
                        {
                            strHexin = strHexin.Replace(strAddInfo, strAddPrintReceiptInfo);
                        }
                        else
                        {
                            strHexin = strHexin.Replace(strAddInfoEncrypt, strAddPrintReceiptInfoEncrypt);
                        }
                    }

                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.TotalClaimAmount)), strHexin);

                    //if (AdditionalInfo != true)
                    //{
                    //    strHexin = strHexin.Replace(strAddInfo, "");
                    //}
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }

                    //    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", encodeDecimal(HicapsParams.TotalClaimAmount)), strHexin);

                    string claimField = FieldEncoder.Right("00000000" + HicapsParams.ProviderNumberId, 8);
                    string encryptData = "";
                    if (HicapsParams.EncryptionFlag)
                    {
                        // Bit of a workaround.  Only First 8 bytes are encrypted of EA Field.
                        encryptData = FieldEncoder.encodeAlphaEncrypt("30", claimField, HicapsParams.KVC);
                        encryptData = encryptData.Substring(12, 23);
                        claimField = FieldEncoder.ConvertToHex(encryptData);
                    }
                    foreach (string claimData in HicapsParams.ClaimData)
                    {
                        claimField += claimData;
                    }
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("EA", claimField), strHexin);

                    // Add Patient Card Information
                    if (HicapsParams.PatientData != null && HicapsParams.PatientData.Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PN", HicapsParams.getPatientDataAsFieldString()), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        if (!HicapsParams.EncryptionFlag)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("30", HicapsParams.PrimaryAccountNumber), strHexin);
                        }
                        else
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        }
                    }
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;

                case "22":      // Quote
                    strHexin = "02 00 65 36 30 30 30 30 30 30 30 30 30 31 30 32 32 30 30 30 1C 4D 58 00 02 46 30 1C 4C 42 00 01 31 1C 03 49";
                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                     // Print receipt on terminal functionality

                    if (HicapsParams.PrintReceiptOnTerminal)
                    {
                        if (!HicapsParams.EncryptionFlag)
                        {
                            strHexin = strHexin.Replace(strAddInfo, strAddPrintReceiptInfo);
                        }
                        else
                        {
                            strHexin = strHexin.Replace(strAddInfoEncrypt, strAddPrintReceiptInfoEncrypt);
                        }
                    }

                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }

                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.TotalClaimAmount)), strHexin);

                    string quoteField = FieldEncoder.Right("00000000" + HicapsParams.ProviderNumberId, 8);

                    string encryptQuoteData = "";
                    if (HicapsParams.EncryptionFlag)
                    {
                        // Bit of a workaround.  Only First 8 bytes are encrypted of EA Field.
                        encryptQuoteData = FieldEncoder.encodeAlphaEncrypt("30", quoteField, HicapsParams.KVC);
                        encryptQuoteData = encryptQuoteData.Substring(12, 23);
                        quoteField = FieldEncoder.ConvertToHex(encryptQuoteData);
                    }
                    foreach (string claimData in HicapsParams.ClaimData)
                    {
                        quoteField += claimData;
                    }
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("EA", quoteField), strHexin);

                    // Add Patient Card Information
                    if (HicapsParams.PatientData != null && HicapsParams.PatientData.Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PN", HicapsParams.getPatientDataAsFieldString()), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        if (!HicapsParams.EncryptionFlag)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("30", HicapsParams.PrimaryAccountNumber), strHexin);
                        }
                        else
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        }
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "DP":  // Eftpos Deposit... Works same as refund
                    strHexin = "02 00 45 36 30 30 30 30 30 30 30 30 30 31 30 44 50 30 30 30 1C 4D 58 00 02 46 30 1C 4D 52 00 06 35 34 38 31 36 30 1C 4D 4E 00 08 31 32 33 34 35 36 37 38 1C 4D 58 00 02 46 30 1C 03 7C";

                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    if (HicapsParams.MRPwd == "")
                    {
                        strHexin = strHexin.Replace("4D 52 00 06 35 34 38 31 36 30 1C ", "");
                    }
                    else
                    {
                        string strPwd = "";
                        // Must be only 6 numeric values
                        HicapsParams.MRPwd = HicapsParams.MRPwd.Substring(0, 6);
                        foreach (char c in HicapsParams.MRPwd)
                        {
                            strPwd = strPwd + String.Format("{0:X2} ", (int)c);
                        }
                        strHexin = strHexin.Replace("4D 52 00 06 35 34 38 31 36 30 ", "4D 52 00 0" + HicapsParams.MRPwd.Length.ToString() + " " + strPwd);
                    }

                    string cancelAmount = FieldEncoder.encodeDecimal(HicapsParams.CancelAmount);
                    if (HicapsParams.CancelAmount > 0)
                    {
                        //34 30 00 05 31 32 35 30 30 1C
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.CancelAmount)), strHexin);
                    }
                    else
                    {

                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.CancelAmount)), strHexin);
                    }

                    if (HicapsParams.MerchantId != "")
                    {
                        string strMerchant = "";
                        foreach (char c in HicapsParams.MerchantId)
                        {
                            strMerchant = strMerchant + String.Format("{0:X2} ", (int)c);
                        }

                        strHexin = strHexin.Replace("4D 4E 00 08 31 32 33 34 35 36 37 38 ", "4D 4E 00 08 " + strMerchant);
                    }
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("31", HicapsParams.USExpiryDate), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C1", HicapsParams.CCV), strHexin);
                        if (HicapsParams.CCVReason > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C2", HicapsParams.CCVReason.ToString()), strHexin);
                        }
                        if (HicapsParams.CCVSource > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C3", HicapsParams.CCVSource.ToString()), strHexin);
                        }
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                        strHexin = FieldEncoder.encodePrintOptions(HicapsParams, strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "26":      // Refund
                    strHexin = "02 00 45 36 30 30 30 30 30 30 30 30 30 31 30 32 36 30 30 30 1C 4D 52 00 06 35 34 38 31 36 30 1C 4D 4E 00 08 31 32 33 34 35 36 37 38 1C 4D 58 00 02 46 30 1C 03 7C";

                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    if (HicapsParams.MRPwd == "")
                    {
                        strHexin = strHexin.Replace("4D 52 00 06 35 34 38 31 36 30 1C ", "");
                    }
                    else
                    {
                        string strPwd = "";
                        // Must be only 6 numeric values
                        HicapsParams.MRPwd = HicapsParams.MRPwd.Substring(0, 6);
                        foreach (char c in HicapsParams.MRPwd)
                        {
                            strPwd = strPwd + String.Format("{0:X2} ", (int)c);
                        }
                        strHexin = strHexin.Replace("4D 52 00 06 35 34 38 31 36 30 ", "4D 52 00 0" + HicapsParams.MRPwd.Length.ToString() + " " + strPwd);
                    }

                    if (HicapsParams.CancelAmount > 0)
                    {
                        //34 30 00 05 31 32 35 30 30 1C
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.CancelAmount)), strHexin);
                    }
                    else
                    {
                        HicapsParams.CancelAmount = 0;
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.CancelAmount)), strHexin);
                    }


                    if (HicapsParams.MerchantId != "")
                    {
                        string strMerchant = "";
                        foreach (char c in HicapsParams.MerchantId)
                        {
                            strMerchant = strMerchant + String.Format("{0:X2} ", (int)c);
                        }

                        strHexin = strHexin.Replace("4D 4E 00 08 31 32 33 34 35 36 37 38 ", "4D 4E 00 08 " + strMerchant);
                    }

                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("31", HicapsParams.USExpiryDate), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C1", HicapsParams.CCV), strHexin);
                        if (HicapsParams.CCVReason > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C2", HicapsParams.CCVReason.ToString()), strHexin);
                        }
                        if (HicapsParams.CCVSource > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C3", HicapsParams.CCVSource.ToString()), strHexin);
                        }
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                        strHexin = FieldEncoder.encodePrintOptions(HicapsParams, strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "A0": // Print Last Receipt
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 30 30 30 30 1C 4D 58 00 02 44 30 1C 03 45";
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (HicapsParams.PrintReceiptOnTerminal)
                    {
                        // Not sure why b
                        strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintReceiptInfo);
                    }
                    else
                    {
                        // Not sure why, but this requirses 81 instead of C1 to not print
                        strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintReceiptInfoPms);
                    }

                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "EF":      // Cancel
                    // Jp Changed... New field called 'LB' = Loyalty Bonus [4C 42 00 01 31 1C]
                    if (!HicapsParams.NonSettlementFlag)
                    {
                        strHexin = "02 00 64 36 30 30 30 30 30 30 30 30 30 31 30 45 46 30 30 30 1C 4D 58 00 02 44 30 1C 4C 42 00 01 31 1C 03 6A";
                    }
                    else
                    {
                        strHexin = "02 00 64 36 30 30 30 30 30 30 30 30 30 31 30 32 34 30 30 30 1C 4D 58 00 02 44 30 1C 4C 42 00 01 31 1C 03 6A";
                    }
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("EA", HicapsParams.ProviderNumberId), strHexin);

                    if (HicapsParams.PrintReceiptOnTerminal)
                    {
                        strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintReceiptInfo);
                    }
                    else
                    {
                        strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintReceiptInfoPms);
                    }

                    if (HicapsParams.CancelAmount > 0)
                    {                    
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.CancelAmount)), strHexin);
                    }
                    else
                    {
                        HicapsParams.CancelAmount = 0;
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.CancelAmount)), strHexin);
                    }
                    // Add Patient Card Information
                    if (HicapsParams.PatientData != null && HicapsParams.PatientData.Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PN", HicapsParams.getPatientDataAsFieldString()), strHexin);
                    }
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("30", HicapsParams.PrimaryAccountNumber), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.RrnNumber))
                    {
                        HicapsParams.RrnNumber = FieldEncoder.Right("000000000000" + HicapsParams.RrnNumber, 12);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("79", HicapsParams.RrnNumber), strHexin);
                    }
                    else
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("79", new string(' ', 12)), strHexin);
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "HT": // Hicaps Totals
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 48 54 30 30 30 1C 4D 58 00 02 44 30 1C 03 45";
                    if (HicapsParams.TodayTransactions == false)
                    {
                        // Make mx field = 4D 58 00 02 43 31
                        if (HicapsParams.PrintReceiptOnTerminal)
                        {
                            strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintYesterdayTransactionsTerm);
                        }
                        else
                        {
                            strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintYesterdayTransactionsPms);
                        }
                    }
                    else
                    {
                        if (HicapsParams.PrintReceiptOnTerminal)
                        {
                            strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintReceiptInfo);
                        }
                        else
                        {
                            // Do nothing
                            strHexin = strHexin.Replace(strAddPrintReceiptInfoPms, strAddPrintReceiptInfoPms);
                        }
                    }

                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.ProviderNumberId) && HicapsParams.ProviderNumberId != "00000000")
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PR", HicapsParams.ProviderNumberId), strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);

                    break;
                case "HL":// Hicaps Transaction Listings
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 48 4C 30 30 30 1C 4D 58 00 02 44 30 1C 03 45";

                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    if (!string.IsNullOrEmpty(HicapsParams.ProviderNumberId) && HicapsParams.ProviderNumberId != "00000000")
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PR", HicapsParams.ProviderNumberId), strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);

                    break;
                case "EL": // Eftpos Transaction Listings
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 45 4C 30 30 30 1C 4D 58 00 02 44 30 1C 03 45";
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MN", HicapsParams.MerchantId), strHexin);
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);

                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "50":      // Card read
                    strHexin = "02 00 25 36 30 30 30 30 30 30 30 30 30 31 30 35 30 30 30 30 1C 4D 58 00 02 44 30 1C 03 0B";

                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;

                case "61":      // Settlement
                    strHexin = "02 00 32 36 30 30 30 30 30 30 30 30 30 31 30 36 31 30 30 30 1C 03 01";
                    if (HicapsParams.PrintReceiptOnTerminal)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MX", "D0"), strHexin);
                    }
                    else
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MX", "D2"), strHexin);
                    }
                                      if (!string.IsNullOrEmpty(HicapsParams.MerchantId))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MN", HicapsParams.MerchantId.PadRight(15)), strHexin);
                    }
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.SettlementType))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("ST", HicapsParams.SettlementType), strHexin);
                    }

                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;

                case "D0":      // terminal test                   
                    strHexin = "02 00 25 36 30 30 30 30 30 30 30 30 30 31 30 44 30 30 30 30 1C 4D 58 00 02 44 30 1C 03 7A";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "A4":      // providers details
                    //strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 4D 50 30 30 30 1C 03 2D";
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 34 30 30 30 1C 03 45";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "AP": // All provider details
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 50 30 30 30 1C 03 45";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "AM": // All Merchant details
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 4D 30 30 30 1C 03 45";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "A5":      // cards details
                    strHexin = "02 00 25 36 30 30 30 30 30 30 30 30 30 31 30 41 35 30 30 30 1C 4D 58 00 02 44 30 1C 03 7A";                  
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "A6": // Item Response Code
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 36 30 30 30 1C 03 47";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "A7": // Txn Response Code
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 37 30 30 30 1C 03 46";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "A8": // Item List
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 41 38 30 30 30 1C 03 49";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "MP":      // providers details
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 4D 50 30 30 30 1C 03 2D";
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
                case "E7":      // cash advance
                    strHexin = "02 00 34 36 30 30 30 30 30 30 30 30 30 31 30 45 37 30 30 30 1C 4D 58 00 02 46 30 1C 03 72";
                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    if (HicapsParams.MultiMerchant)
                    {
                        // strHexin = strHexin.Replace(strMultiMerchant, "");
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MN", HicapsParams.MerchantId), strHexin);
                    }
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (HicapsParams.CashOutAmount > 0)
                    {
                        //34 30 00 05 31 32 35 30 30 1C
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("42", FieldEncoder.encodeDecimal(HicapsParams.CashOutAmount)), strHexin);
                    }
                    else
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("42", FieldEncoder.encodeDecimal(HicapsParams.CashOutAmount)), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("31", HicapsParams.USExpiryDate), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C1", HicapsParams.CCV), strHexin);
                        if (HicapsParams.CCVReason > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C2", HicapsParams.CCVReason.ToString()), strHexin);
                        }
                        if (HicapsParams.CCVSource > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C3", HicapsParams.CCVSource.ToString()), strHexin);
                        }
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                        strHexin = FieldEncoder.encodePrintOptions(HicapsParams, strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;

                case "E8":      // cash plus details
                    strHexin = "02 00 43 36 30 30 30 30 30 30 30 30 30 31 30 45 38 30 30 30 1C 4D 58 00 02 46 30 1C 03 15";
                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    if (HicapsParams.MultiMerchant)
                    {
                        // strHexin = strHexin.Replace(strMultiMerchant, "");
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("MN", HicapsParams.MerchantId), strHexin);
                    }
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }
                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }
                    if (HicapsParams.SaleAmount > 0)
                    {
                        //34 30 00 05 31 32 35 30 30 1C
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.SaleAmount)), strHexin);
                    }
                    else
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("40", FieldEncoder.encodeDecimal(HicapsParams.SaleAmount)), strHexin);
                    }
                    if (HicapsParams.CashOutAmount > 0)
                    {
                        //34 30 00 05 31 32 35 30 30 1C
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("42", FieldEncoder.encodeDecimal(HicapsParams.CashOutAmount)), strHexin);
                    }
                    else
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("42", FieldEncoder.encodeDecimal(HicapsParams.CashOutAmount)), strHexin);
                    }
                    if (!string.IsNullOrEmpty(HicapsParams.PrimaryAccountNumber))
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlphaEncrypt("30", HicapsParams.PrimaryAccountNumber, HicapsParams.KVC), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("31", HicapsParams.USExpiryDate), strHexin);
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C1", HicapsParams.CCV), strHexin);
                        if (HicapsParams.CCVReason > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C2", HicapsParams.CCVReason.ToString()), strHexin);
                        }
                        if (HicapsParams.CCVSource > 0)
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("C3", HicapsParams.CCVSource.ToString()), strHexin);
                        }
                    }
                    // New 2010 Changes... Add PMSKey to Sale Transactions
                    if (!HicapsParams.Disable54AFields)
                    {
                        if (!string.IsNullOrEmpty(HicapsParams.PmsKey))
                        {
                            strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("PK", HicapsParams.PmsKey), strHexin);
                        }
                        strHexin = FieldEncoder.encodePrintOptions(HicapsParams, strHexin);
                    }
                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;



                case "KC":
                    strHexin = "02 00 18 36 30 30 30 30 30 30 30 30 30 31 30 4B 43 30 30 30 1C 03 2D";
                    // Generace Key Value;
                    string keyValue;
                    keyValue = FieldEncoder.getKVCCheck(HicapsParams.KVC);

                    strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("KV", keyValue), strHexin);
                    strHexout = FieldEncoder.ConvertToHex(strHexin);

                    break;
                case "MC":      // Medicare Claim

                    strHexin = "02 00 80 36 30 30 30 30 30 30 30 30 30 31 30 4d 43 30 30 30 1C 4D 58 00 02 46 30 1C 03 21";

                    if (!HicapsParams.EncryptionFlag)
                    {
                        strHexin = strHexin.Replace("4D 58 00 02 46 30 1C", "4D 58 00 02 44 30 1C");
                    }
                    if (HicapsParams.ComputerName != null && HicapsParams.ComputerName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CN", HicapsParams.ComputerName), strHexin);
                    }

                    if (HicapsParams.VendorName != null && HicapsParams.VendorName.Trim().Length > 0)
                    {
                        strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("VN", HicapsParams.VendorName), strHexin);
                    }

                    switch (HicapsParams.ClaimType)
                    {
                        case 1: strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CT", "1"), strHexin); break;
                        case 2: strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CT", "2"), strHexin); break;
                        case 3: strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CT", "3"), strHexin); break;
                        case 4: strHexin = FieldEncoder.addSubHexStringField(FieldEncoder.encodeAlpha("CT", "4"), strHexin); break;
                        default: break;
                    }
                  
                    // Encode m0 field
                    string strM0Field = FieldEncoder.encodeM0Field(HicapsParams);
                    if (HicapsParams.EncryptionFlag)
                    {
                        strM0Field = FieldEncoder.encodeFieldEncrypt(strM0Field, HicapsParams.KVC);
                    }
                    // ENcode m1 field;
                  
                    string strM1Field = FieldEncoder.encodeM1Field(HicapsParams);
                    if (HicapsParams.EncryptionFlag)
                    {
                        strM1Field = FieldEncoder.encodeFieldEncrypt(strM1Field, HicapsParams.KVC);
                    }
                    strHexin = FieldEncoder.addSubHexStringField(strM0Field, strHexin);
                    strHexin = FieldEncoder.addSubHexStringField(strM1Field, strHexin);


                    strHexout = FieldEncoder.ConvertToHex(strHexin);
                    break;
            }       // end switch strTransCode

            string strLRCvalue = FieldEncoder.GetChecksum(strHexout.Substring(1, strHexout.Length - 2));         // calc LRC of string less _STX and LRC
            strRequestMsg = strHexout.Substring(0, strHexout.Length - 1) + strLRCvalue;
            // bRequestMsg = enc.GetBytes(strRequestMsg);
            // JP fixed.
            bRequestMsg = Encoding.Default.GetBytes(strRequestMsg);
            byte[] bMsglen = FieldEncoder.IntToBCD(bRequestMsg.Length - 5, 2);
            bRequestMsg[1] = bMsglen[0];              // plug in the bcd message length
            bRequestMsg[2] = bMsglen[1];
            byte bLrc = FieldEncoder.GetChecksum(bRequestMsg);     // plug in the LRC byte

            return strRequestMsg;
        }

#region Helper Functions

#endregion
    }
}
