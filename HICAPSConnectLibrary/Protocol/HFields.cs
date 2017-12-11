using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HICAPSConnectLibrary.Protocol.Fields;

namespace HICAPSConnectLibrary.Protocol
{
    public class HFields
    {
        //TODO Add Documentation, and set fields to be correct values
        public static HField AuthResponseCode = new HField() { Key = "00", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 2 };
        public static HField ApprovalCode = new HField() { Key = "01", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };
        public static HField ResponseText = new HField() { Key = "02", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 40 };
        public static HField TransactionDate = new HField() { Key = "03", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };
        public static HField TransactionTime = new HField() { Key = "04", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };
        public static HField TerminalID = new HField() { Key = "14", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 16 };
        public static HField PrimaryAccountNumber = new HField() { Key = "30", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 19 };
        public static HField ExpiryDate = new HField() { Key = "31", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 4 };
        public static HField TransactionAmount = new HField() { Key = "40", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 12 };
        public static HField CashAmount = new HField() { Key = "42", HType = HFieldType.BCD, MinLength = 6, MaxLength = 12 };
        public static HField BenefitAmount = new HField() { Key = "43", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 12 };
        public static HField SurchargeAmount = new HField() { Key = "44", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 12 };
        public static HField InvoiceNumber = new HField() { Key = "65", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };
        public static HField RetrievalReferenceNumber = new HField() { Key = "79", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 12 };
        public static  HField CCVField = new HField() { Key = "C1", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };
        public static HField CCVReason = new HField() { Key = "C2", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 1 };
        public static HField CCVSource = new HField() { Key = "C3", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 1 };
        public static HField ComputerName = new HField() { Key = "CN", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 40 };
        public static HField AcquirerID = new HField() { Key = "DD", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 2 };
        public static HField HICAPSData = new HField() { Key = "EA", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = uint.MaxValue };
        public static HField IPAddress = new HField() { Key = "IA", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 15};
        public static HField IPPort = new HField() { Key = "IP", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 5 };
        public static HField KeyCheck = new HField() { Key = "KC", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };

        public static HField MedicareM0 = new HField() { Key = "M0", HType = HFieldType.Medicare, MinLength = 0, MaxLength = uint.MaxValue };
        public static HField MedicareM1 = new HField() { Key = "M1", HType = HFieldType.Medicare, MinLength = 0, MaxLength = uint.MaxValue };
        public static HField MedicareM2 = new HField() { Key = "M2", HType = HFieldType.Medicare, MinLength = 0, MaxLength = uint.MaxValue };
        public static HField MedicareM3 = new HField() { Key = "M3", HType = HFieldType.Medicare, MinLength = 0, MaxLength = uint.MaxValue };
        public static HField MultiMerchant = new HField() { Key = "MM", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 8};
        public static HField MerchantID = new HField("MN",HFieldType.AlphaNumeric,0,15);
        public static  HField ProviderName = new HField() { Key = "MP", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 16 };
        public static  HField RefundPassword = new HField() { Key = "MR", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 6 };
        public static  HField AdditionalInfo = new HField() { Key = "MX", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 2 };
        public static  HField NetworkName = new HField() { Key = "NN", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 32 };
        public static  HField PMSKey = new HField() { Key = "PK", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = uint.MaxValue };

        public static  HField StatusMessageText = new HField() { Key = "SM", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 40 };
        public static  HField TerminalSwipe = new HField() { Key = "TS", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 1 };
        public static  HField VendorName = new HField() { Key = "VN", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 40 };
        public static  HField TerminalVersion = new HField() { Key = "VR", HType = HFieldType.AlphaNumeric, MinLength = 0, MaxLength = 8 };
        //TODO Document/update this list
        public static  List<HField> Fields = new List<HField>() { AuthResponseCode, ApprovalCode, ResponseText, TransactionDate, TransactionTime, 
                                                                        TerminalID, PrimaryAccountNumber, ExpiryDate, TransactionAmount, CashAmount, BenefitAmount, 
                                                                        SurchargeAmount, InvoiceNumber, RetrievalReferenceNumber, CCVField, CCVReason, CCVSource, 
                                                                        ComputerName, AcquirerID, HICAPSData, IPAddress, IPPort, KeyCheck, 
                                                                        MedicareM0, MedicareM1, MedicareM2, MedicareM3, 
                                                                        MultiMerchant, MerchantID, ProviderName, RefundPassword, AdditionalInfo, 
                                                                        NetworkName, PMSKey, StatusMessageText, TerminalSwipe, VendorName, TerminalVersion };

    }
}
