using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HicapsConnectIntegrate.Class;

namespace HicapsConnectIntegrate.Class
{
    /// <summary>
    /// Contains a  list of all Eftpos Transactions that have been processed through the Terminal.
    /// For .NET languages, you may directly iterate through the List Collections to get each object.
    /// For those using the ActiveX control, you must use the getTrasactionCount and getTransaction Methods 
    /// using a for-next loop.  The getTransactionMethod is a 1-based method.  i.e First object sits at Index 1.
    /// </summary>
   public class EftposTransListingResponse : SaleCashoutResponse  
    {
        private string _subTransCode;

        public string SubTransCode
        {
            get { return _subTransCode; }
            set { _subTransCode = value; }
        }

        private DateTime _transactionDate;

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { _transactionDate = value; }
        }

        private List<SaleResponse> _saleTransactionList;
        public List<SaleResponse> SaleTransactionList
        {
            get { return _saleTransactionList; }
            set { _saleTransactionList = value; }
        }

        private List<CashoutResponse> _cashoutTransactionList;
        public List<CashoutResponse> CashoutTransactionList
        {
            get { return _cashoutTransactionList; }
            set { _cashoutTransactionList = value; }
        }

        private List<SaleCashoutResponse> _saleCashoutTransactionList;
        public List<SaleCashoutResponse> SaleCashoutTransactionList
        {
            get { return _saleCashoutTransactionList; }
            set { _saleCashoutTransactionList = value; }
        }

        private List<RefundResponse> _refundTransactionList;
        public List<RefundResponse> RefundTransactionList
        {
            get { return _refundTransactionList; }
            set { _refundTransactionList = value; }
        }

        private List<EftposDepositResponse> _eftposDepositTransactionList;
        public List<EftposDepositResponse> EftposDepositTransactionList
        {
            get { return _eftposDepositTransactionList; }
            set { _eftposDepositTransactionList = value; }
        }
        public EftposTransListingResponse()
        {
            EftposDepositTransactionList = new List<EftposDepositResponse>();
            RefundTransactionList = new List<RefundResponse>();
            SaleCashoutTransactionList = new List<SaleCashoutResponse>();
            SaleTransactionList = new List<SaleResponse>();
            CashoutTransactionList = new List<CashoutResponse>();
        }
    }
}
