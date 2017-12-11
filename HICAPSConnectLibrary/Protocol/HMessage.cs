using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using HICAPSConnectLibrary.Encrypt;

namespace HICAPSConnectLibrary.Protocol
{
    /// <summary>
    /// Class represents a object representation of the HCPOS Transaction Message
    /// </summary>
    [Serializable]
    public class HMessage
    {
       
        public string TransTypeCode = "";

        public byte FormatVersion = (byte)'1';
        public byte RequestResponseIndicator = (byte)'1';
        public string ResponseCode = "00";
        public byte MoreIndicator = (byte)'0';
        public bool disableEdit = false;
        public Dictionary<string, string> FieldValues = new Dictionary<string, string>();
        public byte[] Key = null;
        private List<string> FieldEncrypt = new List<string> {"EA","M0","M1","M2","M3","PN","30"};
        public bool CheckDigitVerified = false;
        /// <summary>
        /// Default constructor, creates an empty message.
        /// </summary>
        public HMessage()
        {
        }

       
        /// <summary>
        /// Constructor, default initalised using the spacey hex string parameter
        /// </summary>
        /// <param name="hexData">Spacey Hex string like 02 00 01 30 30 30 ......</param>
        public HMessage(string hexData, byte[] key = null)
        {
            // Turn on encryption
            Key = key;
            // HexData is like this "02 00 01 30 30 30 ......"
            byte[] byteHexData = ConvertToHex(hexData);
            LoadFromByteArray(byteHexData);
        }

        /// <summary>
        /// Constructor, default initalised using byte array.
        /// </summary>
        /// <param name="hexData">byte array containg raw protocol message</param>
        public HMessage(byte[] rawBytes, byte[] key = null)
        {
            // Turn on encryption
            Key = key;
            LoadFromByteArray(rawBytes);
        }

        /// <summary>
        /// Extracts the Presentation Data values
        /// </summary>
        /// <param name="bHicapsData">byte array containg raw protocol message</param>
        /// <param name="offsetStart">offset of byte array pointing to beginnning of presentation data</param>
        /// <param name="format">Value of format</param>
        /// <param name="rri">Value of rri</param>
        /// <param name="transactionCode">Value of Transaction Code</param>
        /// <param name="responseCode">Value of Response Code</param>
        /// <param name="moreIndicator">Value of More indicator</param>
        private void ExtractPresentationHeader(byte[] bHicapsData, ref int offsetStart, ref byte format, ref byte rri, ref string transactionCode, ref string responseCode, ref byte moreIndicator)
        {
            // Format
            format = bHicapsData[offsetStart];
            offsetStart++;

            // Request Response Induicator
            rri = bHicapsData[offsetStart];
            offsetStart++;

            // Transaction Code
            byte[] transactionByteCode = new byte[2];
            Array.Copy(bHicapsData, offsetStart, transactionByteCode, 0, 2);

            transactionCode = Encoding.ASCII.GetString(transactionByteCode);

            offsetStart += 2;

            // ResponseCode
            byte[] responseByteCode = new byte[2];
            Array.Copy(bHicapsData, offsetStart, responseByteCode, 0, 2);

            responseCode = Encoding.Default.GetString(responseByteCode);
            offsetStart += 2;
            // More indicator
            // Request Response Induicator
            moreIndicator = bHicapsData[offsetStart];
            offsetStart++;

            // One more for separator
            offsetStart++;
        }

        /// <summary>
        /// Extract the value as a SubField
        /// </summary>
        /// <param name="bHicapsData">Byte array of raw protocol message</param>
        /// <param name="offsetStart">offset of byte array pointing to beginnning of sub field data</param>
        /// <param name="fieldText">Value of Field Text</param>
        /// <param name="eaSubDataField">Value of EA Sub Field type</param>
        public void ExtractSubField(byte[] bHicapsData, ref int offsetStart, ref string fieldText, ref string eaSubDataField, string transTypeCode = "")
        {
            try
            {
                int size;
                byte[] eaSubDataFieldType = new byte[2];
                byte[] bcdSize = new byte[2];

                // Sub Field Type
                Array.Copy(bHicapsData, offsetStart, eaSubDataFieldType, 0, eaSubDataFieldType.Length);
                eaSubDataField = Encoding.Default.GetString(bHicapsData, offsetStart, 2);
                offsetStart += 2;

                // Size
                Array.Copy(bHicapsData, offsetStart, bcdSize, 0, bcdSize.Length);
                size = DoubleBcdToInt(bcdSize);  // Should be 40
                offsetStart += 2;

                if (Key != null && Key.Length > 0 && FieldEncrypt.Contains(eaSubDataField))
                {
                    var field = new byte[size];
                    Array.Copy(bHicapsData, offsetStart, field, 0, size);
                    byte[] dataArrayOutput = TripleDesOFB.OFBCrypt(field, Key);
                    if (eaSubDataField == "EA" && (HTransaction.ClaimTransactions.Contains(transTypeCode)))
                    {
                        for (int i = 8; i < dataArrayOutput.Length; i++)
                        {
                            dataArrayOutput[i] = field[i];
                        }
                    }
                    fieldText = Encoding.Default.GetString(dataArrayOutput);
                }
                else
                {
                    // Sub Field Value Text
                    fieldText = Encoding.Default.GetString(bHicapsData, offsetStart, size);
                }
                offsetStart += size;
                // Maybe we should check that at "Offset there is in fact a 1C character ?"
                offsetStart += 1; // 1C Separator;
            }
            catch (Exception)
            {
                offsetStart += 1;
                fieldText = "??";
                eaSubDataField = "??";
            }
            return;
        }

        /// <summary>
        /// Converts a BCD value to an INT.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private int DoubleBcdToInt(byte[] size)
        {
            //            int value;
            int value = System.Convert.ToInt32(size[1].ToString("X2"));
            int value100 = System.Convert.ToInt32(size[0].ToString("X2")) * 100;
            return value + value100;
        }

