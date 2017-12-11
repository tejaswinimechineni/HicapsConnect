using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using HicapsConnectClient12.Properties;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for Cancel.xaml
    /// </summary>
    public partial class Cancel : Page
    {
        MainWindow main;
        PleaseWait wait = new PleaseWait();

        public Cancel()
        {
            InitializeComponent();
            main = (MainWindow) Application.Current.MainWindow;
        }

        public void UpdateTransactions()
        {
            // update the list view with filtered down log
            TransactionListBox.ItemsSource = main.claimLog.Log.Where(r => 
            {
                bool shouldUse = r.TransactionDate.Date == DateTime.Today;
                   //    && (r.TerminalId ?? "").Trim() == (main.currentTerminal.GetTermId() ?? "");
                return shouldUse;
            });
            TransactionListBox.Items.Refresh();
        }

        private void SendManualButton_Click(object sender, RoutedEventArgs e)
        {
            string RRN = RRNTextBox.Text;
            string provider = ProviderCombo.Text;
            decimal transAmount;
            bool transAmountExist = Decimal.TryParse(Utils.stripCurrencyFormatting(BenifitAmountTextBox.Text), 
                                                        out transAmount);
            if (!transAmountExist) transAmount = 0;

            bool success = sendCancel(RRN, provider, transAmount);
            if (success)
            {
                ClearManualFields();
            }
        }

        private void ClearManualFields()
        {
            ProviderCombo.SelectedIndex = -1;
            RRNTextBox.Clear();
            BenifitAmountTextBox.Text = "";
        }

        private bool sendCancel(string RRN, string provider, decimal transAmount)
        {
            main.hicaps.DisplayStatusWindow(true);

            HicapsConnectControl.HicapsConnectControl.ClaimCancelRequest request
                = new HicapsConnectControl.HicapsConnectControl.ClaimCancelRequest();
            
            // set provider
            request.ProviderNumberId = Utils.getProviderNumberId(provider);
            if (request.ProviderNumberId == null)
            {
                // inform user that a correct provider is needed
                MessageBox.Show("The selected provider is not valid. Please select another one.");
                return false;
            }

            request.PmsKey = Utils.PMSKey;
            request.SoftwareVendorName = Utils.VendorName;

            // set RRN
            request.RrnNumber = RRN;
            request.TransactionAmount = transAmount;

            request.PrintReceiptOnTerminal = Properties.Settings.Default.PrintReceiptCancelClaim;
            request.ServerUrl = main.currentTerminal;

            // send request
            main.SyncStatus("Sending Claim Cancel Request");
            HicapsConnectControl.HicapsConnectControl.ClaimCancelResponse response
                = main.hicaps.sendClaimCancelRequest(request);
            main.ResetStatus();

            if (response.ResponseCode != "00")
            {
                new ResponseError(response.ResponseCode, response.ResponseText).Show();
            }

            
            if (Settings.Default.PrintProviderCancelReceiptLocal)
            {
                //Display wait window
                wait.Visibility = Visibility.Visible;
                ForceUIToUpdate();

                var r = new PharmaceuticalReceipt(response, ReceiptFor.Provider, getSelectedResponse());
                //Close wait window
                wait.Visibility = Visibility.Collapsed;
                
                r.printReport(true);
            }

            // print customer receipt if needed
            if (Settings.Default.PrintCustomerCancelReceiptLocal)
            {
                var r = new PharmaceuticalReceipt(response, ReceiptFor.Customer, getSelectedResponse());
                r.printReport(false);
            }

            main.hicaps.DisplayStatusWindow(false);

            if (response.ResponseCode == "00")
            {
                var claim = main.claimLog.Log.FirstOrDefault(r => r.RrnNumber == RRN);
                main.claimLog.LogCancelledClaim(claim);
                main.claimLog.Log.Remove(claim);
                main.claimLog.SaveLogFile();
            }
            UpdateTransactions();
            return response.ResponseCode == "00";

        }

        private void ShowForms(bool show)
        {
            foreach (var page in Application.Current.Windows.OfType<Page>())
            {
                page.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static void ForceUIToUpdate()
        {
            var frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }        

        private HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse getSelectedResponse()
        {
            if (TransactionListBox.SelectedIndex > -1)
            {
                return (HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse)
                        TransactionListBox.SelectedItem;
            }
            else
            {
                return null;
            }
            
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (TransactionListBox.SelectedIndex >= 0)
            {
                HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse r
                    = getSelectedResponse();

                sendCancel(r.RrnNumber, r.ProviderNumberId, r.BenefitAmount);
            }
        }
        
        private void RRNTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int x;
            e.Handled = !int.TryParse(e.Text, out x);
        }

        private void BenifitAmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.OnlyCurrencyCharacters();
        }

        private void ClearManualButton_Click(object sender, RoutedEventArgs e)
        {
            ClearManualFields();
        }

        private void BenifitAmountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Xceed.Wpf.Toolkit.WatermarkTextBox t
                = (Xceed.Wpf.Toolkit.WatermarkTextBox)BenifitAmountTextBox.Template.FindName("PART_TextBox", BenifitAmountTextBox);
            if (t != null)
            {
                t.TextChanged += new TextChangedEventHandler(BenifitAmountTextBox_TextChanged);
            }
        }

        // a regex we can check as the user types (matches partially typed currency)
        Regex currency = new Regex(@"^\$?(\d*)(\.(\d*))?$");
        private void BenifitAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Xceed.Wpf.Toolkit.WatermarkTextBox t
                = (Xceed.Wpf.Toolkit.WatermarkTextBox)BenifitAmountTextBox.Template.FindName("PART_TextBox", BenifitAmountTextBox);
            Match m = currency.Match(Utils.stripCurrencyFormatting(t.Text));
            GroupCollection g = m.Groups;

            if (t != null && m.Success)
            {
                // auto-add decimal place 
                if (g[1].Length > 4 || g[3].Length > 2)
                {
                    t.Text = "$" + g[1].Value.LimitLength(4) + "." + (g[1].Value.SubstringOrEmpty(4) + g[3].Value).LimitLength(2);
                    t.CaretIndex = t.Text.Length;
                }
            }

            // limit length to 8 to prevent typing long sequences of invalid characters
            if (t != null)
            {
                t.Text = t.Text.LimitLength(8);
            }
        }
    }
}