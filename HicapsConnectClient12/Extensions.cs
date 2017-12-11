using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows;
using System.IO;

namespace HicapsConnectClient12
{
    static class Extensions
    {

        public static IEnumerable<string> ToIEnum(this StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                yield return sr.ReadLine();
            }
        }


        // a hack to bypass the inability to set ReadOnly = false on messages
        public static HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse
            CloneWithoutReadonlySet(this HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse c)
        {
            return new HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse
            {
                MsgId = c.MsgId,
                FormatVersion = c.FormatVersion,
                RequestResponseIndicator = c.RequestResponseIndicator,
                MoreIndicator = c.MoreIndicator,
                ServerUrl = c.ServerUrl,
                ComputerName = c.ComputerName,
                SoftwareVendorName = c.SoftwareVendorName,
                ComPort = c.ComPort,
                ResponseTime = c.ResponseTime,
                ResponseText = c.ResponseText,
                ResponseCode = c.ResponseCode,
                ServerVersion = c.ServerVersion,
                ClientVersion = c.ClientVersion,
                TerminalVersion = c.TerminalVersion,
                PrimaryAccountNumber = c.PrimaryAccountNumber,
                ExpiryDate = c.ExpiryDate,
                TransactionAmount = c.TransactionAmount,
                BenefitAmount = c.BenefitAmount,
                ProviderNumberId = c.ProviderNumberId,
                MembershipId = c.MembershipId,
                TransactionDate = c.TransactionDate,
                TerminalId = c.TerminalId,
                RrnNumber = c.RrnNumber,
                ClaimDetails = c.ClaimDetails,
                ClaimDetailsStr = c.ClaimDetailsStr,
                PatientNameDetails = c.PatientNameDetails,
                PatientNameDetailsStr = c.PatientNameDetailsStr,
                ProviderName = c.ProviderName,
                MerchantId = c.MerchantId,
            };
        }

        public static HicapsConnectControl.HicapsConnectControl.QuotePharmResponse
            CloneWithoutReadonlySet(this HicapsConnectControl.HicapsConnectControl.QuotePharmResponse c)
        {
            return new HicapsConnectControl.HicapsConnectControl.QuotePharmResponse
            {
                MsgId = c.MsgId,
                FormatVersion = c.FormatVersion,
                RequestResponseIndicator = c.RequestResponseIndicator,
                MoreIndicator = c.MoreIndicator,
                ServerUrl = c.ServerUrl,
                ComputerName = c.ComputerName,
                SoftwareVendorName = c.SoftwareVendorName,
                ComPort = c.ComPort,
                ResponseTime = c.ResponseTime,
                ResponseText = c.ResponseText,
                ResponseCode = c.ResponseCode,
                ServerVersion = c.ServerVersion,
                ClientVersion = c.ClientVersion,
                TerminalVersion = c.TerminalVersion,
                PrimaryAccountNumber = c.PrimaryAccountNumber,
                ExpiryDate = c.ExpiryDate,
                TransactionAmount = c.TransactionAmount,
                BenefitAmount = c.BenefitAmount,
                ProviderNumberId = c.ProviderNumberId,
                MembershipId = c.MembershipId,
                TransactionDate = c.TransactionDate,
                TerminalId = c.TerminalId,
                RrnNumber = c.RrnNumber,
                ClaimDetails = c.ClaimDetails,
                ClaimDetailsStr = c.ClaimDetailsStr,
                PatientNameDetails = c.PatientNameDetails,
                PatientNameDetailsStr = c.PatientNameDetailsStr,
                ProviderName = c.ProviderName,
                MerchantId = c.MerchantId,
            };
        }


        public static string GetTermId(this string s)
        {
            string[] a = s.Split(':');
            return a.Length >= 3 ? a[2] : null;
        }

        internal static string NullTrim(this string myString)
        {
            return (myString ?? "").Trim();
        }