        /// <summary>
        /// Takes the HMessage object and convert it to the byte representation of this object using the HCPOS specification
        /// </summary>
        /// <returns> byte array of message represented by the current object state</returns>
        public byte[] SaveToByteArray()
        {
            // Start of Presentation Header Segment
            string strHexIn = "02 00 00 36 30 30 30 30 30 30 30 30 30 31";

            // Message Type.
            if ((RequestResponseIndicator == '2')) { strHexIn += " 32"; } //Status
            if ((RequestResponseIndicator == '1')) { strHexIn += " 31"; } //Response
            if ((RequestResponseIndicator == '0')) { strHexIn += " 30"; } // Request

            // Transaction Code.
            byte[] byteTransCode = Encoding.ASCII.GetBytes(TransTypeCode);
            strHexIn += " " + BitConverter.ToString(byteTransCode).Replace("-", " ");

            // Response Code.
            byte[] byteRespCode = Encoding.ASCII.GetBytes(ResponseCode);
            strHexIn += " " + BitConverter.ToString(byteRespCode).Replace("-", " ");

            // More Indicator
            if ((MoreIndicator == '1')) { strHexIn += " 31"; } // More Messages.  Part of a stream
            if ((MoreIndicator == '0')) { strHexIn += " 30"; } // Single Message.

            // Final Separator + EOT + Checksum
            strHexIn += " 1C 03 00";

            // End of Presentation Header Segment
            // Field Value Message Guts.
            foreach (var myRow in FieldValues)
            {
                if (Key != null && Key.Length > 0 && (FieldEncrypt.Contains(myRow.Key) && !HTransaction.NoEncryptionTransactions.Contains(TransTypeCode)))
                {
                    strHexIn = addSubHexStringField(encodeAlpha(myRow.Key, myRow.Value), strHexIn, Key, (myRow.Key == "EA"));
                }
                else
                {
                    strHexIn = addSubHexStringField(encodeAlpha(myRow.Key, myRow.Value), strHexIn);
                }
            }

            // Update message length and checksum fields and then return
            return UpdateCRCByteLen(strHexIn);
        }

        #region MessageBuilder Functions

        /// <summary>
        ///  Code to add a Subfield to the string hex list data
        /// </summary>
        /// <param name="newHexfield"></param>
        /// <param name="hexStringData"></param>
        /// <param name="KVC"></param>
        /// <returns></returns>
        private static string addSubHexStringField(string newHexfield, string hexStringData, byte[] KVC = null,bool firstEightBytes = false)
        {
            string tmpString;
            if (string.IsNullOrEmpty(newHexfield))
            {
                return hexStringData;
            }
            hexStringData = hexStringData.ToUpper();
            if (KVC != null && KVC.Length > 0)
            {
                newHexfield = encodeFieldEncrypt(newHexfield, KVC, firstEightBytes);
            }
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

        /// <summary>
        /// Encodes a value into spacey Hex.
        /// </summary>
        /// <param name="myType">HCPOS field type</param>
        /// <param name="value">Value to encode</param>
        /// <param name="separator" default="true">Add the 1C field separator, default is true</param>
        /// <returns>Spacey Hex string of encoded value</returns>
        public static string encodeAlpha(string myType, string value, bool separator = true)
        {
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
                string len = Convert.ToInt32(value.Length).ToString("D4");
                result = myType + " " + len.Substring(0, 2) + " " + len.Substring(2, 2) + " " + result;
                if (separator) result = result + " 1C";
                return result;
            }
            else return "";
        }
        /// <summary>
        /// Creates the Rolling Code parameter, used for Encryptiong routines
        /// </summary>
        /// <param name="MerchantId"></param>
        /// <returns></returns>
        public static string GenerateRollingCode(string MerchantId)
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
        /// <summary>
        /// Creates the KVC value used for Encryption routines
        /// </summary>
        /// <param name="terminalId"></param>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        public static byte[] GenerateKey(string terminalId, string merchantId)
        {
            if (terminalId == null){terminalId = "";}
            if (merchantId == null) { merchantId = ""; }

            string rollingCode = GenerateRollingCode(merchantId);
            string hash1 = terminalId.PadRight(8, ' ') + merchantId.PadLeft(8, ' ');
            string hash2 = rollingCode.PadLeft(8, '\0') + "".PadLeft(8, '\0');
            byte[] array1 = Encoding.Default.GetBytes(hash1);
            byte[] array2 = Encoding.Default.GetBytes(hash2);
            byte[] key = new byte[16];

            for (int i = 0; i < array1.Length; i++)
            {
                key[i] = (byte)((int)array1[i] ^ (int)array2[i]);
                // Shift bit left by 1
                key[i] = (byte)((int)key[i] << 1);
            }
            return key;
        }

        /// <summary>
        /// Creates the KVC Check value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] GenerateRawKVC(byte[] rawKey)
        {
            return TripleDesOFB.OFBKeyCheck(rawKey);
        }

        /// <summary>
        /// Creates the KVC Check value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GenerateKVC(byte[] key)
        {
            byte[] dataOutput = GenerateRawKVC(key);
            return BitConverter.ToString(dataOutput, 0, 3).Replace("-", "");  
        }

        /// <summary>
        /// Encode a string and type, encrypting it using the KVC value
        /// </summary>
        /// <param name="myType"></param>
        /// <param name="value"></param>
        /// <param name="KVC"></param>
        /// <returns>Spacey Hex string of encoded value</returns>
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

        ///// <summary>
        ///// Encode a string and type, encrypting it using the KVC value
        ///// </summary>
        ///// <param name="unEncryptedField">String variable</param>
        ///// <param name="KVC">KVC value</param>
        ///// <returns>Bytearray of encoded bytes</returns>
        //internal static byte[] EncryptField(string eaData, byte[] KVC)
        //{
        //    return TripleDesOFB.OFBCrypt(Encoding.Default.GetBytes(eaData), KVC);
        //}

