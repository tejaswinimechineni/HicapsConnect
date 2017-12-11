using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HICAPSConnectLibrary.Utils;

namespace HICAPSConnectLibrary.Protocol
{
    public class DataQuery
    {

        private static readonly string STR_DefaultTerminalId = "SA000A";
        private static readonly string STR_DefaultMerchantId = "12345678";
        private static readonly string STR_DefaultMerchantName = "MERCHANT NAME";
        private static readonly string STR_DefaultProviderId = "1234567A";
        private static readonly string STR_DefaultProviderName = "PROVIDER NAME";
       
        #region Merchants
        internal static string getDefaultTerminalId()
        {
            try
            {
                string[] lineData;
                lineData = FileRepo.getFileData(FileRepo.Merchants);
                if (lineData.Length > 0)
                {
                    if (lineData[0].Length >= 6)
                    {
                        return lineData[0].Substring(0, 6) + "  ";
                    }
                    else
                    {
                        // ???? Invalid file or something ?
                        string terminalId = STR_DefaultTerminalId;
                        UpdateMerchantTerminalId(terminalId);
                        return terminalId;
                    }
                }
            }
            catch (Exception) {}
            return STR_DefaultTerminalId;
        }
        internal static void UpdateMerchantTerminalId(string terminalId)
        {
            string[] lineData;
            string[] terminalIdEnd = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            lineData = FileRepo.getFileData(FileRepo.Merchants);
            int linePadding = 63;
            terminalId = (terminalId + new string(' ', 8)).Substring(0, 8);
            for (int i = 0; i < lineData.Count(); i++)
            {
                if (i + 1 == lineData.Count() || i >= 23)
                {
                    terminalId = terminalId.Substring(0, 5) + "X";
                }
                else
                {

                    terminalId = terminalId.Substring(0, 5) + terminalIdEnd[i];
                }
                terminalId = (terminalId + new string(' ', 8)).Substring(0, 8);
                if (!string.IsNullOrEmpty(lineData[i]) && lineData[i].Length > 8)
                {
                    lineData[i] = (terminalId + lineData[i].Substring(8) + new string(' ', linePadding)).Substring(0,
                        linePadding);
                }
            }
            FileRepo.setFileData(FileRepo.Merchants, lineData);
            UpdateProviderTerminalId();
        }
        internal static Dictionary<string, string> getTerminalMerchantList()
        {
            string[] lineData;
            Dictionary<string, string> TerminalMerchantList = new Dictionary<string, string>();
            lineData = FileRepo.getFileData(FileRepo.Merchants);
            foreach (string myRow in lineData)
            {
                if (myRow.Length >= 16)
                {
                    if (!TerminalMerchantList.ContainsKey(myRow.Substring(0, 6)))
                    {
                        TerminalMerchantList.Add(myRow.Substring(0, 6), myRow.Substring(8, 8));
                    }
                }
            }
            return TerminalMerchantList;
        }
        internal static string getMerchantName(string strMerchantName)
        {
            string[] lineData;
            lineData = FileRepo.getFileData(FileRepo.Merchants);
            foreach (string myRow in lineData)
            {
                if (myRow.Length > 24)
                {
                    if (myRow.Substring(8, 8).Trim() == strMerchantName.Trim())
                    {
                        return myRow.Substring(23).Trim();
                    }
                }
            }
            return "";
        }
        internal static string getDefaultMerchantId()
        {
            try
            {
                string[] lineData = FileRepo.getFileData(FileRepo.Merchants);
                if (lineData.Length > 0)
                {
                    return lineData[0].Substring(8, 8);
                }
                else
                {
                    return STR_DefaultMerchantId;
                }
            }
            catch (Exception) { }
            return STR_DefaultMerchantId;
        }
        internal static bool isValidMerchant(string strMerchantName)
        {
            if (!string.IsNullOrEmpty(getMerchantName(strMerchantName)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        #endregion
        #region Providers
        internal static string getDefaultProviderId()
        {
            try
            {
                string[] lineData = FileRepo.getFileData(FileRepo.Providers);
                if ((lineData.Length > 0) && (lineData[0].Length >= 24))
                {
                    return lineData[0].Substring(16, 8);
                }
                else
                {
                    return STR_DefaultProviderId;
                }
            }
            catch (Exception) {}
            return STR_DefaultProviderId;
        }

        internal static string[] getTranscodelist()
        {
            try
            {
                string[] lineData = FileRepo.getFileData(FileRepo.TransCodeList);
                return lineData;
            }
            catch (Exception) {}
            return new string[0];
        }
        internal static string[] getItemResponseCodes()
        {
            string[] lineData = FileRepo.getFileData(FileRepo.ItemResponseCodes);
            return lineData;
        }

        internal static string getProviderName(string providerNumberId)
        {
            string[] lineData = FileRepo.getFileData(FileRepo.Providers);
            foreach (string myRow in lineData)
            {
                if (myRow.Length > 32)
                {
                    if (myRow.Substring(23, 8) == providerNumberId)
                    {
                        return myRow.Substring(31, 16);
                    }
                }
            }
            // Not Found
            return "";
        }
        internal static string getProviderIdFromMerchantId(string merchantId)
        {
            string[] lineData = FileRepo.getFileData(FileRepo.Providers);
            foreach (string myRow in lineData)
            {
                if (myRow.Length > 32)
                {
                    if (myRow.Substring(8, 8) == merchantId)
                    {
                        return myRow.Substring(23, 8);
                    }
                }
            }
            // Not found
            return "";
        }
        internal static string getProviderNameFromMerchantId(string merchantId)
        {
            string providerNumberId = getProviderIdFromMerchantId(merchantId);
            if (!string.IsNullOrEmpty(providerNumberId))
            {
                return getProviderName(providerNumberId);
            }
            // Not found
            return "";
        }
        internal static void UpdateProviderTerminalId()
        {
            string[] lineData = FileRepo.getFileData(FileRepo.Providers);
            Dictionary<string, string> TerminalMerchantList = getTerminalMerchantList();
            int linePadding = 58;
            for (int i = 0; i < lineData.Count(); i++)
            {
                string terminalId = lineData[i].Substring(0, 6);
                string merchantId = lineData[i].Substring(8, 8);
                if (TerminalMerchantList.ContainsValue(merchantId))
                {
                    terminalId = (from p in TerminalMerchantList
                                  where p.Value == merchantId
                                  select p.Key).FirstOrDefault();
                }
                else
                {
                    terminalId = TerminalMerchantList.ElementAt(0).Key;
                    merchantId = TerminalMerchantList.ElementAt(0).Value;
                }
                lineData[i] = ((terminalId + new string(' ', 8)).Substring(0, 8) +
                               (merchantId + new string(' ', 8)).Substring(0, 8) +
                               lineData[i].Substring(16) + new string(' ', linePadding)).Substring(0, linePadding);

            }
            //TODO
            FileRepo.setFileData(FileRepo.Providers, lineData);

            return;
        }
        internal static bool isValidProvider(string strProviderNumber)
        {
            if (!string.IsNullOrEmpty(getProviderName(strProviderNumber).Trim()))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        #endregion




        internal static string getHealthFundLineLimit(string cardNumber)
        {
            string[] lineData = FileRepo.getFileData(FileRepo.CardList);
            foreach (string myRow in lineData)
            {
                if (myRow.Length >= 16)
                {
                    if (myRow.Substring(6, 8) == cardNumber.Substring(0, 8))
                    {
                        return myRow.Substring(14, 2);
                    }
                }
            }
            // Not found
            return "";
        }

        internal static string getHealthFundName(string cardNumber)
        {
            string[] lineData = FileRepo.getFileData(FileRepo.CardList);
            foreach (string myRow in lineData)
            {
                if (myRow.Length >= 16)
                {
                    if (myRow.Substring(6, 8) == cardNumber.Substring(0, 8))
                    {
                        return myRow.Substring(0, 6);
                    }
                }
            }
            // Not found
            return "";
        }
    }
}
