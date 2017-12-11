using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Globalization;
using System.Xml;
using System.Diagnostics;
using System.Runtime.Serialization;
using HicapsTools;

namespace HicapsConnectControl
{
    // Delegate
    public delegate void TerminalListChangedEventHandler(string param);

    //Source interface for events to be exposed.
    //Add GuidAttribute to the source interface to supply an explicit System.Guid.
    //Add InterfaceTypeAttribute to indicate that interface is IDispatch interface.
    [GuidAttribute("5FE4AC0A-BB2B-4aa7-AF62-9EEC70163978")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ControlEvents

        //Add a DisIdAttribute to any members in the source 
    //interface to specify the COM DispId.
    {
        [DispIdAttribute(0x60020000)]
        void OnTerminalListChanged(string param);
    }

    ///	<summary>
    ///	Summary	description	for	UserControl1.
    ///	</summary>
    [ProgId("HicapsConnectControl.HicapsConnectControl")]
    [GuidAttribute("1FCF8E0D-AF98-395D-9DD4-2B0E137EE3D0")]
    //[ClassInterface(ClassInterfaceType.AutoDual)]

    [ClassInterface(ClassInterfaceType.AutoDual), ComSourceInterfaces(typeof(ControlEvents))]
    public partial class HicapsConnectControl 
    {
        #region local vars
        private const string STR_ACK00000000000000000 = "ACK00000000000000000";
        private const string STR_ACK10000000000000000 = "ACK10000000000000000";
        private readonly string STR_AckReceived = "Received, Waiting for Terminal";
        private readonly string STR_Connecting = "Connecting";
        private readonly string STR_DestinationErrorCouldNotConnectToServer = "Destination Error, Could not connect to Server{0}";
        private readonly string STR_NetworkError = "Network Error";
        private readonly string STR_VersionIncompat = "Please update HICAPS Connect as version is incompatible";
        private readonly string STR_NetworkRequestTimedOut = "Network Request timed out{0}";
       // private readonly string STR_NetworkResponseTimedOut = "Network Response timed out{0}";
        private readonly string STR_QueGoAway = "Que ? Go away";
       // private readonly string STR_Reading = "Reading";
        private readonly string STR_Sending = "Sending";
        private readonly string STR_SendingRequest = "Sending request...";
        private readonly string STR_StandAloneRequestTimedOutCheckServiceIsRunning = "StandAlone Request timed out, Check Service is running";
       // private readonly int TCP_ServerPort = 11000;
        private readonly int UPD_ListenPort = 11001;
        //List<string> serverList;
        //List<string> updServerList;
        //List<string> ignoreServerList;
        //List<string> terminalServerList;
        private string _defaultServerUrl = "";
        private bool rescanLocalTerminals = true;
        private StatusForm _myStatusForm;

        private bool _resetTerminalListThread = false;

        private Dictionary<string, DateTime> ignoreServerIpSeen = new Dictionary<string, DateTime>();

        private Dictionary<string, string> localTerminalIdList = new Dictionary<string, string>();

        bool noNetworkEnabled = false;

        private Dictionary<string, DateTime> serverIpSeen = new Dictionary<string, DateTime>();

        // Reduce timeout to 5000 seconds
        private int shortTimeout = 5000;
        private bool showStatusWindow = false;
        // New 3.4+ code line.
        //TerminalId, Location
        private Dictionary<string, string> terminalIdList = new Dictionary<string, string>();
        private Dictionary<string, DateTime> terminalIdSeen = new Dictionary<string, DateTime>();
        #endregion

        #region DefaultAcquiere

        private const string NABAcquirer = "1";
        private const string HICAPSAcquirer = "2";
        private const string MedicareAcquirer = "3";

        private string LastAcquirerID = HICAPSAcquirer;
        #endregion
        /* Com oVerrides */

        //  HicapsConnectProtocol MessageProtocol;
        #region re-defed classes from HicapsConnectIntergrate to get it to work with ActiveX correctly..
        //  This is a quick fix.... it shouldn't really be necessary I thought to do it this way
        
            //////UDP udp; // = new UDP(11005, 11003);

        public class BaseMessage
        {
            protected string _validationOkMessage = "Validated Ok";
            private bool _readonly = false;

            public bool ReadOnly
            {
                get { return _readonly; }
                set { if (value) { _readonly = value; } }
            }

          
           

            /// <summary>
            /// Network Message Id.  For Future Use
            /// </summary>
            private string _msgId;
            public string MsgId
            {
                get { return _msgId; }
                set { _msgId = value; }
            }

            private byte _formatVersion;
            /// <summary>
            /// Always set to 1
            /// </summary>
            /// 
            public byte FormatVersion
            {
                get { return _formatVersion; }
                set { _formatVersion = value; }
            }

            private byte _requestResponseIndicator;
            /// <summary>
            /// ‘0’ 	This message is a request message which requires a response. The terminal will print all receipts.
            /// </summary>
            /// 
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
            private string _serverUrl;
            /// <summary>
            /// Destination of Server to go to format is IP:PORT:TERMINALID:COMX
            /// </summary>

            public string ServerUrl
            {
                get { return _serverUrl; }
                set { _serverUrl = value; }
            }

            private string _computerName;
            /// <summary>
            /// Name of the computer source creating the message
            /// </summary>
            public string ComputerName
            {
                get { return _computerName; }
                set { _computerName = value; }
            }
        
            private string _softwareVendorName;
            /// <summary>
            /// Name of the Software vendor
            /// </summary>

            public string SoftwareVendorName
            {
                get { return _softwareVendorName; }
                set { _softwareVendorName = value; }
            }

           

            public static string extractComPort(string url)
            {
                string[] serverParts = url.Split(':');
                // TODO put the Server url into an object
                if (serverParts.Count() > 3)
                {
                    return serverParts[3];
                }
                return "";
            }
            public static string extractTerminalId(string url)
            {
                string[] serverParts = url.Split(':');
                // TODO put the Server url into an object
                
                if (serverParts.Count() > 3)
                {
                    return serverParts[2];
                }
                if (serverParts.Count() == 2)  // KG 20 Jan 2015
                {
                    return url;
                }
                if (!string.IsNullOrEmpty(url) && url.Trim().Length == 6)
                {
                    return url;
                }
                return "";
            }
            /// <summary>
            /// Constructor
            /// </summary>
            public BaseMessage() { MsgId = System.Guid.NewGuid().ToString(); }
            public static string extractClassName(string xmlMessage)
            {
                string myClassName = "";
                int pos = xmlMessage.IndexOf("?>");
                if (pos > 0)
                {
                    int tagpos = xmlMessage.IndexOf('<', pos);
                    int endtagpos = xmlMessage.IndexOf('>', tagpos);
                    string tag = xmlMessage.Substring(tagpos + 1, endtagpos);
                    string[] tags = tag.Split(' ');
                    if (tags.Any())
                        myClassName = tags[0];
                }

                return myClassName;
            }

            public virtual string[] breakupLineFields(int Index)
            {
                throw new NotImplementedException();
            }

            public string getPropertyValueAsString(string propertyName)
            {
                PropertyInfo[] properties = this.GetType().GetProperties();
                List<string> myList = new List<string>();
                foreach (PropertyInfo property in properties)
                {

                    if (property.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (property.GetValue(this, null) != null && property.GetValue(this, null).GetType() == myList.GetType())
                        {
                            return "";
                        }
                        else
                        {
                            return String.Format("{0}", property.GetValue(this, null) ?? "");
                        }
                    }
                }
                throw new ArgumentOutOfRangeException(String.Format("Property [{0}] does not exist", propertyName));
                return "";
            }

            public string ToDebugString()
            {
                string myoutput = "";
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
                return myoutput;
            }

            public virtual bool validateMessage(ref string validationMessage)
            {
                // Implement in decendant
                //throw new NotImplementedException("Implement in decendant");
                validationMessage = _validationOkMessage;
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

            protected bool isAlphaNumeric(string strValue)
            {
                string regExpression = @"^[a-zA-Z0-9]+$";
                return Regex.IsMatch(strValue, regExpression);

            }

            protected bool isNumeric(string strValue)
            {
                string regExpression = @"/(^-?\d\d*\.\d*$)|(^-?\d\d*$)|(^-?\.\d\d*$)/";
                return Regex.IsMatch(strValue, regExpression);
            }

            protected bool isSpaceAlphaNumeric(string strValue)
            {
                string regExpression = @"^[a-zA-Z0-9 ]+$";
                return Regex.IsMatch(strValue, regExpression);

            }

            protected string Left(string leftString, int length)
            {
                return leftString.Substring(0, length);
            }

            protected string Right(string rightString, int length)
            {
                return rightString.Substring(rightString.Length - length);
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
                    return "Bodypart ID must be no more than 2 numeric characters eg, 11";
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
                    return "Invalid CCV Field, Field must only contain Numeric digits";
                }
                if (CCV.Length > 6)
                {
                    return "Invalid CCV Field, Field can only be from 0 to 6 digits long";
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
                    return "CCV not Supplied, Must answer CCVReason Code, valid values are 1,2,3";
                }
                if (!string.IsNullOrEmpty(CCV) && (CCVReason != null && CCVReason != 0))
                {
                    return "CCV Supplied, CCV Reason must be blank";
                }

                if (CCVReason > 4 || CCVReason < 0)
                {
                    return "Invalid CCV Reason Field, valid values are 1,2,3";
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
                    return "Invalid CCV Source Field, valid values are 1,2,3,4";
                }
                return "";
            }

            protected string validateEftposPAN(string PrimaryAccountNumber)
            {
                PrimaryAccountNumber = PrimaryAccountNumber.Trim();
                if (!isNumeric(PrimaryAccountNumber))
                {
                    return "Invalid Primary Account Number field, Field must only contain Numeric digits";
                }
                if (PrimaryAccountNumber.Length > 0 && PrimaryAccountNumber.Length < 13)
                {
                    return "Invalid Primary Account Number field, Field must be at least 13 digits long";
                }
                if (PrimaryAccountNumber.Length > 19)
                {
                    return "Invalid Primary Account Number field, Field can only be at most 19 digits long";
                }
                return "";
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
                        return "Expiry date must be entered if PrimaryAccountNumber is set ";
                    }
                }

                if (expiryDate.Length != 4)
                {
                    return "Invalid Expiry Date, must be MMYY, eg 0109";
                }
                if (!isNumeric(expiryDate))
                {
                    return "Invalid Expiry Date, must be MMYY, must be 4 Numeric numbers";
                }
                string month = Left(expiryDate, 2);
                if ("01,02,03,04,05,06,07,08,09,10,11,12,".IndexOf(month + ",") < 0)
                {
                    return "Invalid Expiry Date, not a valid month";
                }

                return "";
            }

            protected string validateFeeAmount(decimal feeAmount)
            {

                if (feeAmount <= 0)
                {
                    return "Fee amount must be greater than 0";
                }
                if (feeAmount >= 10000)
                {
                    return "Fee amount must be less than 10000.00";
                }
                if (feeAmount.ToString().LastIndexOf('.') > 0)
                {
                    // Check for only 2 decimal places
                    if (feeAmount.ToString().Length - feeAmount.ToString().LastIndexOf('.') > 3)
                    {
                        return String.Format("Fee must only contain 2 decimal points, [{0}]", feeAmount);
                    }
                }
                return "";
            }

            protected string validateItemNumber(string itemNumber)
            {

                if (itemNumber.Trim().Length > 4)// || !isNumeric(itemNumber))
                {
                    return "Item Number must be no more than 4 numeric characters eg, 1001";
                }
                return "";
            }

            protected string validateMerchantId(string MerchantId)
            {
                if (MerchantId == null || MerchantId.Trim().Length <= 0)
                {
                    return "MerchantId must be supplied\r\n";
                }
                if (MerchantId.Trim().Length > 8 || MerchantId.Trim().Length < 8)
                {
                    return "Supplied Merchant ID is invalid\r\n";
                }
                return "";
            }
            protected string validatePatientId(string patientId)
            {

                if (patientId.Trim().Length > 2 || !isNumeric(patientId))
                {
                    return "Patient ID must be 2 numeric characters eg, 01";
                }
                return "";
            }

            protected string validatePatientName(string patientName)
            {
                if (patientName.Trim().Length > 28)
                {
                    return "Patient name must be less than 28 characters";
                }
                return "";
            }

            protected string validateProviderNumberId(string ProviderNumberId)
            {
                if (ProviderNumberId == null || ProviderNumberId.Trim().Length <= 0)
                {
                    return "ProviderNumberId must be supplied\r\n";
                }

                if (ProviderNumberId.Trim().Length > 8)
                {
                    return "ProviderNumberId must not be greater than 8 characters\r\n";
                }
                if (!isAlphaNumeric(ProviderNumberId.Trim()))
                {
                    return "ProviderNumberId must only contain 8 AlphaNumeric Characters\r\n";
                }
                return "";
            }
            #region Medicare Validation Code
            protected string validateMedicareClaimLine(int claimType, string claimDetailsLine)
            {
                string validationMessage = "";
                if (claimType > 0 && claimType <= 4)
                {
                    string itemNumber = claimDetailsLine.Substring(0, 6).Trim(); //= "12345";
                    string chargeAmount = claimDetailsLine.Substring(6, 6).Trim(); ;//= "0000060";
                    string dateofservice = claimDetailsLine.Substring(12, 8).Trim(); ;//= "01072009";
                    string itemOverrideCode = claimDetailsLine.Substring(20, 2).Trim(); //= "AP"; //"AP"
                    string lspn = claimDetailsLine.Substring(22, 6).Trim();//= "123456"; //123456
                    string equipmentId = claimDetailsLine.Substring(28, 5).Trim();//= "12345"; //12345
                    string selfDeemedCde = claimDetailsLine.Substring(33, 1).Trim();//= "Y";  //Y
                    string contribPatientAmount = claimDetailsLine.Substring(34, 6);//= "000000"; // "0000000";
                    string spcid = claimDetailsLine.Substring(40, 4); // = "    "

                    validationMessage += validateMedicareItemNumber(itemNumber);
                    validationMessage += validateMedicareTransactionAmount(chargeAmount);
                    validationMessage += validateMedicareDateStr(dateofservice);
                    validationMessage += validateMedicareItemOverrideCode(itemOverrideCode);
                    validationMessage += validateMedicareLspn(lspn);
                    validationMessage += validateMedicareSelfDeemedCde(selfDeemedCde);
                    validationMessage += validateMedicareTransactionAmount(contribPatientAmount);
                    validationMessage += validateMedicareSpcId(spcid);
                    // TODO add in Medicare Claim Line Validation
                }
                return validationMessage;
            }

            protected string validateMedicareProviderNumberId(string ProviderNumberId)
            {
                string errorMessage = validateProviderNumberId(ProviderNumberId);
                if (!isValidProviderNumberId(ProviderNumberId))
                {
                    errorMessage += "Invalid Medicare Provider Number (Checkdigit Failed)";
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
            private string validateMedicareDateStr(string dateofservice)
            {
                if (dateofservice.Length != 8 || !isNumeric(dateofservice))
                {
                    return "Invalid Date, must be DDMMYYYY, eg 01122009";
                }
                string day = dateofservice.Substring(0, 2);
                int iDay = Convert.ToInt32(day);
                if (iDay > 31 || iDay < 0)
                {
                    return "Invalid Day, not a valid day";
                }

                string month = dateofservice.Substring(2, 2);
                int iMonth = Convert.ToInt32(month);
                if ("01,02,03,04,05,06,07,08,09,10,11,12,".IndexOf(month + ",") < 0)
                {
                    return "Invalid Date, not a valid month";
                }
                string year = dateofservice.Substring(4, 4);
                int iYear = Convert.ToInt32(year);

                if (iYear < 2000)
                {
                    return "Invalid year, not a valid year";
                }
                try
                {
                    DateTime myDate = new DateTime(iYear, iMonth, iDay);
                }
                catch (Exception ex)
                {
                    return "Invalid date, not a valid Date";
                }
                return "";
            }

            private string validateMedicareEquipmentId(string EquipmentId)
            {
                if (!string.IsNullOrEmpty(EquipmentId))
                {
                    if (!isNumeric(EquipmentId))
                    {
                        return "Invalid EquipmentId, EquipmentId must be a 5 digit numeric number";
                    }
                }
                return "";
            }

            private string validateMedicareItemNumber(string itemNumber)
            {
                if (string.IsNullOrEmpty(itemNumber) || itemNumber.Length >= 6 || !isNumeric(itemNumber))
                {
                    return "Invalid Medicare Item Number, item number can only be numerics, with length of at most 6";
                }
                return "";
            }

            private string validateMedicareItemOverrideCode(string itemOverrideCode)
            {
                string validationMessage = "";
                if (!string.IsNullOrEmpty(itemOverrideCode))
                {
                    if ("AP,AO,NC,".IndexOf(itemOverrideCode + ",") <= 0)
                    {
                        return "Invalid item Override Code, valid values are blank, AP, AO, NC";
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
                        return "Invalid LSPN, LSPN must be a 6 digit numeric number";
                    }
                }
                return "";
            }

            private string validateMedicareSelfDeemedCde(string selfDeemedCde)
            {
                if (!string.IsNullOrEmpty(selfDeemedCde))
                {
                    if (selfDeemedCde != "Y" && selfDeemedCde != "N")
                    {
                        return "Invalid selfDeemedCde, Valid values are 'Y' and 'N'";
                    }
                }
                return "";
            }

            private string validateMedicareSpcId(string spcid)
            {
                // TODO write validation.
                if (!string.IsNullOrEmpty(spcid))
                {
                    if (spcid.Length > 4)
                    {
                        return "Invalid SPCId, SPCI must not be greater than length 4";
                    }
                }
                return "";
            }
            private string validateMedicareTransactionAmount(string chargeAmount)
            {
                // in theory this shouldn't happen.... but in case it does
                if (string.IsNullOrEmpty(chargeAmount) || !isNumeric(chargeAmount) || chargeAmount.Length != 6)
                {
                    return "Invalid medicare amount, value must be in cents";
                }
                int amount = Convert.ToInt32(chargeAmount);
                if (amount < 0)
                {
                    return "Invalid medicare Charge amount, value must be positive";
                }
                return "";
            }
            #endregion
            protected string validateRefundPassword(string refundPassword)
            {
                if (!isNumeric(refundPassword) || refundPassword.Trim().Length != 4)
                {
                    return "Refund password must be a 4 digit number";
                }
                return "";
            }

            protected string validateRRN(string RRN)
            {
                if (RRN == null || RRN.Trim().Length <= 0)
                {
                    return "RRN must be supplied\r\n";
                }
                if (RRN.Trim().Length > 12)
                {
                    return "RRN must be no more than 12 numeric characters";
                }
                return "";
            }
            protected string validateServerUrl(string serverUrl)
            {

                return "";
            }

            protected string validateServiceDate(DateTime serviceDate)
            {
                if (serviceDate.Date > System.DateTime.Now.Date)
                {
                    return "Service date cannot be greater than today";
                }

                return "";
            }

            protected string validateTransactionAmount(decimal transAmount, string transType)
            {
                if (transType == null || transType == "") transType = "Transaction";

                if (transAmount <= 0)
                {
                    return String.Format("{0} amount must be greater than 0\r\n", transType);
                }
                if (transAmount >= 10000)
                {
                    return String.Format("{0} amount must be less than 10000.00\r\n", transType);
                }
                return "";
            }

            protected void validateScriptNumber(string scriptNumber)
            {
                if (string.IsNullOrEmpty(scriptNumber) || scriptNumber == string.Empty || !isNumeric(scriptNumber) || scriptNumber.Length > 10 || scriptNumber.Length < 1)
                {
                    throw new InvalidOperationException("Pharmacy Script number is missing or invalid");
                }
            }
            //public bool setAttribute(string fieldName, string fieldValue)
            //{
            //    PropertyInfo[] properties = this.GetType().GetProperties();
            //    foreach (PropertyInfo property in properties)
            //    {
            //        if (property.Name.ToLower() == fieldName.Trim().ToLower())
            //        {
            //            if (property.PropertyType == typeof(string))
            //            {
            //                property.SetValue(this, fieldValue, null);//(this, p.GetValue(sourceObject, null), null);
            //            }
            //            else if (property.PropertyType == typeof(decimal))
            //            {
            //                try
            //                {
            //                    decimal myNum = System.Convert.ToDecimal(fieldValue);
            //                    property.SetValue(this, myNum, null);//(this, p.GetValue(sourceObject, null), null);
            //                }
            //                catch (Exception)
            //                {
            //                    return false;
            //                }
            //            }
            //            else if (property.PropertyType == typeof(int))
            //            {
            //                try
            //                {
            //                    int myNum = System.Convert.ToInt32(fieldValue);
            //                    property.SetValue(this, myNum, null);//(this, p.GetValue(sourceObject, null), null);
            //                }
            //                catch (Exception)
            //                {
            //                    return false;
            //                }
            //            }
            //            return true;
            //        }
            //    }
            //    return false;
            //}
        }

        public class AllItemListRequest : BaseRequest
        {
        }

        public class AllItemListResponse : BaseResponse
        {
            private List<string> _itemListDescription;

            private DateTime _transactionDate;

            public AllItemListResponse()
            {
                _itemListDescription = new List<string>();
            }

            public List<string> ItemListDescription
            {
                get { return _itemListDescription; }
                set { if (!ReadOnly) { _itemListDescription = value; } }
            }
            public string[] ItemListDescriptionStr
            {
                get { return _itemListDescription.ToArray(); }
                set { }
            }
            public DateTime TransactionDate
            {
                get
                {
                    return _transactionDate;
                }
                set
                {
                    if (!ReadOnly) { _transactionDate = value; }
                }
            }
        }
        public class AllPharmItemListRequest : AllItemListRequest
        {

        }
        public class AllPharmItemListResponse : AllItemListResponse
        {

        }

        public class AllItemResponseCodeListRequest : BaseRequest
        {
        }

        public class AllItemResponseCodeListResponse : BaseResponse
        {
            private List<string> _itemResponseCodeDescription;

            private DateTime _transactionDate;

            public AllItemResponseCodeListResponse()
            {
                _itemResponseCodeDescription = new List<string>();
            }

            public List<string> ItemResponseCodeDescription
            {
                get { return _itemResponseCodeDescription; }
                set { if (!ReadOnly) { _itemResponseCodeDescription = value; } }
            }
            public DateTime TransactionDate
            {
                get
                {
                    return _transactionDate;
                }
                set
                {
                    if (!ReadOnly) { _transactionDate = value; }
                }
            }
        }

        public class AllMerchantListRequest : BaseRequest
        {
        }

        public class AllMerchantListResponse : BaseResponse
        {
            private List<string> _merchantListDetails;

            private DateTime _transactionDate;

            public AllMerchantListResponse()
            {
                MerchantListDetails = new List<string>();
            }

            public List<string> MerchantListDetails
            {
                get { return _merchantListDetails; }
                set { if (!ReadOnly) { _merchantListDetails = value; } }
            }
            public string[] MerchantListDetailsStr
            {
                get { return _merchantListDetails.ToArray(); }
                set { }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
            public override string[] breakupLineFields(int Index)
            {
                string[] myData = new string[3];
                try
                {
                    string fieldData = MerchantListDetails[Index];
                    myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                    myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                    myData[2] = fieldData.Substring(23, 40).Trim(); // Description
                }
                catch (Exception ex)
                {
                    // throw new IndexOutOfRangeException();
                }

                return myData;
            }
        }

        public class AllProviderListRequest : BaseRequest
        {

        }

        public class AllProviderListResponse : BaseResponse
        {
            private List<string> _providerListDetails;

            private DateTime _transactionDate;

            public AllProviderListResponse()
            {
                ProviderListDetails = new List<string>();
            }

            public List<string> ProviderListDetails
            {
                get { return _providerListDetails; }
                set { if (!ReadOnly) { _providerListDetails = value; } }
            }
            public string[] ProviderListDetailsStr
            {
                get { return _providerListDetails.ToArray(); }
                set { }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }

            public override string[] breakupLineFields(int Index)
            {
                string[] myData = new string[7];
                try
                {
                    string fieldData = ProviderListDetails[Index];
                    myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                    myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                    myData[2] = fieldData.Substring(23, 8).Trim(); // Provider Id
                    myData[3] = fieldData.Substring(31, 16).Trim(); //Provider Name
                    myData[4] = fieldData.Substring(47, 1).Trim(); // Provider Type
                    myData[5] = fieldData.Substring(48, 8).Trim(); // Payee Provider Id
                    myData[6] = fieldData.Substring(56, 2).Trim(); // Provider List Num

                }
                catch (Exception ex)
                {
                    // throw new IndexOutOfRangeException();
                }

                return myData;
            }
        }

        public class AllTransCodeListRequest : BaseRequest
        {
        }

        public class AllTransCodeListResponse : BaseResponse
        {
            private DateTime _transactionDate;
            private List<string> _transCodeDescription;

            public AllTransCodeListResponse()
            {
                _transCodeDescription = new List<string>();
            }

            public DateTime TransactionDate
            {
                get
                {
                    return _transactionDate;
                }
                set
                {
                    if (!ReadOnly) { _transactionDate = value; }
                }
            }

            public List<string> TransCodeDescription
            {
                get { return _transCodeDescription; }
                set { if (!ReadOnly) { _transCodeDescription = value; } }
            }
        }


        public class BaseRequest : BaseMessage
        {
            private string _clientVersion;
            private string _pmsKey;

            public string ClientVersion
            {
                get { return _clientVersion; }
                set { _clientVersion = value; }
            }

            public string PmsKey
            {
                get { return _pmsKey; }
                set { _pmsKey = value; }
            }
        }

        public class BaseResponse : BaseMessage
        {

            /// <summary>
            /// Version of the Client.
            /// </summary>
            private string _clientVersion = HVersion.BuildVersion;

            private string _comPort;
            private string _responseCode;

            private string _responseText;

            private DateTime _responseTime;

            /// <summary>
            /// Version of the Server
            /// </summary>
            private string _serverVersion = "";

            private string _terminalVersion;

            [DataMember(Order = 14)]
            public string ClientVersion
            {
                get { return _clientVersion; }
                set { if (!ReadOnly) { _clientVersion = value; } }
            }

            [DataMember(Order = 9)]
            public string ComPort
            {
                get { return _comPort; }
                set { _comPort = value; }
            }
            /// <summary>
            /// Identifies the result of the transaction in a response. Response code should be set to ‘00’ for all request messages.
            /// </summary>
            /// <remarks></remarks>
            [DataMember(Order = 12)]
            public string ResponseCode
            {
                get { return _responseCode; }
                set { if (!ReadOnly) { _responseCode = value; } }
            }

            /// <summary>
            /// Text equivilant of ResponseCode
            /// </summary>
            [DataMember(Order = 11)]
            public string ResponseText
            {
                get { return _responseText; }
                set { if (!ReadOnly) { _responseText = value; } }
            }

            /// <summary>
            /// Time of Response
            /// </summary>
            [DataMember(Order = 10)]
            public DateTime ResponseTime
            {
                get { return _responseTime; }
                set { if (!ReadOnly) { _responseTime = value; } }
            }
            [DataMember(Order = 13)]
            public string ServerVersion
            {
                get { return _serverVersion; }
                set { if (!ReadOnly) { _serverVersion = value; } }
            }
            [DataMember(Order = 15)]
            public string TerminalVersion
            {
                get { return _terminalVersion; }
                set { if (!ReadOnly) { _terminalVersion = value; } }
            }
        }

        public class CardListRequest : BaseRequest
        {

        }

        public class CardListResponse : BaseResponse
        {

            private List<string> _cardFundDetails;

            private DateTime _transactionDate;

            public CardListResponse()
            {
                CardFundDetails = new List<string>();
            }

            public List<string> CardFundDetails
            {
                get { return _cardFundDetails; }
                set { if (!ReadOnly) { _cardFundDetails = value; } }
            }
            public string[] CardFundDetailsStr
            {
                get { return _cardFundDetails.ToArray(); }
                set { }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
            public override string[] breakupLineFields(int Index)
            {
                string[] myData = new string[3];
                try
                {
                    string fieldData = CardFundDetails[Index];
                    myData[0] = fieldData.Substring(0, 6).Trim();  //Fund Name
                    myData[1] = fieldData.Substring(6, 14).Trim(); // Card Prefix
                    myData[2] = fieldData.Substring(20, 2).Trim(); // Line Count
                }
                catch (Exception ex)
                {
                    // throw new IndexOutOfRangeException();
                }
                return myData;
            }

        }
        public class CardReadRequest : BaseRequest
        {
        }

        public class CardReadResponse : BaseResponse
        {

            private string _expiryDate;
            private string _primaryAccountNumber;
            private string _secondTrack;

            private string _trackdata;

            private DateTime _transactionDate;

            public CardReadResponse() { }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    if (!ReadOnly) { _expiryDate = value; }
                }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }
            public string SecondTrack
            {
                get
                {
                    return _secondTrack;
                }
                set
                {
                    if (!ReadOnly) { _secondTrack = value; }
                }
            }
            public string TrackData
            {
                get
                {
                    return _trackdata;
                }
                set
                {
                    if (!ReadOnly) { _trackdata = value; }
                }
            }

            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
        }


        public class CashoutRequest : BaseRequest
        {
            private decimal _cashoutAmount;

            private string _CCV;

            private int _CCVReason;

            private int _CCVSource;

            private string _expiryDate;

            // Required for Multi-Merchant
            private string _merchantId;

            private string _primaryAccountNumber;

            private bool _printCustomerReceipt;

            private bool _printCustomerReceiptPrompt;

            private bool _printMerchantReceipt;

            public decimal CashoutAmount
            {
                get { return _cashoutAmount; }
                set { _cashoutAmount = value; }
            }

            public string CCV
            {
                get { return _CCV; }
                set { _CCV = value; }
            }

            public int CCVReason
            {
                get { return _CCVReason; }
                set { _CCVReason = value; }
            }

            public int CCVSource
            {
                get { return _CCVSource; }
                set { _CCVSource = value; }
            }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    _expiryDate = value;
                }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { _merchantId = value; }
            }
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
            public bool PrintCustomerReceipt
            {
                get { return _printCustomerReceipt; }
                set { _printCustomerReceipt = value; }
            }

            public bool PrintCustomerReceiptPrompt
            {
                get { return _printCustomerReceiptPrompt; }
                set { _printCustomerReceiptPrompt = value; }
            }

            public bool PrintMerchantReceipt
            {
                get { return _printMerchantReceipt; }
                set { _printMerchantReceipt = value; }
            }
            public override bool validateMessage(ref string validationMessage)
            {
                validationMessage = "";

                validationMessage += validateTransactionAmount(CashoutAmount, "Cashout");
                validationMessage += validateMerchantId(MerchantId);

                return checkValidationMessage(validationMessage);
            }

        }
        public class CashoutResponse : BaseResponse
        {
            private string _approvalCode;
            private string _authResponseCode;

