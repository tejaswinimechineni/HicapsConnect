using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Reporting.WinForms;
using System.Windows;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Diagnostics;
using HicapsConnectClient12.Properties;
using System.Reflection;

namespace HicapsConnectClient12
{
    public enum ReceiptType { Claim, Cancel, Quote }
    public enum ReceiptFor { Provider, Customer }

    public class PharmaceuticalReceipt
    {
        // Printing details
        private string siteName = null;
        public string SiteName { get { return siteName; } set { siteName = value; } }
        private string siteAddress = null;
        public string SiteAddress { get { return siteAddress; } set { siteAddress = value; } }
        private string phone = null;
        public string Phone { get { return phone; } set { phone = value; } }
        private string fax = null;
        public string Fax { get { return fax; } set { fax = value; } }
        private string email = null;
        public string Email { get { return email; } set { email = value; } }

        // printing logic taken from 
        // http://msdn.microsoft.com/en-us/library/ms252091.aspx
        private int m_currentPageIndex;
        private IList<Stream> m_streams;

        // Receipt details
        private ReceiptFor receiptFor;
        private ReceiptType receiptType;
        private HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse claim; //GJ changed to claimPharmResponse
        private HicapsConnectControl.HicapsConnectControl.ClaimCancelResponse cancel;
        private HicapsConnectControl.HicapsConnectControl.QuotePharmResponse quote;

        // class data
        private MainWindow main = (MainWindow) Application.Current.MainWindow;

        public PharmaceuticalReceipt() { }

        public PharmaceuticalReceipt(HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse c,
                                        ReceiptFor f)
        {
            receiptType = ReceiptType.Claim;
            receiptFor = f;
            claim =  c; //GJ changed to claimPharmResponse
        }

        public PharmaceuticalReceipt(HicapsConnectControl.HicapsConnectControl.ClaimCancelResponse c,
                                        ReceiptFor f,
                                        HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse r)
        {
            receiptType = ReceiptType.Cancel;
            receiptFor = f;
            cancel = c;
            claim = r; //GJ changed to claimPharmResponse
        }

        public PharmaceuticalReceipt(HicapsConnectControl.HicapsConnectControl.QuotePharmResponse c)
        {
            receiptType = ReceiptType.Quote;
            quote = c;
        }

        public static bool siteInfoExists()
        {
            bool result = exist(Settings.Default.SiteAddress)
                || exist(Settings.Default.SiteEmail)
                || exist(Settings.Default.SiteFax)
                || exist(Settings.Default.SiteName)
                || exist(Settings.Default.SitePhone);

            return result;
        }

        private static bool exist(string s)
        {
            return s != null && s != "";
        }

        public void receiptSetup()
        {
            // load site info from application settings
            loadSiteInfo();

            // grab site details
            if (!(new ReceiptSetup(this).ShowDialog() ?? false))
            {
                return;
            }

            // update application settings
            saveSiteInfo();
        }

        public void setUpReport(LocalReport myReport, bool hackMargins)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream report = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + "RDLC" + ".ReceiptProviderCopy.rdlc");
            if (hackMargins)
            {
                // add a margin to all sides by hacking the RDLC before passing it to 
                // the report viewer control. This isn't currently used.
                // the .ToIEnum extension reads the stream line-by-line.
                var streamString = new StreamReader(report, Encoding.UTF8).ReadToEnd();

                string filtered = streamString.Replace(
                    @"    </PageFooter>
    <PageHeight>29.7cm</PageHeight>
    <PageWidth>21cm</PageWidth>
    <LeftMargin>0cm</LeftMargin>
    <RightMargin>0cm</RightMargin>
    <TopMargin>0cm</TopMargin>
    <BottomMargin>0cm</BottomMargin>
    <ColumnSpacing>0.13cm</ColumnSpacing>
    <Style />
  </Page>", @"    </PageFooter>
    <PageHeight>29.7cm</PageHeight>
    <PageWidth>21cm</PageWidth>
    <LeftMargin>0.5cm</LeftMargin>
    <RightMargin>2.3cm</RightMargin>
    <TopMargin>0cm</TopMargin>
    <BottomMargin>0cm</BottomMargin>
    <ColumnSpacing>0.13cm</ColumnSpacing>
    <Style />
  </Page>");
                // load data into memorystream, use this to load report
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Position = 0;
                    StreamWriter wr = new StreamWriter(ms, Encoding.UTF8);
                    wr.Write(streamString);
                    wr.Flush();
                    ms.Position = 0;
                    myReport.LoadReportDefinition(ms);
                }
            }
            else
            {
                myReport.LoadReportDefinition(report);
            }