        /// <summary>
        /// Encode a string and type, encrypting it using the KVC value
        /// </summary>
        /// <param name="unEncryptedField">String variable</param>
        /// <param name="KVC">KVC value</param>
        /// <returns>Spacey Hex string of encoded value</returns>
        public static string encodeFieldEncrypt(string unEncryptedField, byte[] KVC, bool firstEight = false)
        {
            if (KVC == null) { return unEncryptedField; }
            string dataField = unEncryptedField.Substring(12, unEncryptedField.Length - (3 + 12));
            byte[] dataArray = Encoding.Default.GetBytes(ConvertToHex2(dataField));
            byte[] dataArrayOutput = TripleDesOFB.OFBCrypt(dataArray, KVC);
            if (firstEight)
            {
                for(int i=8;i<dataArrayOutput.Length;i++)
                {
                    dataArrayOutput[i] = dataArray[i];
                }
            }
            return unEncryptedField.Substring(0, 12) + BitConverter.ToString(dataArrayOutput).Replace("-", " ") + " 1C";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="KVC"></param>
        /// <returns></returns>
        public static string decodeAlphaEncrypt(string fieldValue, byte[] KVC)
        {
            if (string.IsNullOrEmpty(fieldValue)) { return fieldValue; }
            if (KVC == null) { return fieldValue; }

            byte[] dataArray = Encoding.Default.GetBytes(fieldValue);
            byte[] dataArrayOutput = TripleDesOFB.OFBCrypt(dataArray, KVC);
            return Encoding.Default.GetString(dataArrayOutput);
        }
        /// <summary>
        /// Update the length and checksum bytes for the Protocol message strHexin
        /// </summary>
        /// <param name="strHexIn"></param>
        /// <returns></returns>
        private byte[] UpdateCRCByteLen(string strHexIn)
        {
            byte[] byteHexout = ConvertToHex(strHexIn);
            UpdateByteLen(ref byteHexout);
            UpdateChecksum(ref byteHexout);
            return byteHexout;
        }

        /// <summary>
        /// Update the length bytes for the Protocol Message byteHexout
        /// </summary>
        /// <param name="byteHexout"></param>
        public void UpdateByteLen(ref byte[] byteHexout)
        {
            byte[] bMsglen = IntToBCD(byteHexout.Length - 5);
            byteHexout[1] = bMsglen[0];
            byteHexout[2] = bMsglen[1];

            return;
        }

        /// <summary>
        /// Determine and update the checksum value of the Protocol Message byteHexout
        /// </summary>
        /// <param name="hexMessage"></param>
        private void UpdateChecksum(ref byte[] byteHexout)
        {
            byte[] bSum = new byte[byteHexout.Length - 2];
            Array.Copy(byteHexout, 1, bSum, 0, byteHexout.Length - 2);
            int checksum = 0;
            foreach (byte b in bSum)
            {
                checksum ^= b;
            }
            byteHexout[byteHexout.Length - 1] = (byte)checksum;
            
        }
        /// <summary>
        /// Verify the checksum value of the Protocol Message byteHexout
        /// </summary>
        /// <param name="hexMessage"></param>
        private bool VerifyChecksum(byte[] byteHexout)
        {
            int checksum = 0;
            for(int i=1;i<byteHexout.Length - 1; i++)
            {
                checksum ^= byteHexout[i];
            }
            return (byteHexout[byteHexout.Length-1] == (byte)checksum);
        }
        /// <summary>
        /// Convert integer into a BCD byte array value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte[] IntToBCD(int value)
        {
            string len = Convert.ToInt32(value).ToString("D4");
            return ConvertToHex(len.Substring(0, 2) + " " + len.Substring(2, 2));
        }

        public static byte[] ShortIntToBCD(int value)
        {
            string len = value.ToString("D2");
            return ConvertToHex(len);
        }

        /// <summary>
        /// Static method to create a default claim response from a given claim request string
        /// </summary>
        /// <param name="claimRequest">string containing claim message</param>
        /// <param name="benefitPercent">percentage of benefit to pay</param>
        /// <param name="MembershipId">Membership ID</param>
        /// <returns></returns>
        public static string encodeClaimResponseText(string claimRequest, decimal benefitPercent, string MembershipId)
        {
            string returnMessage = "";
            decimal Percent100 = 100.00M;

            returnMessage = claimRequest.Substring(0, 8); // Provider Number id
            returnMessage += Right("00000000" + MembershipId, 8); //Membership Id
            string lineByLine = claimRequest.Substring(8);
            int lineCount = lineByLine.Length / 18;
            bool loyaltyFlag = false;
            bool topupFlag = false;
            for (int z = 0; z < lineCount; z++)
            {
                string itemNumber = lineByLine.Substring((z * 18) + 2, 4).Trim();
                if (itemNumber.EndsWith("719")) { loyaltyFlag = true; }
                if (itemNumber.EndsWith("718")) { topupFlag = true; }

                string feeCharged = lineByLine.Substring((z * 18) + 12, 6);

                decimal benefitValue = ((Convert.ToDecimal(feeCharged) * benefitPercent) / Percent100);
                returnMessage += lineByLine.Substring(z * 18, 18) + encodeDecimal(benefitValue) + "00";
            }
            if (loyaltyFlag)
            {
                returnMessage += lineByLine.Substring((lineCount - 1) * 18, 2) + "X999" +
                    lineByLine.Substring((lineCount - 1) * 18 + 6, 2) +
                    lineByLine.Substring((lineCount - 1) * 18 + 8, 4) +
                    encodeDecimal(Convert.ToDecimal(0)) +
                    encodeDecimal(Convert.ToDecimal(0)) + "00";
            }
            if (topupFlag)
            {
                returnMessage += lineByLine.Substring((lineCount - 1) * 18, 2) + "X980" +
                    lineByLine.Substring((lineCount - 1) * 18 + 6, 2) +
                    lineByLine.Substring((lineCount - 1) * 18 + 8, 4) +
                    encodeDecimal(Convert.ToDecimal(0)) +
                    encodeDecimal(Convert.ToDecimal(0)) + "00";
            }
            return returnMessage;
        }
        internal static string encodeClaimPatientData(string claimRequest, Dictionary<int, string> dictionary)
        {
            string lineByLine = claimRequest.Substring(8);
            int lineCount = lineByLine.Length / 18;
            var patients = new List<int>();
            for (int z = 0; z < lineCount; z++)
            {
                int patientId = lineByLine.Substring((z * 18), 2).Trim().ToInt();
                if (!patients.Contains(patientId))
                {
                    patients.Add(patientId);
                }
            }
            string returnValue = "";
            var retValue = new List<byte>();
            foreach (var patient in patients)
            {
                if (dictionary.ContainsKey(patient))
                {
                    retValue.AddRange(ShortIntToBCD(patient));
                 var patientName = dictionary[patient].Trim();
                 retValue.AddRange(ShortIntToBCD(patientName.Length));
                 retValue.AddRange(Encoding.Default.GetBytes(patientName));
                }
            }

            returnValue = Encoding.Default.GetString(retValue.ToArray());
            return returnValue;
        }
        /// <summary>
        /// Encodes decimal value into HCPOS value String
        /// </summary>
        /// <param name="decimalValue"></param>
        /// <returns></returns>
        public static string encodeDecimal(decimal? decimalValue)
        {
            if (decimalValue == null) { decimalValue = 0; }
            string encodeAmount = "00000000" + (decimalValue * System.Convert.ToDecimal(100)).ToString();
            if (encodeAmount.IndexOf('.') > 0) { encodeAmount = HMessage.Left(encodeAmount, encodeAmount.IndexOf('.')); }
            encodeAmount = Right(encodeAmount, 6);
            encodeAmount = encodeAmount.Replace(".", "");
            return encodeAmount;
        }

        #region Medicare Methods

        /// <summary>
        /// Code to extract Medicare Sub Field
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="offset"></param>
        /// <param name="fieldSize"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public static byte[] ExtractMedicareSubField(byte[] dataArray, ref int offset, ref int fieldSize, ref string fieldId)
        {
            // Transaction Code
            byte[] transactionByteCode = new byte[2];
            Array.Copy(dataArray, offset, transactionByteCode, 0, 2);
            offset += 2;
            fieldSize = dataArray[offset];
            offset++;

            // Now extract field Data
            byte[] fieldData = new byte[fieldSize];
            Array.Copy(dataArray, offset, fieldData, 0, fieldSize);
            offset += fieldSize;

            // Set the ref Field ID to DF-02 etc
            fieldId = BitConverter.ToString(transactionByteCode);
            return fieldData;
        }

        /// <summary>
        /// Code to Extrace Medicare Sub Sub Field
        /// </summary>
        /// <param name="dataArray"></param>
        /// <param name="offset"></param>
        /// <param name="fieldSize"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        private static byte[] ExtractMedicareSubSubField(byte[] dataArray, ref int offset, ref int fieldSize, ref string fieldId)
        {
            // Transaction Code
            byte[] transactionByteCode = new byte[1];
            Array.Copy(dataArray, offset, transactionByteCode, 0, 1);
            offset += 1;
            fieldSize = dataArray[offset];
            offset += 1;

            // Now extract field Data
            byte[] fieldData = new byte[fieldSize];
            Array.Copy(dataArray, offset, fieldData, 0, fieldSize);
            offset += fieldSize;

            // Set the ref Field ID to DF-02 etc
            fieldId = BitConverter.ToString(transactionByteCode);
            return fieldData;
        }

        /// <summary>
        /// Takes the M0 Data string and decodes it according to medicare specs
        /// </summary>
        /// <param name="MCM0Data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> breakupMedicareM0Field(string MCM0Data)
        {
            Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            byte[] dataArray;
            string fieldId = "";
            //fieldText = "";
            int offset = 0, fieldSize = 0;
            int subOffset = 0;

            dataArray = Encoding.Default.GetBytes(MCM0Data);
            if (MCM0Data.Length > 3)
            {
                string badfield = MCM0Data.Substring(3, 1);
                if (badfield != "\xDF")
                {
                    MCM0Data = MCM0Data.Substring(0, 2) + MCM0Data.Substring(3);
                }
            }
            // Convert to Byte Array

            dataArray = Encoding.Default.GetBytes(MCM0Data);
            // Extract Claim Data Elements
            byte[] claimDataElement = ExtractMedicareSubField(dataArray, ref offset, ref fieldSize, ref fieldId);
            while (subOffset < claimDataElement.Length)
            {
                byte[] subField = ExtractMedicareSubField(claimDataElement, ref subOffset, ref fieldSize, ref fieldId);
                switch (fieldId)
                {
                    case "DF-05":
                    case "DF-07":
                    case "DF-32":
                        medicareFields.Add(fieldId, BitConverter.ToString(subField).Replace("-", ""));
                        break;

                    default: medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                }
            }

            string fieldDateData = "";
            // If more data exists extract claimSecondDataElement
            if (offset + 2 < dataArray.Length)
            {
                byte[] claimSecondDataElement = ExtractMedicareSubField(dataArray, ref offset, ref fieldSize, ref fieldId);
                // FieldDID = df0b

                subOffset = 0;
                while (subOffset < claimSecondDataElement.Length)
                {
                    byte[] subField = ExtractMedicareSubField(claimSecondDataElement, ref subOffset, ref fieldSize, ref fieldId);
                    switch (fieldId)
                    {
                        case "DF-11": fieldDateData = BitConverter.ToString(subField).Replace("-", "");
                            medicareFields.Add(fieldId, fieldDateData);
                            break;

                        case "DF-0D": fieldDateData = BitConverter.ToString(subField).Replace("-", "");
                            medicareFields.Add(fieldId, fieldDateData);
                            break;

                        default: medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                    }
                }
            }
            return medicareFields;
        }

        /// <summary>
        /// Takes the M1 Data string and decodes it according to medicare specs
        /// </summary>
        /// <param name="MCM1Data"></param>
        /// <returns></returns>
        private static List<Dictionary<string, string>> breakupMedicareDataM1(string MCM1Data)
        {
            List<Dictionary<string, string>> medicareLinesResult = new List<Dictionary<string, string>>();
            Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            byte[] dataArray;
            dataArray = Encoding.Default.GetBytes(MCM1Data);
            string fieldId = "", fieldText = "";
            int offset = 0, fieldSize = 0;
           // int number;

            if (string.IsNullOrEmpty(MCM1Data)) { return medicareLinesResult; }

            int lineOffset = 0;

            do
            {
                // Extract Claim Data Elements
                byte[] claimDataElement = ExtractMedicareSubField(dataArray, ref lineOffset, ref fieldSize, ref fieldId);
                offset = 0;
                medicareFields = new Dictionary<string, string>();
                // lineOffset += claimDataElement.Length;
                while (offset < claimDataElement.Length)
                {
                    byte[] subField = ExtractMedicareSubSubField(claimDataElement, ref offset, ref fieldSize, ref fieldId);
                    switch (fieldId)
                    {
                        case "D1": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        case "D2": medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                        case "D3": medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                        case "D4": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        case "D5": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        case "D6": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        case "D7": medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                        case "D8": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        case "D9": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        case "DA": medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                        case "DB": medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                        case "DC": medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                        case "DD": fieldText = BitConverter.ToString(subField).Replace("-", ""); medicareFields.Add(fieldId, fieldText); break;
                        default: medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                    }
                }
                medicareLinesResult.Add(medicareFields);
            } while (lineOffset < dataArray.Length);
            return medicareLinesResult;
        }

        /// <summary>
        /// Takes the M2 Data string and decodes it according to medicare specs
        /// </summary>
        /// <param name="MCM2Data"></param>
        /// <returns></returns>
        private Dictionary<string, string> breakupMedicareDataM2(string MCM2Data)
        {
            // Explanation Codes...
            Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            byte[] dataArray;
            dataArray = Encoding.Default.GetBytes(MCM2Data);
            string fieldId = "";
            //fieldText = "";
          //  string explanationCode = "", explanationText = "";
            int offset = 0, fieldSize = 0;
            if (string.IsNullOrEmpty(MCM2Data)) { return medicareFields; }
            int lineOffset = 0;
           // int lineId = 0;
            do
            {
                // Extract Claim Data Elements
                byte[] claimDataElement = ExtractMedicareSubField(dataArray, ref lineOffset, ref fieldSize, ref fieldId);
                offset = 0;
                // lineOffset += claimDataElement.Length;
                while (offset < claimDataElement.Length)
                {
                    byte[] subField = ExtractMedicareSubSubField(claimDataElement, ref offset, ref fieldSize, ref fieldId);
                    medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
                }
            } while (lineOffset < dataArray.Length);
            return medicareFields;
        }

        /// <summary>
        /// Takes the M3 Data string and decodes it according to medicare specs
        /// </summary>
        /// <param name="MCM3Data"></param>
        /// <returns></returns>
        private Dictionary<string, string> breakupMedicareDataM3(string MCM3Data)
        {
            Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            byte[] dataArray;
            dataArray = Encoding.Default.GetBytes(MCM3Data);
            string fieldId = "";
            //fieldText = "";
            //int offset = 0
            int fieldSize = 0;
            if (string.IsNullOrEmpty(MCM3Data)) { return medicareFields; }
            // Extract Claim Data Elements
            //byte[] claimDataElement = ExtractMedicareSubField(dataArray, ref offset, ref fieldSize, ref fieldId);
            byte[] claimDataElement = Encoding.Default.GetBytes(MCM3Data);
            // FieldDID = df02
            // Now taking claimDataElement
            int subOffset = 0;
            while (subOffset < claimDataElement.Length)
            {
                byte[] subField = ExtractMedicareSubField(claimDataElement, ref subOffset, ref fieldSize, ref fieldId);
                medicareFields.Add(fieldId, Encoding.Default.GetString(subField)); break;
            }
            return medicareFields;
        }

        #endregion Medicare Methods

        #region Medicare Encoding Methods

        /// <summary>
        ///
        /// </summary>
        /// <param name="medicareFields"></param>
        /// <param name="medicareClaimType"> Type of medicare claim 1=FullyPaid, 2=PairtPaid, 3=unpaid, 4= BulkBull.  Equivialent to Hicaps CT Field</param>
        /// <returns></returns>
        public static string encodeM0Field(Dictionary<string, string> medicareFields, int medicareClaimType, bool includeM0 = true)
        {
            string m0Field = "";
            int sizm0Field = 0;
            //DF 03 09 30 30 30 30 30 30 30 30 30
            if (medicareFields.ContainsKey("DF-03"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("03", medicareFields["DF-03"]) + " ";
                sizm0Field += medicareFields["DF-03"].Length;
            }
            //DF 04 01 4E
            if (medicareFields.ContainsKey("DF-04"))
            {
                //TODO check to make sure DF-04 field is 1 byte.
                m0Field = m0Field + "DF " + encodeMedicareAlpha("04", medicareFields["DF-04"]) + " ";
                sizm0Field += 1;
            }
            //DF 05 10 12 34 56 78 90 12 34 56 78 90 ClaimantMedicareCardNum
            if (medicareFields.ContainsKey("DF-05"))
            {
                string field = medicareFields["DF-05"];
                m0Field = m0Field + "DF 05 05 " + field.Substring(0, 2) + " "
                                            + field.Substring(2, 2) + " "
                                            + field.Substring(4, 2) + " "
                                            + field.Substring(6, 2) + " "
                                            + field.Substring(8, 2) + " ";
                sizm0Field += 5;
            }
            // DF 06 01 31 ClaimantIRN
            if (medicareFields.ContainsKey("DF-06"))
            {
                //TODO check to make sure DF-06 field is 1 byte.
                m0Field = m0Field + "DF " + encodeMedicareAlpha("06", medicareFields["DF-06"]) + " ";
                sizm0Field += medicareFields["DF-06"].Length;
            }

            //DF 07 10 12 34 56 78 90 12 34 56 78 90 PatientMedicareCardNum
            if (medicareFields.ContainsKey("DF-07"))
            {
                string field = medicareFields["DF-07"];
                m0Field = m0Field + "DF 07 05 " + field.Substring(0, 2) + " "
                                            + field.Substring(2, 2) + " "
                                            + field.Substring(4, 2) + " "
                                            + field.Substring(6, 2) + " "
                                            + field.Substring(8, 2) + " ";
            }
            // DF 08 01 31 PatientIRN
            if (medicareFields.ContainsKey("DF-08"))
            {
                //TODO check to make sure DF-08 field is 1 byte.
                m0Field = m0Field + "DF " + encodeMedicareAlpha("08", medicareFields["DF-08"]) + " ";
                sizm0Field += medicareFields["DF-08"].Length;
            }
            // DF 41 CEV Request indicator
            if (medicareFields.ContainsKey("DF-41"))
            {
                //TODO check to make sure DF-41 field is 1 byte.
                m0Field = m0Field + "DF " + encodeMedicareAlpha("41", medicareFields["DF-41"]) + " ";
                sizm0Field += medicareFields["DF-41"].Length;
            }

            // DF 42 08 30 31 32 33 34 35 36 37  PayeeProviderNum
            //
            if (medicareFields.ContainsKey("DF-42"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("42", medicareFields["DF-42"]) + " ";
                sizm0Field += medicareFields["DF-42"].Length;
            }
            // DF 0A 08 30 31 32 33 34 35 36 37 ServicingProviderNum
            //
            if (medicareFields.ContainsKey("DF-0A"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("0A", medicareFields["DF-0A"]) + " ";
                sizm0Field += medicareFields["DF-0A"].Length;
            }
            // This is correct
            sizm0Field = m0Field.Replace(" ", "").Length / 2;
            // DF 0C  ?? PseudoProviderNum

            #region Second Level Data Elementa

            //Second Level Data Elements
            string secondLevelm0Field = "";

            //DF 0D 04 01 12 20 09
            if (medicareFields.ContainsKey("DF-0D"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF 0D 04 " + medicareFields["DF-0D"] + " ";
            }
            // DF 0E 01 53
            if (medicareFields.ContainsKey("DF-0E"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("0E", medicareFields["DF-0E"]) + " ";
            }
            // DF 0F 01 53
            if (medicareFields.ContainsKey("DF-0F"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("0F", medicareFields["DF-0F"]) + " ";
            }
            // DF 10 08 32 31 30 32 31 32 32 31
            if (medicareFields.ContainsKey("DF-10"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("10", medicareFields["DF-10"]) + " ";
            }

            #region Requesting part

            //DF 11 04 01 12 20 09
            if (medicareFields.ContainsKey("DF-11"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF 11 04 " + medicareFields["DF-11"] + " ";
            }
            // DF 12 01 53
            if (medicareFields.ContainsKey("DF-12"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("12", medicareFields["DF-12"]) + " ";
            }
            // DF 13 01 53
            if (medicareFields.ContainsKey("DF-13"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("13", medicareFields["DF-13"]) + " ";
            }
            // DF 14 08 32 31 30 32 31 32 32 31
            if (medicareFields.ContainsKey("DF-14"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("14", medicareFields["DF-14"]) + " ";
            }

            #endregion Requesting part

            // DF 15 01 4F ServiceType Cde
            // ServiceTypeCde not valid for Bulk Bill
            if (medicareFields.ContainsKey("DF-15"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("15", medicareFields["DF-15"]) + " ";
            }
            // Add DF 0B to start of secondLevel Data Elemetnts
            string secondFieldSize = (secondLevelm0Field.Replace(" ", "").Length / 2).ToString("X2");
            if (secondFieldSize != "00")
            {
                if (medicareClaimType != 4)
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

            #endregion Second Level Data Elementa

            // Add DF02/DF40 to start.
            // Get length of fields.
            if (medicareClaimType != 4)
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

            if (includeM0) m0Field = "4D 30 " + fieldsize.Substring(0, 2) + " " + fieldsize.Substring(2, 2) + " " + m0Field + " 1C";
            return m0Field;
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Encode M3 Message field according to medicare specs
        /// </summary>
        private static string encodeM3ResponseField(string transactionId, int medicareClaimType)
        {
            string m3Field = "";
           // int sizm3Field = 0;
            if (string.IsNullOrEmpty(transactionId))
            {
                transactionId = (DateTime.Now.ToString("fffssffffddmm") + generateInvoiceNumber()).PadLeft(12, '0').Substring(0, 12) + "s";
            }
            m3Field = "DF " + encodeMedicareAlpha("01", transactionId);
            // int fieldlen = m3Field.Replace(" ", "").Length / 2;
            //string fieldsize = fieldlen.ToString("D4");

            //// m3Field = "4D 33 " + fieldsize.Substring(0, 2) + " " + fieldsize.Substring(2, 2) + " " + m3Field + " 1C";
            return m3Field;
        }

        /// <summary>
        /// Encode M0 Message field according to medicare specs
        /// </summary>
        /// <param name="medicareFields">List of fields and values</param>
        /// <param name="medicareClaimType">Type of Medicare claim</param>
        /// <returns></returns>
        private static string encodeM0ResponseField(Dictionary<string, string> medicareFields, int medicareClaimType)
        {
            string m0Field = "";
            int sizm0Field = 0;
            //DF 61 01 30
            if (medicareFields.ContainsKey("DF-61"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("61", medicareFields["DF-61"]) + " ";
                sizm0Field += medicareFields["DF-61"].Length;
            }
            //DF 62 01 30
            if (medicareFields.ContainsKey("DF-62"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("62", medicareFields["DF-62"]) + " ";
                sizm0Field += medicareFields["DF-62"].Length;
            }

            //DF 07 10 12 34 56 78 90 12 34 56 78 90 PatientMedicareCardNum
            if (medicareFields.ContainsKey("DF-07"))
            {
                string field = medicareFields["DF-07"];
                m0Field = m0Field + "DF 07 05 " + field.Substring(0, 2) + " "
                                            + field.Substring(2, 2) + " "
                                            + field.Substring(4, 2) + " "
                                            + field.Substring(6, 2) + " "
                                            + field.Substring(8, 2) + " ";
                sizm0Field += 5;
            }
            // DF 08 01 31 PatientIRN
            if (medicareFields.ContainsKey("DF-08"))
            {
                //TODO check to make sure DF-08 field is 1 byte.
                m0Field = m0Field + "DF " + encodeMedicareAlpha("08", medicareFields["DF-08"]) + " ";
                sizm0Field += medicareFields["DF-08"].Length;
            }
            // DF 35 01 31 Acceptance code
            if (medicareFields.ContainsKey("DF-32"))
            {
                m0Field = m0Field + "DF 32 01 " + medicareFields["DF-32"].PadLeft(2, '0') + " ";
                sizm0Field += 1;
            }

            // DF 35 01 31 Patient First Name
            if (medicareFields.ContainsKey("DF-35"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("35", medicareFields["DF-35"]) + " ";
                sizm0Field += medicareFields["DF-35"].Length;
            }
            // DF 35 01 31 Patient LAst Name
            if (medicareFields.ContainsKey("DF-36"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("36", medicareFields["DF-36"]) + " ";
                sizm0Field += medicareFields["DF-36"].Length;
            }
            // DF 0A 08 30 31 32 33 34 35 36 37 ServicingProviderNum
            //
            if (medicareFields.ContainsKey("DF-0A"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("0A", medicareFields["DF-0A"]) + " ";
                sizm0Field += medicareFields["DF-0A"].Length;
            }
            // DF 0C 08 30 31 32 33 34 35 36 37 Pseudo ServicingProviderNum
            //
            if (medicareFields.ContainsKey("DF-0C"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("0C", medicareFields["DF-0C"]) + " ";
                sizm0Field += medicareFields["DF-0C"].Length;
            }
            // DF 17 01 72 ?? In Confidence Key
            //
            if (medicareFields.ContainsKey("DF-17"))
            {
                m0Field = m0Field + "DF " + encodeMedicareAlpha("17", medicareFields["DF-17"]) + " ";
                sizm0Field += medicareFields["DF-17"].Length;
            }
            // Second part

            #region Second Level Data Elementa

            //Second Level Data Elements
            string secondLevelm0Field = "";

            // DF 38 08 30 31 32 33 34 35 36 37 ServicingProvider Name
            //
            if (medicareFields.ContainsKey("DF-38"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("38", medicareFields["DF-38"]) + " ";
            }
            // DF 66 08 30 31 32 33 34 35 36 37 Referring Provider Name
            //
            if (medicareFields.ContainsKey("DF-66"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("66", medicareFields["DF-66"]) + " ";
            }
            // DF 67 08 30 31 32 33 34 35 36 37 Requesting Provider Name
            //
            if (medicareFields.ContainsKey("DF-67"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("67", medicareFields["DF-67"]) + " ";
            }

            //DF 0D 04 01 12 20 09
            if (medicareFields.ContainsKey("DF-0D"))
            {
                string field = medicareFields["DF-0D"];
                secondLevelm0Field = secondLevelm0Field + "DF 0D 04 " + field.Substring(0, 2) + " "
                                            + field.Substring(2, 2) + " "
                                            + field.Substring(4, 2) + " "
                                            + field.Substring(6, 2) + " ";
            }
            // DF 0E 01 53
            if (medicareFields.ContainsKey("DF-0E"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("0E", medicareFields["DF-0E"]) + " ";
            }
            // DF 0F 01 53
            if (medicareFields.ContainsKey("DF-0F"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("0F", medicareFields["DF-0F"]) + " ";
            }
            // DF 10 08 32 31 30 32 31 32 32 31
            if (medicareFields.ContainsKey("DF-10"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("10", medicareFields["DF-10"]) + " ";
            }

            #region Requesting part

            //DF 11 04 01 12 20 09
            if (medicareFields.ContainsKey("DF-11"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF 11 04 " + medicareFields["DF-11"] + " ";
            }
            // DF 12 01 53
            if (medicareFields.ContainsKey("DF-12"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("12", medicareFields["DF-12"]) + " ";
            }
            // DF 13 01 53
            if (medicareFields.ContainsKey("DF-13"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("13", medicareFields["DF-13"]) + " ";
            }
            // DF 14 08 32 31 30 32 31 32 32 31
            if (medicareFields.ContainsKey("DF-14"))
            {
                secondLevelm0Field = secondLevelm0Field + "DF " + encodeMedicareAlpha("14", medicareFields["DF-14"]) + " ";
            }

            #endregion Requesting part

            // Update length of field
            sizm0Field = (m0Field.Replace(" ", "").Length / 2);
            // Add DF0B DF64 to start of secondLevel Data Elements
            string secondFieldSize = (secondLevelm0Field.Replace(" ", "").Length / 2).ToString("X2");
            if (secondFieldSize != "00")
            {
                if (medicareClaimType != 4)
                {
                    // Patient Claim using nothing
                    m0Field = m0Field.Trim() + " " + secondLevelm0Field;
                    // Re update m0 Size
                    sizm0Field = (m0Field.Replace(" ", "").Length / 2);
                }
                else
                {
                    // Bulk Bill using DF 43
                    m0Field = m0Field + "DF 64 " + secondFieldSize + " " + secondLevelm0Field;
                }
            }
            // Add DF30/DF60 to start.
            // Get length of fields.
            if (medicareClaimType != 4)
            {
                m0Field = m0Field.Trim();
                m0Field = "DF 30 " + sizm0Field.ToString("X2") + " " + m0Field;
            }
            else
            {
                // Bulk Bull using DF 60 field
                m0Field = m0Field.Trim();
                m0Field = "DF 60 " + sizm0Field.ToString("X2") + " " + m0Field;
            }
            ////encryptSubField(HicapsParams, ref m0Field);
            //int fieldlen = m0Field.Replace(" ", "").Length / 2;
            //string fieldsize = fieldlen.ToString("D4");

            //m0Field = "4D 30 " + fieldsize.Substring(0, 2) + " " + fieldsize.Substring(2, 2) + " " + m0Field + " 1C";
            return m0Field;

            #endregion Second Level Data Elementa
        }

        public static string encodeM1Field(List<Dictionary<string, string>> medicareFields, int medicareClaimType)
        {
            string m1Field = "";
            string m1SubField = "";
            int m1SubFieldlen = 0;
            foreach (var myRow in medicareFields)
            {
                foreach (var myFieldRow in myRow)
                {
                    switch (myFieldRow.Key)
                    {
                        case "D1":// fee
                        case "D4": //patient contrib
                        case "D5": // sched fee
                        case "D6": //service benefit
                        case "DD":
                        case "D8": // lspn
                            m1SubField = m1SubField + myFieldRow.Key + " 03 " + myFieldRow.Value.Substring(0, 2) + " " + myFieldRow.Value.Substring(2, 2) + " " + myFieldRow.Value.Substring(4, 2) + " ";
                            break;

                        case "D9": m1SubField = m1SubField + "D9 04 " + myFieldRow.Value.Substring(0, 2) + " " + myFieldRow.Value.Substring(2, 2) + " " + myFieldRow.Value.Substring(4, 2) + " " + myFieldRow.Value.Substring(6, 2) + " ";
                            break;

                        case "DA":
                        case "DB":
                        case "DC":
                        case "D2":
                        case "D7":
                        case "D3":
                        default:
                            m1SubField = m1SubField + encodeMedicareAlpha(myFieldRow.Key, myFieldRow.Value) + " ";
                            break;
                    }
                }
                // add in field separator
                // m1SubField = m1SubField.Trim();
                m1SubFieldlen = m1SubField.Replace(" ", "").Length / 2;
                if (medicareClaimType == 4)
                {
                    m1Field = m1Field + "DF 45 " + m1SubFieldlen.ToString("X2") + " " + m1SubField;
                    m1SubField = "";
                }
                else
                {
                    m1Field = m1Field + "DF 16 " + m1SubFieldlen.ToString("X2") + " " + m1SubField;
                    m1SubField = "";
                }
            }
            return m1Field.Trim();
        }

        public string encodeM2Field(Dictionary<string, string> medicareFields, int medicareClaimType)
        {
            return "";
        }

        public string encodeM3Field(Dictionary<string, string> medicareFields, int medicareClaimType)
        {
            return "";
        }

        public static string encodeMedicareAlpha(string myType, string value)
        {
            string len;
            string result;

            if (!string.IsNullOrEmpty(value))
            {
                result = BitConverter.ToString(Encoding.Default.GetBytes(value)).Replace("-", " ");
                len = Convert.ToInt32(value.Length).ToString("X2");
                result = myType + " " + len + " " + result;

                return result;
            }
            else return "";
        }

      

        #endregion Medicare Encoding Methods

        public static string PadLeftZero(string value, int length)
        {
            return HMessage.Right(new string('0', length) + value.Trim(), length);
        }

        public static string ConvertToHex2(string asciiString)
        {
            // Jp attempt v2. // Fix for still getting bizarre conversions
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

        public static byte[] ConvertToHex(string asciiString)
        {
            // Jp attempt v2. // Fix for still getting bizarre conversions
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
            return myList.ToArray();
        }

        [Obsolete]
        protected static string Right(string rightString, int length)
        {
            return rightString.Right(length);
        }

        [Obsolete]
        protected static string Left(string leftString, int length)
        {
            return leftString.Left(length);
        }

        public static string generateRRN()
        {
            return Right("000000000000" + DateTime.Now.ToString("fffssffffddmm"), 12);
        }

        public string TryGetFieldValue(string keyValue, string defaultValue = null)
        {
            if (FieldValues != null && FieldValues.ContainsKey(keyValue))
            {
                return FieldValues[keyValue];
            }
            return defaultValue;
        }

        public void CopyObject<T>(T sourceObject)
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

        #endregion MessageBuilder Functions
        /// <summary>
        /// Decode byte array into object representation
        /// </summary>
        /// <param name="bHicapsData">Raw byte array of HCPOS message</param>
        public void LoadFromByteArray(byte[] bHicapsData)
        {
            // Static id's
            int presentationStartOffset = 13;
            int presentationHeaderLength = 22;
            int transTypeByteIndex = 15; // Position of Transaction Code
            // PresentationFields
            byte[] transTypeByteCode = new byte[2];
            string transCode;

            byte format = new byte();
            byte rri = new byte();
            string transactionCode = "";
            string responseCode = "";
            byte moreIndicator = new byte();

            string eaSubDataField = "";

            int offsetStart = 21;
            string fieldText = "";

            int endOfTextPos = 0;
           // int strtOfTextPos = -1;



            if (bHicapsData.Length > presentationHeaderLength)
            {
                transTypeByteCode[0] = bHicapsData[transTypeByteIndex];
                transTypeByteCode[1] = bHicapsData[transTypeByteIndex + 1];
                transCode = Encoding.Default.GetString(transTypeByteCode);
                TransTypeCode = transCode;
                // Extract EaData Field Block
                endOfTextPos = Array.LastIndexOf(bHicapsData, Convert.ToByte(3));

                // Okay Now lets breakup the EaData Message
                offsetStart = presentationStartOffset;
                fieldText = "";

                ExtractPresentationHeader(bHicapsData, ref offsetStart, ref  format, ref  rri, ref  transactionCode, ref  responseCode, ref  moreIndicator);

                TransTypeCode = transCode;
                MoreIndicator = moreIndicator;
                RequestResponseIndicator = rri;
                FormatVersion = format;
                ResponseCode = responseCode;
                while (offsetStart < endOfTextPos)
                {
                    ExtractSubField(bHicapsData, ref offsetStart, ref fieldText, ref eaSubDataField, transCode);
                    //#added by Amy Q# if it contains already the key, do nothing
                    if (FieldValues.ContainsKey(eaSubDataField) == false)
                    {
                       FieldValues.Add(eaSubDataField, fieldText);
                    }
                }
               CheckDigitVerified = VerifyChecksum(bHicapsData);
            }
        }

        public bool isPartOfNetwork(string networkName)
        {
            if (FieldValues.ContainsKey("NN"))
            {
                if (FieldValues["NN"] != networkName.SHA256())
                {
                    return false;
                }
            }
            else
            {
                if (!networkName.IsNullOrWhiteSpace())
                {
                    return false;
                }
            }

            return true;
        }
        public static string generateInvoiceNumber()
        {
            Random random = new Random();
            return Right("000000000000" + random.Next(1000, 999999), 6);
        }

        public static string GenerateSurcharge(string amount, decimal percentage)
        {
            try
            {
                decimal transAMount = (Convert.ToDecimal(amount) / Convert.ToDecimal(100));
                return HMessage.PadLeftZero(encodeDecimal(transAMount * percentage), 12);
            }
            catch (Exception )
            {
                return HMessage.PadLeftZero("0", 12);
            }
        }

        public static string GenerateM0Response(string M0Data, int medicareClaimType, string cardNumber)
        {
            string serviceProviderName = "";
            string requestProviderName = "";
            string referProviderName = "";
            Dictionary<string, string> requestFields = breakupMedicareM0Field(M0Data);
            Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            // First Level DF-60
            if (medicareClaimType == 4)
            {
                // Concession status.  Only valid for Bulk Bill
                if (requestFields.ContainsKey("DF-41"))
                {
                    if (requestFields["DF-41"] == "Y")
                    {
                        medicareFields.Add("DF-61", "Y"); // Consession status
                    }
                }

                medicareFields.Add("DF-62", "Y"); // Medicare Eligibility Status
            }
            cardNumber = cardNumber.PadLeft(10, '1');
            medicareFields.Add("DF-07", cardNumber); // Card PAN Number
            if (requestFields.ContainsKey("DF-08"))
            {
                medicareFields.Add("DF-08", requestFields["DF-08"]); // Patient UPI
            }
            medicareFields.Add("DF-32", "00"); // Assessment Code
            medicareFields.Add("DF-35", "PATIENT FIRSTNAME"); // Patient First Name
            medicareFields.Add("DF-36", "LASTNAME"); // Patient Last Name

            if (requestFields.ContainsKey("DF-0A"))
            {
                string provider = requestFields["DF-0A"];
                serviceProviderName = DataQuery.getProviderName(provider);
                if (string.IsNullOrEmpty(serviceProviderName)) { serviceProviderName = "DR SERVICE SPOCK"; }

                medicareFields.Add("DF-0A", provider); // Provider Num
            }
            medicareFields.Add("DF-0C", "6433653F"); // Psuedo Provider Num ????
            medicareFields.Add("DF-17", "r"); // In Confidence Key TEKr = 'r' TEKs = 's'

            medicareFields.Add("DF-38", serviceProviderName.Trim()); // Provider Name

            // Referring Doctor
            if (requestFields.ContainsKey("DF-10"))
            {
                string provider = requestFields["DF-10"];
                medicareFields.Add("DF-10", provider); // Referring Provider Num
                referProviderName = DataQuery.getProviderName(provider);
                if (string.IsNullOrEmpty(referProviderName)) { referProviderName = "DR REFER SPOCK"; }

                medicareFields.Add("DF-66", referProviderName); // Referring Provider Name
            }

            // Requesting Doctor
            if (requestFields.ContainsKey("DF-14"))
            {
                string provider = requestFields["DF-14"];
                medicareFields.Add("DF-14", provider); // Requesting Provider Num
                referProviderName = DataQuery.getProviderName(provider);
                if (string.IsNullOrEmpty(referProviderName)) { referProviderName = "DR REQUEST SPOCK"; }

                medicareFields.Add("DF-67", referProviderName); // Referring Provider Name
            }

            if (requestFields.ContainsKey("DF-0D"))
            {
                medicareFields.Add("DF-0D", requestFields["DF-0D"]); // Referring Date
            }
            if (requestFields.ContainsKey("DF-0E"))
            {
                medicareFields.Add("DF-0E", requestFields["DF-0E"]); // Referral Period Type Code
            }
            if (requestFields.ContainsKey("DF-0F"))
            {
                medicareFields.Add("DF-0F", requestFields["DF-0F"]); // Referral Override Code
            }
            if (requestFields.ContainsKey("DF-12"))
            {
                medicareFields.Add("DF-12", requestFields["DF-12"]); // Referral Override Code
            }

            //medicareFields.Add("DF-0E", "I"); // Referring Date

            return HMessage.ConvertToHex2(encodeM0ResponseField(medicareFields, medicareClaimType)).ToString();
        }

        public static string GenerateM3Response(int medicareClaimType)
        {
            return HMessage.ConvertToHex2(encodeM3ResponseField(null, medicareClaimType));
            //Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            //return "4D 33 00 27 DF 01 18 30 30 38 31 30 30 38 31 38 31 37 35 38 32 30 38 363530323931325303"
        }

        public static string GenerateM1Response(string M1Data, int medicareClaimType)
        {
            Dictionary<string, string> medicareFields = new Dictionary<string, string>();
            List<Dictionary<string, string>> m1Fields = new List<Dictionary<string, string>>();

            // Note fully paid claims, no data comes back for this field.
            if (medicareClaimType == 1)
            {
                return "";
            }

            m1Fields = breakupMedicareDataM1(M1Data);
            // All items sched fee = $150.  Medicare benefit = $112.50
            string hexSchedFee = Right("000000" + 15000.ToString("X"), 6);
            string hexSchedBenefit = Right("000000" + 11250.ToString("X"), 6);
            for (int i = 0; i < m1Fields.Count; i++)
            {
                // Line Level Loop
                if (medicareClaimType == 4)
                {
                    // Add Benefit Assigned
                    int amount = 15000;
                    string hexField = Right("000000" + amount.ToString("X"), 6);
                    m1Fields[i].Add("DD", hexField);
                }
                if ((medicareClaimType == 2) || (medicareClaimType == 3))
                {
                    int iChargeAmount = 0;
                    // Add Benefit Assigned
                    if (m1Fields[i].ContainsKey("D1"))
                    {
                        iChargeAmount = int.Parse(m1Fields[i]["D1"], NumberStyles.AllowHexSpecifier);
                    }

                    m1Fields[i].Add("D5", hexSchedFee); // Shedule fee
                    m1Fields[i].Add("D6", hexSchedBenefit); // Service Benefit
                    m1Fields[i].Add("D7", "0"); // Explanation Code
                }
            }

            return HMessage.ConvertToHex2(encodeM1Field(m1Fields, medicareClaimType));
        }

        public static string GenerateM2Response(string M2Data, int p)
        {
            throw new NotImplementedException();
        }

        public string ToFormattedString()
        {
            var sb = new StringBuilder();
            foreach (var field in FieldValues)
            {
                sb.AppendLine(field.Key + ":" + field.Value);
            }
            return sb.ToString();
        }

        
    }
}
