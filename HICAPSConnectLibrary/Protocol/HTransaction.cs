using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HICAPSConnectLibrary.Protocol
{
    /// <summary>
    /// Class containing strings for all transaction types the terminal supports
    /// </summary>
    public static class HTransaction
    {
        #region Claim Transactions
        
        /// <summary>
        /// Cancel Claim Transaction
        /// Value = EF
        /// </summary>
        public const string CancelClaims = "EF";

        /// <summary>
        /// Cancel Settlement Claim Transaction
        /// Value = 24
        /// </summary>
        public const string CancelSettlementClaims = "24";


        /// <summary>
        /// HICAPS Claim Transaction
        /// Value = 21
        /// </summary>
        public const string Claims = "21";

        /// <summary>
        /// Quote Transaction
        /// Value = 22
        /// </summary>
        public const string Quotes = "22";

        /// <summary>
        /// HICAPS Claim Transaction
        /// Value = 23
        /// </summary>
        public const string SettlementClaims = "23";

        /// <summary>
        /// Medicare Claim Transaction
        /// Value = MC
        /// </summary>
        public const string MedicareClaims = "MC";

        /// <summary>
        /// HICAPS Claim Totals Transaction
        /// Value = HT
        /// </summary>
        public const string ClaimTotals = "HT";

        /// <summary>
        /// HICAPS Claim Listing Transaction
        /// Value = HL
        /// </summary>
        public const string ClaimListing = "HL";

        
        #endregion
        #region Terminal Functions
        /// <summary>
        /// Card Read transaction
        /// Value = 50
        /// </summary>
        public const string CardRead = "50";

        /// <summary>
        /// Terminal Test Transaction
        /// Value = D0
        /// </summary>
        public const string TerminalTest = "D0";
        
        /// <summary>
        /// IP Terminal Test Transaction
        /// Value = D0
        /// </summary>
        public const string TerminalIPTest = "D1";

        /// <summary>
        /// Status Message Transaction
        /// Value = TS
        /// </summary>
        public const string TerminalStatus = "TS";

        /// <summary>
        /// Key Check Message Transaction
        /// Value = KC
        /// </summary>
        public const string KeyCheck = "KC";

        /// <summary>
        /// Print Last Receipt Transaction
        /// Value = A0
        /// </summary>
        public const string PrintLastReceipt = "A0";

        #endregion
        #region EFTPos/Merchant

        /// <summary>
        /// EFTPOS Sale Transaction
        /// Value = 20
        /// </summary>
        public const string Sale = "20";

        /// <summary>
        /// EFTPOS Refund Transaction
        /// Value = 26
        /// </summary>
        public const string Refund = "26";

        /// <summary>
        /// Direct Deposit Transaction
        /// Value = DP
        /// </summary>
        public const string EftposDeposit = "DP";

        /// <summary>
        /// EFTPOS Cashout Transaction
        /// Value = E7
        /// </summary>
        public const string Cashout = "E7";

        /// <summary>
        /// EFTPOS Sale and Cashout Transaction
        /// Value = E8
        /// </summary>
        public const string SaleCashout = "E8";


        /// <summary>
        /// HICAPS Eftpos Transaction Listing
        /// Value = EL
        /// </summary>
        public const string EftposListing = "EL";


        #endregion
        #region List Transactions
        /// <summary>
        /// Item List Transaction
        /// Value = A8
        /// </summary>
        public const string AllItemList = "A8";

        /// <summary>
        /// Provider List Transaction
        /// Value = A4
        /// </summary>
        public const string ProviderList = "A4";

        /// <summary>
        /// Card Prefix, and Line count transaction
        /// Value = A5
        /// </summary>
        public const string CardList = "A5";

        /// <summary>
        /// Item Response Codes
        /// Value = A6
        /// </summary>
        public const string ItemResponseList = "A6";

        /// <summary>
        /// Transaction Response List
        /// Value = A7
        /// </summary>
        public const string TransactionResponseList = "A7";

        /// <summary>
        /// All Providers List Transaction
        /// Value = AP
        /// </summary>
        public const string AllProviderList = "AP";

        /// <summary>
        /// All Merchant List Transaction
        /// Value = AM
        /// </summary>
        public const string AllMerchantList = "AM";

        /// <summary>
        /// All Medicare Providers List Transaction
        /// Value = MP
        /// </summary>
        public const string AllMedicareProviderList = "MP";

        /// <summary>
        /// Settlement Transaction
        /// Value = 61
        /// </summary>
        public const string Settlement = "61";

      

       #endregion
        #region Obsoleted Transaction Types

        /// <summary>
        /// Void Transaction
        /// Value = 42
        /// </summary>
        public const string Void = "42";

        /// <summary>
        /// Void Transaction
        /// Value = 55
        /// </summary>
        public const string ReservedVisa = "55";

        #endregion

        public static List<string> ClaimTransactions = new List<string>(){Claims,Quotes,SettlementClaims};
        public static List<string> NoEncryptionTransactions = new List<string>() { CardRead}; 
        /// <summary>
        /// Convert Service Layer Object Name/Type to internal HicapsComm string message array variable.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="myRequest"></param>
        /// <returns></returns>
        public static string getTransactionType<T>(T myRequest)
        {

            switch (myRequest.GetType().ToString())
            {
                case "HicapsConnectIntegrate.Class.SaleRequest":
                    return Sale;
                case "HicapsConnectIntegrate.Class.ClaimPharmRequest":
                case "HicapsConnectIntegrate.Class.ClaimRequest":
                    return Claims;
                case "HicapsConnectIntegrate.Class.QuotePharmRequest":
                case "HicapsConnectIntegrate.Class.QuoteRequest":
                    return Quotes;
                case "HicapsConnectIntegrate.Class.RefundRequest":
                    return Refund;
                // case "Void Not Implemented":
                //	return "Void";
                case "HicapsConnectIntegrate.Class.CardReadRequest":
                    return CardRead;
                // case "55 Not Implemented":
                //	return "55";
                case "HicapsConnectIntegrate.Class.SettlementRequest":
                    return Settlement;
                case "HicapsConnectIntegrate.Class.MerchantListRequest":
                    return ProviderList;
                case "HicapsConnectIntegrate.Class.CardListRequest":
                    return CardList;
                case "HicapsConnectIntegrate.Class.TerminalTestRequest":
                    return TerminalTest;
                case "HicapsConnectIntegrate.Class.CashoutRequest":
                    return Cashout;
                case "HicapsConnectIntegrate.Class.SaleCashoutRequest":
                    return SaleCashout;
                case "HicapsConnectIntegrate.Class.ClaimCancelRequest":
                    return CancelClaims;
                case "HicapsConnectIntegrate.Class.MedicareMerchantListRequest":
                    return AllMedicareProviderList;
                case "HicapsConnectIntegrate.Class.MedicareClaimRequest":
                    return MedicareClaims;
                case "HicapsConnectIntegrate.Class.EftposDepositRequest":
                    return EftposDeposit;
                case "HicapsConnectIntegrate.Class.AllProviderListRequest":
                    return AllProviderList;
                case "HicapsConnectIntegrate.Class.AllMerchantListRequest":
                    return AllMerchantList;
                case "HicapsConnectIntegrate.Class.PrintLastReceiptRequest":
                    return PrintLastReceipt;
                case "HicapsConnectIntegrate.Class.HicapsTotalsRequest":
                    return ClaimTotals;
                case "HicapsConnectIntegrate.Class.HicapsTransListingRequest":
                    return ClaimListing;
                case "HicapsConnectIntegrate.Class.EftposTransListingRequest":
                    return EftposListing;
                case "HicapsConnectIntegrate.Class.KeyCheckRequest":
                    return KeyCheck;
                case "HicapsConnectIntegrate.Class.AllItemResponseCodeListRequest":
                    return ItemResponseList;
                case "HicapsConnectIntegrate.Class.AllTransCodeListRequest":
                    return TransactionResponseList;
                case "HicapsConnectIntegrate.Class.AllItemListRequest":
                    return AllItemList;
                default: return "";
            }
        }

    }
}