        internal static string ToCurrency(this decimal myValue)
        {
            return (myValue.ToString("C") ?? "").Trim();
        }
        internal static string ToCurrency(this decimal? myValue)
        {
            return (myValue ?? 0).ToString("C") ?? "".Trim();
        }
        internal static bool OnlyCurrencyCharacters(this string s)
        {
            bool answer = true;
            int dummy;
            foreach (char c in s)
            {
                answer = answer && (int.TryParse(c.ToString(), out dummy)
                                    || c == '$' || c == '.');
            }
            return answer;
        }
        internal static bool OnlyNumbers(this string s)
        {
            bool answer = true;
            int dummy;
            foreach (char c in s)
            {
                answer = answer && int.TryParse(c.ToString(), out dummy);
            }
            return answer;
        }
        internal static decimal? ParseCurrency(this string input)
        {
            decimal d;
            return Decimal.TryParse((input ?? "").Trim()
                                                 .Replace("$", "")
                                                 .Replace("+", "")
                                                 .Replace("_", "")
                                                 .Replace(",", "")
                                                 .Trim()
                                             , out d)
                   ? (decimal?) d
                   : null;
        }
        internal static string LimitLength(this string s, int maxLen)
        {
            return s.Length <= maxLen ? s : s.Substring(0, maxLen);
        }
        internal static string SubstringOrEmpty(this string s, int startIndex)
        {
            return s.Length <= startIndex ? "" : s.Substring(startIndex);
        }
        internal static string SubstringOrNull(this string s, int startIndex)
        {
            return s.Length <= startIndex ? null : s.Substring(startIndex);
        }
        internal static string JustItemNumber(this string s)
        {
            return s.Length <= 4 ? s : s.Substring(0, 4);
        }
        internal static char CharAt(this string s, int i)
        {
            return s.Length < i ? s[i] : (char)0;
        }
        internal static DateTime? ToDateTime(this string s)
        {
            DateTime d;
            return DateTime.TryParse(s, out d) ? (DateTime?)d : null;
        }
        internal static decimal? ToDecimal(this string s)
        {
            decimal d;
            return decimal.TryParse(s, out d) ? (decimal?)d : null;
        }
        internal static ClaimItemDetails ParseClaimDetails(this string s, DateTime claimDate)
        {
            string[] fields = new string[7];
            if (s.Length == 26)
            {
                // parse date
                string day = s.Substring(8, 2);
                string month = s.Substring(10, 2);
                int iday, imonth;
                bool validDay = int.TryParse(day, out iday);
                bool validMonth = int.TryParse(month, out imonth);
                if (!(validDay && validMonth)) return null;
                DateTime dos = new DateTime(claimDate.Year, imonth, iday);
                
                // parse claim and benefit amounts
                string claim = s.Substring(12, 6);
                string benefit = s.Substring (18, 6);
                decimal dclaim, dbenefit;
                bool validClaim = decimal.TryParse(claim, out dclaim);
                bool validBenefit = decimal.TryParse(benefit, out dbenefit);
                if (!(validClaim && validBenefit)) return null;

                // divide by 100 to account for decimal place
                dclaim = dclaim / 100;
                dbenefit = dbenefit / 100;

                return new ClaimItemDetails
                {
                    PatientID = s.Substring(0, 2),
                    ItemNumber = s.Substring(2, 4),
                    BodyPartNumber = s.Substring(6, 2),
                    DateOfService = dos,
                    ClaimAmount = dclaim,
                    BenefitAmount = dbenefit,
                    ItemResponseCode = s.Substring(24, 2)
                };
            }
            else return null;
        }


        public static string DumpControlTemplate(this Control ctrl)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = true
            };

            StringBuilder strbuild = new StringBuilder();
            XmlWriter xmlwrite = XmlWriter.Create(strbuild, settings);
            XamlWriter.Save(ctrl.Template, xmlwrite);
            return strbuild.ToString();
        }
    }

    internal class ClaimItemDetails
    {
        public string PatientID { get; set; }
        public string ItemNumber { get; set; }
        public string BodyPartNumber { get; set; }
        public DateTime DateOfService { get; set; }
        public decimal ClaimAmount { get; set; }
        public decimal BenefitAmount { get; set; }
        public string ItemResponseCode { get; set; }
    }
}
