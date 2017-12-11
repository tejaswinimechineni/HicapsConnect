using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Used to request that the terminal return a Settlement list of all Transactions that have been processed through the Terminal.
    /// If No ProviderNumberId is set, then all transactions for All providers will be returned, 
    /// otherwise just the individual providers transactions will be returned.
    /// </summary>
   public class HicapsTotalsRequest : BaseRequest 
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
        private bool _todayTransactions;

        public bool TodayTransactions
        {
            get { return _todayTransactions; }
            set { _todayTransactions = value; }
        }
        private bool _printReceiptOnTerminal;
        public bool PrintReceiptOnTerminal
        {
            get { return _printReceiptOnTerminal; }
            set { _printReceiptOnTerminal = value; }
        }
        public override bool validateMessage(ref string validationMessage)
        {
            validationMessage = "";

            validationMessage += validateProviderNumberId(ProviderNumberId);
            return checkValidationMessage(validationMessage);


        }
    }
}
