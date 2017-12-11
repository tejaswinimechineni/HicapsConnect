using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Response object containing the Result of the Sale Transaction.
    /// </summary>
    public class SaleCashoutResponse : SaleResponse
    {
        private decimal _cashoutAmount;

        public decimal CashoutAmount
        {
            get { return _cashoutAmount; }
            set { _cashoutAmount = value; }
        }

        public SaleCashoutResponse() { }
    }
}
