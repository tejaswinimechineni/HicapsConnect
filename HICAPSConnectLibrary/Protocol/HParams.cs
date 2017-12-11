using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HICAPSConnectLibrary.Protocol;

namespace HICAPSConnectLibrary.Protocol
{
    public class HParams
    {
        public string CardNumber { get; set; }

        private Dictionary<int, string> _patientNames = new Dictionary<int, string>();
        public Dictionary<int, string> PatientNames
        {
            get { return _patientNames; }
            set { _patientNames = value; }
        }

        public string CardType { get; set; }

        private string _expiryDate;
        public string ExpiryDate
        {
            get
            {
                if (string.IsNullOrEmpty(_expiryDate))
                {
                    return "    ";
                }
                else
                {
                    return _expiryDate;
                }
            }
            set { _expiryDate = value; }
        }

        public string TrackData { get; set; }

        public string TrackData2 { get; set; }

        private string _merchantId = DataQuery.getDefaultMerchantId();
        public string MerchantId
        {
            get { return _merchantId; }
            set { _merchantId = value; }
        }

        private string _providerId = DataQuery.getDefaultProviderId();
        public string ProviderId
        {
            get { return _providerId; }
            set { _providerId = value; }
        }

        private string _terminalId =DataQuery.getDefaultTerminalId();
        public string TerminalId
        {
            get { return _terminalId; }
            set { _terminalId = value; }
        }
        private decimal _surchargePercent = 0.1M;
        public decimal SurchargePercent
        {
            get { return _surchargePercent; }
            set { _surchargePercent = value; }
        }

        private decimal _benefitPercent = 0.6M;
        public decimal BenefitPercent
        {
            get { return _benefitPercent; }
            set { _benefitPercent = value; }
        }

        public string MembershipId { get; set; }

        private string _providerName = "PROVIDER NAME";
        public string ProviderName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }

        public string CardDescription { get; set; }

        private List<HMessage> _transactionList = new List<HMessage> ();

        public List<HMessage> TransactionList
        {
            get { return _transactionList; }
            set { _transactionList = value; }
        }

        public List<string> MerchantIdList = new List<string>();
        public string IPAddress { get; set; }

        public string IPPort { get; set; }
        public string UDPPort { get; set; }

        public string NetworkName { get; set; }

        private string _version = "EVO_01.00.01";
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }
    }
}
