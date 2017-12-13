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
using Xceed.Wpf.Toolkit;
using System.Text.RegularExpressions;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for Eftpos.xaml
    /// </summary>
    public partial class Eftpos : Page
    {
        private MainWindow main;
        private bool print_receipts { get { return Properties.Settings.Default.PromptEftposReceipt; } }
        private bool prompt_customer_receipt { get { return Properties.Settings.Default.PrintEftposReceipt; } }

        public Eftpos()
        {
            InitializeComponent();
            main = (MainWindow) Application.Current.MainWindow;
        }

        private void SaleButton_Click(object sender, RoutedEventArgs e)
        {
            SaleButton.IsEnabled = false;
            if (main.hicaps == null)
            {
                Debug.WriteLine("SaleButton_Click: no hicaps control!");
                SaleButton.IsEnabled = true;
                return;
            }
        //    main.hicaps.DisplayStatusWindow(true);

            HicapsConnectControl.HicapsConnectControl.SaleRequest request = null;
            
            Decimal cashout;
            Decimal sale;

            bool isCashout = Decimal.TryParse(Utils.stripCurrencyFormatting(CashoutTextBox.Text), 
                                                out cashout);
            bool isSale = Decimal.TryParse(Utils.stripCurrencyFormatting(SaleTextBox.Text), 
                                                out sale);
            bool doCashout = false;

            if (isCashout && cashout > 0 && isSale)
            {
                request = new HicapsConnectControl.HicapsConnectControl.SaleCashoutRequest();
                ((HicapsConnectControl.HicapsConnectControl.SaleCashoutRequest) request)
                    .CashoutAmount = cashout;
                doCashout = true;
            }
            else if ((!isCashout || cashout <= 0) && isSale)
            {
                request = new HicapsConnectControl.HicapsConnectControl.SaleRequest();
            }
            else if (isCashout && cashout > 0 && !isSale)
            {
                // ignore this case, since the user has clicked sale
                System.Windows.MessageBox.Show("Please enter a sale amount");
                SaleButton.IsEnabled = true;
                return;
            }

            if (main.currentTerminal == null || main.currentTerminal == "")
            {
                System.Windows.MessageBox.Show("No terminal selected. Please select a terminal");
                SaleButton.IsEnabled = true;
                return;
            }

            if (request == null)
            {
                Debug.WriteLine("Error! request didn't get instantiated");
                SaleButton.IsEnabled = true;
                return;
            }

            request.ServerUrl = main.currentTerminal;
            request.PmsKey = Utils.PMSKey;
            request.SoftwareVendorName = Utils.VendorName;

            request.PrintMerchantReceipt = print_receipts;
            request.PrintCustomerReceipt = print_receipts;
            request.PrintCustomerReceiptPrompt = prompt_customer_receipt;
            request.PmsKey = Utils.PMSKey;
            request.SoftwareVendorName = Utils.VendorName;

            request.TransactionAmount = sale;
            request.MerchantId = Utils.getMerchantId(MerchantCombo.Text);


            // we use this for both Sale and SaleCashout since SaleCashoutResponse
            // is a subclass of SaleResponse and we only use the fields from SaleResponse.
            HicapsConnectControl.HicapsConnectControl.SaleResponse response;

            if (doCashout)
            {
                // cast back to SaleCashoutRequest.
                // This is ok because doCashout is true
                main.SyncStatus("Sending Sale Cashout Request...");
                HicapsConnectControl.HicapsConnectControl.SaleCashoutResponse cResponse
                    = main.hicaps.sendSaleCashout(
                    (HicapsConnectControl.HicapsConnectControl.SaleCashoutRequest) request);

                response = cResponse;
            }
            else
            {
                main.SyncStatus("Sending Sale Request...");
                response = main.hicaps.sendSale(request);
            }
            main.ResetStatus();

            if (response.ResponseCode != "00")
            {
                new ResponseError(response.ResponseCode, response.ResponseText).Show();
            }
           // main.hicaps.DisplayStatusWindow(false);
            RefundButton.Visibility = System.Windows.Visibility.Visible;
            SaleButton.IsEnabled = true;

            ResetFields();
        }

        private void ResetFields()
        {
            ProviderNameTextBox.Text = "";
            ProviderNumberTextBox.Text = "";
            ClaimTotalTextBox.Text = "";
            HealthFundBenefitTextBox.Text = "";
            OutstandingAmountTextBox.Text = "";
            SaleTextBox.Text = "";
            CashoutTextBox.Text = "";
            MerchantCombo.SelectedValue = null;
            MerchantCombo.IsEnabled = true;
        }

        private void RefundButton_Click(object sender, RoutedEventArgs e)
        {
            RefundButton.IsEnabled = false;
            //main.hicaps.DisplayStatusWindow(true);

            HicapsConnectControl.HicapsConnectControl.RefundRequest request
                = new HicapsConnectControl.HicapsConnectControl.RefundRequest();

            // get sale amount
            Decimal sale;
            bool saleThere = Decimal.TryParse(Utils.stripCurrencyFormatting(SaleTextBox.Text), out sale);
            if (!saleThere)
            {
                System.Windows.MessageBox.Show("Please enter a valid refund amount into the Sale field");
                RefundButton.IsEnabled = true;
                return;
            }
            request.TransactionAmount = sale;

            // get other fields
            request.MerchantId = Utils.getMerchantId(MerchantCombo.Text);
            request.ServerUrl = main.currentTerminal;
            request.PmsKey = Utils.PMSKey;
            request.SoftwareVendorName = Utils.VendorName;
            
            request.PrintCustomerReceipt = print_receipts;
            request.PrintCustomerReceiptPrompt = prompt_customer_receipt;
            request.PrintMerchantReceipt = print_receipts;

            // get response
            HicapsConnectControl.HicapsConnectControl.RefundResponse response
                = main.hicaps.sendRefund(request);

            if (response.ResponseCode == "00")
            {
                ResetFields();
            }

           // main.hicaps.DisplayStatusWindow(false);
            RefundButton.IsEnabled = true;
        }

        private void ReCalculateTotal()
        {
            Decimal? sale = SaleTextBox.Text.ParseCurrency();
            Decimal? cashout = CashoutTextBox.Text.ParseCurrency();
            Debug.WriteLine("sale: " + sale + ", cashout: " + cashout);

            if (sale != null && cashout != null)
            {
                TotalTextBox.Text = (sale + cashout).ToString();
            }
            else if (sale != null)
            {
                TotalTextBox.Text = sale.ToString();
            }
            else if (cashout != null)
            {
                TotalTextBox.Text = cashout.ToString();
            }
            else
            {
                TotalTextBox.Text = "";
            }
        }

        internal void doClaimToSale()
        {
            Debug.Write("doing claim to sale");
            if (main.claim.LatestResponse != null)
            {
                lock (main.claim.LatestResponse)
                {

                    // fill fields of form
                    ProviderNameTextBox.Text = main.claim.LatestResponse.ProviderName;
                    ProviderNumberTextBox.Text = main.claim.LatestResponse.ProviderNumberId;

                    // todo: is gap really calculated this way?
                    decimal claimTotal = main.claim.LatestResponse.TransactionAmount;
                    decimal benefitAmount = main.claim.LatestResponse.BenefitAmount;
                    decimal gap = claimTotal - benefitAmount;

                    ClaimTotalTextBox.Text = claimTotal.ToString("C");
                    HealthFundBenefitTextBox.Text = benefitAmount.ToString("C");
                    OutstandingAmountTextBox.Text = gap.ToString("C");
                    SaleTextBox.Text = gap.ToString();

                    // switch to eftpos tab
                    main.Tabs.SelectedItem = main.EftposTab;
                    RefundButton.Visibility = System.Windows.Visibility.Hidden;

                    // select correct merchant and disable combo
                    for (int i = 0; i < MerchantCombo.Items.Count; ++i)
                    {
                        string merc = Utils.getMerchantId((string)MerchantCombo.Items.GetItemAt(i));
                        if (merc == main.claim.LatestResponse.MerchantId)
                        {
                            MerchantCombo.SelectedIndex = i;
                        }
                    }
                    MerchantCombo.IsEnabled = false;
                }
            }
        }

        public void reEnableMerchantMenu()
        {
            MerchantCombo.IsEnabled = true;
        }

        private void SaleTextBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ReCalculateTotal();
        }

        private void CashoutTextBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ReCalculateTotal();
        }

        private void SaleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.OnlyCurrencyCharacters();
        }

        private void CashoutTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.OnlyCurrencyCharacters();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
        }
    }
}