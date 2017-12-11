using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HICAPSConnectLibrary.Encrypt;

namespace HICAPSConnectLibrary.Utils
{
    public static class FieldEncoder
    {
        #region Checksum Methods
        /// <summary>
        /// Get the checksum value for the byte array
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte GetChecksum(byte[] b)
        {
            uint checksum = 0;
            b[0] = 0x00;                    // clear the _STX byte
            b[b.Length - 1] = 0x00;         // clear the LRC byte
            for (int i = 0; i < b.Length; i++)
            {
                checksum ^= b[i];
            }
          
            b[0] = 0x02;                                    // restore the _STX byte
            b[b.Length - 1] = Convert.ToByte(checksum);     // put in the LRC byte calculated
            return Convert.ToByte(checksum);
        }

        /// <summary>
        /// Get the checksum value for the byte array
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string GetChecksum(string s)
        {
            byte[] bSum;
            bSum = Encoding.Default.GetBytes(s);
            int checksum = 0;
            foreach (byte b in bSum)
            {
                checksum ^= b;
            }
            return Char.ConvertFromUtf32(checksum);
        }
        #endregion
        #region Byte level Functions
        /// <summary>
        /// Convert a integer into a BCD (Binary Coded Decimal format)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] IntToBCD(int val, int size)
        {

           // string strResult = "";
            string myStr = val.ToString("0000");
            char[] szArr = myStr.ToCharArray();
            string strVal1 = (((int)(szArr[0] - '0') * 10) + ((int)(szArr[1] - '0'))).ToString("00");
            string strVal2 = (((int)(szArr[2] - '0') * 10) + ((int)(szArr[3] - '0'))).ToString("00");

            byte[] barr1 = GetBytes(strVal1 + strVal2);

            return barr1;

        }

       

      
        public static byte[] GetBytes(string hexString)
        {
            int discarded = 0;
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
                else
                    discarded++;
            }
            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                discarded++;
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }
  #endregion

        #region General Encoding Functions
        /// <summary>
        /// Encodes a given ascii string using default separator
        /// </summary>
        /// <param name="myType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string encodeAlpha(string myType, string value)
        {
            return encodeAlpha(myType, value, true);
        }

        /// <summary>
        /// Encodes a given ascii string
        /// </summary>
        /// <param name="myType"></param>
        /// <param name="value"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string encodeAlpha(string myType, string value, bool separator)
        {
            string len;
            string result;
            // Bizare reason but for C1 Field.  If blank/null send this particular string back instead
            // of skipping the field.
            if (myType == "C1" && string.IsNullOrEmpty(value))
            {
                result = "43 31 00 00";
                if (separator) result = result + " 1C";
                return result;
            }

            if (!string.IsNullOrEmpty(value))
            {
                result = BitConverter.ToString(Encoding.Default.GetBytes(value)).Replace("-", " ");
                myType = BitConverter.ToString(Encoding.Default.GetBytes(myType)).Replace("-", " ");
                len = Convert.ToInt32(value.Length).ToString("D4");
                result = myType + " " + len.Substring(0, 2) + " " + len.Substring(2, 2) + " " + result;
                if (separator) result = result + " 1C";
                return result;
            }
            else return "";
        }
        /// <summary>
        /// Encode a decimal value to Hex
        /// </summary>
        /// <param name="decimalValue"></param>
        /// <returns></returns>
        public static string encodeDecimal(decimal? decimalValue)
        {
            string encodeAmount;
            if (decimalValue == null) { decimalValue = 0; }
            encodeAmount = "00000000" + (decimalValue * System.Convert.ToDecimal(100)).ToString();
            if (encodeAmount.IndexOf('.') > 0) { encodeAmount = Left(encodeAmount, encodeAmount.IndexOf('.')); }
            encodeAmount = Right(encodeAmount, 6);
            encodeAmount = encodeAmount.Replace(".", "");
            return encodeAmount;
        }

        #endregion
        #region Specific Medicare Encoding Functions
        /// <summary>
        /// Encode an alpha string using Medicare encoding logic
        /// </summary>
        /// <param name="myType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string encodeMedicareAlpha(string myType, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string result = BitConverter.ToString(Encoding.Default.GetBytes(value)).Replace("-", " ");

