using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Contains either the basic return Data, or the elements from the last transaction.
    /// </summary>
   public class PrintLastReceiptResponse : ClaimResponse 
    {
        private string _subTransCode;

        public string SubTransCode
        {
            get { return _subTransCode; }
            set { _subTransCode = value; }
        }
              
        //private string _rrnNumber;

        //public string RrnNumber
        //{
        //    get { return _rrnNumber; }
        //    set { _rrnNumber = value; }
        //}
        private string _invoiceNumber;

        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set { _invoiceNumber = value; }
        }
       
    }
}
