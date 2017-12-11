
// ClaimLog.cs - auto-generated classes to represent the ClaimLog.log file when 
// it is deserialised.
using System.Xml.Serialization;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Xml;
using System.Runtime.Serialization;
using System.Reflection;
using System.Security.Cryptography;

namespace HicapsConnectClient12
{

    /*
     * ClaimLog.
     * Allows ClaimResponse objects to be logged to 
     * a file and retrieved.
     */
    public class ClaimLog
    {
        private List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse> log;
        private List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse> logCancelled;
        private List<HicapsConnectControl.HicapsConnectControl.QuotePharmResponse> logQuotes;
        private List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse> logDeclined;

        private string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                + Path.DirectorySeparatorChar
                                + "HicapsConnect"
                                + Path.DirectorySeparatorChar
                                + "ClaimLog.log";

        private readonly string _filenameCancelled = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), string.Format("{0}{1}{2}", "HicapsConnect", Path.DirectorySeparatorChar, "CancelledClaimLog.log"));
        private readonly string _filenameQuotes = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), string.Format("{0}{1}{2}", "HicapsConnect", Path.DirectorySeparatorChar, "QuotesLog.log"));
        private readonly string _declinedFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), string.Format("{0}{1}{2}", "HicapsConnect", Path.DirectorySeparatorChar, "declinedClaimsLog.log"));


        public List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse> Log
        {
            get { return log; }
        }

        public ClaimLog(string _filename)
        {
            log = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>();
            logCancelled = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>();
            logQuotes = new List<HicapsConnectControl.HicapsConnectControl.QuotePharmResponse>();
            logDeclined = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>();
            if (_filename != null)
            {
                filename = _filename;
            }
            ReadLogFile(filename, "claim");
            ReadLogFile(_declinedFilename, "declined");
            ReadCancelledLogFile();
            ReadQuotesLogFile();
            CleanLog();
        }

        private void CleanLog()
        {
            if (log != null)
            {
                // delete all that isn't within the last 7 days
                log = log.Where(r => DateTime.Today.Subtract(r.TransactionDate.Date) <= TimeSpan.FromDays(7))
                         .ToList();
            }
        }

        public void LogClaim(HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse response)
        {
            // set readonly to false (by cheating). Otherwise the serializer has trouble 
            // re-creating the object
            if (response.ResponseCode == "00")
            {
                log.Add(response.CloneWithoutReadonlySet());
                SaveLogFile();
            }
            else
            {
                logDeclined.Add(response.CloneWithoutReadonlySet());
                SaveLogFileDeclined();
            }
        }

        private static void CreatePathMissing(string path)
        {
            try
            {
                path = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(path))
                {
                    bool folderExists = Directory.Exists(path);
                    if (!folderExists)
                        Directory.CreateDirectory(path);
                }
            }
            catch
            {
            }
        }
        private void ReadLogFile(string filepath, string logToLoad)
        {
            try
            {
                CreatePathMissing(filepath);
                using (FileStream file = new FileStream(filepath, FileMode.OpenOrCreate))
                {
                    var transform = new AesCryptoServiceProvider();
                    using (var cryptoStream = new CryptoStream(file,
                        transform.CreateDecryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Read))
                    {
                        var textReader = new XmlTextReader(cryptoStream);
                        var serialiser = new DataContractSerializer(typeof(HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse[]));
                        switch (logToLoad)
                        {
                            case "claim":
                                log = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>(
                            (HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse[])
                                serialiser.ReadObject(textReader));
                                break;

                            case "declined":
                                logDeclined = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>(
                            (HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse[])
                                serialiser.ReadObject(textReader));
                                break;
                        }
                    }

                    file.Close();
                }
            }
            catch
            {
                Debug.WriteLine("couldn't read log file correctly");

                switch (logToLoad)
                {
                    case "claim":
                        log = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>();
                        break;

                    case "declined":
                        logDeclined = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>();
                        break;
                }
                ReCreateFile(filepath);
            }
        }



        public void SaveLogFile()
        {
            try
            {
                DataContractSerializer serialiser = new DataContractSerializer(log.GetType());
                FileStream file = File.Open(filename, FileMode.Create, FileAccess.Write);
                
                // create crypto stream
                var transform = new AesCryptoServiceProvider();
                using (var cryptoStream = new CryptoStream(file,
                        transform.CreateEncryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Write))
                {
                    serialiser.WriteObject(cryptoStream, log);
                }
                
                file.Close();
            }
            catch
            {
                Debug.WriteLine("SaveLogFile: couldn't save log file");
            }
        }
        //GJ: The following methods where copied from the existing ones to save the log file but to be used
        // for the cancelled claims.
        public void LogCancelledClaim(HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse response)
        {
            // set readonly to false (by cheating). Otherwise the serializer has trouble 
            // re-creating the object
            logCancelled.Add(response);
            SaveCancelledLogFile();
        }

        private void SaveCancelledLogFile()
        {
            try
            {
                var serialiser = new DataContractSerializer(logCancelled.GetType());
                var file = File.Open(_filenameCancelled, FileMode.Create, FileAccess.Write);

                // create crypto stream
                var transform = new AesCryptoServiceProvider();
                using (var cryptoStream = new CryptoStream(file,
                        transform.CreateEncryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Write))
                {
                    serialiser.WriteObject(cryptoStream, logCancelled);
                }

                file.Close();
            }
            catch
            {
                Debug.WriteLine("SaveCancelledLogFile: couldn't save log file");
            }
        }

        private void ReadCancelledLogFile()
        {
            try
            {
                CreatePathMissing(_filenameCancelled);
                using (var file = new FileStream(_filenameCancelled, FileMode.OpenOrCreate))
                {
                    var transform = new AesCryptoServiceProvider();
                    using (var cryptoStream = new CryptoStream(file,
                        transform.CreateDecryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Read))
                    {
                        var textReader = new XmlTextReader(cryptoStream);
                        var serialiser = new DataContractSerializer(typeof(HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse[]));
                        logCancelled = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>(
                            (HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse[])
                                serialiser.ReadObject(textReader));
                    }

                    file.Close();
                }
            }
            catch
            {
                Debug.WriteLine("couldn't read log file correctly");
                logCancelled = new List<HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse>();
                ReCreateFile(_filenameCancelled);
            }
        }

        //GJ: The following methods where copied from the existing ones to save the log file but to be used
        // for Quotes. Note that the quotes were previously save with the claims, but it was requested to only display claims
        // in the cancel tab, this is the way to have them separate but still keep track of them.
        public void LogQuote(HicapsConnectControl.HicapsConnectControl.QuotePharmResponse response)
        {
            // set readonly to false (by cheating). Otherwise the serializer has trouble 
            // re-creating the object
            logQuotes.Add(response);
            SaveQuotesLogFile();
        }

        private void SaveQuotesLogFile()
        {
            try
            {
                var serialiser = new DataContractSerializer(logQuotes.GetType());
                var file = File.Open(_filenameQuotes, FileMode.Create, FileAccess.Write);

                // create crypto stream
                var transform = new AesCryptoServiceProvider();
                using (var cryptoStream = new CryptoStream(file,
                        transform.CreateEncryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Write))
                {
                    serialiser.WriteObject(cryptoStream, logQuotes);
                }

                file.Close();
            }
            catch
            {
                Debug.WriteLine("SaveQuotesLogFile: couldn't save log file");
            }
        }

        private void ReadQuotesLogFile()
        {
            try
            {
                CreatePathMissing(_filenameQuotes);
                using (var file = new FileStream(_filenameQuotes, FileMode.OpenOrCreate))
                {
                    var transform = new AesCryptoServiceProvider();
                    using (var cryptoStream = new CryptoStream(file,
                        transform.CreateDecryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Read))
                    {
                        var textReader = new XmlTextReader(cryptoStream);
                        var serialiser = new DataContractSerializer(typeof(HicapsConnectControl.HicapsConnectControl.QuotePharmResponse[]));
                        logQuotes = new List<HicapsConnectControl.HicapsConnectControl.QuotePharmResponse>(
                            (HicapsConnectControl.HicapsConnectControl.QuotePharmResponse[])
                                serialiser.ReadObject(textReader));
                    }

                    file.Close();
                }
            }
            catch
            {
                Debug.WriteLine("couldn't read log file correctly");
                logQuotes = new List<HicapsConnectControl.HicapsConnectControl.QuotePharmResponse>();
                ReCreateFile(_filenameQuotes);
            }
        }

        private static void ReCreateFile(string filenameToReCreate)
        {
            File.Delete(filenameToReCreate);
            File.Create(filenameToReCreate);
        }

        public void SaveLogFileDeclined()
        {
            try
            {
                DataContractSerializer serialiser = new DataContractSerializer(logDeclined.GetType());
                FileStream file = File.Open(_declinedFilename, FileMode.Create, FileAccess.Write);

                // create crypto stream
                var transform = new AesCryptoServiceProvider();
                using (var cryptoStream = new CryptoStream(file,
                        transform.CreateEncryptor(Utils.GetMachineSpecificKey(), Utils.HexToByte(Utils.StorageIV)),
                        CryptoStreamMode.Write))
                {
                    serialiser.WriteObject(cryptoStream, logDeclined);
                }

                file.Close();
            }
            catch
            {
                Debug.WriteLine("SaveLogFile: couldn't save log file");
            }
        }

        #region old reads/writes

        /*private void ReadLogFile()
    {
        try
        {
            StreamReader file = new StreamReader(filename, false);
            XmlSerializer x = new XmlSerializer(typeof(ArrayOfClaimResponse));
            ArrayOfClaimResponse data = (ArrayOfClaimResponse)x.Deserialize(file);
            file.Close();

            log = new List<HicapsConnectControl.HicapsConnectControl.ClaimResponse>();
            if (data != null)
            {
                foreach (ArrayOfClaimResponseClaimResponse cur in data.ClaimResponse)
                {
                    HicapsConnectControl.HicapsConnectControl.ClaimResponse r
                        = new HicapsConnectControl.HicapsConnectControl.ClaimResponse();
                    r.BenefitAmount = cur.BenefitAmount;

                    // skip claim details since the UI doesn't use them

                    r.ClientVersion = cur.ClientVersion;
                    r.ComPort = cur.ComPort;
                    r.ComputerName = cur.ComputerName;
                    r.ExpiryDate = cur.ExpiryDate;
                    r.FormatVersion = cur.FormatVersion;
                    r.MembershipId = cur.MembershipId;
                    r.MerchantId = cur.MerchantId;
                    r.MoreIndicator = cur.MoreIndicator;
                    r.MsgId = cur.MsgId;

                    // skip patient name details since UI doesn't use
                    r.PatientNameDetails = new List<string>();

                    r.PrimaryAccountNumber = cur.PrimaryAccountNumber;
                    r.ProviderName = cur.ProviderName;
                    r.ProviderNumberId = cur.ProviderNumberId;
                    r.RequestResponseIndicator = cur.RequestResponseIndicator;
                    r.ResponseCode = cur.ResponseCode;
                    r.ResponseText = cur.ResponseText;
                    r.ResponseTime = cur.ResponseTime;
                    r.RrnNumber = cur.RrnNumber;
                    r.ServerUrl = cur.ServerUrl;
                    r.ServerVersion = cur.ServerVersion;

                    // skip software vendor name (not needed)

                    r.TerminalId = cur.TerminalId;
                    r.TerminalVersion = cur.TerminalVersion;
                    r.TransactionAmount = cur.TransactionAmount;
                    r.TransactionDate = cur.TransactionDate;

                    log.Add(r);
                }
            }


        }
        catch
        {
            Debug.WriteLine("couldn't read log file correctly");
            log = new List<HicapsConnectControl.HicapsConnectControl.ClaimResponse>();
        }
    }*/
        /*private void ReadLogFile()
    {
        // Read contents of file and deserialize to log
        try
        {
            StreamReader r = new StreamReader(filename);
            XmlSerializer x = new XmlSerializer (typeof (string[]));
            string[] serialisedClaims = (string[]) x.Deserialize(r);

            log = new List<HicapsConnectControl.HicapsConnectControl.ClaimResponse>();

            // deserialise each claim and add to log
            foreach (string claim in serialisedClaims)
            {
                log.Add(Utils.Deserialize<HicapsConnectControl.HicapsConnectControl.ClaimResponse>(claim));
            }
        }
        catch 
        {
            Debug.Write("couldn't read log properly");
        }

    }*/
        /*private void SaveLogFile()
        {
            // serialise claims into a list of strings
            List<string> serialisedClaims = new List<string>();
            foreach (var claim in log)
            {
                serialisedClaims.Add(Utils.Serialize<HicapsConnectControl.HicapsConnectControl.ClaimResponse>(claim));
            }

            // serialise the list of strings and try to write to file
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(List<string>));
                StreamWriter outfile = new StreamWriter(filename);
                x.Serialize(outfile, serialisedClaims);
            }
            catch { }
        }*/
        #endregion
    }






    #region auto-generated gunk


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ArrayOfClaimResponse
    {

        private ArrayOfClaimResponseClaimResponse[] claimResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ClaimResponse")]
        public ArrayOfClaimResponseClaimResponse[] ClaimResponse
        {
            get
            {
                return this.claimResponseField;
            }
            set
            {
                this.claimResponseField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ArrayOfClaimResponseClaimResponse
    {

        private bool readOnlyField;

        private string msgIdField;

        private byte formatVersionField;

        private byte requestResponseIndicatorField;

        private byte moreIndicatorField;

        private string serverUrlField;

        private string computerNameField;

        private string comPortField;

        private System.DateTime responseTimeField;

        private string responseTextField;

        private string responseCodeField;

        private string serverVersionField;

        private string clientVersionField;

        private string terminalVersionField;

        private string primaryAccountNumberField;

        private bool primaryAccountNumberFieldSpecified;

        private string expiryDateField;

        private bool expiryDateFieldSpecified;

        private decimal transactionAmountField;

        private decimal benefitAmountField;

        private string providerNumberIdField;

        private string membershipIdField;

        private System.DateTime transactionDateField;

        private string terminalIdField;

        private string rrnNumberField;

        private bool rrnNumberFieldSpecified;

        private ArrayOfClaimResponseClaimResponseClaimDetails claimDetailsField;

        private ArrayOfClaimResponseClaimResponseClaimDetailsStr claimDetailsStrField;

        private object patientNameDetailsField;

        private object patientNameDetailsStrField;

        private string providerNameField;

        private string merchantIdField;

        /// <remarks/>
        public bool ReadOnly
        {
            get
            {
                return this.readOnlyField;
            }
            set
            {
                this.readOnlyField = value;
            }
        }

        /// <remarks/>
        public string MsgId
        {
            get
            {
                return this.msgIdField;
            }
            set
            {
                this.msgIdField = value;
            }
        }

        /// <remarks/>
        public byte FormatVersion
        {
            get
            {
                return this.formatVersionField;
            }
            set
            {
                this.formatVersionField = value;
            }
        }

        /// <remarks/>
        public byte RequestResponseIndicator
        {
            get
            {
                return this.requestResponseIndicatorField;
            }
            set
            {
                this.requestResponseIndicatorField = value;
            }
        }

        /// <remarks/>
        public byte MoreIndicator
        {
            get
            {
                return this.moreIndicatorField;
            }
            set
            {
                this.moreIndicatorField = value;
            }
        }

        /// <remarks/>
        public string ServerUrl
        {
            get
            {
                return this.serverUrlField;
            }
            set
            {
                this.serverUrlField = value;
            }
        }

        /// <remarks/>
        public string ComputerName
        {
            get
            {
                return this.computerNameField;
            }
            set
            {
                this.computerNameField = value;
            }
        }

        /// <remarks/>
        public string ComPort
        {
            get
            {
                return this.comPortField;
            }
            set
            {
                this.comPortField = value;
            }
        }

        /// <remarks/>
        public System.DateTime ResponseTime
        {
            get
            {
                return this.responseTimeField;
            }
            set
            {
                this.responseTimeField = value;
            }
        }

        /// <remarks/>
        public string ResponseText
        {
            get
            {
                return this.responseTextField;
            }
            set
            {
                this.responseTextField = value;
            }
        }

        /// <remarks/>
        public string ResponseCode
        {
            get
            {
                return this.responseCodeField;
            }
            set
            {
                this.responseCodeField = value;
            }
        }

        /// <remarks/>
        public string ServerVersion
        {
            get
            {
                return this.serverVersionField;
            }
            set
            {
                this.serverVersionField = value;
            }
        }

        /// <remarks/>
        public string ClientVersion
        {
            get
            {
                return this.clientVersionField;
            }
            set
            {
                this.clientVersionField = value;
            }
        }

        /// <remarks/>
        public string TerminalVersion
        {
            get
            {
                return this.terminalVersionField;
            }
            set
            {
                this.terminalVersionField = value;
            }
        }

        /// <remarks/>
        public string PrimaryAccountNumber
        {
            get
            {
                return this.primaryAccountNumberField;
            }
            set
            {
                this.primaryAccountNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PrimaryAccountNumberSpecified
        {
            get
            {
                return this.primaryAccountNumberFieldSpecified;
            }
            set
            {
                this.primaryAccountNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public string ExpiryDate
        {
            get
            {
                return this.expiryDateField;
            }
            set
            {
                this.expiryDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExpiryDateSpecified
        {
            get
            {
                return this.expiryDateFieldSpecified;
            }
            set
            {
                this.expiryDateFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal TransactionAmount
        {
            get
            {
                return this.transactionAmountField;
            }
            set
            {
                this.transactionAmountField = value;
            }
        }

        /// <remarks/>
        public decimal BenefitAmount
        {
            get
            {
                return this.benefitAmountField;
            }
            set
            {
                this.benefitAmountField = value;
            }
        }

        /// <remarks/>
        public string ProviderNumberId
        {
            get
            {
                return this.providerNumberIdField;
            }
            set
            {
                this.providerNumberIdField = value;
            }
        }

        /// <remarks/>
        public string MembershipId
        {
            get
            {
                return this.membershipIdField;
            }
            set
            {
                this.membershipIdField = value;
            }
        }

        /// <remarks/>
        public System.DateTime TransactionDate
        {
            get
            {
                return this.transactionDateField;
            }
            set
            {
                this.transactionDateField = value;
            }
        }

        /// <remarks/>
        public string TerminalId
        {
            get
            {
                return this.terminalIdField;
            }
            set
            {
                this.terminalIdField = value;
            }
        }

        /// <remarks/>
        public string RrnNumber
        {
            get
            {
                return this.rrnNumberField;
            }
            set
            {
                this.rrnNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RrnNumberSpecified
        {
            get
            {
                return this.rrnNumberFieldSpecified;
            }
            set
            {
                this.rrnNumberFieldSpecified = value;
            }
        }

        /// <remarks/>
        public ArrayOfClaimResponseClaimResponseClaimDetails ClaimDetails
        {
            get
            {
                return this.claimDetailsField;
            }
            set
            {
                this.claimDetailsField = value;
            }
        }

        /// <remarks/>
        public ArrayOfClaimResponseClaimResponseClaimDetailsStr ClaimDetailsStr
        {
            get
            {
                return this.claimDetailsStrField;
            }
            set
            {
                this.claimDetailsStrField = value;
            }
        }

        /// <remarks/>
        public object PatientNameDetails
        {
            get
            {
                return this.patientNameDetailsField;
            }
            set
            {
                this.patientNameDetailsField = value;
            }
        }

        /// <remarks/>
        public object PatientNameDetailsStr
        {
            get
            {
                return this.patientNameDetailsStrField;
            }
            set
            {
                this.patientNameDetailsStrField = value;
            }
        }

        /// <remarks/>
        public string ProviderName
        {
            get
            {
                return this.providerNameField;
            }
            set
            {
                this.providerNameField = value;
            }
        }

        /// <remarks/>
        public string MerchantId
        {
            get
            {
                return this.merchantIdField;
            }
            set
            {
                this.merchantIdField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ArrayOfClaimResponseClaimResponseClaimDetails
    {

        private string stringField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ArrayOfClaimResponseClaimResponseClaimDetailsStr
    {

        private string stringField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
        public string @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }
    }


    #endregion
}