            private decimal _cashoutAmount;

            private string _expiryDate;

            private string _invoiceNumber;

            private string _merchantId;

            private string _primaryAccountNumber;

            private string _printReceiptData;

            private string _providerName;

            private string _rrnNumber;

            private decimal _surchargeAmount;

            private string _terminalId;

            private string _terminalSwipe;

            private DateTime _transactionDate;

            public string ApprovalCode
            {
                get { return _approvalCode; }
                set { if (!ReadOnly) { _approvalCode = value; } }
            }

            public string AuthResponseCode
            {
                get { return _authResponseCode; }
                set { if (!ReadOnly) { _authResponseCode = value; } }
            }
            public decimal CashoutAmount
            {
                get { return _cashoutAmount; }
                set { _cashoutAmount = value; }
            }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    if (!ReadOnly) { _expiryDate = value; }
                }
            }

            public string InvoiceNumber
            {
                get { return _invoiceNumber; }
                set { if (!ReadOnly) { _invoiceNumber = value; } }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }

            public string PrintReceiptData
            {
                get { return _printReceiptData; }
                set { if (!ReadOnly) { _printReceiptData = value; } }
            }

            public string ProviderName
            {
                get { return _providerName; }
                set { if (!ReadOnly) { _providerName = value; } }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { if (!ReadOnly) { _rrnNumber = value; } }
            }

            public decimal SurchargeAmount
            {
                get { return _surchargeAmount; }
                set { _surchargeAmount = value; }
            }

            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }

            public string TerminalSwipe
            {
                get { return _terminalSwipe; }
                set { if (!ReadOnly) { _terminalSwipe = value; } }
            }

            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
        }

        public class ClaimCancelRequest : BaseRequest
        {
            // This is to Flag for Non-Settlement Claims
            // PMS do not use this normally, and is restricted
            // via PMSKey on the Terminal.
            private bool _nonSettlementFlag = false;

            private int _patientCount = 0;
            private List<string> _patientNameDetails;
            private string _primaryAccountNumber;
            private bool _printReceiptOnTerminal;
            private string _providerNumberId;
            private string _rrnNumber;

            private decimal _transactionAmount;

            public ClaimCancelRequest()
            {
                TransactionAmount = 0;
                _patientNameDetails = new List<string>();
            }

            public bool NonSettlementFlag
            {
                get { return _nonSettlementFlag; }
                set { _nonSettlementFlag = value; }
            }

            // Patient Data
            public List<string> PatientNameDetails
            {
                get { return _patientNameDetails; }
                set { }
            }

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

            public bool PrintReceiptOnTerminal
            {
                get { return _printReceiptOnTerminal; }
                set { _printReceiptOnTerminal = value; }
            }

            public string ProviderNumberId
            {
                get { return _providerNumberId; }
                set
                {
                    _providerNumberId = Right("00000000" + value, 8);
                }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { _rrnNumber = value; }
            }
            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }
            public void addPatientName(string patientId, string patientName)
            {
                string zerofill = "00000000";
                string errorMessage = "";
                errorMessage += validatePatientName(patientName);
                errorMessage += validatePatientId(patientId);
                if (errorMessage.Length > 0) { throw new InvalidCastException(errorMessage); }
                if (PatientNameDetails.Count == 10) { throw new InvalidCastException("Only 10 different patients may be added"); };

                patientId = Right((zerofill + patientId ?? "00".Trim()), 2);
                patientName = patientName.Trim();
                _patientNameDetails.Add(patientId + patientName.Trim());
                _patientCount++;
            }
        }



        public class ClaimCancelResponse : BaseResponse
        {
            private decimal _benefitAmount;
            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            // 
            private List<string> _claimDetails;

            private string _expiryDate;
            private string _membershipId;
            private string _merchantId;
            private string _primaryAccountNumber;
            private string _providerName;
            private string _providerNumberId;
            private string _rrnNumber;
            private string _terminalId;

            private DateTime _transactionDate;

            public ClaimCancelResponse()
            {
                _claimDetails = new List<string>();
            }

            public decimal BenefitAmount
            {
                get { return _benefitAmount; }
                set { _benefitAmount = value; }
            }

            public List<string> ClaimDetails
            {
                get { return _claimDetails; }
                set { if (!ReadOnly) { _claimDetails = value; } }
            }

            public string[] ClaimDetailsStr
            {
                get { return _claimDetails.ToArray(); }
                set { }
            }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    if (!ReadOnly) { _expiryDate = value; }
                }
            }

            public string MembershipId
            {
                get { return _membershipId; }
                set { _membershipId = value; }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }

            public string ProviderName
            {
                get { return _providerName; }
                set { if (!ReadOnly) { _providerName = value; } }
            }

            public string ProviderNumberId
            {
                get { return _providerNumberId; }
                set { _providerNumberId = value; }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { if (!ReadOnly) { _rrnNumber = value; } }
            }
            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }

            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
        }

        public class ClaimPharmRequest : ClaimRequest
        {
            private List<string> _scriptDetails;

            public List<string> ScriptDetails
            {
                get { return _scriptDetails; }
                set { }
            }
            public ClaimPharmRequest()
            {
                _scriptDetails = new List<string>();
            }
            public override void addClaimLine(string patientId, string itemNumber, string bodyPart, DateTime serviceDate, decimal fee)
            {
                throw new InvalidOperationException("Method is invalid, use addClaimPharmLine instead");
            }
            public void addClaimPharmLine(string patientId, string itemNumber, string scriptNumber, bool compoundDrugs, DateTime serviceDate, decimal fee)
            {
                string bodyPart = "00";
                if (compoundDrugs) { bodyPart = "01"; }
                scriptNumber = (scriptNumber ?? "").Trim();

                validateScriptNumber(scriptNumber);
                base._addClaimLine(ref patientId, ref itemNumber, ref bodyPart, serviceDate, fee);
                _scriptDetails.Add(scriptNumber);
            }

           
        }
        public class ClaimPharmResponse : ClaimResponse
        {
            private List<string> _scriptDetails;

            public ClaimPharmResponse()
            {
                _scriptDetails = new List<string>();
            }

            public List<string> ScriptDetails
            {
                get { return _scriptDetails; }
                set { if (!ReadOnly) { _scriptDetails = value; } }
            }

            public string[] ScriptDetailsStr
            {
                get { return _scriptDetails.ToArray(); }
                set { }
            }
        }

        public class ClaimRequest : BaseRequest
        {
            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            // 
            private List<string> _claimDetails;

            private int _claimLineCount = 0;
            private string _membershipId;
            // This is to Flag for Non-Settlement Claims
            // PMS do not use this normally, and is restricted
            // via PMSKey on the Terminal.
            private bool _nonSettlementFlag = false;

            private int _patientCount = 0;
            private List<string> _patientNameDetails;
            private string _primaryAccountNumber;
            private bool _printReceiptOnTerminal;
            private string _providerNumberId;
            private decimal _transactionAmount;

            public ClaimRequest()
            {
                _claimDetails = new List<string>();
                _patientNameDetails = new List<string>();
                TransactionAmount = 0;
            }

            public List<string> ClaimDetails
            {
                get { return _claimDetails; }
                set { }
            }

            public string MembershipId
            {
                get { return _membershipId; }
                set { _membershipId = value; }
            }

            public bool NonSettlementFlag
            {
                get { return _nonSettlementFlag; }
                set { _nonSettlementFlag = value; }
            }

            public List<string> PatientNameDetails
            {
                get { return _patientNameDetails; }
                set { }
            }

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

            public bool PrintReceiptOnTerminal
            {
                get { return _printReceiptOnTerminal; }
                set { _printReceiptOnTerminal = value; }
            }

            public string ProviderNumberId
            {
                get { return _providerNumberId; }
                set
                {
                    string error = validateProviderNumberId(value);
                    if (error.Length > 0)
                    {
                        throw new InvalidOperationException(error);
                    }
                    else
                    {
                        _providerNumberId = Right("00000000" + value, 8);
                    }
                }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }
            public virtual void addClaimLine(string patientId, string itemNumber, string bodyPart, DateTime serviceDate, decimal fee)
            {
                _addClaimLine(ref patientId, ref itemNumber, ref bodyPart, serviceDate, fee);
            }

            public void addPatientName(string patientId, string patientName)
            {
                string zerofill = "00000000";
                string errorMessage = "";
                errorMessage += validatePatientName(patientName);
                errorMessage += validatePatientId(patientId);
                if (errorMessage.Length > 0) { throw new InvalidCastException(errorMessage); }
                if (PatientNameDetails.Count == 10) { throw new InvalidCastException("Only 10 different patients may be added"); };

                patientId = Right((zerofill + patientId ?? "00".Trim()), 2);
                patientName = patientName.Trim();
                _patientNameDetails.Add(patientId + patientName.Trim());
                _patientCount++;
            }
            public override bool validateMessage(ref string validationMessage)
            {
                validationMessage = validateClaimMessage(validationMessage);
                return checkValidationMessage(validationMessage);
            }

            internal void _addClaimLine(ref string patientId, ref string itemNumber, ref string bodyPart, DateTime serviceDate, decimal fee)
            {
                string data = "";
                string dateservice = "";
                string feeamount = "00";
                string zerofill = "00000000";
                string errorMessage = "";
                if (bodyPart == null) { bodyPart = ""; }
                if (patientId == null) { patientId = ""; }
                if (itemNumber == null) { itemNumber = ""; }
                errorMessage += validatePatientId(patientId);
                errorMessage += validateItemNumber(itemNumber);
                errorMessage += validateBodyPart(bodyPart);
                errorMessage += validateFeeAmount(fee);
                errorMessage += validateServiceDate(serviceDate);
                if (errorMessage.Length > 0) { throw new InvalidCastException(errorMessage); }

                patientId = Right((zerofill + patientId ?? "00".Trim()), 2);
                //itemNumber = Right(zerofill + itemNumber ?? "0000".Trim(), 4);
                itemNumber = Left(itemNumber + new string(' ', 4), 4);
                bodyPart = Right(zerofill + bodyPart ?? "00".Trim(), 2);
                // 
                if (!isNumeric(patientId)) { patientId = "00"; throw new InvalidCastException("Invalid patient id, PatientID must contain numeric values only eg 00"); }
                if (!isSpaceAlphaNumeric(itemNumber)) { itemNumber = "0000"; throw new InvalidCastException("Invalid Item Number, Item Number must only contain AlphaNumeric values only eg A-Z, a-z, 0-9, and spaces"); }
                if (!isAlphaNumeric(bodyPart)) { itemNumber = "00"; throw new InvalidCastException("Invalid Bodypart id, Bodypart must contain numeric values only eg 11"); }
                dateservice = Right(zerofill + serviceDate.Day.ToString(), 2) + Right(zerofill + serviceDate.Month.ToString(), 2);
                // Get fee into 012500" format for 125.00
                feeamount = "00000000" + (fee * System.Convert.ToDecimal(100)).ToString();
                if (feeamount.IndexOf('.') > 0) { feeamount = Left(feeamount, feeamount.IndexOf('.')); }
                feeamount = Right(feeamount, 6);

                // Claim Data format stored as a string array to help serialisation
                //Format is  + Separator
                // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
                data = patientId + itemNumber + bodyPart + dateservice + feeamount;
                _claimDetails.Add(data);
                TransactionAmount += fee;
                _claimLineCount++;
            }
            private string validateClaimMessage(string validationMessage)
            {
                validationMessage = "";

                validationMessage += validateTransactionAmount(TransactionAmount, "Claim");
                if (ClaimDetails == null || ClaimDetails.Count <= 0)
                {
                    validationMessage += "Claim must have at least one line";
                }
                if (ClaimDetails.Count > 24)
                {
                    validationMessage += "Claim can only have a maximum of 24 lines";
                }

                if (PatientNameDetails.Count != _patientCount)
                {
                    validationMessage += "Patient Details are invalid";
                }
                if (ClaimDetails.Count != _claimLineCount)
                {
                    validationMessage += "Claim Details are invalid";
                }
                validationMessage += validateProviderNumberId(ProviderNumberId);
                return validationMessage;
            }

        }




        public class ClaimResponse : BaseResponse
        {
            private decimal _benefitAmount;
            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
            // 
            private List<string> _claimDetails;

            private string _expiryDate;
            private string _membershipId;
            private string _merchantId;
            private List<string> _patientNameDetails;
            private string _primaryAccountNumber;
            private string _providerName;

            private string _providerNumberId;

            private string _rrnNumber;

            private string _terminalId;

            private decimal _transactionAmount;

            private DateTime _transactionDate;

            public ClaimResponse()
            {
                ClaimDetails = new List<string>();
                TransactionAmount = 0;
            }

            public decimal BenefitAmount
            {
                get { return _benefitAmount; }
                set { if (!ReadOnly) { _benefitAmount = value; } }
            }

            public List<string> ClaimDetails
            {
                get { return _claimDetails; }
                set { if (!ReadOnly) { _claimDetails = value; } }
            }

            public string[] ClaimDetailsStr
            {
                get { return _claimDetails.ToArray(); }
                set { }
            }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    if (!ReadOnly) { _expiryDate = value; }
                }
            }

            public string MembershipId
            {
                get { return _membershipId; }
                set { if (!ReadOnly) { _membershipId = value; } }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public List<string> PatientNameDetails
            {
                get { return _patientNameDetails; }
                set { if (!ReadOnly) { _patientNameDetails = value; } }
            }

            public string[] PatientNameDetailsStr
            {
                get
                {
                    return _patientNameDetails.ToArray();
                }
                set { }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }
            public string ProviderName
            {
                get { return _providerName; }
                set { if (!ReadOnly) { _providerName = value; } }
            }

            public string ProviderNumberId
            {
                get { return _providerNumberId; }
                set { if (!ReadOnly) { _providerNumberId = value; } }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { if (!ReadOnly) { _rrnNumber = value; } }
            }

            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { if (!ReadOnly) { _transactionAmount = value; } }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
           
        }

        public class EftposDepositRequest : RefundRequest
        {

        }

        public class EftposDepositResponse : RefundResponse
        {

        }

        public class EftposTransListingRequest : BaseRequest
        {
            // Required for Multi-Merchant
            private string _merchantId;

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { _merchantId = value; }
            }
        }

        public class EftposTransListingResponse : SaleCashoutResponse
        {
            private List<CashoutResponse> _cashoutTransactionList;
            private List<EftposDepositResponse> _eftposDepositTransactionList;
            private List<RefundResponse> _refundTransactionList;
            private List<SaleCashoutResponse> _saleCashoutTransactionList;
            private List<SaleResponse> _saleTransactionList;
            private string _subTransCode;

            private DateTime _transactionDate;

            public EftposTransListingResponse()
            {
                EftposDepositTransactionList = new List<EftposDepositResponse>();
                RefundTransactionList = new List<RefundResponse>();
                SaleCashoutTransactionList = new List<SaleCashoutResponse>();
                SaleTransactionList = new List<SaleResponse>();
                CashoutTransactionList = new List<CashoutResponse>();
            }

            public List<CashoutResponse> CashoutTransactionList
            {
                get { return _cashoutTransactionList; }
                set { if (!ReadOnly) { _cashoutTransactionList = value; } }
            }

            public List<EftposDepositResponse> EftposDepositTransactionList
            {
                get { return _eftposDepositTransactionList; }
                set { if (!ReadOnly) { _eftposDepositTransactionList = value; } }
            }

            public List<RefundResponse> RefundTransactionList
            {
                get { return _refundTransactionList; }
                set { if (!ReadOnly) { _refundTransactionList = value; } }
            }

            public List<SaleCashoutResponse> SaleCashoutTransactionList
            {
                get { return _saleCashoutTransactionList; }
                set { if (!ReadOnly) { _saleCashoutTransactionList = value; } }
            }

            public List<SaleResponse> SaleTransactionList
            {
                get { return _saleTransactionList; }
                set { if (!ReadOnly) { _saleTransactionList = value; } }
            }

            public string SubTransCode
            {
                get { return _subTransCode; }
                set { _subTransCode = value; }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
            public CashoutResponse getCashoutTransaction(int index)
            {
                if ((index <= 0) || (index > _cashoutTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _cashoutTransactionList.Count));
                }
                return (CashoutResponse)_cashoutTransactionList[index - 1];
            }

            public int getCashoutTransactionCount()
            {
                return _cashoutTransactionList.Count;
            }
            public EftposDepositResponse getEftposDepositTransaction(int index)
            {
                if ((index <= 0) || (index > _eftposDepositTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _eftposDepositTransactionList.Count));
                }
                return (EftposDepositResponse)_eftposDepositTransactionList[index - 1];
            }

            public int getEftposDepositTransactionCount()
            {
                return _eftposDepositTransactionList.Count;
            }

            public RefundResponse getRefundTransaction(int index)
            {
                if ((index <= 0) || (index > _refundTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _refundTransactionList.Count));
                }
                return (RefundResponse)_refundTransactionList[index - 1];
            }

            public int getRefundTransactionCount()
            {
                return _refundTransactionList.Count;
            }

            public SaleCashoutResponse getSaleCashoutTransaction(int index)
            {
                if ((index <= 0) || (index > _saleCashoutTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _saleCashoutTransactionList.Count));
                }
                return (SaleCashoutResponse)_saleCashoutTransactionList[index - 1];
            }

            public int getSaleCashoutTransactionCount()
            {
                return _saleCashoutTransactionList.Count;
            }

            public SaleResponse getSaleTransaction(int index)
            {
                if ((index <= 0) || (index > _saleTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _saleTransactionList.Count));
                }
                return (SaleResponse)_saleTransactionList[index - 1];
            }

            public int getSaleTransactionCount()
            {
                return _saleTransactionList.Count;
            }
        }

        public class GetTerminalRequest : BaseRequest
        {
        }

        public class GetTerminalResponse : BaseResponse
        {
            private List<string> _serverData;
            private List<string> _terminalList;

            public GetTerminalResponse() { TerminalList = new List<string>(); }

            public List<string> ServerData
            {
                get { return _serverData; }
                set { _serverData = value; }
            }

            public List<string> TerminalList
            {
                get { return _terminalList; }
                set { if (!ReadOnly) { _terminalList = value; } }
            }
            public string[] TerminalListStr
            {
                get { return _terminalList.ToArray(); }
                set { }
            }
        }

        public class HicapsTotalsRequest : BaseRequest
        {
            private bool _printReceiptOnTerminal;
            private string _providerNumberId;

            private bool _todayTransactions = true;

            public bool PrintReceiptOnTerminal
            {
                get { return _printReceiptOnTerminal; }
                set { _printReceiptOnTerminal = value; }
            }

            public string ProviderNumberId
            {
                get { return _providerNumberId; }
                set
                {
                    _providerNumberId = Right("00000000" + value, 8);
                }
            }
            public bool TodayTransactions
            {
                get { return _todayTransactions; }
                set { _todayTransactions = value; }
            }
        }

        public class HicapsTotalsResponse : BaseResponse
        {
            private List<string> _hicapsTotalsListDetails;
            private DateTime _transactionDate;

            public HicapsTotalsResponse()
            {
                HicapsTotalsListDetails = new List<string>();
            }

            public List<string> HicapsTotalsListDetails
            {
                get { return _hicapsTotalsListDetails; }
                set { if (!ReadOnly) { _hicapsTotalsListDetails = value; } }
            }

            public string[] HicapsTotalsListDetailsStr
            {
                get { return _hicapsTotalsListDetails.ToArray(); }
                set { }
            }

            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
        }

        public class HicapsTransListingRequest : BaseRequest
        {
            private string _providerNumberId;

            public string ProviderNumberId
            {
                get { return _providerNumberId; }
                set
                {
                    _providerNumberId = Right("00000000" + value, 8);
                }
            }
        }

        public class HicapsTransListingResponse : ClaimResponse
        {
            private List<ClaimCancelResponse> _claimCancelTransactionList;
            private List<ClaimResponse> _claimTransactionList;
            private List<QuoteResponse> _quoteTransactionList;
            private string _subTransCode;

            public HicapsTransListingResponse()
            {
                ClaimTransactionList = new List<ClaimResponse>();
                ClaimCancelTransactionList = new List<ClaimCancelResponse>();
                QuoteTransactionList = new List<QuoteResponse>();
            }

            public List<ClaimCancelResponse> ClaimCancelTransactionList
            {
                get { return _claimCancelTransactionList; }
                set { if (!ReadOnly) { _claimCancelTransactionList = value; } }
            }

            public List<ClaimResponse> ClaimTransactionList
            {
                get { return _claimTransactionList; }
                set { if (!ReadOnly) { _claimTransactionList = value; } }
            }

            public List<QuoteResponse> QuoteTransactionList
            {
                get { return _quoteTransactionList; }
                set { if (!ReadOnly) { _quoteTransactionList = value; } }
            }

            public string SubTransCode
            {
                get { return _subTransCode; }
                set { if (!ReadOnly) { _subTransCode = value; } }
            }
            public ClaimCancelResponse getClaimCancelTransaction(int index)
            {
                if ((index <= 0) || (index > _claimCancelTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _claimCancelTransactionList.Count));
                }
                return (ClaimCancelResponse)_claimCancelTransactionList[index - 1];
            }

            public int getClaimCancelTransactionCount()
            {
                return _claimCancelTransactionList.Count;
            }

            public ClaimResponse getClaimTransaction(int index)
            {
                if ((index <= 0) || (index > _claimTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _claimTransactionList.Count));
                }
                return (ClaimResponse)_claimTransactionList[index - 1];
            }

            public int getClaimTransactionCount()
            {
                return _claimTransactionList.Count;
            }
            public QuoteResponse getQuoteTransaction(int index)
            {
                if ((index <= 0) || (index > _quoteTransactionList.Count))
                {
                    throw new IndexOutOfRangeException(String.Format("Zero based array, index given is out of range, there is only {0} items in array", _quoteTransactionList.Count));
                }
                return (QuoteResponse)_quoteTransactionList[index - 1];
            }

            public int getQuoteTransactionCount()
            {
                return _quoteTransactionList.Count;
            }
        }

        public class MedicareClaimRequest : BaseRequest
        {
            private string _accountReferenceId;

            private decimal _benefitAssigned;

            ////public string LspNum
            ////{
            ////    get { return _lspNum; }
            ////    set { _lspNum = value; }
            ////}
            private string _cevRequestInd;

            private string _ClaimantIRN;

            private string _claimantMedicareCardNum;

            // Claim Data format stored as a string array to help serialisation
            //Format is  + Separator
            //  ItemNumber + ItemDescription +  ChargeAmount + dateofService + ItemOverrideCode + LspNum )
            // 
            private List<string> _claimDetails;

            // SELECT MEDICARE TXN
            // 1. FULLY PAID
            //  2. PART PAID
            // 3. UN-PAID
            // 4. BULK BILL
            private int _claimType;

            private string _PatientIRN;

            private string _patientMedicareCardNum;

            private string _payeeProviderNum;

            private DateTime _referralIssueDate;

            private string _referralOverrideTypeCde;

            private string _referralPeriodTypeCde;

            private string _referringProviderNum;

            private string _requestingProviderNum;

            private DateTime _requestIssueDate;

            private string _requestOverrideTypeCde;

            private string _requestPeriodTypeCde;

            //public string RequestTypeCde
            //{
            //    get { return _requestTypeCde; }
            //    set { _requestTypeCde = value; }
            //}
            private string _serviceTypeCde;

            private string _servicingProviderNum;

            private decimal _transactionAmount;

            public MedicareClaimRequest()
            {
                _claimDetails = new List<string>();
                TransactionAmount = 0;
            }

            public string AccountReferenceId
            {
                get { return _accountReferenceId; }
                set { _accountReferenceId = value; }
            }

            public decimal BenefitAssigned
            {
                get { return _benefitAssigned; }
                set { _benefitAssigned = value; }
            }

            ////private string _lspNum;
            public string CevRequestInd
            {
                get { return _cevRequestInd; }
                set { _cevRequestInd = value; }
            }

            public string ClaimantIRN
            {
                get { return _ClaimantIRN; }
                set { _ClaimantIRN = value; }
            }

            //public string MerchantId
            //{
            //    get { return (_merchantId ?? "").Trim(); }
            //    set { _merchantId = value; }
            //}
            public string ClaimantMedicareCardNum
            {
                get { return _claimantMedicareCardNum; }
                set { _claimantMedicareCardNum = value; }
            }

            ////public string ItemOverrideCode
            ////{
            ////    get { return _itemOverrideCode; }
            ////    set { _itemOverrideCode = value; }
            ////}
            public List<string> ClaimDetails
            {
                get { return _claimDetails; }
                set { }
            }

            public int ClaimType
            {
                get { return _claimType; }
                set { _claimType = value; }
            }
            public string PatientIRN
            {
                get { return _PatientIRN; }
                set { _PatientIRN = value; }
            }

            // Required for Multi-Merchant
            //private string _merchantId;
            public string PatientMedicareCardNum
            {
                get { return _patientMedicareCardNum; }
                set { _patientMedicareCardNum = value; }
            }
            public string PayeeProviderNum
            {
                get { return _payeeProviderNum; }
                set { _payeeProviderNum = value; }
            }

            public DateTime ReferralIssueDate
            {
                get { return _referralIssueDate; }
                set { _referralIssueDate = value; }
            }

            public string ReferralOverrideTypeCde
            {
                get { return _referralOverrideTypeCde; }
                set { _referralOverrideTypeCde = value; }
            }

            public string ReferralPeriodTypeCde
            {
                get { return _referralPeriodTypeCde; }
                // Where we have Alpha indicators ie : ReferralPeriodTypeCde 
                // Can you please ensure that they are sent in upper case

                set { _referralPeriodTypeCde = (value ?? "").ToUpper(); }
            }

            public string ReferringProviderNum
            {
                get { return _referringProviderNum; }
                set { _referringProviderNum = value; }
            }

            public string RequestingProviderNum
            {
                get { return _requestingProviderNum; }
                set { _requestingProviderNum = value; }
            }

            public DateTime RequestIssueDate
            {
                get { return _requestIssueDate; }
                set { _requestIssueDate = value; }
            }

            public string RequestOverrideTypeCde
            {
                get { return _requestOverrideTypeCde; }
                set { _requestOverrideTypeCde = value; }
            }

            public string RequestPeriodTypeCde
            {
                get { return _requestPeriodTypeCde; }
                set { _requestPeriodTypeCde = value; }
            }

            //private string _requestTypeCde;
            public string ServiceTypeCde
            {
                get { return _serviceTypeCde; }
                set { _serviceTypeCde = value; }
            }

            public string ServicingProviderNum
            {
                get { return _servicingProviderNum; }
                set { _servicingProviderNum = value; }
            }
            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { }
            }
            //// These are per line.... shouldn't be here I think
            ////private decimal _chargeAmount;

            ////public decimal ChargeAmount
            ////{
            ////    get { return _chargeAmount; }
            ////    set { _chargeAmount = value; }
            ////}

            ////private string _dateOfService;

            ////public string DateOfService
            ////{
            ////    get { return _dateOfService; }
            ////    set { _dateOfService = value; }
            ////}

            ////private string _itemNum;

            ////public string ItemNum
            ////{
            ////    get { return _itemNum; }
            ////    set { _itemNum = value; }
            ////}

            ////private string _itemDescription;

            ////public string ItemDescription
            ////{
            ////    get { return _itemDescription; }
            ////    set { _itemDescription = value; }
            ////}

            ////private string _itemOverrideCode;
            public void addMediClaimLine(string itemNum, decimal chargeAmount, DateTime dateOfService, string itemOverrideCde, string lspNum, string equipmentId, string selfDeemedCde, decimal contribPatientAmount, string spcId)
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
                itemNum = Right(new String(' ', 6) + itemNum, 6);
                //bodyPart = Right(zerofill + bodyPart, 2);
                // 
                if (!isSpaceAlphaNumeric(itemNum)) { itemNum = "000000"; throw new IndexOutOfRangeException("Invalid Item Number, Item Number must only contain AlphaNumeric values only eg A-Z, a-z, 0-9, and spaces"); }
                //if (!isNumeric(bodyPart)) { itemNumber = "00"; throw new IndexOutOfRangeException("Invalid Bodypart id, Bodypart must contain numeric values only eg 11"); }
                dateofservice = Right(zerofill + dateOfService.Day.ToString(), 2) + Right(zerofill + dateOfService.Month.ToString(), 2) + Right(zerofill + dateOfService.Year.ToString(), 4);
                //    // Get fee into 012500" format for 125.00
                feeamount = (chargeAmount * System.Convert.ToDecimal(100)).ToString();
                if (feeamount.IndexOf('.') > 0) { feeamount = Left(feeamount, feeamount.IndexOf('.')); }
                feeamount = Right(zerofill + feeamount, 6);

                // Contrib Patient Amount
                contribPatientAmountStr = (contribPatientAmount * System.Convert.ToDecimal(100)).ToString();
                if (contribPatientAmountStr.IndexOf('.') > 0) { contribPatientAmountStr = Left(contribPatientAmountStr, contribPatientAmountStr.IndexOf('.')); }
                contribPatientAmountStr = Right(zerofill + contribPatientAmountStr, 6);

                lspNum = Right((new String(' ', 6) + lspNum), 6);
                itemOverrideCde = Right((new String(' ', 2) + itemOverrideCde), 2);
                equipmentId = Right((new String(' ', 5) + equipmentId), 5);
                spcId = Right((new String(' ', 4) + spcId), 4);
                selfDeemedCde = Right(("  " + selfDeemedCde), 2);
                // Claim Data format stored as a string array to help serialisation
                //Format is  + Separator
                // Patient Id + ItemNumber + BodyPart +  DDMM + FeeAmount)
                //        6          6          8                  2           6           5            2                       6                  4
                data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr + spcId;
                _claimDetails.Add(data);

                //ChargeAmount += fee;
            }
            public override bool validateMessage(ref string validationMessage)
            {
                validationMessage = "";
                if (ClaimType != 4)
                {
                    validationMessage += validateTransactionAmount(TransactionAmount, "MedicareClaim");
                }
                if (ClaimDetails == null || ClaimDetails.Count <= 0)
                {
                    validationMessage += "Claim must have at least one line";
                }
                if (ClaimDetails.Count > 14)
                {
                    validationMessage += "Claim can only have a maximum of 14 lines";
                }

                validationMessage += validateProviderNumberId(ServicingProviderNum);
                return checkValidationMessage(validationMessage);


            }
        }

        public class MedicareClaimResponse : BaseResponse
        {

            private string _acceptanceTypeCde;
            private string _accountPaidInd;
            private string _accountReferenceId;
            private string _assessmentErrorCde;
            private string _claimantFirstName;
            private string _ClaimantIRN;
            private string _claimantLastName;
            private string _claimantMedicareCardNum;
            private List<string> _medicareClaimDetails;

            private string _medicareEligibilityStatus;

            private string _medicareError;

            private string _merchantId;

            private string _patientFirstName;

            private string _PatientIRN;

            private string _patientLastName;

            private string _patientMedicareCardNum;

            private string _payeeProviderNum;

            private string _primaryAccountNumber;

            private DateTime _referralIssueDate;

            private string _referralOverrideTypeCde;

            private string _referralPeriodTypeCde;

            private string _referringProviderName;

            private string _referringProviderNum;

            private string _requestingProviderNum;

            private DateTime _requestIssueDate;

            private string _requestOverrideTypeCde;

            private string _requestTypeCde;

            //public string InConfidenceKey
            //{
            //    get { return _inConfidenceKey; }
            //    set { if (!ReadOnly) { _inConfidenceKey = value; } }
            //}
            private string _serviceTypeCde;

            private string _servicingProviderName;

            private string _servicingProviderNum;

            private string _terminalId;

            private decimal _transactionAmount;

            private DateTime _transactionDate;

            private string _transactionId;

            public MedicareClaimResponse()
            {
                MedicareClaimDetails = new List<string>();
            }

            public string AcceptanceTypeCde
            {
                get { return _acceptanceTypeCde; }
                set { if (!ReadOnly) { _acceptanceTypeCde = value; } }
            }

            public string AccountPaidInd
            {
                get { return _accountPaidInd; }
                set { if (!ReadOnly) { _accountPaidInd = value; } }
            }

            public string AccountReferenceId
            {
                get { return _accountReferenceId; }
                set { if (!ReadOnly) { _accountReferenceId = value; } }
            }

            public string AssessmentErrorCde
            {
                get { return _assessmentErrorCde; }
                set { if (!ReadOnly) { _assessmentErrorCde = value; } }
            }

            public string ClaimantFirstName
            {
                get { return _claimantFirstName; }
                set { if (!ReadOnly) { _claimantFirstName = value; } }
            }

            public string ClaimantIRN
            {
                get { return _ClaimantIRN; }
                set { if (!ReadOnly) { _ClaimantIRN = value; } }
            }

            public string ClaimantLastName
            {
                get { return _claimantLastName; }
                set { if (!ReadOnly) { _claimantLastName = value; } }
            }

            public string ClaimantMedicareCardNum
            {
                get { return _claimantMedicareCardNum; }
                set { if (!ReadOnly) { _claimantMedicareCardNum = value; } }
            }

            public List<string> MedicareClaimDetails
            {
                get { return _medicareClaimDetails; }
                set { if (!ReadOnly) { _medicareClaimDetails = value; } }
            }

            public string[] MedicareClaimDetailsStr
            {
                get { return _medicareClaimDetails.ToArray(); }
                set { }
            }
            public string MedicareEligibilityStatus
            {
                get { return _medicareEligibilityStatus; }
                set { if (!ReadOnly) { _medicareEligibilityStatus = value; } }
            }

            public string MedicareError
            {
                get { return _medicareError; }
                set { if (!ReadOnly) { _medicareError = value; } }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string PatientFirstName
            {
                get { return _patientFirstName; }
                set { if (!ReadOnly) { _patientFirstName = value; } }
            }

            public string PatientIRN
            {
                get { return _PatientIRN; }
                set { if (!ReadOnly) { _PatientIRN = value; } }
            }

            public string PatientLastName
            {
                get { return _patientLastName; }
                set { if (!ReadOnly) { _patientLastName = value; } }
            }

            public string PatientMedicareCardNum
            {
                get { return _patientMedicareCardNum; }
                set { if (!ReadOnly) { _patientMedicareCardNum = value; } }
            }

            public string PayeeProviderNum
            {
                get { return _payeeProviderNum; }
                set { if (!ReadOnly) { _payeeProviderNum = value; } }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }
            public DateTime ReferralIssueDate
            {
                get { return _referralIssueDate; }
                set { if (!ReadOnly) { _referralIssueDate = value; } }
            }

            public string ReferralOverrideTypeCde
            {
                get { return _referralOverrideTypeCde; }
                set { _referralOverrideTypeCde = value; }
            }

            public string ReferralPeriodTypeCde
            {
                get { return _referralPeriodTypeCde; }
                set { _referralPeriodTypeCde = value; }
            }

            public string ReferringProviderName
            {
                get { return _referringProviderName; }
                set { _referringProviderName = value; }
            }

            public string ReferringProviderNum
            {
                get { return _referringProviderNum; }
                set { if (!ReadOnly) { _referringProviderNum = value; } }
            }

            public string RequestingProviderNum
            {
                get { return _requestingProviderNum; }
                set { if (!ReadOnly) { _requestingProviderNum = value; } }
            }

            public DateTime RequestIssueDate
            {
                get { return _requestIssueDate; }
                set { if (!ReadOnly) { _requestIssueDate = value; } }
            }

            public string RequestOverrideTypeCde
            {
                get { return _requestOverrideTypeCde; }
                set { if (!ReadOnly) { _requestOverrideTypeCde = value; } }
            }

            public string RequestTypeCde
            {
                get { return _requestTypeCde; }
                set { if (!ReadOnly) { _requestTypeCde = value; } }
            }

            //private string _inConfidenceKey;
            public string ServiceTypeCde
            {
                get { return _serviceTypeCde; }
                set { _serviceTypeCde = value; }
            }

            public string ServicingProviderName
            {
                get { return _servicingProviderName; }
                set { _servicingProviderName = value; }
            }

            public string ServicingProviderNum
            {
                get { return _servicingProviderNum; }
                set { _servicingProviderNum = value; }
            }
            //private string _psuedoProviderNum;

            public string TerminalId
            {
                get { return _terminalId; }
                set { _terminalId = value; }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }

            //public string PsuedoProviderNum
            //{
            //    get { return _psuedoProviderNum; }
            //    set { if (!ReadOnly) { _psuedoProviderNum = value; } }
            // }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
            public string TransactionId
            {
                get { return _transactionId; }
                set { _transactionId = value; }
            }
            public override string[] breakupLineFields(int Index)
            {
                string[] myData = new string[13];
                //        6          6          8                  2               6           5            2                       6                  4           6                 4                   6
                //data = itemNum + feeamount + dateofservice + itemOverrideCde + lspNum + equipmentId + selfDeemedCde + contribPatientAmountStr + spcId + benefitAmountStr + explanationCode + benefitAssignedAmountStr;

                try
                {
                    string fieldData = MedicareClaimDetails[Index];
                    myData[0] = fieldData.Substring(0, 6).Trim();  // itemNum
                    myData[1] = fieldData.Substring(6, 6).Trim(); // feeamount
                    myData[2] = fieldData.Substring(12, 8).Trim(); // dateofservice
                    myData[3] = fieldData.Substring(20, 2).Trim(); //itemOverrideCde
                    myData[4] = fieldData.Substring(22, 6).Trim(); //LSPN
                    myData[5] = fieldData.Substring(28, 5).Trim(); //equipmentId
                    myData[6] = fieldData.Substring(33, 2).Trim(); //selfDeemedCde
                    myData[7] = fieldData.Substring(35, 6).Trim(); //contribPatientAmountStr
                    myData[8] = fieldData.Substring(41, 4).Trim(); //spcId
                    myData[9] = fieldData.Substring(45, 6).Trim(); //benefitAmountStr
                    myData[10] = fieldData.Substring(51, 4).Trim(); //explanationCode
                    myData[11] = fieldData.Substring(55, 6).Trim(); //benefitAssignedAmountStr
                    myData[12] = fieldData.Substring(61, 6).Trim(); //Schedule Fee Amount


                }
                catch (Exception ex)
                {
                    // throw new IndexOutOfRangeException();
                }

                return myData;
            }
        }


        public class MedicareMerchantListRequest : BaseRequest
        {

        }

        public class MedicareMerchantListResponse : BaseResponse
        {

            private List<string> _medicareMerchantListDetails;

            private DateTime _transactionDate;

            public MedicareMerchantListResponse()
            {
                MedicareMerchantListDetails = new List<string>();
            }

            public List<string> MedicareMerchantListDetails
            {
                get { return _medicareMerchantListDetails; }
                set { if (!ReadOnly) { _medicareMerchantListDetails = value; } }
            }

            public string[] MedicareMerchantListDetailsStr
            {
                get { return _medicareMerchantListDetails.ToArray(); }
                set { }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
            public override string[] breakupLineFields(int Index)
            {
                string[] myData = new string[4];
                try
                {
                    string fieldData = MedicareMerchantListDetails[Index];
                    myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                    myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                    myData[2] = fieldData.Substring(23, 8).Trim(); // Provider Id
                    myData[3] = fieldData.Substring(31, 16).Trim(); //Provider Name
                }
                catch (Exception ex)
                {
                    // throw new IndexOutOfRangeException();
                }

                return myData;
            }
        }

        public class MerchantListRequest : BaseRequest
        {

        }


        public class MerchantListResponse : BaseResponse
        {

            private List<string> _merchantListDetails;

            private DateTime _transactionDate;

            public MerchantListResponse()
            {
                MerchantListDetails = new List<string>();
            }

            public List<string> MerchantListDetails
            {
                get { return _merchantListDetails; }
                set { if (!ReadOnly) { _merchantListDetails = value; } }
            }

            public string[] MerchantListDetailsStr
            {
                get { return _merchantListDetails.ToArray(); }
                set { }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
            public override string[] breakupLineFields(int Index)
            {
                string[] myData = new string[4];
                try
                {
                    string fieldData = MerchantListDetails[Index];
                    myData[0] = fieldData.Substring(0, 8).Trim();  // Terminal Id
                    myData[1] = fieldData.Substring(8, 15).Trim(); // Merchant Id
                    myData[2] = fieldData.Substring(23, 8).Trim(); // Provider Id
                    myData[3] = fieldData.Substring(31, 16).Trim(); //Provider Name
                }
                catch (Exception ex)
                {
                    // throw new IndexOutOfRangeException();
                }

                return myData;
            }
        }
        public class PrintLastReceiptRequest : BaseRequest
        {
            private bool _printReceiptOnTerminal;
            public bool PrintReceiptOnTerminal
            {
                get { return _printReceiptOnTerminal; }
                set { _printReceiptOnTerminal = value; }
            }

            private string _acquirerId;
            public string AcquirerId
            {
                get { return _acquirerId; }
                set { _acquirerId = value; }
            }

        }

        public class PrintLastReceiptResponse : ClaimResponse
        {
            private string _invoiceNumber;
            private string _rrnNumber;
            private string _subTransCode;

            public string InvoiceNumber
            {
                get { return _invoiceNumber; }
                set { if (!ReadOnly) { _invoiceNumber = value; } }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { if (!ReadOnly) { _rrnNumber = value; } }
            }

            public string SubTransCode
            {
                get { return _subTransCode; }
                set { if (!ReadOnly) { _subTransCode = value; } }
            }
        }

        public class QuoteRequest : ClaimRequest
        {
        }

        public class QuoteResponse : ClaimResponse
        {
        }
        public class QuotePharmRequest : ClaimPharmRequest
        {
        }

        public class QuotePharmResponse : ClaimPharmResponse
        {
        }
        public class RefundRequest : BaseRequest
        {
            private string _CCV;
            private int _CCVReason;
            private int _CCVSource;
            private string _expiryDate;
            // Required for Multi-Merchant
            private string _merchantId;

            // Required for Multi-Merchant
            private string _merchantPassword;

            private string _primaryAccountNumber;
            private bool _printCustomerReceipt;
            private bool _printCustomerReceiptPrompt;
            private bool _printMerchantReceipt;
            private bool _promptCardDetails = false;
            private decimal _transactionAmount;

            public string CCV
            {
                get { return _CCV; }
                set { _CCV = value; }
            }

            public int CCVReason
            {
                get { return _CCVReason; }
                set { _CCVReason = value; }
            }

            public int CCVSource
            {
                get { return _CCVSource; }
                set { _CCVSource = value; }
            }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    _expiryDate = value;
                }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { _merchantId = value; }
            }

            public string MerchantPassword
            {
                get { return _merchantPassword; }
                set { _merchantPassword = value; }
            }

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

            public bool PrintCustomerReceipt
            {
                get { return _printCustomerReceipt; }
                set { _printCustomerReceipt = value; }
            }

            public bool PrintCustomerReceiptPrompt
            {
                get { return _printCustomerReceiptPrompt; }
                set { _printCustomerReceiptPrompt = value; }
            }

            public bool PrintMerchantReceipt
            {
                get { return _printMerchantReceipt; }
                set { _printMerchantReceipt = value; }
            }

            public bool PromptCardDetails
            {
                get { return _promptCardDetails; }
                set { _promptCardDetails = value; }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }
            public override bool validateMessage(ref string validationMessage)
            {
                validationMessage = "";

                validationMessage += validateTransactionAmount(TransactionAmount, "Refund");
                validationMessage += validateMerchantId(MerchantId);
                // validationMessage += validateRefundPassword(MerchantPassword);
                return checkValidationMessage(validationMessage);
            }
        }


        public class RefundResponse : BaseResponse
        {
            private string _approvalCode;
            private string _authResponseCode;

            private string _expiryDate;

            private string _invoiceNumber;

            private string _merchantId;

            private string _primaryAccountNumber;

            private string _printReceiptData;

            private string _providerName;

            private string _rrnNumber;

            private decimal _surchargeAmount;

            private string _terminalId;

            private string _terminalSwipe;

            private decimal _transactionAmount;

            private DateTime _transactionDate;

            public string ApprovalCode
            {
                get { return _approvalCode; }
                set { if (!ReadOnly) { _approvalCode = value; } }
            }

            public string AuthResponseCode
            {
                get { return _authResponseCode; }
                set { if (!ReadOnly) { _authResponseCode = value; } }
            }
            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    if (!ReadOnly) { _expiryDate = value; }
                }
            }

            public string InvoiceNumber
            {
                get { return _invoiceNumber; }
                set { if (!ReadOnly) { _invoiceNumber = value; } }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }

            public string PrintReceiptData
            {
                get { return _printReceiptData; }
                set { if (!ReadOnly) { _printReceiptData = value; } }
            }

            public string ProviderName
            {
                get { return _providerName; }
                set { if (!ReadOnly) { _providerName = value; } }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { _rrnNumber = value; }
            }

            public decimal SurchargeAmount
            {
                get { return _surchargeAmount; }
                set { _surchargeAmount = value; }
            }

            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }

            public string TerminalSwipe
            {
                get { return _terminalSwipe; }
                set { if (!ReadOnly) { _terminalSwipe = value; } }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }

            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
        }



        public class SaleCashoutRequest : SaleRequest
        {
            private decimal _cashoutAmount;

            public decimal CashoutAmount
            {
                get { return _cashoutAmount; }
                set { if (!ReadOnly) { _cashoutAmount = value; } }
            }
        }

        public class SaleCashoutResponse : SaleResponse
        {
            private decimal _cashoutAmount;

            public SaleCashoutResponse() { }
            public decimal CashoutAmount
            {
                get { return _cashoutAmount; }
                set { _cashoutAmount = value; }
            }
        }

        public class SaleRequest : BaseRequest
        {
            private string _CCV;
            private int _CCVReason;
            private int _CCVSource;
            private string _expiryDate;
            // Required for Multi-Merchant
            private string _merchantId;

            private string _primaryAccountNumber;
            private bool _printCustomerReceipt;
            private bool _printCustomerReceiptPrompt;
            private bool _printMerchantReceipt;
            private bool _promptCardDetails = false;
            private decimal _transactionAmount;

            public SaleRequest() { }

            public string CCV
            {
                get { return _CCV; }
                set { _CCV = value; }
            }

            public int CCVReason
            {
                get { return _CCVReason; }
                set { _CCVReason = value; }
            }

            public int CCVSource
            {
                get { return _CCVSource; }
                set { _CCVSource = value; }
            }

            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    _expiryDate = value;
                }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { _merchantId = value; }
            }

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

            public bool PrintCustomerReceipt
            {
                get { return _printCustomerReceipt; }
                set { _printCustomerReceipt = value; }
            }

            public bool PrintCustomerReceiptPrompt
            {
                get { return _printCustomerReceiptPrompt; }
                set { _printCustomerReceiptPrompt = value; }
            }

            public bool PrintMerchantReceipt
            {
                get { return _printMerchantReceipt; }
                set { _printMerchantReceipt = value; }
            }

            public bool PromptCardDetails
            {
                get { return _promptCardDetails; }
                set { _promptCardDetails = value; }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }
        }


        public class SaleResponse : BaseResponse
        {
            private string _approvalCode;
            private string _authResponseCode;

            private string _expiryDate;

            private string _invoiceNumber;

            private string _merchantId;

            private string _primaryAccountNumber;

            private string _printReceiptData;

            private string _providerName;

            private string _rrnNumber;

            private decimal _surchargeAmount;

            private string _terminalId;

            private decimal _transactionAmount;

            private DateTime _transactionDate;

            public string ApprovalCode
            {
                get { return _approvalCode; }
                set { if (!ReadOnly) { _approvalCode = value; } }
            }

            public string AuthResponseCode
            {
                get { return _authResponseCode; }
                set { if (!ReadOnly) { _authResponseCode = value; } }
            }
            public string ExpiryDate
            {
                get
                {
                    return _expiryDate;
                }
                set
                {
                    if (!ReadOnly) { _expiryDate = value; }
                }
            }

            public string InvoiceNumber
            {
                get { return _invoiceNumber; }
                set { if (!ReadOnly) { _invoiceNumber = value; } }
            }

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string PrimaryAccountNumber
            {
                get
                {
                    return _primaryAccountNumber;
                }
                set
                {
                    if (!ReadOnly) { _primaryAccountNumber = value; }
                }
            }

            public string PrintReceiptData
            {
                get { return _printReceiptData; }
                set { if (!ReadOnly) { _printReceiptData = value; } }
            }

            public string ProviderName
            {
                get { return _providerName; }
                set { if (!ReadOnly) { _providerName = value; } }
            }

            public string RrnNumber
            {
                get { return _rrnNumber; }
                set { if (!ReadOnly) { _rrnNumber = value; } }
            }

            public decimal SurchargeAmount
            {
                get { return _surchargeAmount; }
                set { _surchargeAmount = value; }
            }

            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }

            public decimal TransactionAmount
            {
                get { return _transactionAmount; }
                set { _transactionAmount = value; }
            }

            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }
        }


        public class SettlementRequest : BaseRequest
        {
            private string _acquirerId;

            private string _merchantId;

            private bool _printTxnListing;

            private string _settlementType;

            public string AcquirerId
            {
                get { return _acquirerId; }
                set { _acquirerId = value; }
            }
            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { _merchantId = value; }
            }

            public bool PrintTxnListing
            {
                get { return _printTxnListing; }
                set { _printTxnListing = value; }
            }

            public string SettlementType
            {
                get { return _settlementType; }
                set { _settlementType = value; }
            }
        }

        public class SettlementResponse : BaseResponse
        {
            private string _acquirerId;
            private string _authResponseCode;

            private string _merchantId;

            private string _providerName;

            private string _terminalId;

            public string AcquirerId
            {
                get { return _acquirerId; }
                set { if (!ReadOnly) { _acquirerId = value; } }
            }

            public string AuthResponseCode
            {
                get { return _authResponseCode; }
                set { if (!ReadOnly) { _authResponseCode = value; } }
            }
            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string ProviderName
            {
                get { return _providerName; }
                set { if (!ReadOnly) { _providerName = value; } }
            }

            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }
        }

        public class StatusResponse : BaseResponse
        {
        }

        public class TerminalTestRequest : BaseRequest
        {

        }


        public class TerminalTestResponse : BaseResponse
        {

            private string _merchantId;
            private string _terminalId;

            private DateTime _transactionDate;

            public string MerchantId
            {
                get { return (_merchantId ?? "").Trim(); }
                set { if (!ReadOnly) { _merchantId = value; } }
            }

            public string TerminalId
            {
                get { return _terminalId; }
                set { if (!ReadOnly) { _terminalId = value; } }
            }
            public DateTime TransactionDate
            {
                get { return _transactionDate; }
                set { if (!ReadOnly) { _transactionDate = value; } }
            }


        }
        #endregion
        #region Delegates/Events
        private bool _loadEventFired = false;
        private bool _serverListenThreadStarted = false;
        private Thread fh;
        // Find Hicaps Terminal Thread
        //begin Observer pattern

       
   

        public event TerminalListChangedEventHandler TerminalListChanged;
     
        // JP Add null handler exceptions
        public void OnTerminalListChanged(string param)
        {
            if (TerminalListChanged != null)
            {
                // Call the Event
                try
                {
                    TerminalListChanged(param);
                }
                catch { }
            }
        }

        public delegate void StatusChangedEventHandler(string param);
        public event StatusChangedEventHandler StatusChanged;

        // JP Add null handler exceptions
        public void OnStatusChanged(string param)
        {
            if (StatusChanged != null)
            {
                // Call the Event
                try
                {
                    StatusChanged(param);
                }
                catch (Exception) { }
            }
        }

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
  


        #endregion


        public string SessionID;

        public HicapsConnectControl()
        {
            try
            {
                // Initalise Session Variable.
                SessionID = Process.GetCurrentProcess().SessionId.ToString();

                InitializeComponent();
                
                _myStatusForm = new StatusForm(this);
                _myStatusForm.Hide();

                ProcessClass.CheckAndRunController();
                //this.EnabledChanged += new EventHandler(HicapsConnectControl_EnabledChanged);

                HicapsConnectVersion.Text = HVersion.BuildVersion;
                lCopyright.Text = AssemblyCopyright;
            }
            catch { }
            try
            {
                Type[] myTypes = new Type[] { typeof(BaseMessage), 
                    typeof(BaseRequest), 
                    typeof(GetTerminalRequest),
                    typeof(GetTerminalResponse),
                    typeof(StatusResponse)               
                };
               XmlSerializer.FromTypes( myTypes );  
            }catch {}
        }


        void HicapsConnectControl_EnabledChanged(object sender, EventArgs e)
        {
            //if (this.Enabled)
            //{

            //    StartServerListenThread();
            //}
            //else
            //{
            //    ResetTerminalList();
            //}

        }

        private void ResetTerminalList()
        {
            rescanLocalTerminals = true;
            _resetTerminalListThread = true;
        }

        private void intResetTerminalList()
        {
            _resetTerminalListThread = false;
            lock (terminalIdList)
            {
                terminalIdList.Clear();
            }
            lock (terminalIdSeen)
            {
                terminalIdSeen.Clear();
            }
            lock (serverIpSeen)
            {
                serverIpSeen.Clear();
            }
            lock (localTerminalIdList)
            {
                localTerminalIdList.Clear();
            }
            lock (ignoreServerIpSeen)
            {
                ignoreServerIpSeen.Clear();
            }
        }
        /// <summary>
        /// Gets the assembly copyright.
        /// </summary>
        /// <value>The assembly copyright.</value>
        private string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return "";
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        private void HicapsConnectControl_OnLoad(object sender, EventArgs e)
        {
            StartServerListenThread();
        }

        private void InitControl()
        {
            // Make sure this only runs once.
            if (_loadEventFired)
            {
                return;
            }
            else
            {
                _loadEventFired = true;
            }
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);


            _defaultServerUrl = HicapsTools.ProcessClass.getUserConfigFileSetting("DefaultServerUrl", "").ToUpper();
            if (getStandAloneMode() || !isNetworkAvailable())
            {
                //     noNetworkEnabled = true;
            }

            string terminalOn = HicapsTools.ProcessClass.getUserConfigFileSetting("StatusBox", "true").ToLower();
            if (terminalOn == "false")
            {
                DisplayStatusWindow(false);
            }
            else
            {
                DisplayStatusWindow(true);
            }
        }

   
        void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            ResetTerminalList();
        }

    
        void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
        private bool isNetworkAvailable()
        {
            return NetworkClass.isNetworkAvailable();

        }
  

        public void StartServerListenThread()
        {
            //todo maybe this should give up after a certain amount of time ?
            // i.e the machine simply doesn't have any terminals ?????
            try
            {
                if (localTerminalIdList.Count == 0)
                {
                    UpdateLocalTerminals();
                }
            }
            catch (Exception) { }

            try
            {
                if (fh != null)
                {
                    if (fh.IsAlive) { return; }
                }
            }
            catch (Exception) { }

            InitControl();

            if (_serverListenThreadStarted == false)
            {
                _serverListenThreadStarted = true;
              

                fh = new Thread(new ThreadStart(findServers));
                fh.Name = "SearchThread";
                fh.IsBackground = true;
                fh.Start();
            }
        }

        private void UpdateLocalTerminals()
        {
            //if (!rescanLocalTerminals) return;
            //rescanLocalTerminals = false;
            string[] localTerms = getTerminals("0.0.0.0:0");

            foreach (string terminal in localTerms)
            {
                if (!string.IsNullOrEmpty(terminal) && !terminal.Contains("No Terminals"))
                {
                    string[] parts = terminal.Split(':');

                    lock (terminalIdList)
                    {
                        if (terminalIdList.ContainsKey(parts[0]))
                        {
                            terminalIdList[parts[0]] = "0.0.0.0:0:" + terminal;
                        }
                        else
                        {
                            terminalIdList.Add(parts[0], "0.0.0.0:0:" + terminal);
                            OnTerminalListChanged(parts[0]);
                        }
                    }
                    lock (terminalIdSeen)
                    {
                        if (terminalIdSeen.ContainsKey(parts[0]))
                        {
                            terminalIdSeen[parts[0]] = DateTime.Now.AddYears(1);
                        }
                        else
                        {
                            terminalIdSeen.Add(parts[0], DateTime.Now.AddYears(1)); // This always gets priority

                        }
                    }
                    lock (localTerminalIdList)
                    {
                        if (localTerminalIdList.ContainsKey(parts[0]))
                        {
                            localTerminalIdList[parts[0]] = "0.0.0.0:0:" + terminal; ;
                        }
                        else
                        {
                            localTerminalIdList.Add(parts[0], "0.0.0.0:0:" + terminal); // This always gets priority
                        }
                    }
                }
            }

        }

        private void StopServerListenThread()
        {
            _serverListenThreadStarted = false;
            try
            {
                fh.Join();
            }
            catch { }
        }

        #region API Methods

        public string[] getTerminalList()
        {
            String[] terminalList = getTerminalList(true);

            return terminalList;
        }

        #endregion

        private string[] getTerminalList(bool reCheckServers)
        {
            //  bool localTerminalFound = false;
            StartServerListenThread();

            Dictionary<string, string> updateList = new Dictionary<string, string>();
            KeyValuePair<string, string>[] myData;
            lock (terminalIdList)
            {
                myData = terminalIdList.ToArray();
            }
            foreach (var myRow in myData)
            {
                if (myRow.Value.Split(':').Count() == 2)
                {
                    string[] terminals = getTerminals(myRow.Value);

                    foreach (string myTerminal in terminals)
                    {
                        if (myTerminal.Contains(myRow.Key))
                        {
                            updateList.Add(myRow.Key, myRow.Value + ":" + myTerminal);
                        }
                    }
                }
                else
                {
                    updateList.Add(myRow.Key, myRow.Value);
                }

                // if(myRow.Value.Contains("0.0.0.0:0:"))
                //  {
                //      localTerminalFound = true;
                // }
            }
            // If we have no local terminals, do a quick recheck of local service.
            //  if (!localTerminalFound) { UpdateLocalTerminals(); }
          // if(rescanLocalTerminals){
                UpdateLocalTerminals();            
          //  }
            // update back
            foreach (var myRow in updateList)
            {
                lock (terminalIdList)
                {
                    terminalIdList[myRow.Key] = myRow.Value;
                }
            }

            return updateList.Values.ToArray();
        }
       

        // 
        /// <summary>
        /// Returns a list of Terminal Id's that sit on the Network; 
        /// </summary>
        /// <returns></returns>
        public string[] getTerminalListById()
        {

            List<string> termList = new List<string>();
            string[] terminalList = getTerminalList();
            foreach (string serverUrl in terminalList)
            {
                termList.Add(BaseMessage.extractTerminalId(serverUrl));
            }
            return termList.Distinct().ToArray();
        }

        /// <summary>
        /// Used to Test Connectivity between the PMS and the Terminal.
        /// </summary>
        /// <param name="terminal"></param
        /// >
        /// <returns></returns>
        public TerminalTestResponse sendTerminalTest(string terminal)
        {
            TerminalTestRequest myRequest = new TerminalTestRequest();
            myRequest.ServerUrl = terminal;
            return sendObject<TerminalTestRequest, TerminalTestResponse>(myRequest);
        }


        /// <summary>
        /// Requests the Terminal to perform a test Card read function.
        /// The CardReadResponse contains the information stored on the card track.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public CardReadResponse sendCardRead(CardReadRequest myRequest)
        {
            return sendObject<CardReadRequest, CardReadResponse>(myRequest);
        }
        
        public MerchantListResponse sendMerchantList(MerchantListRequest myRequest)
        {
            return sendObject<MerchantListRequest, MerchantListResponse>(myRequest);

        }

        public MedicareMerchantListResponse sendMedicareMerchantList(MedicareMerchantListRequest myRequest)
        {
            return sendObject<MedicareMerchantListRequest, MedicareMerchantListResponse>(myRequest);
        }
        /// <summary>
        /// Used to send a Settlement Transaction to the Terminal.
        /// Object Mandatory Fields: 
        /// 1.	AcquirerId Normally set to 1
        /// Optional Fields
        /// 1.	None
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public SettlementResponse sendSettlement(SettlementRequest myRequest)
        {
            return sendObject<SettlementRequest, SettlementResponse>(myRequest);
        }
        /// <summary>
        /// Returns a list of all HealthFund Card Prefixes, and a identifier which shows the maximum number of claim 
        /// lines a particular Fund allows.
        /// This information is needed to ensure you do not exceed the maximum number of lines.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public CardListResponse sendCardList(CardListRequest myRequest)
        {
            return sendObject<CardListRequest, CardListResponse>(myRequest);
        }


       

        /// <summary>
        /// Used to submit a Health Claim to the Health Fund for Processing.
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	ProviderNumberId
        /// 3.	At least one claim line must be added using the ClaimRequest.addClaimLine Method.
        /// Optional Fields
        /// 1.	None
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public ClaimResponse sendClaimRequest(ClaimRequest myRequest)
        {
            return sendObject<ClaimRequest, ClaimResponse>(myRequest);

        }

       
        /// <summary>
        /// Used to submit a Medicare Claim to Medicare.
        /// Mandatory Fields: 
        /// 1.	ClaimType
        /// 2.	ServiceTypeCde
        /// 3.	ServicingProviderNum
        /// Optional Fields
        /// 1.	Refer to Medicare Easyclaim Guide
        /// Conditional Fields
        /// 1.	Refer to Medicare Easyclaim Guide
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public MedicareClaimResponse sendMedicareClaimRequest(MedicareClaimRequest myRequest)
        {
            return sendObject<MedicareClaimRequest, MedicareClaimResponse>(myRequest);
        }

        /// <summary>
        /// Used to submit a Health Claim Quote to the Health Fund for Processing.
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	ProviderNumberId
        /// 3.	At least one claim line must be added using the ClaimRequest.addClaimLine Method.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public QuoteResponse sendQuoteRequest(QuoteRequest myRequest)
        {
            return sendObject<QuoteRequest, QuoteResponse>(myRequest);
        }

        /// <summary>
        /// Used to Cancel a Health Claim that was previously sent to the Health Fund for Processing.
        /// Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	TransactionAmount > 0
        /// 3.	RRN
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public ClaimCancelResponse sendClaimCancelRequest(ClaimCancelRequest myRequest)
        {
            return sendObject<ClaimCancelRequest, ClaimCancelResponse>(myRequest);
        }

        /// <summary>
        /// Used to send a Eftpos Sale Transaction to the Terminal
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	TransactionAmount > 0
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public SaleResponse sendSale(SaleRequest myRequest)
        {
            //if (!myRequest.PrimaryAccountNumber.isNumeric())
            //{
            //    //if (DisplayPanPrompt() || myRequest.PromptCardDetails)
            //    //{
            //    //    PanPrompt myPrompt = new PanPrompt(ref myRequest);
            //    //    DialogResult myResult = myPrompt.ShowDialog();
            //    //    if (myResult == DialogResult.Cancel)
            //    //    {
            //    //        SaleResponse myResponse = new SaleResponse();
            //    //        myResponse.ResponseCode = "TC";
            //    //        myResponse.ResponseText = "TRANSACTION CANCELLED";
            //    //        myResponse.TransactionAmount = myRequest.TransactionAmount;
            //    //        myResponse.ReadOnly = true;
            //    //        return myResponse;
            //    //    }
            //    //}
            //}
            return sendObject<SaleRequest, SaleResponse>(myRequest);
        }

        /// <summary>
        /// Returns a list of all Eftpos Transactions that have been submitted for Today or Yesterday.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public EftposDepositResponse sendEftposDeposit(EftposDepositRequest myRequest)
        {

            string strMRPwd = "";
            strMRPwd = RefundPwd.ShowBox("", "");
            if (strMRPwd == null)
            {
                EftposDepositResponse myResponse = new EftposDepositResponse();
                myResponse.ResponseCode = "TC";
                myResponse.ResponseText = "TRANSACTION CANCELLED";
                myResponse.TransactionAmount = myRequest.TransactionAmount;
                myResponse.ReadOnly = true;
                return myResponse;
            }

            if (strMRPwd != "")
                strMRPwd = string.Format("{0:d6}", Convert.ToInt32(strMRPwd));

            myRequest.MerchantPassword = strMRPwd;
            myRequest.ServerUrl = getDefaultServer(myRequest.ServerUrl);
            return sendObject<EftposDepositRequest, EftposDepositResponse>(myRequest);
        }


          
        /// <summary>
        /// Used to send a Eftpos Refund to refund money back to the card holder.  
        /// A popup window will appear requiring the user to insert in the Merchant Password.
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	TransactionAmount > 0
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public RefundResponse sendRefund(RefundRequest myRequest)
        {

            string strMRPwd = "";
            strMRPwd = RefundPwd.ShowBox("", "");
            if (strMRPwd == null)
            {
                RefundResponse myResponse = new RefundResponse();
                myResponse.ResponseCode = "TC";
                myResponse.ResponseText = "TRANSACTION CANCELLED";
                myResponse.TransactionAmount = myRequest.TransactionAmount;
                myResponse.ReadOnly = true;
                return myResponse;
            }
            if (strMRPwd != "")
                strMRPwd = string.Format("{0:d6}", Convert.ToInt32(strMRPwd));

            myRequest.MerchantPassword = strMRPwd;
            myRequest.ServerUrl = getDefaultServer(myRequest.ServerUrl);

            if (!myRequest.PrimaryAccountNumber.isNumeric())
            {
                if (DisplayPanPrompt() || myRequest.PromptCardDetails)
                {
                    PanPrompt myPrompt = new PanPrompt(ref myRequest);
                    DialogResult myResult = myPrompt.ShowDialog();
                    if (myResult == DialogResult.Cancel)
                    {
                        RefundResponse myResponse = new RefundResponse();
                        myResponse.ResponseCode = "TC";
                        myResponse.ResponseText = "TRANSACTION CANCELLED";
                        myResponse.TransactionAmount = myRequest.TransactionAmount;
                        myResponse.ReadOnly = true;
                        return myResponse;
                    }
                }
            }
            return sendObject<RefundRequest, RefundResponse>(myRequest);
        }

        
        /// <summary>
        /// Used to send a combined Sale and Cashout Transaction to the Terminal
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	TransactionAmount > 0
        /// 3.	CashoutAmount > 0
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public SaleCashoutResponse sendSaleCashout(SaleCashoutRequest myRequest)
        {
            return sendObject<SaleCashoutRequest, SaleCashoutResponse>(myRequest);
        }

        /// <summary>
        /// Sends a Eftpos Cashout request to the terminal.
        /// Note.  Cashout Request on a Credit Card is NOT allowed.
        /// Object Mandatory Fields: 1.	MerchantId
        ///                          2.	CashoutAmount > 0
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>

        public CashoutResponse sendCashout(CashoutRequest myRequest)
        {
            return sendObject<CashoutRequest, CashoutResponse>(myRequest);

        }
     



       

  
        private static T Deserialize<T>(string xmlData)
            where T : BaseMessage
        {

            using (MemoryStream myResultMS = new MemoryStream(Encoding.Default.GetBytes(xmlData)))
            {

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(myResultMS);

                ////myList.AddRange((typeof(T).GetProperties().AsEnumerable()) );
                //DataContractSerializer dSerializer = new DataContractSerializer(typeof(T));
                //return (T)dSerializer.ReadObject(XmlDictionaryReader.CreateTextReader(myResultMS, new XmlDictionaryReaderQuotas()), false);

            }
        }

    
       
        
        private static string Serialize<T>(T myBo)
            where T : BaseMessage
        {
            using (MemoryStream myMS = new MemoryStream())
            {

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(myMS, myBo);
                return Encoding.Default.GetString(myMS.ToArray());
                //DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                //serializer.WriteObject(myMS, myBo);
                //return Encoding.Default.GetString(myMS.ToArray());
            }
        }

   
        /* Generic Method */
        /// <summary>
        /// Generic method to serialise/deserialise objects for message sending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        /// 
        private U sendObject<T, U>(T myRequest)
            where T : BaseRequest
            where U : BaseResponse, new()
        {
            string xmlRequest = "", xmlResponse = "";
            string errMessage = "";
            U myResult;
            bool serverURLCheck = true;

            SetLastAcquirer<T>();

            if (string.IsNullOrEmpty(myRequest.ServerUrl))
            {
                myRequest.ServerUrl = "";
            }
            else
            {
                myRequest.ServerUrl = myRequest.ServerUrl.Trim();
            }
            try
            {
                // Fix serialiser error if user has managed to delete temp directory
                if (!Directory.Exists(Path.GetTempPath()))
                {
                    Directory.CreateDirectory(Path.GetTempPath());
                }

            }
            catch { }
            // Always force TerminalId Mode...
            string fallbackURL = "";
            if ((myRequest.ServerUrl ?? "").Length > 6)
            {
                string[] parts = myRequest.ServerUrl.Split(':');
                if (parts.Length > 3)
                {
                    fallbackURL = myRequest.ServerUrl;
                    myRequest.ServerUrl = parts[2];
                }

            }
            if ((myRequest.ServerUrl ?? "").Length == 6)
            {
                string serverURL = getServerUrlFromTerminalId(myRequest.ServerUrl);
                if (string.IsNullOrEmpty(serverURL))
                {
                    Thread.Sleep(3000);
                    Application.DoEvents();
                    getTerminalListById();
                    serverURL = getServerUrlFromTerminalId(myRequest.ServerUrl);
                }
                if (string.IsNullOrEmpty(serverURL) && !string.IsNullOrEmpty(fallbackURL))
                {
                    serverURL = fallbackURL;
                }

                if (string.IsNullOrEmpty(serverURL) && string.IsNullOrEmpty(fallbackURL))
                {

                    serverURLCheck = false;
                    errMessage = "Could not find Terminal on Network";
                    xmlResponse = returnNetworkErrorXml("FC", errMessage, typeof(U).Name.ToString());
                }
                myRequest.ServerUrl = serverURL;
            }
            // New Default Terminal Code hList
            if (string.IsNullOrEmpty(myRequest.ServerUrl))
            {
                string defaultServerURL = getDefaultServer(myRequest.ServerUrl);
                string terminalId = BaseMessage.extractTerminalId(defaultServerURL);
                if (terminalId.Trim().Length > 0)
                {
                    defaultServerURL = getServerUrlFromTerminalId(terminalId);
                    if (string.IsNullOrEmpty(defaultServerURL))
                    {
                        Thread.Sleep(3000);
                        Application.DoEvents();
                        getTerminalListById();
                        defaultServerURL = getServerUrlFromTerminalId(terminalId);
                    }
                    myRequest.ServerUrl = defaultServerURL;
                }
            }
            if (string.IsNullOrEmpty(myRequest.ServerUrl))
            {
                serverURLCheck = false;
                errMessage = "Could not find Terminal on Network";
                xmlResponse = returnNetworkErrorXml("FC", errMessage, typeof(U).Name.ToString());
            }

            // Old code
            //myRequest.ServerUrl = getDefaultServer(myRequest.ServerUrl);
            myRequest.ComputerName = SystemInformation.ComputerName;
            myRequest.ClientVersion = HVersion.BuildVersion;
            // Lets run some Object Checks.

            xmlRequest = Serialize<T>(myRequest);
        //    File.WriteAllText("C:\\HicapsConnect\\XML\\" + typeof(T).ToString(),xmlRequest);
            //xmlRequest = myBo.generateXmlRequest(terminal);
            if (!myRequest.validateMessage(ref errMessage))
            {
                xmlResponse = returnNetworkErrorXml("FC", errMessage, typeof(U).Name.ToString());
            }
            else if (serverURLCheck)
            {

                //// Multi-Threading Code... In the end didn't really achieve anything
                ////MessageProtocol.ServerUrl = myRequest.ServerUrl;
                ////MessageProtocol.XmlRequest = xmlRequest;

                ////Thread newThread = new Thread(new ThreadStart(MessageProtocol.SendMessage));
                ////newThread.IsBackground = true;
                ////newThread.Start();
                ////while (newThread.IsAlive)
                ////{
                ////    Thread.Sleep(500);
                ////}
                ////xmlResponse = MessageProtocol.XmlResponse;
                try
                {
                    // Change this from 0.0.0.0 to :0: to support sending to Ip terminals through local service.
                    if (!myRequest.ServerUrl.Contains(":0:"))
                    {
                        xmlResponse = SendMessage(myRequest.ServerUrl, xmlRequest, 90000);
                    }
                    else
                    {
                        xmlResponse = SendMessageLocal(myRequest.ServerUrl, xmlRequest, 90000);
                    }
                }
                catch (Exception)
                {
                    xmlResponse = returnNetworkErrorXml("FC", STR_NetworkRequestTimedOut, typeof(U).Name.ToString());
                }
            }
            StatusWindowClose();
            if (string.IsNullOrEmpty(xmlResponse)) { xmlResponse = string.Empty; }
            xmlResponse = xmlResponse.Replace("&#x1;", "");
            xmlResponse = xmlResponse.Replace("&#x0;", "");
            xmlResponse = xmlResponse.Replace("&#x", "");
            xmlResponse = xmlResponse.Replace("\0", "");

            try
            {
                //  serializer = new DataContractSerializer(typeof(U));
                //  MemoryStream myResultMS = new MemoryStream(Encoding.Default.GetBytes(xmlResponse));
                ////  myResult = (U)serializer.Deserialize(myResultMS);
                //    myResult = (U)serializer.ReadObject(XmlDictionaryReader.CreateTextReader(myResultMS, new XmlDictionaryReaderQuotas()),false);
                myResult = Deserialize<U>(xmlResponse);
            }
            catch (Exception)
            {
                myResult = new U();
                myResult.ResponseCode = "EN";
                myResult.ResponseText = STR_NetworkRequestTimedOut;
            }
        //    File.WriteAllText("C:\\HicapsConnect\\XML\\" + typeof(U).ToString(), xmlResponse);
            // Protect the response object from changes.
            myResult.ReadOnly = true;
            StatusWindowClose();

            return myResult;
        }

        private void SetLastAcquirer<T>() where T : BaseRequest
        {
            if (typeof (T) == typeof (SaleRequest) ||
                typeof (T) == typeof (CashoutRequest) ||
                typeof (T) == typeof (SaleCashoutRequest) ||
                typeof (T) == typeof (RefundRequest) ||
                typeof (T) == typeof (EftposDepositRequest)
                )
            {
                LastAcquirerID = NABAcquirer;
            }

            if (typeof (T) == typeof (ClaimRequest) ||
                typeof (T) == typeof (QuoteRequest) ||
                typeof (T) == typeof (ClaimPharmRequest) ||
                typeof (T) == typeof (ClaimCancelRequest)
                )
            {
                LastAcquirerID = HICAPSAcquirer;
            }

            if (typeof (T) == typeof (MedicareClaimRequest))
            {
                LastAcquirerID = MedicareAcquirer;
            }
        }

        public EftposTransListingResponse sendEftposTransListing(EftposTransListingRequest myRequest)
        {
            /* New Generic Methods */
            return sendObject<EftposTransListingRequest, EftposTransListingResponse>(myRequest);
        }

      
        /// <summary>
        /// Returns a list of all Hicaps Transaction Submitted for Processing
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public HicapsTransListingResponse sendHicapsTransListing(HicapsTransListingRequest myRequest)
        {
            /* New Generic Methods */
            return sendObject<HicapsTransListingRequest, HicapsTransListingResponse>(myRequest);
        }

        public GetTerminalResponse sendGetTerminals(GetTerminalRequest myRequest)
        {
            return sendObject<GetTerminalRequest, GetTerminalResponse>(myRequest);
        }

        /// <summary>
        /// Returns a list of the settlement Totals for all Eftpos and HIcaps transactions that have been submitted.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public HicapsTotalsResponse sendHicapsTotals(HicapsTotalsRequest myRequest)
        {
            return sendObject<HicapsTotalsRequest, HicapsTotalsResponse>(myRequest);

        }

        /// <summary>
        /// Re-Prints the last receipt that was printed from the terminal.  If the PrintReceiptOnTerminal property 
        /// is set to False, the data from the previous transaction is returned instead of printing.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public PrintLastReceiptResponse sendPrintLastReceipt(PrintLastReceiptRequest myRequest)
        {
            myRequest.AcquirerId = LastAcquirerID ?? HICAPSAcquirer;
            return sendObject<PrintLastReceiptRequest, PrintLastReceiptResponse>(myRequest);
        }

       

        /// <summary>
        /// Returns a list of all Merchants stored inside the terminals Memory.
        /// This information is used to set the MerchantId fields for Eftpos and Claim Transactions.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public AllMerchantListResponse sendAllMerchantList(AllMerchantListRequest myRequest)
        {
            return sendObject<AllMerchantListRequest, AllMerchantListResponse>(myRequest);
        }

        /// <summary>
        /// Returns a list of all Providers stored inside the terminals Memory.
        /// 
        /// This information is used to set the ProviderNumber information for the processing of Claims.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public AllProviderListResponse sendAllProviderList(AllProviderListRequest myRequest)
        {
            return sendObject<AllProviderListRequest, AllProviderListResponse>(myRequest);
        }

        private static string returnNetworkErrorXml(string errorCode, string errorMessage, string classname)
        {
            string xmlResponse;
            BaseResponse myResponse = new BaseResponse();
            myResponse.ResponseCode = errorCode;
            myResponse.ResponseText = errorMessage;
            myResponse.ResponseTime = DateTime.Now;

            xmlResponse = Serialize<BaseResponse>(myResponse);

            // Make Xml match correct request type
            xmlResponse = xmlResponse.Replace("BaseResponse", classname.Replace("Request", "Response"));
            return xmlResponse;
        }

        public string[] toArray(List<string> myVar)
        {
            return myVar.ToArray();
        }

        public AllTransCodeListResponse sendAllTransCodeListResponse(AllTransCodeListRequest myRequest)
        {
            return sendObject<AllTransCodeListRequest, AllTransCodeListResponse>(myRequest);
        }
        /// <summary>
        /// Used to submit a Health Claim Pharmacy to the Health Fund for Processing.
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	ProviderNumberId
        /// 3.	At least one claim line must be added using the ClaimRequest.addClaimLine Method.
        /// Optional Fields
        /// 1.	None
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public ClaimPharmResponse sendClaimPharmRequest(ClaimPharmRequest myRequest)
        {
            return sendObject<ClaimPharmRequest, ClaimPharmResponse>(myRequest);

        }

        /// <summary>
        /// Used to submit a Health Claim Pharmacy Quote to the Health Fund for Processing.
        /// Object Mandatory Fields: 
        /// 1.	MerchantId
        /// 2.	ProviderNumberId
        /// 3.	At least one claim line must be added using the ClaimRequest.addClaimLine Method.
        /// </summary>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public QuotePharmResponse sendQuotePharmRequest(QuotePharmRequest myRequest)
        {
            return sendObject<QuotePharmRequest, QuotePharmResponse>(myRequest);
        }

        /// <summary>
        /// Returns the version of HICAPS Connect
        /// </summary>
        /// <returns></returns>
        public string getVersion()
        {
            return HVersion.BuildVersion;
        }
        public AllItemListResponse sendAllItemList(AllItemListRequest myRequest)
        {
            // Force Item list to go through local service to IP endpoint.
            //myRequest.ServerUrl = "0:0:0:0::11000:XXXXXX:COMIP";
            return sendObject<AllItemListRequest, AllItemListResponse>(myRequest);
        }
        public AllPharmItemListResponse sendAllPharmItemList(AllPharmItemListRequest myRequest)
        {
            // Force Item list to go through local service to IP endpoint.
            //myRequest.ServerUrl = "0:0:0:0::11000:XXXXXX:COMIP";
            return sendObject<AllPharmItemListRequest, AllPharmItemListResponse>(myRequest);
        }
        public AllItemResponseCodeListResponse sendAllItemResponseCodeList(AllItemResponseCodeListRequest myRequest)
        {
            return sendObject<AllItemResponseCodeListRequest, AllItemResponseCodeListResponse>(myRequest);
        }

        delegate void SetTextCallback(string text);

        internal void SetStatusText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.lStatus.InvokeRequired)
            {
                //SetTextCallback d = new SetTextCallback(SetStatusText);
                //this.Invoke(d, new object[] { text });
            }
            else
            {
                this.lStatus.Text = text;
                this.lStatus.Refresh();
                // Thread.Sleep(100);
            }
        }


        static void Log(string message)
        {

            // Console.WriteLine(message);
            // Do nothing at the moment
        }

        private static void OpenDevelopmentWindow()
        {
            // Compiler flag
#if (DEVELOPMENT)
            DevelopmentLicence myForm = new DevelopmentLicence();
            myForm.ShowDialog();
#endif
        }


        private bool DisplayPanPrompt()
        {
            try
            {

                string displayPopup = HicapsTools.ProcessClass.getUserConfigFileSetting("ManualPanPopup", "false");
                if (displayPopup == "true")
                {
                    return true;
                }
            }
            catch  { }
            return false;
        }

        // Updated from 1.0.3.32
        private void findServers()
        {
            DateTime seenTerminalTime;
            Socket remoteHosts;
            IPEndPoint iep;
            EndPoint ep;
            try
            {
                remoteHosts = new Socket(AddressFamily.InterNetwork,
                          SocketType.Dgram, ProtocolType.Udp);
                remoteHosts.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                iep = new IPEndPoint(IPAddress.Any, UPD_ListenPort);
                ep = (EndPoint)iep;
                remoteHosts.Bind(iep);
                remoteHosts.ReceiveTimeout = 2000;
            }
            catch  { _serverListenThreadStarted = false; return; }
            string remoteIp = "";
            //lock (serverList)
            //{
            //    serverList.Clear();
            //    // Get any Local Standalone terminals.
            //    serverList.Add("0.0.0.0:0");
            //    //Thread.Sleep(200);
            //}
            //OnTerminalListChanged("0.0.0.0:0");
            string NetworkName = HicapsTools.ProcessClass.getUserConfigFileSetting("NetworkName", "");
            NetworkName = NetworkName.CalculateMD5();
            while (_serverListenThreadStarted)
            {
                //Log("Looping");
                if (_resetTerminalListThread)
                {
                    NetworkName = HicapsTools.ProcessClass.getUserConfigFileSetting("NetworkName", "");
                    NetworkName = NetworkName.CalculateMD5();

                    intResetTerminalList();
                }
                byte[] data = new byte[1024];
                int recv = 0;
                try
                {
                    recv = remoteHosts.ReceiveFrom(data, ref ep);
                    if (ep.AddressFamily == AddressFamily.InterNetwork)
                    {
                        remoteIp = ep.ToString();
                    }
                    else
                    {
                        remoteIp = "";
                    }

                }
                catch
                {
                    recv = 0;
                    Thread.Sleep(1000);
                }
                string stringData = Encoding.Default.GetString(data, 0, recv);
                string entry = stringData + "+" + ep.ToString();
                string[] fields = entry.Split('+');
                bool fireChangedEvent = false;

                // If version 1.0.0.87+
                if (_resetTerminalListThread)
                {
                    NetworkName = HicapsTools.ProcessClass.getUserConfigFileSetting("NetworkName", "");
                    NetworkName = NetworkName.CalculateMD5();
                    intResetTerminalList();
                }

                // Check that Terminal is on the same Network.
                if (!string.IsNullOrEmpty(NetworkName))
                {
                    if (fields.Length >= 12 && !string.IsNullOrEmpty(fields[11]))
                    {
                        // Okay if network name is different ignore terminal
                        if (fields[11] != NetworkName) { fields = new string[0]; }
                    }
                    else
                    {
                        // Network name has been set, but either older version of HICAPSConnect, or NetworkName is not set on a
                        //remote terminal but, on this client session it is set.
                        fields = new string[0];
                    }
                }
                if (string.IsNullOrEmpty(NetworkName))
                {
                    if (fields.Length >= 12 && !string.IsNullOrEmpty(fields[11]))
                    {
                        // Okay This terminal has a Network Name, but we are not looking for them
                        // So ignore this terminal
                        fields = new string[0];
                        //if (fields[11] != NetworkName) { fields = new string[0]; }
                    }
                }
                if (fields.Count() >= 10)
                {
                    string[] fieldTerminals = fields[9].Split(':');


                    try
                    {
                        seenTerminalTime = DateTime.ParseExact(fields[5], "yyyyMMddHHmmssff", CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        seenTerminalTime = DateTime.Now;
                    }
                    if (fields[7].Contains('~'))
                    {
                        // New 1,0,3,5+ or 1,0,2,88+ Application
                        fieldTerminals = fields[7].Split(':');
                    }

                    foreach (string terminalField in fieldTerminals)
                    {
                        string terminal = "";
                        string commPort = "";

                        // This first check is to resolve the issue where
                        // two servers are broadcasting that they have the same terminal.
                        // We check our Dictionary to compare the times in which the server
                        // last saw the terminal.  Only the latest one will be used.
                        if (!string.IsNullOrEmpty(terminalField))
                        {
                            if (terminalField.Contains('~'))
                            {
                                string[] dataX = terminalField.Split('~');
                                terminal = dataX[0];
                                if (dataX.Count() > 1)
                                {
                                    commPort = dataX[1];
                                }
                                else
                                {
                                    commPort = "";
                                }
                            }
                            else
                            {
                                // Legacy Client Protocol 
                                terminal = terminalField;
                                commPort = "";
                            }

                            // This gives me a list of all the times I have gotten a udp
                            // packet from the server 
                            // Override ip in fields[3] with remoteIp from endpoint
                            // Removed 19/05/2014.  If server specifies a IP, this overrides it causing in correct information

                            //if (fields[3].Contains(':') && fields[3].Contains('.') && remoteIp.Contains(':') && remoteIp.Contains('.'))
                            //{
                            //    string[] subFields1 = fields[3].Split(':');
                            //    string[] subFields2 = remoteIp.Split(':');
                            //    if (subFields1[0] != subFields2[0] && subFields1.Length == 2)
                            //    {
                            //        fields[3] = subFields2[0] + ':' + subFields1[1];
                            //    }
                            //}
                            if (serverIpSeen != null)
                            {
                                lock (serverIpSeen)
                                {
                                    if (!serverIpSeen.ContainsKey(fields[3]))
                                    {
                                        serverIpSeen.Add(fields[3], DateTime.Now);
                                    }
                                    else
                                    {
                                        serverIpSeen[fields[3]] = DateTime.Now;
                                    }
                                }
                            }
                            if (terminalIdSeen != null && terminalIdList != null)
                            {
                                lock (terminalIdSeen)
                                {
                                    if (terminalIdSeen.ContainsKey(terminal))
                                    {
                                        // termSeenDate is the DateTime the terminal was last seen
                                        DateTime termSeenDate = terminalIdSeen[terminal];
                                        if (termSeenDate < seenTerminalTime)
                                        {
                                            // the terminal time is earlier than the one on this server
                                            // so remove from list
                                            terminalIdSeen.Remove(terminal);
                                            // and remove from our internal server list.
                                            lock (terminalIdList)
                                            {
                                                if (!terminalIdList.ContainsKey(terminal))
                                                {
                                                    terminalIdList.Remove(terminal);
                                                }
                                            }
                                        }
                                    }

                                    if (!terminalIdSeen.ContainsKey(terminal))
                                    {
                                        terminalIdSeen.Add(terminal, DateTime.ParseExact(fields[5], "yyyyMMddHHmmssff", CultureInfo.InvariantCulture));
                                    }
                                }
                            }
                            // Check for new terminal
                            if (terminalIdList != null)
                            {
                                lock (terminalIdList)
                                {
                                    if (!terminalIdList.ContainsKey(terminal))
                                    {
                                        if (!string.IsNullOrEmpty(commPort))
                                        {
                                            // New Code.
                                            terminalIdList.Add(terminal, fields[3] + ":" + terminal + ":" + commPort);
                                        }
                                        else
                                        {
                                            terminalIdList.Add(terminal, fields[3]);
                                        }
                                        fireChangedEvent = true;
                                    }
                                }
                            }
                        }
                        if (fireChangedEvent) { OnTerminalListChanged(terminal); }
                        Thread.Sleep(100);
                    }

                }



               
                Thread.Sleep(100);

                // Code to remove out of teminal lists, servers that are turned off etc.
                try
                {
                    KeyValuePair<string, DateTime>[] myServers;
                    lock (serverIpSeen)
                    {
                        myServers = serverIpSeen.ToArray();
                    }

                    foreach (var myServer in myServers)
                    {
                        if (myServer.Value < DateTime.Now.AddMinutes(-1))
                        {
                            try
                            {
                                lock (serverIpSeen)
                                {
                                    serverIpSeen.Remove(myServer.Key);
                                }
                            }
                            catch (Exception) { }
                            // Have not seen server for a minute,
                            List<string> deleteTerminal = new List<string>();
                            KeyValuePair<string, string>[] myTerminals;
                            lock (terminalIdList)
                            {
                                myTerminals = terminalIdList.ToArray();
                            }
                            try
                            {
                                // Get a delete list for all the terminals on that server
                                foreach (var myTerminal in myTerminals)
                                {
                                    if (myTerminal.Value.Contains(myServer.Key))
                                    {
                                        deleteTerminal.Add(myTerminal.Key);
                                    }
                                }
                            }
                            catch (Exception) { }
                            try
                            {
                                foreach (var deleteTermId in deleteTerminal)
                                {
                                    lock (terminalIdList)
                                    {
                                        terminalIdList.Remove(deleteTermId);
                                    }
                                    lock (terminalIdSeen)
                                    {
                                        terminalIdSeen.Remove(deleteTermId);
                                    }
                                }
                            }
                            catch (Exception) { }

                        }
                    }
                }
                catch (Exception) { }

            }

            _serverListenThreadStarted = false;
        }

        /// <summary>
        /// This method will look through the current list of found terminals
        /// searching for the specific Terminal id
        /// 
        /// If it finds one, then it will return the appropriate terminal id
        /// 
        /// if it cannout find it, then it will return empty string.
        /// </summary>
        /// <param name="terminalId"></param>
        /// <returns></returns>
        private string getServerUrlFromTerminalId(string terminalId)
        {
            if (terminalId == "XXXXXX")
            {
                return "0.0.0.0:0:" + terminalId + ":COM?";
            }
            if (terminalIdList.ContainsKey(terminalId))
            {
                return terminalIdList[terminalId];
            }


            return "";
        }

        private string[] getTerminals(string server)
        {
            string terminalList = "";
            //Log(server);
            try
            {
                lock (ignoreServerIpSeen)
                {
                    if (ignoreServerIpSeen.ContainsKey(server))
                    {
                        if (ignoreServerIpSeen[server] < DateTime.Now.AddSeconds(-60))
                        {
                            ignoreServerIpSeen.Remove(server);
                        }
                    }
                }
                if (ignoreServerIpSeen.ContainsKey(server))
                {
                    Log("Ignoring " + server + "(Not Accessible)");
                }
                else
                {
                    Log(server);
                    bool tmpFlag = showStatusWindow;
                    showStatusWindow = false;

                    GetTerminalRequest myRequest = new GetTerminalRequest();
                    string xmlRequest = Serialize<GetTerminalRequest>(myRequest);

                    string xmlResponse = "";
                    if (server.Contains("0.0.0.0"))
                    {
                        xmlResponse = SendMessageLocal(server, xmlRequest, 2000);
                    }
                    else
                    {
                        xmlResponse = SendMessage(server, xmlRequest, 2000);
                    }
                    showStatusWindow = tmpFlag;
                    if (xmlResponse != null && xmlResponse != "")
                    {

                        GetTerminalResponse myResult = Deserialize<GetTerminalResponse>(xmlResponse);
                        if (myResult.ResponseCode == "ED")
                        {
                            ignoreServerIpSeen.Add(server, DateTime.Now);
                        }
                        return myResult.TerminalList.ToArray();
                    }
                    else
                    {
                        ignoreServerIpSeen.Add(server, DateTime.Now);
                        return terminalList.Split('+');
                    }

                }

            }
            catch (Exception)
            {

                return terminalList.Split('+');
            }
            return terminalList.Split('+');

        }




        private bool IsValidXML(string value)
        {
            try
            {
                // Check we actually have a value
                if (string.IsNullOrEmpty(value) == false)
                {
                    // Try to load the value into a document
                    XmlDocument xmlDoc = new XmlDocument();

                    xmlDoc.LoadXml(value);

                    // If we managed with no exception then this is valid XML!
                    return true;
                }
                else
                {
                    // A blank value is not valid xml
                    return false;
                }
            }
            catch (System.Xml.XmlException)
            {
                return false;
            }
        }

        void MessageProtocol_CompletedMessage()
        {
            //throw new NotImplementedException();
        }

        void MessageProtocol_StatusMessage(string param)
        {

            OnStatusChanged(param);
        }


        private string SendMessage(string server, string XmlRequest, int timeout)
        {
            string XmlResponse = "";
            string clearXmlRequest = XmlRequest;
            int serverConnectTimeout = 5000; // 5 Seconds
            OpenDevelopmentWindow();


            try
            {
                string[] serveripandport;
                serveripandport = server.Split(':');
                string ip = serveripandport[0];
                int port = System.Convert.ToInt32(serveripandport[1]);

                //TcpClient client = new TcpClient(ip, port);
                connectDone.Reset();
                TcpClient client = new TcpClient();
                try
                {
                    setStatus(STR_Connecting + ":\r\n" + ip);
                    client.BeginConnect(ip, port, new AsyncCallback(ConnectCallback), client); // here the clients starts to connect asynchronously (because it is asynchronous it doesn't have a timeout only events are triggered)
                    // immediately after calling beginconnect the control is returned to the method, now we need to wait for the event or connection timeout
                    connectDone.WaitOne(serverConnectTimeout, false); // now application waits here for either an connection event (fi. connection accepted) or the timeout specified by connectionTimeout (in milliseconds)

                    if (!client.Connected) { throw new SocketException(10061); } /*10061 = Could not connect to server */

                    StatusWindowOpen();
                    if (serveripandport.Length >= 4)
                    {
                        if (serveripandport[3] == "COMX")
                        {
                            StatusWindowNonModal();
                        }
                    }
                    client.SendTimeout = timeout;
                    client.ReceiveTimeout = timeout;
                    // client.NoDelay = true;
                    client.ReceiveBufferSize = 32768;
                    // First 20 bytes = length in Ascii.
                    //string messageSize = "00000000000000000000" + XmlRequest.Length;
                    //messageSize = messageSize.Substring(messageSize.Length - 20);
                    //XmlRequest = messageSize + XmlRequest;
                    var myProtocol = new HicapsConnectProtocol(HicapsTools.ProcessClass.getUserConfigFileSetting("NetworkName", ""));
                    XmlRequest = myProtocol.encodeXmlMessage(XmlRequest, true);
                    setStatus(STR_Sending + ":\r\n" + ip);
                    Byte[] request = Encoding.Default.GetBytes(XmlRequest);
                    client.GetStream().Write(request, 0, request.Length);
                    client.GetStream().Flush();

                    byte[] xmlLength;
                    string messageLength;
                    int lenbytesRead;
                    bool messageLoop = false;
                    bool statusLoop = true;
                    do
                    {
                        // Big Status Loop.
                        do
                        {
                            // Inner Ack Loop, maybe we can get rid off.
                            xmlLength = new byte[20];
                            messageLength = "";
                            lenbytesRead = client.GetStream().Read(xmlLength, 0, 20);//s.Receive(incomingBuffer);
                            messageLength = Encoding.Default.GetString(xmlLength);
                            // Log("Message size is " + messageLength);
                            switch (messageLength)
                            {
                                case STR_ACK00000000000000000:
                                    setStatus(STR_AckReceived + ":\r\n" + ip);
                                    messageLoop = true;
                                    break;
                                default:

                                    //setStatus(STR_Reading);
                                    messageLoop = false; break;
                            }
                        } while (messageLoop && client.Connected);
                        int xmlMessageSize = Convert.ToInt32(messageLength);

                        //Byte[] response = new Byte[xmlMessageSize];
                        // int bytesRead = client.GetStream().Read(response, 0, xmlMessageSize);
                        //byte[] resultArray = new byte[bytesRead];
                        // Array.Copy(response, resultArray, bytesRead);
                        //XmlResponse = Encoding.Default.GetString(response).Trim();
                        List<byte> dataBuffer = new List<byte>();
                        int bytesReadSoFar = 0;
                        do
                        {
                            Byte[] response = new Byte[xmlMessageSize];
                            int bytesRead = client.GetStream().Read(response, 0, xmlMessageSize);
                            bytesReadSoFar += bytesRead;
                            byte[] resultArray = new byte[bytesRead];
                            Array.Copy(response, resultArray, bytesRead);
                            dataBuffer.AddRange(resultArray);
                        } while (bytesReadSoFar < xmlMessageSize);

                        XmlResponse = Encoding.Default.GetString(dataBuffer.ToArray(), 0, dataBuffer.Count).Trim();

                        // Log("Received response: " + XmlResponse);
                        if (XmlResponse.IndexOf('<') < 0)
                        {
                            //     Log("Message was encrypted");
                            XmlResponse = XmlResponse.Replace("\0", "");
                            XmlResponse = myProtocol.decodeXmlMessage(XmlResponse, true);

                        }
                        if (XmlResponse.IndexOf("<StatusResponse") > 0)
                        {
                            //Do something with the status message....
                            try
                            {
                                StatusResponse myResult = Deserialize<StatusResponse>(XmlResponse);
                     //           File.WriteAllText("C:\\HicapsConnect\\XML\\" + typeof(StatusResponse).ToString(), XmlResponse);
                                if (myResult.ResponseText == STR_AckReceived)
                                {
                                    setStatus(myResult.ResponseText + ":\r\n" + ip);
                                }
                                else
                                {
                                    setStatus(myResult.ResponseText);
                                }
                                statusLoop = true;
                            }
                            catch (Exception)
                            {
                                statusLoop = false;
                            }
                        }
                        else
                        {
                            statusLoop = false;
                        }

                    } while (client.Connected && statusLoop);
                    client.Close();
                }
                catch (SocketException)
                {

                    string className = BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("ED", String.Format(STR_DestinationErrorCouldNotConnectToServer, server), className);
                }
                catch (TimeoutException)
                {
                    string className = BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("EN", String.Format(STR_NetworkRequestTimedOut, server), className);
                }
                catch (IOException)
                {
                    string className = BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("EN", String.Format(STR_NetworkError, server), className);
                }
                catch (Exception)
                {
                    string className = BaseMessage.extractClassName(clearXmlRequest);
                    XmlResponse = returnNetworkErrorXml("EN", STR_NetworkError, className);
                }

                client.Close();
                StatusWindowClose();
            }
            catch (Exception)
            {
              
            }
            if (string.IsNullOrEmpty(XmlResponse)) { XmlResponse = string.Empty; }
            XmlResponse = XmlResponse.Replace("&#x1;", "");
            XmlResponse = XmlResponse.Replace("&#x0;", "");
            XmlResponse = XmlResponse.Replace("&#x", "");
            XmlResponse = XmlResponse.Replace("\0", "");

            if (!string.IsNullOrEmpty(XmlResponse))
            {
                XmlResponse = XmlResponse.Trim();
            }

            if (string.IsNullOrEmpty(XmlResponse) )
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", STR_NetworkError, className);
            }
            if (XmlResponse == STR_QueGoAway)
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", STR_VersionIncompat, className);
            }
            setStatus("Waiting");

            if (!IsValidXML(XmlResponse))
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", STR_NetworkError, className);
            }
            StatusWindowClose();
            return XmlResponse;
        }

        private string SendMessageLocal(string server, string XmlRequest, int timeout)
        {
            string XmlResponse = "";
            string clearXmlRequest = XmlRequest;
            int serverConnectTimeout = 1000;
            OpenDevelopmentWindow();

            string[] fields = server.Split(':');
        //    string sessionId = Process.GetCurrentProcess().SessionId.ToString();

            if (ProcessClass.GetConsoleSessionId().ToString() == SessionID)
            {
                SessionID = "";
            }
            if (!ProcessClass.isTerminalServerInstalled())
            {
                SessionID = "";
            }
            try
            {
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "HicapsConnectPipe" + SessionID, PipeDirection.InOut);

                // Connect to the pipe or wait until the pipe is available.
                string addString = "";
                if (!string.IsNullOrEmpty(SessionID)) { addString = "(" + SessionID + ")"; }
                setStatus(STR_Connecting + " Local" + addString);

                pipeClient.Connect(serverConnectTimeout);


                Log(STR_SendingRequest);
                StatusWindowOpen();
                HicapsConnectProtocol myProtocol = new HicapsConnectProtocol(HicapsTools.ProcessClass.getUserConfigFileSetting("NetworkName", ""));
                XmlRequest = myProtocol.encodeXmlMessage(XmlRequest, true);
                setStatus(STR_Connecting + " Local" + addString);
                Byte[] request = Encoding.Default.GetBytes(XmlRequest);
                pipeClient.Write(request, 0, request.Length);
                pipeClient.Flush();

                byte[] xmlLength;
                string messageLength;
                int lenbytesRead;
                bool messageLoop = false;
                bool statusLoop = true;
                do
                {
                    // Big Status Loop.
                    do
                    {
                        // Inner Ack Loop, maybe we can get rid off.
                        xmlLength = new byte[20];
                        messageLength = "";
                        lenbytesRead = pipeClient.Read(xmlLength, 0, 20);//s.Receive(incomingBuffer);
                        messageLength = Encoding.Default.GetString(xmlLength);
                        // Log("Message size is " + messageLength);
                        switch (messageLength)
                        {
                            case STR_ACK00000000000000000:
                                setStatus(STR_AckReceived);
                                messageLoop = true;
                                break;
                            default:

                                //setStatus(STR_Reading);
                                messageLoop = false; break;
                        }
                    } while (messageLoop && pipeClient.IsConnected);
                    int xmlMessageSize = Convert.ToInt32(messageLength);

                    //Byte[] response = new Byte[xmlMessageSize];
                    // int bytesRead = client.GetStream().Read(response, 0, xmlMessageSize);
                    //byte[] resultArray = new byte[bytesRead];
                    // Array.Copy(response, resultArray, bytesRead);
                    //XmlResponse = Encoding.Default.GetString(response).Trim();
                    List<byte> dataBuffer = new List<byte>();
                    int bytesReadSoFar = 0;
                    do
                    {
                        Byte[] response = new Byte[xmlMessageSize];
                        int bytesRead = pipeClient.Read(response, 0, xmlMessageSize);
                        bytesReadSoFar += bytesRead;
                        byte[] resultArray = new byte[bytesRead];
                        Array.Copy(response, resultArray, bytesRead);
                        dataBuffer.AddRange(resultArray);
                    } while (bytesReadSoFar < xmlMessageSize);

                    XmlResponse = Encoding.Default.GetString(dataBuffer.ToArray(), 0, dataBuffer.Count).Trim();

                    // Log("Received response: " + XmlResponse);
                    if (XmlResponse.IndexOf('<') < 0)
                    {
                        //  Log("Message was encrypted");
                        XmlResponse = XmlResponse.Replace("\0", "");
                        XmlResponse = myProtocol.decodeXmlMessage(XmlResponse, true);

                    }
                    if (XmlResponse.IndexOf("<StatusResponse") > 0)
                    {
                        //Do something with the status message....
                        try
                        {
                            StatusResponse myResult = Deserialize<StatusResponse>(XmlResponse);
              //              File.WriteAllText("C:\\HicapsConnect\\XML\\" + typeof(StatusResponse).ToString(), XmlResponse);
                            if (myResult.ResponseText == STR_AckReceived)
                            {
                                setStatus(myResult.ResponseText + " Local");
                            }
                            else
                            {
                                setStatus(myResult.ResponseText);
                            }
                            statusLoop = true;
                        }
                        catch (Exception ex)
                        {
                            statusLoop = false;
                        }
                    }
                    else
                    {
                        statusLoop = false;
                    }

                } while (pipeClient.IsConnected && statusLoop);
                pipeClient.Close();

            }
            catch (TimeoutException te)
            {
                setStatus("Waiting");
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", String.Format(STR_StandAloneRequestTimedOutCheckServiceIsRunning), className);
                return XmlResponse;
            }
            catch (Exception ex) { }
            
            if (string.IsNullOrEmpty(XmlResponse)) { XmlResponse = string.Empty; }
            XmlResponse = XmlResponse.Replace("&#x1;", "");
            XmlResponse = XmlResponse.Replace("&#x0;", "");
            XmlResponse = XmlResponse.Replace("&#x", "");
            XmlResponse = XmlResponse.Replace("\0", "");

            if (!string.IsNullOrEmpty(XmlResponse))
            {
                XmlResponse = XmlResponse.Trim();
            }
            if (string.IsNullOrEmpty(XmlResponse))
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", STR_NetworkError, className);
            }

            if (XmlResponse == STR_QueGoAway)
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", STR_VersionIncompat, className);
            }

            setStatus("Waiting");
            if (string.IsNullOrEmpty(XmlResponse))
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", String.Format(STR_StandAloneRequestTimedOutCheckServiceIsRunning), className);
            }
            if (!IsValidXML(XmlResponse))
            {
                string className = BaseMessage.extractClassName(clearXmlRequest);
                XmlResponse = returnNetworkErrorXml("EN", STR_NetworkError, className);
            }
            StatusWindowClose();


            return XmlResponse;
        }

        private void setStatus(string status)
        {
            //lStatus.Text = status;
            //lStatus.Refresh();
            SetStatusText(status);
            if (showStatusWindow)
            {
                OnStatusChanged(status);
                Application.DoEvents();
            }

            //if (_myStatusForm != null)
            //{
            //    _myStatusForm.SetText(status);
            //    _myStatusForm.BringToFront();
            //}
            // Thread.Sleep(100);
        }

        private void StatusWindowClose()
        {
            try
            {
                if (_myStatusForm != null && !_myStatusForm.IsDisposed && _myStatusForm.Visible)
                {
                    _myStatusForm.Hide();
                }
            }
            catch
            { }
        }

        public string getDefaultServerConfig(string server)
        {
            return getDefaultServer(server);
        }

        /// <summary>
        /// Returns true/false if server is operating in standalone/no-network mode.
        /// </summary>
        /// <returns></returns>
        public bool getStandAloneMode()
        {
            // New 1.0.3._3 Code.
            // If no network exists, regardless of any setting return true;
            if (!isNetworkAvailable()) return true;
            // If Network exists, but configuration file says force on then return true
            if (ProcessClass.getUserConfigFileSetting("NoNetwork", "false").ToLower() == "true")
            {
                return true;
            }
            // Leave it at Auto
            return false;

        }

        private static bool validateServerUrl(string serverUrl)
        {
            return true;
        }

        // this function gets called when an connection event occurs... possible events: client connected, connection refused etc. ...but it doesn't have a connection timeout...it waits as long as necessary to get anykind of reply
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                TcpClient client = (TcpClient)ar.AsyncState;
                client.EndConnect(ar); // Complete the connection.
                // Console.WriteLine("TcpClient connected to {0}", client.Client.RemoteEndPoint.ToString());
                connectDone.Set(); // trigger the connectDone event 
            }
            catch
            {
                // Console.WriteLine(e.ToString());
            }
        }

      
        /// <summary>
        /// Some languages (Delphi and Powerbuilder) have difficulty creating the required request Objects.  
        /// This method will return a initialised request object, if the object name is passed to it.
        /// Psuedo Code
        ///  Set requestObject = HicaosConnectControl.createObject(“ClaimRequest”)
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public object createobject(string className)
        {
            switch (className.Trim().ToLower())
            {
                case "allitemlistrequest": return new AllItemListRequest();
                case "allitemlistresponse": return new AllItemListResponse();
                case "allpharmitemlistrequest": return new AllPharmItemListRequest();
                case "allpharmitemlistresponse": return new AllPharmItemListResponse();
                case "allitemresponsecodelistrequest": return new AllItemResponseCodeListRequest();
                case "allitemresponsecodelistresponse": return new AllItemResponseCodeListResponse();
                case "allmerchantlistrequest": return new AllMerchantListRequest();
                case "allmerchantlistresponse": return new AllMerchantListResponse();
                case "allproviderlistrequest": return new AllProviderListRequest();
                case "allproviderlistresponse": return new AllProviderListResponse();
                case "alltranscodelistrequest": return new AllTransCodeListRequest();
                case "alltranscodelistresponse": return new AllTransCodeListResponse();
                case "cardlistrequest": return new CardListRequest();
                case "cardlistresponse": return new CardListResponse();
                case "cardreadrequest": return new CardReadRequest();
                case "cardreadresponse": return new CardReadResponse();
                case "cashoutrequest": return new CashoutRequest();
                case "cashoutresponse": return new CashoutResponse();
                case "claimcancelrequest": return new ClaimCancelRequest();
                case "claimcancelresponse": return new ClaimCancelResponse();
                case "claimrequest": return new ClaimRequest();
                case "claimresponse": return new ClaimResponse();
                case "claimpharmrequest": return new ClaimPharmRequest();
                case "claimpharmresponse": return new ClaimPharmResponse();
                case "eftposdepositrequest": return new EftposDepositRequest();
                case "eftposdepositresponse": return new EftposDepositResponse();
                case "eftpostranslistingrequest": return new EftposTransListingRequest();
                case "eftpostranslistingresponse": return new EftposTransListingResponse();
                case "eftposttranslistingrequest": return new EftposTransListingRequest();
                case "eftposttranslistingresponse": return new EftposTransListingResponse();
                case "getterminalrequest": return new GetTerminalRequest();
                case "getterminalresponse": return new GetTerminalResponse();
                case "hicapstotalsrequest": return new HicapsTotalsRequest();
                case "hicapstotalsresponse": return new HicapsTotalsResponse();
                case "hicapstranslistingrequest": return new HicapsTransListingRequest();
                case "hicapstranslistingresponse": return new HicapsTransListingResponse();
                case "medicareclaimrequest": return new MedicareClaimRequest();
                case "medicareclaimresponse": return new MedicareClaimResponse();
                case "medicaremerchantlistrequest": return new MedicareMerchantListRequest();
                case "medicaremerchantlistresponse": return new MedicareMerchantListResponse();
                case "merchantlistrequest": return new MerchantListRequest();
                case "merchantlistresponse": return new MerchantListResponse();
                case "printlastreceiptrequest": return new PrintLastReceiptRequest();
                case "printlastreceiptresponse": return new PrintLastReceiptResponse();
                case "quoterequest": return new QuoteRequest();
                case "quoteresponse": return new QuoteResponse();
                case "refundrequest": return new RefundRequest();
                case "refundresponse": return new RefundResponse();
                case "salecashoutrequest": return new SaleCashoutRequest();
                case "salecashoutresponse": return new SaleCashoutResponse();
                case "salerequest": return new SaleRequest();
                case "saleresponse": return new SaleResponse();
                case "settlementrequest": return new SettlementRequest();
                case "settlementresponse": return new SettlementResponse();
                case "terminaltestrequest": return new TerminalTestRequest();
                case "terminaltestresponse": return new TerminalTestResponse();
                default: return null;
            }
            return null;
        }

        public static string getDefaultServer(string server)
        {
            if (server == null || server.Trim().Length == 0)
            {

                server = ProcessClass.getUserConfigFileSetting("DefaultTerminal", "");
                if (validateServerUrl(server) == false)
                {
                    ProcessClass.setUserConfigFileSetting("DefaultTerminal", "");
                    server = "";
                }
            }
            return server;
        }
        private void StatusWindowNonModal()
        {
            try
            {
                if (showStatusWindow)
                {
                    if (_myStatusForm != null && !_myStatusForm.IsDisposed)
                    {
                        _myStatusForm.TopMost = false;
                        Application.DoEvents();
                        Thread.Sleep(200);
                    }
                }
            }
            catch (Exception)
            { }
        }
        /// <summary>
        /// Used to turn on or off the status display window.  By default the status window will always be displayed
        /// </summary>
        /// <param name="flag"></param>
        public void DisplayStatusWindow(bool flag)
        {
            showStatusWindow = flag;
            if (!flag)
            {
                StatusWindowClose();
            }
        }
        private void StatusWindowOpen()
        {
            try
            {
                if (showStatusWindow)
                {
                    if (_myStatusForm == null || _myStatusForm.IsDisposed)
                    {
                        _myStatusForm = new StatusForm(this);
                    }
                    _myStatusForm.Show();
                    _myStatusForm.BringToFront();
                }
                else
                {
                    StatusWindowClose();
                }
            }
            catch
            { }
        }
       

 

        #region Encryption Routines
        //private static string EncryptTextToMemory(string Data, byte[] Key, byte[] IV)
        //{
        //    try
        //    {
        //        // Create a MemoryStream.
        //        MemoryStream mStream = new MemoryStream();
        //        // Create a CryptoStream using the MemoryStream 
        //        // and the passed key and initialization vector (IV).
        //        TripleDESCryptoServiceProvider myPop = new TripleDESCryptoServiceProvider();
        //        //myPop.Mode = CipherMode.OFB;

        //        CryptoStream cStream = new CryptoStream(mStream,
        //           myPop.CreateEncryptor(Key, IV),
        //           CryptoStreamMode.Write);

        //        // Convert the passed string to a byte array.
        //        byte[] toEncrypt = Encoding.Default.GetBytes(Data);

        //        // Write the byte array to the crypto stream and flush it.
        //        cStream.Write(toEncrypt, 0, toEncrypt.Length);
        //        cStream.FlushFinalBlock();

        //        // Get an array of bytes from the 
        //        // MemoryStream that holds the 
        //        // encrypted data.
        //        byte[] ret = mStream.ToArray();

        //        // Close the streams.
        //        cStream.Close();
        //        mStream.Close();

        //        // Return the encrypted buffer.
        //        return Convert.ToBase64String(ret);
        //    }
        //    catch (CryptographicException e)
        //    {
        //        //  Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
        //        return null;
        //    }

        //}

        //private static string DecryptTextFromMemory(string stringData, byte[] Key, byte[] IV)
        //{
        //    byte[] Data;
        //    try
        //    {
        //        // Create a new MemoryStream using the passed 
        //        // array of encrypted data.
        //        // stringData = Base64Data String.
        //        Data = Convert.FromBase64String(stringData);
        //        MemoryStream msDecrypt = new MemoryStream(Data);

        //        // Create a CryptoStream using the MemoryStream 
        //        // and the passed key and initialization vector (IV).
        //        CryptoStream csDecrypt = new CryptoStream(msDecrypt,
        //            new TripleDESCryptoServiceProvider().CreateDecryptor(Key, IV),
        //            CryptoStreamMode.Read);

        //        // Create buffer to hold the decrypted data.
        //        byte[] fromEncrypt = new byte[Data.Length];

        //        // Read the decrypted data out of the crypto stream
        //        // and place it into the temporary buffer.
        //        csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

        //        //Convert the buffer into a string and return it.
        //        string buffer = Encoding.Default.GetString(fromEncrypt);
        //        // Wipe any nulls.
        //        buffer.Replace("\0", "").Trim();
        //        return buffer;
        //    }
        //    catch (CryptographicException e)
        //    {
        //        //  Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
        //        return null;
        //    }
        //}
        #endregion
        #region ComInterop COde
        //[ComRegisterFunction()]
        //public static void RegisterClass(string key)
        //{
        //    // Strip off HKEY_CLASSES_ROOT\ from the passed key as I don't need it
        //    StringBuilder sb = new StringBuilder(key);
        //    sb.Replace(@"HKEY_CLASSES_ROOT\", "");

        //    // Open the CLSID\{guid} key for write access
        //    RegistryKey k = Registry.ClassesRoot.OpenSubKey(sb.ToString(), true);

        //    // And create the 'Control' key - this allows it to show up in 
        //    // the ActiveX control container 
        //    RegistryKey ctrl = k.CreateSubKey("Control");
        //    ctrl.Close();

        //    // Next create the CodeBase entry - needed if not string named and GACced.
        //    RegistryKey inprocServer32 = k.OpenSubKey("InprocServer32", true);
        //    inprocServer32.SetValue("CodeBase", Assembly.GetExecutingAssembly().CodeBase);
        //    inprocServer32.Close();

        //    // Finally close the main key
        //    k.Close();
        //}
        //[ComUnregisterFunction()]
        //public static void UnregisterClass(string key)
        //{
        //    StringBuilder sb = new StringBuilder(key);
        //    sb.Replace(@"HKEY_CLASSES_ROOT\", "");

        //    // Open HKCR\CLSID\{guid} for write access
        //    RegistryKey k = Registry.ClassesRoot.OpenSubKey(sb.ToString(), true);

        //    // Delete the 'Control' key, but don't throw an exception if it does not exist
        //    k.DeleteSubKey("Control", false);

        //    // Next open up InprocServer32
        //    RegistryKey inprocServer32 = k.OpenSubKey("InprocServer32", true);

        //    // And delete the CodeBase key, again not throwing if missing 
        //    k.DeleteSubKey("CodeBase", false);

        //    // Finally close the main key 
        //    k.Close();
        //}
        [ComRegisterFunction]
        static void ComRegister(Type t)
        {

            string keyName = @"CLSID\" + t.GUID.ToString("B");
            using (RegistryKey key =
                     Registry.ClassesRoot.OpenSubKey(keyName, true))
            {
                key.CreateSubKey("Control").Close();
                using (RegistryKey subkey = key.CreateSubKey("MiscStatus"))
                {
                    subkey.SetValue("", "131457");
                }
                using (RegistryKey subkey = key.CreateSubKey("TypeLib"))
                {
                    Guid libid = Marshal.GetTypeLibGuidForAssembly(t.Assembly);
                    subkey.SetValue("", libid.ToString("B"));
                }
                using (RegistryKey subkey = key.CreateSubKey("Version"))
                {
                    Version ver = t.Assembly.GetName().Version;
                    string version =
                      string.Format("{0}.{1}",
                                    ver.Major,
                                    ver.Minor);
                    if (version == "0.0") version = "1.0";
                    subkey.SetValue("", version);
                }
                using (RegistryKey subkey = key.OpenSubKey("InprocServer32", true))
                {
                    string strver = t.Assembly.GetName().Version.ToString();

                    RegistryKey inprocServer32 = subkey.OpenSubKey(strver, true);
                    //    // Next create the CodeBase entry - needed if not string named and GACced.
                    inprocServer32.SetValue("CodeBase", Assembly.GetExecutingAssembly().CodeBase);
                    inprocServer32.Close();
                }
            }
        }

        [ComUnregisterFunction]
        static void ComUnregister(Type t)
        {
            // Delete entire CLSID\{clsid} subtree
            string keyName = @"CLSID\" + t.GUID.ToString("B");
            Registry.ClassesRoot.DeleteSubKeyTree(keyName);
        }

        #endregion


        public void ClearTerminalList()
        {
           
            ResetTerminalList();
        }
    }
}