                string len = Convert.ToInt32(value.Length).ToString("X2");
                result = myType + " " + len + " " + result;

                return result;
            }
            else return "";
        }
        /// <summary>
        /// Takes a value, and coverts it to Spacey Hex. i.e 1E 3F 4D
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string encodeHexString(decimal amount)
        {
            string stringValue = "";
            try
            {

                stringValue = (amount * System.Convert.ToDecimal(100)).ToString();

                byte[] barr1 = Encoding.ASCII.GetBytes(stringValue);
                string str = BitConverter.ToString(barr1);
                str = str.Replace("-", " ");
                return str;
            }
            catch (Exception )
            {
                string strLen = stringValue.Length.ToString("{0:X2}");

                throw;
            }

        }
        public static string encodeM0Field(TerminalParameters HicapsParams)
        {
            string m0Field = "";
            int sizm0Field = 0;
            //DF 03 09 30 30 30 30 30 30 30 30 30
            if (!string.IsNullOrEmpty(HicapsParams.AccountReferenceId))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("03", HicapsParams.AccountReferenceId) + " ";
                sizm0Field += HicapsParams.AccountReferenceId.Length;
            }
            //DF 04 01 4E
            if (HicapsParams.ClaimType != 4)
            {
                if (HicapsParams.AccountPaidInd == "N")
                {
                    m0Field = m0Field + "DF 04 01 4E "; sizm0Field += 1;
                }
                else
                {
                    m0Field = m0Field + "DF 04 01 59 "; sizm0Field += 1;
                }
            }
            // Do not send down claimant details for Bulk Bill
            if (HicapsParams.ClaimType != 4)
            {
                //DF 05 10 12 34 56 78 90 12 34 56 78 90 ClaimantMedicareCardNum
                if (!string.IsNullOrEmpty(HicapsParams.ClaimantMedicareCardNum))
                {
                    string field = HicapsParams.ClaimantMedicareCardNum;
                    m0Field = m0Field + "DF 05 05 " + field.Substring(0, 2) + " "
                                                + field.Substring(2, 2) + " "
                                                + field.Substring(4, 2) + " "
                                                + field.Substring(6, 2) + " "
                                                + field.Substring(8, 2) + " ";
                    sizm0Field += 5;
                }
                // DF 06 01 31 ClaimantIRN
                if (!string.IsNullOrEmpty(HicapsParams.ClaimantIRN))
                {
                    m0Field = m0Field + "DF " + encodeMedicareAlpha("06", HicapsParams.ClaimantIRN) + " ";
                    sizm0Field += HicapsParams.ClaimantIRN.Length;
                }
            }
            //DF 07 10 12 34 56 78 90 12 34 56 78 90 PatientMedicareCardNum
            if (!string.IsNullOrEmpty(HicapsParams.PatientMedicareCardNum))
            {
                string field = HicapsParams.PatientMedicareCardNum;
                m0Field = m0Field + "DF 07 05 " + field.Substring(0, 2) + " "
                                            + field.Substring(2, 2) + " "
                                            + field.Substring(4, 2) + " "
                                            + field.Substring(6, 2) + " "
                                            + field.Substring(8, 2) + " ";
                sizm0Field += 5;
            }
            // DF 08 01 31 PatientIRN
            if (!string.IsNullOrEmpty(HicapsParams.PatientIRN))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("08", HicapsParams.PatientIRN) + " ";
                sizm0Field += HicapsParams.PatientIRN.Length;
            }
            // DF 41 CEV Request indicator
            if (!string.IsNullOrEmpty(HicapsParams.CevRequestInd))
            {
                HicapsParams.CevRequestInd = HicapsParams.CevRequestInd.ToUpper();
                m0Field = m0Field + "DF " + encodeMedicareAlpha("41", HicapsParams.CevRequestInd) + " ";
                sizm0Field += HicapsParams.CevRequestInd.Length;
            }

            // DF 42 08 30 31 32 33 34 35 36 37  PayeeProviderNum
            //
            if (!string.IsNullOrEmpty(HicapsParams.PayeeProviderNum))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("42", HicapsParams.PayeeProviderNum) + " ";
                sizm0Field += HicapsParams.PayeeProviderNum.Length;
            }
            // DF 0A 08 30 31 32 33 34 35 36 37 ServicingProviderNum
            //
            if (!string.IsNullOrEmpty(HicapsParams.ServicingProviderNum))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("0A", HicapsParams.ServicingProviderNum) + " ";
                sizm0Field += HicapsParams.ServicingProviderNum.Length;
            }
            // This is correct
            sizm0Field = m0Field.Replace(" ", "").Length / 2;
            // DF 0C  ?? PseudoProviderNum
            #region Second Level Data Elementa
            //Second Level Data Elements
            string secondLevelm0Field = "";

            //DF 0D 04 01 12 20 09
            if (HicapsParams.ReferralIssueDate != null && HicapsParams.ReferralIssueDate > new DateTime(2000, 1, 1))
            {
                string dateTimeField = HicapsParams.ReferralIssueDate.Day.ToString("D2") + " " + HicapsParams.ReferralIssueDate.Month.ToString("D2") + " " + HicapsParams.ReferralIssueDate.Year.ToString().Substring(0, 2) + " " + HicapsParams.ReferralIssueDate.Year.ToString().Substring(2, 2);
                secondLevelm0Field = secondLevelm0Field + "DF 0D 04 " + dateTimeField + " ";
            }
            // DF 0E 01 53
            if (!string.IsNullOrEmpty(HicapsParams.ReferralPeriodTypeCde))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("0E", HicapsParams.ReferralPeriodTypeCde.Substring(0, 1)) + " ";
            }
            // DF 0F 01 53
            if (!string.IsNullOrEmpty(HicapsParams.ReferralOverrideTypeCde))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("0F", HicapsParams.ReferralOverrideTypeCde.Substring(0, 1)) + " ";
            }
            // DF 10 08 32 31 30 32 31 32 32 31
            if (!string.IsNullOrEmpty(HicapsParams.ReferringProviderNum))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("10", HicapsParams.ReferringProviderNum) + " ";
            }
            #region Requesting part
            //DF 11 04 01 12 20 09
            if (HicapsParams.RequestIssueDate != null && HicapsParams.RequestIssueDate > new DateTime(2000, 1, 1))
            {
                string dateTimeField = HicapsParams.RequestIssueDate.Day.ToString("D2") + " " + HicapsParams.RequestIssueDate.Month.ToString("D2") + " " + HicapsParams.RequestIssueDate.Year.ToString().Substring(0, 2) + " " + HicapsParams.RequestIssueDate.Year.ToString().Substring(2, 2);
                secondLevelm0Field = secondLevelm0Field + "DF 11 04 " + dateTimeField + " ";
            }
            // DF 12 01 53
            if (!string.IsNullOrEmpty(HicapsParams.RequestOverrideTypeCde) && string.IsNullOrEmpty(HicapsParams.RequestingProviderNum))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("12", HicapsParams.RequestOverrideTypeCde.Substring(0, 1)) + " ";
            }
            // DF 13 01 53
            if (!string.IsNullOrEmpty(HicapsParams.RequestTypeCde) && !string.IsNullOrEmpty(HicapsParams.RequestingProviderNum))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("13", HicapsParams.RequestTypeCde.Substring(0, 1)) + " ";
            }
            // DF 14 08 32 31 30 32 31 32 32 31
            if (!string.IsNullOrEmpty(HicapsParams.RequestingProviderNum))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("14", HicapsParams.RequestingProviderNum) + " ";
            }
            #endregion
            // DF 15 01 4F ServiceType Cde
            // ServiceTypeCde not valid for Bulk Bill
            if (HicapsParams.ClaimType != 4)
            {
                if (!string.IsNullOrEmpty(HicapsParams.ServiceTypeCde))
                {
                    //  secondLevelm0Field = secondLevelm0Field + "DF 15 01 " + HicapsParams.ServiceTypeCde.Substring(0, 1) + " ";
                    secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("15", HicapsParams.ServiceTypeCde.Substring(0, 1)) + " ";
                }
            }
            // Add DF 0B to start of secondLevel Data Elemetnts
            string secondFieldSize = (secondLevelm0Field.Replace(" ", "").Length / 2).ToString("X2");
            if (secondFieldSize != "00")
            {
                if (HicapsParams.ClaimType != 4)
                {
                    // Patient Claim using DF 0B
                    m0Field = m0Field + "DF 0B " + secondFieldSize + " " + secondLevelm0Field;
                }
                else
                {
                    // Bulk Bill using DF 43
                    m0Field = m0Field + "DF 43 " + secondFieldSize + " " + secondLevelm0Field;
                }
            }
            #endregion
            // Add DF02/DF40 to start.
            // Get length of fields.
            if (HicapsParams.ClaimType != 4)
            {
                m0Field = m0Field.Trim();
                m0Field = "DF 02 " + sizm0Field.ToString("X2") + " " + m0Field;
            }
            else
            {
                // Bulk Bull using DF 40 field
                m0Field = m0Field.Trim();
                m0Field = "DF 40 " + sizm0Field.ToString("X2") + " " + m0Field;

            }
            //encryptSubField(HicapsParams, ref m0Field);
            int fieldlen = m0Field.Replace(" ", "").Length / 2;
            string fieldsize = fieldlen.ToString("D4");

            m0Field = "4D 30 " + fieldsize.Substring(0, 2) + " " + fieldsize.Substring(2, 2) + " " + m0Field + " 1C";
            return m0Field;
            // throw new NotImplementedException();
        }
        public static string encodeM1Field(TerminalParameters HicapsParams)
        {
            string m1Field = "";
            string m1SubField = "";
            //TODO extract details out of             HicapsParams.ClaimData 

          //  int linecount = 0;
            int m1FieldLen = 0;

            // Each Sub Claim Line
            for (int i = 0; i < HicapsParams.ClaimData.Length; i++)
            {
                //        6          6          8                  2            6           5            1                       6
                //data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr;
                //00002301000021072009        000000
                //00002301000021072009              000000
                string data = HicapsParams.ClaimData[i];
                string itemNumber = data.Substring(0, 6).Trim(); //= "12345";
                string chargeAmount = data.Substring(6, 6).Trim(); ;//= "0000060";
                string dateofservice = data.Substring(12, 8).Trim(); ;//= "01072009";
                string itemOverrideCode = data.Substring(20, 2).Trim(); //= "AP"; //"AP"
                string lspn = data.Substring(22, 6).Trim();//= "123456"; //123456
                string equipmentId = data.Substring(28, 5).Trim();//= "12345"; //12345
                string selfDeemedCde = data.Substring(33, 2).Trim();//= "Y";  //Y
                string contribPatientAmount = data.Substring(35, 6).Trim();//= "000000"; // "0000000";
                string spcid = data.Substring(41, 4).Trim(); // = "    "

                m1FieldLen = 0;
                m1Field = "";
                // D1 = Charge Amount
                if (!string.IsNullOrEmpty(chargeAmount) && chargeAmount != "000000")
                {
                    // can only be size 7
                    int amount = Convert.ToInt32(chargeAmount);
                    string hexField = Right("000000" + amount.ToString("X"), 6);
                    m1Field = m1Field + "D1 03 " + hexField.Substring(0, 2) + " " + hexField.Substring(2, 2) + " " + hexField.Substring(4, 2) + " ";
                    m1FieldLen += 3;
                }
                // D9 = Date of Service
                if (!string.IsNullOrEmpty(dateofservice))
                {
                    // can only be size 8
                    m1Field = m1Field + "D9 04 " + dateofservice.Substring(0, 2) + " " + dateofservice.Substring(2, 2) + " " + dateofservice.Substring(4, 2) + " " + dateofservice.Substring(6, 2) + " ";
                    m1FieldLen += 4;
                }
                // D2 = ItemNumber AlphsNumeric
                if (!string.IsNullOrEmpty(itemNumber))
                {
                    // Can only be size 6
                    m1Field = m1Field + encodeMedicareAlpha("D2", itemNumber) + " ";
                    m1FieldLen += itemNumber.Length;
                }
                // D3 = Item OVerride Code ?
                if (!string.IsNullOrEmpty(itemOverrideCode))
                {
                    // itemOverrideCode can only be 2.
                    m1Field = m1Field + encodeMedicareAlpha("D3", itemOverrideCode) + " ";
                    m1FieldLen += itemOverrideCode.Length;
                }
                // D4 = Patient Contributon amount
                if (!string.IsNullOrEmpty(contribPatientAmount) && contribPatientAmount != "000000")
                {
                    // Can only be most size 7, 9999999
                    int contribAmount = Convert.ToInt32(contribPatientAmount);
                    string hexField2 = Right("000000" + contribAmount.ToString("X"), 6);
                    m1Field = m1Field + "D4 03 " + hexField2.Substring(0, 2) + " " + hexField2.Substring(2, 2) + " " + hexField2.Substring(4, 2) + " ";
                    m1FieldLen += 3;
                }

                // D8 = LSPN
                if (!string.IsNullOrEmpty(lspn))
                {
                    // can only be len 6
                    lspn = Right("000000" + lspn, 6);
                    m1Field = m1Field + "D8 03 " + lspn.Substring(0, 2) + " " + lspn.Substring(2, 2) + " " + lspn.Substring(4, 2) + " ";
                    m1FieldLen += 3;
                }

                // DA selfDeemedCde Id
                if (!string.IsNullOrEmpty(selfDeemedCde))
                {
                    // can only be len 2
                    m1Field = m1Field + encodeMedicareAlpha("DA", selfDeemedCde) + " ";
                    m1FieldLen += selfDeemedCde.Length;
                }

                // DB Equipment Id
                if (!string.IsNullOrEmpty(equipmentId))
                {
                    // Can only be len 5
                    m1Field = m1Field + encodeMedicareAlpha("DB", equipmentId) + " ";
                    m1FieldLen += equipmentId.Length;
                }
                // DC spcid
                if (!string.IsNullOrEmpty(spcid))
                {
                    // Can only be len 4
                    m1Field = m1Field + encodeMedicareAlpha("DC", spcid) + " ";
                    m1FieldLen += equipmentId.Length;
                }
                m1Field = m1Field.Trim();
                // fix length
                m1FieldLen = m1Field.Replace(" ", "").Length / 2;
                if (HicapsParams.ClaimType != 4)
                {
                    m1Field = "DF 16 " + m1FieldLen.ToString("X2") + " " + m1Field;
                }
                else
                {
                    // Bulk Bill
                    m1Field = "DF 45 " + m1FieldLen.ToString("X2") + " " + m1Field;

                }
                m1SubField += m1Field + " ";

                // End of repeat line
            }
            m1Field = m1SubField.Trim();
            int fieldlen = m1Field.Replace(" ", "").Length / 2;
            string fieldsize = fieldlen.ToString("D4");
            m1Field = "4D 31 " + fieldsize.Substring(0, 2) + " " + fieldsize.Substring(2, 2) + " " + m1Field + " 1C";
            return m1Field;
        }
  
        #endregion
        #region Hex Functions


        /// <summary>
        /// Take a string like '00 1E 33 44' and convert it to a string rep
        /// </summary>
        /// <param name="asciiString"></param>
        /// <returns></returns>
        public static string ConvertToHex(string asciiString)
        {
            string[] hexValuesSplit = asciiString.Split(' ');
            string s = "";
            List<byte> myList;
            myList = new List<byte>();
            for (int i = 0; i <= hexValuesSplit.Count() - 1; i++)
            {
                s = hexValuesSplit[i];
                byte value = Convert.ToByte(s, 16);
                myList.Add(value);
            }
            return Encoding.Default.GetString(myList.ToArray());
        }

        /// <summary>
        /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        public static bool IsHexDigit(Char c)
        {
            int numChar;
            int numA = Convert.ToInt32('A');
            int num1 = Convert.ToInt32('0');
            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);
            if (numChar >= numA && numChar < (numA + 6))
                return true;
            if (numChar >= num1 && numChar < (num1 + 10))
                return true;
            return false;
        }

        /// <summary>
        /// Converts a value "1E" to the byte 1E or int 30.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns>byte value for string</returns>
        /// <exception cref="ArgumentException">Hex must be 1 or 2 characters in length</exception>
        private static byte HexToByte(string hex)
        {

            if (hex.Length > 2 || hex.Length <= 0)

                throw new ArgumentException("hex must be 1 or 2 characters in length");

            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);

            return newByte;

        }
        
        /// <summary>
        /// Added new Hex data to existing Hex message.  If ETX present, insert before.
        /// </summary>
        /// <param name="newHexfield"></param>
        /// <param name="hexStringData"></param>
        /// <returns></returns>
        public static string addSubHexStringField(string newHexfield, string hexStringData)
        {
            string tmpString;
            if (string.IsNullOrEmpty(newHexfield))
            {
                return hexStringData;
            }
            hexStringData = hexStringData.ToUpper();
            // Last 5 bytes = ETX + " " + Length  ..
            if (hexStringData.LastIndexOf("03") == (hexStringData.LastIndexOf("1C 03") + 3))
            {
                tmpString = hexStringData.Substring(0, hexStringData.LastIndexOf("03")) + newHexfield + hexStringData.Substring(hexStringData.LastIndexOf("03") - 1);
            }
            else
            {
                tmpString = hexStringData.Substring(0, hexStringData.LastIndexOf("03")) + "1C " + newHexfield + hexStringData.Substring(hexStringData.LastIndexOf("03") - 1);
            }
            return tmpString;
        }

        #endregion

        #region Encode Print Option fields
        public static string encodePrintOptions(TerminalParameters HicapsParams, string strHexin)
        {
            // Receipt Prompting parameters.
            // Documentation says MY Field been added Bit 0-1 Print Merchant Copy, Bit 1-1 Print Customer Copy
            // Big 2-1 Pronpt before printing customer copy.
            //
            // Jp note.... Sample PMS.exe code does not seem to use this logic... in the end I've just
            // added code for all possibilities copying what the PMS.exe sends to the terminal.
            // ?
            // Only valid for pmskey 6730429
            if (HicapsParams.PmsKey != "6730429")
                return strHexin;
            bool fieldAdded = false;
            if ((!HicapsParams.PrintCustomerReceipt) && (!HicapsParams.PrintMerchantReceipt))
            {
                strHexin = addSubHexStringField(encodeAlpha("MY", "00"), strHexin);
                fieldAdded = true;
            }
            if ((!HicapsParams.PrintCustomerReceipt) && (HicapsParams.PrintMerchantReceipt))
            {
                strHexin = addSubHexStringField(encodeAlpha("MY", "80"), strHexin);
                fieldAdded = true;
            }
            if (HicapsParams.PrintCustomerReceipt && !fieldAdded)
            {
                if (HicapsParams.PrintMerchantReceipt && HicapsParams.PrintCustomerReceiptPrompt)
                {
                    strHexin = addSubHexStringField(encodeAlpha("MY", "E0"), strHexin);
                    fieldAdded = true;
                }
                if (!HicapsParams.PrintMerchantReceipt && !HicapsParams.PrintCustomerReceiptPrompt)
                {
                    strHexin = addSubHexStringField(encodeAlpha("MY", "40"), strHexin);
                    fieldAdded = true;
                }
                if (!HicapsParams.PrintMerchantReceipt && HicapsParams.PrintCustomerReceiptPrompt)
                {
                    strHexin = addSubHexStringField(encodeAlpha("MY", "60"), strHexin);
                    fieldAdded = true;
                }
                if (HicapsParams.PrintMerchantReceipt && !HicapsParams.PrintCustomerReceiptPrompt)
                {
                    strHexin = addSubHexStringField(encodeAlpha("MY", "C0"), strHexin);
                    fieldAdded = true;
                }
                if (!fieldAdded)
                {
                    strHexin = addSubHexStringField(encodeAlpha("MY", "80"), strHexin);
                    fieldAdded = true;
                }
                // 
            }
            return strHexin;
        }
        #endregion


        #region Encryption Encoding Functions
        public static string encodeAlphaEncrypt(string myType, string value, byte[] KVC)
        {
            string unEncryptedField = encodeAlpha(myType, value, true);
            // If no Key phrase set return unencrypted field
            if (KVC == null)
            {
                return unEncryptedField;
            }
            // UnEncrypted Field will equal something like 43 4E 00 23 .. .. .. .. 1C"
            return encodeFieldEncrypt(unEncryptedField, KVC);
        }

        public static string encodeFieldEncrypt(string unEncryptedField, byte[] KVC)
        {
            if (KVC == null) { return unEncryptedField; }
            string dataField = unEncryptedField.Substring(12, unEncryptedField.Length - (3 + 12));
            byte[] dataArray = Encoding.Default.GetBytes(ConvertToHex(dataField));
            byte[] dataArrayOutput = new byte[dataArray.Length]; // OFP = same length as input array
            //HICAPS3Des.CallEncryptData(dataArray, dataArray.Length, KVC, dataArrayOutput);
            dataArrayOutput = TripleDesOFB.OFBCrypt(dataArray, KVC);
            return unEncryptedField.Substring(0, 12) + BitConverter.ToString(dataArrayOutput).Replace("-", " ") + " 1C";
        }
      
        public static string getKVCCheck(byte[] KVC)
        {
            string keyValue;
            byte[] zeroArray = { 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] dataOutput = new byte[8];
            dataOutput = TripleDesOFB.OFBCrypt(zeroArray, KVC);
            keyValue = BitConverter.ToString(dataOutput, 0, 3).Replace("-", "");
            return keyValue;
        }
        public static string getKVCCheckFull(byte[] KVC)
        {
            string keyValue;
            byte[] zeroArray = { 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] dataOutput = new byte[8];
            dataOutput = TripleDesOFB.OFBCrypt(zeroArray, KVC);
            keyValue = BitConverter.ToString(dataOutput).Replace("-", "");
            return keyValue;
        }
        public static  byte[] getKVC(string MerchantId, string TerminalId)
        {
            string firstField = Left(TerminalId + new String(' ', 8), 8);
            string rollingPassword;
            rollingPassword = getRollingPassword(MerchantId);
            firstField = firstField + MerchantId;

            string secondField = "\0\0" + rollingPassword + new String((char)0, 8);
            byte[] array1 = Encoding.Default.GetBytes(firstField);
            byte[] array2 = Encoding.Default.GetBytes(secondField);
            byte[] array3 = new byte[16];

            for (int i = 0; i < array1.Length; i++)
            {
                array3[i] = (byte)((int)array1[i] ^ (int)array2[i]);
                // Shift bit left by 1
                array3[i] = (byte)((int)array3[i] << 1);
            }

            return array3;
        }
        private static string getRollingPassword(string MerchantId)
        {
            if (string.IsNullOrEmpty(MerchantId))
            {
                MerchantId = "00000000";
            }
            string date = DateTime.Now.ToString("ddMMyy");
            int iDate = Int32.Parse(date);
            int iMerchant = Int32.Parse(MerchantId.Substring(1, 6));
            iDate = (iDate + iMerchant + 386745);
            iDate %= 1000000;
            return Right("000000" + iDate.ToString(), 6);
        }

        #endregion

        #region String methods

        public static string LeftPad(string field, char character, int length)
        {
            field = field.Trim();
            field = new String(character, length) + field;
            return Right(field, length);
        }
        public static string Right(string rightString, int length)
        {
            return rightString.Substring(rightString.Length - length);
        }
        public static string Left(string leftString, int length)
        {
            return leftString.Substring(0, length);
        }
        public static string FlipExpiryDate(string expiryDate)
        {
            string newExpiry = expiryDate;
            try
            {
                if (!string.IsNullOrEmpty(expiryDate))
                {
                    newExpiry = "0000" + expiryDate;
                    newExpiry = Right(newExpiry, 4);
                    newExpiry = Right(newExpiry, 2) + Left(newExpiry, 2);
                    if (newExpiry == "0000")
                    {
                        newExpiry = "";
                    }
                }
                else
                {
                    newExpiry = "";
                }
            }
            catch {}
            return expiryDate;
        }
        #endregion
    }
}
