using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Send a request to the terminal to print or return the last transaction that was been send to the terminal
    /// </summary>
   public class PrintLastReceiptRequest : BaseRequest 
    {
        private string _acquirerId;
        public string AcquirerId
        {
            get { return _acquirerId; }
            set { _acquirerId = value; }
        }

        private bool _printReceiptOnTerminal;
        public bool PrintReceiptOnTerminal
        {
            get { return _printReceiptOnTerminal; }
            set { _printReceiptOnTerminal = value; }
        }
    }
}