            myReport.EnableExternalImages = true;

            // we're not using the RDLC's data sources, add data sources to a dictionary
            myReport.DataSources.Clear();
            Dictionary<String, object> DataSourceList = new Dictionary<string, object>();
            DataSourceList.Add("BillPaymentDataSet", getBillPaymentDetails());
            DataSourceList.Add("BillingDataSet", getBillingDetails());
            DataSourceList.Add("BillingDetails", getBillingDetailDetails());
            DataSourceList.Add("OtherPayeeDataSet", getOtherPayeeDetails());
            DataSourceList.Add("TableAttributeDataSet", getClinicAttributeDetails());
            DataSourceList.Add("GetPersonTotalBillBalance", GetPersonTotalBillBalance());
            DataSourceList.Add("AddressDetailsBO", getAddressDetails());
            DataSourceList.Add("HubDetails", getHubDetails());
            DataSourceList.Add("ReportHeaderBO", getReportHeader());
            DataSourceList.Add("SiteInfoDataSet", getSiteInfo());
            DataSourceList.Add("ReceiptInfo", getReceiptInfo());

            // add to report
            foreach (var dataSourceRow in DataSourceList)
            {
                ReportDataSource myReportDataSource = new Microsoft.Reporting.WinForms.ReportDataSource(dataSourceRow.Key, dataSourceRow.Value);
                myReport.DataSources.Add(myReportDataSource);
                myReport.Refresh();
            }
        }

        public void printReport(bool preview)
        {
            try
            {
                loadSiteInfo();

                LocalReport myReport = new LocalReport();
                setUpReport(myReport, false);

                // render report as EMF and print it (see printing helper methods)
                Export(myReport);
                try
                {
                    Print(preview);
                }
                catch (Exception e)
                {
                    
                    MessageBox.Show("couldn't print receipt: " + e.Message);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("coudn't generate receipt: " + ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private void loadSiteInfo()
        {
            siteName = Settings.Default.SiteName;
            siteAddress = Settings.Default.SiteAddress;
            phone = Settings.Default.SitePhone;
            fax = Settings.Default.SiteFax;
            email = Settings.Default.SiteEmail;
        }

        private void saveSiteInfo()
        {
            Settings.Default.SiteName = siteName;
            Settings.Default.SiteAddress = siteAddress;
            Settings.Default.SitePhone = phone;
            Settings.Default.SiteFax = fax;
            Settings.Default.SiteEmail = email;
            Settings.Default.Save();
        }

        private IEnumerable<object> getSiteInfo()
        {
            List<object> l = new List<object>();
            l.Add(new
            {
                SiteName = siteName,
                SiteAddress = siteAddress,
                SitePhone = phone,
                SiteFax = fax,
                SiteEmail = email
            });
            return l;
        }

        private IEnumerable<object> getReceiptInfo()
        {
            List<object> l = new List<object>();
            string rtype = receiptType == ReceiptType.Claim ? "Claim" :
                    (receiptType == ReceiptType.Quote ? "Quote" : "Cancel");
            l.Add(new
            {
                ReceiptFor = receiptFor == ReceiptFor.Customer ? "Customer" : "Provider",
                ReceiptType = rtype
            });
            return l;
        }

        private IEnumerable<object> GetPersonTotalBillBalance()
        {
            List<object> l = new List<object>();

            return l;
        }

        private IEnumerable<object> getReportHeader()
        {
            List<object> l = new List<object>();
            l.Add(new { HicapFormat = "A4" });
            return l;
        }

        private IEnumerable<object> getHubDetails()
        {
            List<object> l = new List<object>();
            switch (receiptType)
            {
                case ReceiptType.Claim:
                case ReceiptType.Quote:
                    // QuoteResponse is a subclass of ClaimResponse so we can do this.
                    HicapsConnectControl.HicapsConnectControl.ClaimResponse c = claim;
                    if (receiptType == ReceiptType.Quote) c = quote;
                    l.Add(new
                    {
                        MerchantNumber = c.MerchantId,
                        Cardnumber = c.PrimaryAccountNumber,
                        Membership = c.MembershipId,
                        Authnumber = c.RrnNumber,
                        CreateDatetime = c.ResponseTime,
                        TerminalNumber = c.TerminalId,
                        HicapsCodeDescription = c.ResponseCode + " - " + c.ResponseText,

                        // not in original RDLC file
                        Fund = getFundShortName(c.PrimaryAccountNumber)
                    });
                    break;
                case ReceiptType.Cancel:
                    l.Add(new
                    {
                        MerchantNumber = cancel.MerchantId,
                        Cardnumber = cancel.PrimaryAccountNumber,
                        Membership = cancel.MembershipId,
                        Authnumber = cancel.RrnNumber,
                        CreateDatetime = cancel.ResponseTime,
                        TerminalNumber = cancel.TerminalId,
                        HicapsCodeDescription = cancel.ResponseCode + " - " + cancel.ResponseText,

                        // not in original RDLC file
                        Fund = getFundShortName(cancel.PrimaryAccountNumber)
                    });
                    break;
                default:
                    break;
            }
            
            return l;
        }

        private string getFundShortName(string cardNo)
        {
            string fund = "";
            if (main != null && main.cards != null && cardNo != null)
            {
                foreach (string prefix in main.cards.Keys)
                {
                    if (cardNo.StartsWith(prefix.Substring(0,8)))
                    {
                        fund = main.cards[prefix];
                        break;
                    }
                }
            }

            return fund;
        }

        private IEnumerable<object> getAddressDetails()
        {
            List<object> l = new List<object>();
            return l;
        }

        private IEnumerable<object> getClinicAttributeDetails()
        {
            List<object> l = new List<object>();
            return l;
        }

        private IEnumerable<object> getOtherPayeeDetails()
        {
            List<object> l = new List<object>();
            return l;
        }

        private IEnumerable<object> getBillingDetailDetails()
        {
            List<object> l = new List<object>();

            // create a map of patient numbers to names (if present)
            var m = new Dictionary<string, string>();
            var patientNameDetails = new List<string>();
            string[] patientNameDetailsStr = new string[0];

            // find name details
            switch (receiptType)
            {
                case ReceiptType.Quote: case ReceiptType.Claim: case ReceiptType.Cancel: 
                    var c = claim;
                    if (receiptType == ReceiptType.Quote) c = quote;
                    if (c != null)
                    {
                        patientNameDetails = c.PatientNameDetails;
                        patientNameDetailsStr = c.PatientNameDetailsStr;
                    }
                    break;
                default:
                    break;
            }

            if (patientNameDetails != null)
            {
                foreach (string s in patientNameDetails)
                {
                    if (s.Length > 2) m[s.Substring(0, 2)] = s.Substring(2);
                }
            }
            if (patientNameDetailsStr != null)
            {
                foreach (string s in patientNameDetailsStr)
                {
                    if (s.Length > 2) m[s.Substring(0, 2)] = s.Substring(2);
                }
            }


            // grab billing details
            switch (receiptType)
            {
                // Scopes are used within these cases so they can all use
                // the 'c' variable name (the code is identical)
                case ReceiptType.Claim:
                case ReceiptType.Quote:
                    {
                        var c = claim;

                        

                        if (receiptType == ReceiptType.Quote) c = quote;
                        if (c != null)
                        {
                            int scriptNumberPosition = 0;
                            foreach (string line in c.ClaimDetails)
                            {
                                ClaimItemDetails item = line.ParseClaimDetails(c.TransactionDate);
                                if (item == null) continue;
                                string description = "";
                                if (main.claim.ItemsDictionary.ContainsKey(item.ItemNumber))
                                {
                                    description = main.claim.ItemsDictionary[item.ItemNumber];
                                }else if (main.claim.ItemsDictionary.ContainsKey(item.ItemNumber.Trim()))
                                {
                                    description = main.claim.ItemsDictionary[item.ItemNumber.Trim()];
                                }
                                else if (main.claim.ItemsDictionary.ContainsKey(item.ItemNumber.TrimStart('0')))
                                    {
                                        description = main.claim.ItemsDictionary[item.ItemNumber.TrimStart('0')];
                                    }
                                l.Add(new
                                {
                                    ProviderNumberId = c.ProviderNumberId,
                                    EmployeeName = c.ProviderName,
                                    ServiceDate = item.DateOfService,
                                    ItemNumber = item.ItemNumber,
                                    ItemDescription = description,
                                    ScriptNumber = c.ScriptDetails[scriptNumberPosition], //GJ ADDED
                                    Bodypart = item.BodyPartNumber,
                                    Total = item.ClaimAmount,
                                    AmountPaid = item.BenefitAmount,
                                    AmountToPay = item.ClaimAmount - item.BenefitAmount,
                                    PatientId = item.PatientID,
                                    PatientName = m.ContainsKey(item.PatientID) ? m[item.PatientID] : "",
                                    ItemResponseCode = item.ItemResponseCode,
                                });
                                scriptNumberPosition ++;
                            }

                            // account for case where there were no claim details
                            if (c.ClaimDetails.Count == 0)
                            {
                                l.Add(new
                                {
                                    ProviderNumberId = c.ProviderNumberId,
                                    EmployeeName = c.ProviderName
                                });
                            }
                        }
                        break;
                    }
                case ReceiptType.Cancel:
                    {
                        var c = cancel;
                        if (c != null)
                        {
                            foreach (string line in c.ClaimDetails)
                            {
                                ClaimItemDetails item = line.ParseClaimDetails(c.TransactionDate);
                                if (item == null) continue;
                                string description = "";
                                if (main.claim.ItemsDictionary.ContainsKey(item.ItemNumber))
                                {
                                    description = main.claim.ItemsDictionary[item.ItemNumber];
                                }
                                else if (main.claim.ItemsDictionary.ContainsKey(item.ItemNumber.Trim()))
                                {
                                    description = main.claim.ItemsDictionary[item.ItemNumber.Trim()];
                                }
                                else if (main.claim.ItemsDictionary.ContainsKey(item.ItemNumber.TrimStart('0')))
                                {
                                    description = main.claim.ItemsDictionary[item.ItemNumber.TrimStart('0')];
                                }

                                l.Add(new
                                {
                                    ProviderNumberId = c.ProviderNumberId,
                                    EmployeeName = c.ProviderName,
                                    ServiceDate = item.DateOfService,
                                    ItemNumber = item.ItemNumber,
                                    ItemDescription = description,
                                    Bodypart = item.BodyPartNumber,
                                    Total = item.ClaimAmount,
                                    AmountPaid = item.BenefitAmount,
                                    AmountToPay = item.ClaimAmount - item.BenefitAmount,
                                    PatientId = item.PatientID,
                                    PatientName = m.ContainsKey(item.PatientID) ? m[item.PatientID] : "",
                                    ItemResponseCode = item.ItemResponseCode,
                                });
                            }

                            // account for case where there were no claim details
                            if (c.ClaimDetails.Count == 0)
                            {
                                l.Add(new
                                {
                                    ProviderNumberId = c.ProviderNumberId,
                                    EmployeeName = c.ProviderName
                                });
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
            return l;
        }

        private IEnumerable<object> getBillingDetails()
        {
            List<object> l = new List<object>();

            switch (receiptType)
            {
                case ReceiptType.Claim:
                case ReceiptType.Quote:
                    {
                        var c = claim;
                        if (receiptType == ReceiptType.Quote) c = quote;
                        l.Add(new
                        {
                            ProviderNumberId = c.ProviderNumberId,
                            MerchantNumber = c.MerchantId,
                            Total = c.TransactionAmount,
                            AmountPaid = c.BenefitAmount,
                            AmountToPay = c.TransactionAmount - c.BenefitAmount
                        });
                    }
                    break;
                case ReceiptType.Cancel:
                    {
                        var c = cancel;
                        l.Add(new
                        {
                            // todo: hunt for TransactionAmount in claim log
                            ProviderNumberId = c.ProviderNumberId,
                            MerchantNumber = c.MerchantId,
                            Total = 0,
                            AmountPaid = c.BenefitAmount,
                            AmountToPay = 0 - c.BenefitAmount
                        });
                    }
                    break;
                default:
                    break;
            }

            
            return l;
        }

        private IEnumerable<object> getBillPaymentDetails()
        {
            List<object> l = new List<object>();
            switch (receiptType)
            {
                case ReceiptType.Claim:
                case ReceiptType.Quote:
                    {
                        var c = receiptType == ReceiptType.Claim ? claim : quote;
                        if (!(c.ClaimDetails.Count > 0
                            && c.ClaimDetails[0].Length == 26))
                        {
                            return l;
                        }
                        l.Add(new
                        {
                            PatientHcno = c.ClaimDetails[0].Substring(0, 2)
                        });
                    }
                    break;
                case ReceiptType.Cancel:
                    {
                        var c = cancel;
                        if (!(c.ClaimDetails.Count > 0
                            && c.ClaimDetails[0].Length == 26))
                        {
                            return l;
                        }
                        l.Add(new
                        {
                            PatientHcno = c.ClaimDetails[0].Substring(0, 2)
                        });
                    }
                    break;
                default:
                    break;
            }

            
            return l;
        }


        #region printing helpers (see above for source)
        // Routine to provide to the report renderer, in order to
        //    save an image for each page of the report.
        private Stream CreateStream(string name,
          string fileNameExtension, Encoding encoding,
          string mimeType, bool willSeek)
        {
            Stream stream = new MemoryStream();
            m_streams.Add(stream);
            return stream;
        }

        // Handler for PrintPageEvents
        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            Metafile pageImage = new
               Metafile(m_streams[m_currentPageIndex]);

            // Adjust rectangular area with printer margins.
            System.Drawing.Rectangle adjustedRect = new System.Drawing.Rectangle(
                ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
                ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
                ev.PageBounds.Width,
                ev.PageBounds.Height);
            
            // Draw a white background for the report
            ev.Graphics.FillRectangle(System.Drawing.Brushes.White, adjustedRect);

            // Draw the report content
            ev.Graphics.DrawImage(pageImage, adjustedRect);

            // Prepare for the next page. Make sure we haven't hit the end.
            m_currentPageIndex++;
            ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
        }

        // Export the given report as an EMF (Enhanced Metafile) file.
        private void Export(LocalReport report)
        {
            string deviceInfo =
              @"<DeviceInfo>
                        <OutputFormat>EMF</OutputFormat>
                        <PageWidth>8.5in</PageWidth>
                        <PageHeight>11in</PageHeight>
                        <MarginTop>0.25in</MarginTop>
                        <MarginLeft>0.25in</MarginLeft>
                        <MarginRight>0.25in</MarginRight>
                        <MarginBottom>0.25in</MarginBottom>
                    </DeviceInfo>";
            Warning[] warnings;
            m_streams = new List<Stream>();
            report.Render("Image", deviceInfo, CreateStream,
               out warnings);
            foreach (Stream stream in m_streams)
                stream.Position = 0;

            // dump warnings to console
            foreach (Warning w in warnings)
            {
                Debug.WriteLine(w.Message);
            }
        }

        private void Print(bool preview)
        {
            if (m_streams == null || m_streams.Count == 0)
                throw new Exception("Error: no stream to print.");

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
            
            m_currentPageIndex = 0;

            if (!printDoc.PrinterSettings.IsValid)
            {
                Debug.WriteLine("couldn't find default printer");
                throw new Exception("Error: cannot find the default printer.");
            }
            else
            {
                // launch a report preview
                if (preview)
                {
                    var pr = new ReportPreview(this);
                    pr.Title = receiptFor == ReceiptFor.Customer ? "Customer" : "Provider"
                                + " receipt preview";
                    
                    bool? print = pr.ShowDialog();
                }
                /* Print choice option removed, client wants always print first*/
                // Always Print
                printDoc.Print();
               
            }
        }
        #endregion
    }
}
