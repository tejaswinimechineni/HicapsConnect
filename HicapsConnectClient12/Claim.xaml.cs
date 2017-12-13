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
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using WpfControls;
using System.IO;
using Microsoft.Reporting.WinForms;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using HicapsConnectClient12.Properties;
using System.Threading;
using System.Windows.Threading;


namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for Claim.xaml
    /// </summary>
    
    public partial class Claim : Page
    {
        public List<ItemRow> claimItems { get; set; }
        public Dictionary<string, string> ItemsDictionary { get; set; }
        internal ObjectDB ItemsFeeDictionary;
        PleaseWait wait = new PleaseWait();

        private MainWindow main;

        private HicapsConnectControl.HicapsConnectControl.ClaimResponse _latestResponse
            = CreateInitialLatestResponseForTesting();
        public HicapsConnectControl.HicapsConnectControl.ClaimResponse LatestResponse
        {
            get
            {
                return _latestResponse;
            }
        }

        private bool _QuoteMode;
        public bool QuoteMode
        {
            get
            {
                return _QuoteMode;
            }
        }
        private static bool testDone = false;

        public Claim()
        {
            claimItems = new List<ItemRow>();
            ItemsDictionary = new Dictionary<string, string>();
            ItemsFeeDictionary = new ObjectDB("ItemFee");
            main = (MainWindow) Application.Current.MainWindow;
            InitializeComponent();
            ClaimDetails.ItemsSource = claimItems;
            ItemNumberInput.TextFilter = StartsWithIncludingEmpty;

            _QuoteMode = false;

            if (Settings.Default.EnablePrice)
            {
                ItemFeeInput.IsEnabled = true;
              // Load in dictionary..
                
            }
            // todo: test report rendering, please remove
            /*Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!testDone)
                {
                    testDone = true;

                    // wait for api to pull items before printing test page
                    Thread.Sleep(4);
                    Debug.WriteLine("Sleep finished!");
                    var rec = new PharmaceuticalReceipt(_latestResponse, ReceiptFor.Provider);
                    rec.printReport();
                }
            }));*/
        }

        public void enterQuoteMode()
        {
            ClaimDetailsGroupBox.Header = "QUOTE DETAILS";
            _QuoteMode = true;

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // See if a row is selected and update that row
            if (ValidateFields() && ClaimDetails.SelectedItem != null)
            {
                string bodyPart = "00";
                if (this.UseAlternateBodyPart.IsChecked ?? false)
                {
                    bodyPart = "01";
                }
                IEditableCollectionView view = ClaimDetails.Items as IEditableCollectionView;
                if (!view.CanAddNew) Debug.WriteLine("Can't add new");
                
                Debug.WriteLine("Item selected! updating");
                ItemRow row = (ItemRow) ClaimDetails.SelectedItem;
                row.PatientId = PatientIdInput.Text;
                row.Description = DescriptionTextBox.Text;
                row.Dos = DOSInput.Value;
                row.ItemFee = ItemFeeInput.Text;
                row.ItemNumber = ItemNumberInput.Text;
                row.BodyPart = bodyPart;
                ClaimDetails.Items.Refresh();
                clearEntryFields(false);
            }
        }

        private void ClaimDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // fill the item details field with the newly selected row,
            // if there is any data to fill
            if (e.AddedItems.Count > 0)
            {
                ItemRow row = (ItemRow) ClaimDetails.SelectedItem;
                PatientIdInput.Text = row.PatientId;
                ItemNumberInput.Text = row.ItemNumber;
                DescriptionTextBox.Text = row.Description;
                DOSInput.Value = row.Dos;
                ItemFeeInput.Text = row.ItemFee;
                ScriptInput.Text = row.ScriptNumber;
                UseAlternateBodyPart.IsChecked = !(row.BodyPart == "00" || string.IsNullOrEmpty(row.BodyPart));
                
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                string bodyPart = "00";
                Debug.WriteLine("Item Number added: " + ItemNumberInput.Text);
                if (this.UseAlternateBodyPart.IsChecked ?? false)
                {
                    bodyPart = "01";
                }
                // Save to cache, auto persists to disk if different

                decimal value;
                if (Decimal.TryParse(ItemFeeInput.Text.ParseCurrency().ToString(), out value))
                {
                    ItemsFeeDictionary.setValue(ItemNumberInput.Text, ItemFeeInput.Text);
                }

                string compoundIndicator = string.Empty;

                if (UseAlternateBodyPart.IsChecked == true)
                    compoundIndicator = "Compound";

                var row = new ItemRow(PatientIdInput.Text,
                                          ItemNumberInput.Text,
                                          DescriptionTextBox.Text,
                                          DOSInput.Value,
                                          bodyPart,
                                          ItemFeeInput.Text,
                                          ScriptInput.Text,
                                          compoundIndicator);
                claimItems.Add(row);
                ClaimDetails.Items.Refresh();

                ClaimDetails.SelectedIndex = -1;
                clearEntryFields(false);
            }
        }

        private bool ValidateFields()
        {
            if (ItemNumberInput.Text.Length != 6 &&
                ItemNumberInput.Text.Length < 3)
            {
                MessageBox.Show("Item Number must be either 3 or 4 characters");
                ItemNumberInput.Focus();
                return false;
            }
            //else if (!ItemNumberInput.Text.OnlyNumbers())
            //{
            //    MessageBox.Show("The Item Number must be numeric only");
            //    ItemNumberInput.Focus();
            //    return false;
            //}
            else if (PatientIdInput.Text.Length != 2 &&
                     PatientIdInput.Text.OnlyNumbers())
            {
                MessageBox.Show("Patient Id must be two numeric characters");
                PatientIdInput.Focus();
                return false;
            }
            else if (DOSInput.Value == null)
            {
                MessageBox.Show("Date of Service must be a valid date");
                DOSInput.Focus();
                return false;
            }
            else if (ItemFeeInput.Value == null)
            {
                MessageBox.Show("Item Fee must be a valid dollar value between $0.00 and $9999.99");
                ItemFeeInput.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(DescriptionTextBox.Text))
            {
                MessageBox.Show("Item must be in the list");
                ItemNumberInput.Focus();
                return false;
            }
            else if (string.IsNullOrEmpty(ScriptInput.Text))
            {
                //MessageBox.Show("Script Number must be filled");
                ScriptInput.Text = "0000000000";
                return true;
            }
            else
            {
                return true;
            }
        }

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            claimItems.Remove(ClaimDetails.SelectedItem as ItemRow);
            ClaimDetails.Items.Refresh();
            clearEntryFields(false);
        }

        private void RemoveAllButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAll();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            clearEntryFields(false);
        }

        private void clearEntryFields(bool resetPatientId)
        {
            if (resetPatientId)
                PatientIdInput.Text = "01";

            ClaimDetails.SelectedIndex = -1;
            ItemNumberInput.Text = "";
            DescriptionTextBox.Clear();
            UseAlternateBodyPart.IsChecked = false;
            DOSInput.Value = DateTime.Today;
            ItemFeeInput.Text = "";
            ScriptInput.Text = "";
        }

        private void RemoveAll()
        {
            claimItems.Clear();
            ClaimDetails.Items.Refresh();
            clearEntryFields(true);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;
            HicapsConnectControl.HicapsConnectControl.ClaimPharmRequest request;
            //HicapsConnectControl.HicapsConnectControl.AllItemListRequest itemRequest;

          //  main.hicaps.DisplayStatusWindow(true);

            if (QuoteMode)
            {
                // fields of a QuoteRequest are filled identically to those of a ClaimRequest.
                request = new HicapsConnectControl.HicapsConnectControl.QuotePharmRequest();
            }
            else
            {
                request = new HicapsConnectControl.HicapsConnectControl.ClaimPharmRequest();
            }

            // set provider
            request.ProviderNumberId = Utils.getProviderNumberId(ProviderCombo.Text);
            if (request.ProviderNumberId == null || request.ProviderNumberId == "00000000")
            {
                // inform user that a correct provider is needed
                MessageBox.Show("The selected provider is not valid. Please select another one.");
                SendButton.IsEnabled = true;
                return;
            }

            // add item details
            if (claimItems.Count < 1)
            {
                MessageBox.Show("There must be at least one claim item");
                SendButton.IsEnabled = true;
                return;
            }

            int i = 0;
            foreach (ItemRow row in claimItems)
            {
                string rowNumber = "Row " + i + 1 + ": ";
                if (row.Dos == null)
                {
                    MessageBox.Show(rowNumber + "Please select a date of service");
                    ClaimDetails.SelectedIndex = i;
                    SendButton.IsEnabled = true;
                    return;
                }
                try
                {
                    
                    if (row.ItemNumber.Length < 3)
                    {
                        MessageBox.Show(rowNumber + "Item Number must be 3 or greater characters");
                        ClaimDetails.SelectedIndex = i;
                        SendButton.IsEnabled = true;
                        return;
                    }
                    //else if (!row.ItemNumber.OnlyNumbers())
                    //{
                    //    MessageBox.Show(rowNumber + "Item Number must consist of digits");
                    //    ClaimDetails.SelectedIndex = i;
                    //    return;
                    //}



                    //GJ:Commented the old way to add the line as for the compound indicator should use the Compound column of each row instead of the UseAlternateBodyPart checkbox
                    //request.addClaimPharmLine(row.PatientId, row.ItemNumber, row.ScriptNumber, UseAlternateBodyPart.IsChecked != null && UseAlternateBodyPart.IsChecked.Value,
                    //    row.Dos ?? DateTime.MinValue,
                    //    row.ItemFee.ParseCurrency() ?? 0);

                    request.addClaimPharmLine(row.PatientId, row.ItemNumber, row.ScriptNumber, row.Compound == "Compound",
                        row.Dos ?? DateTime.MinValue,
                        row.ItemFee.ParseCurrency() ?? 0);
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    SendButton.IsEnabled = true;
                    return;
                }
                i++;
            }

            request.ServerUrl = main.currentTerminal;
            request.PmsKey = Utils.PMSKey;
            request.SoftwareVendorName = Utils.VendorName;
            request.PrintReceiptOnTerminal = Properties.Settings.Default.PrintClaimReceipt;
            

            // send request
            HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse response;
            if (QuoteMode)
            {
                
                // converts the request to a QuoteRequest and the response to a ClaimResponse
                // while calling sendQuoteRequest
                main.SyncStatus("Sending Quote Request...");
                response = main.hicaps.sendQuotePharmRequest( (HicapsConnectControl.HicapsConnectControl.QuotePharmRequest) request);
                
                
            }
            else
            {
                main.SyncStatus("Sending Claim Request...");
                response = main.hicaps.sendClaimPharmRequest(request);
            }
            main.ResetStatus();
            if (response != null)
            {
                if (response.GetType() == typeof (HicapsConnectControl.HicapsConnectControl.ClaimPharmResponse))
                {
                    main.claimLog.LogClaim(response);
                }

                if (response.GetType() == typeof(HicapsConnectControl.HicapsConnectControl.QuotePharmResponse))
                {
                    main.claimLog.LogQuote((HicapsConnectControl.HicapsConnectControl.QuotePharmResponse)response);
                }
                
                main.cancel.UpdateTransactions();
                _latestResponse = response;
            }
            dumpClaimResponseToDebug(response);

            if (response.ResponseCode != "00")
            {
                new ResponseError(response.ResponseCode, response.ResponseText).ShowDialog();
            }
            else
            {
                // i.e. successful claim
                clearEntryFields(true);
                claimItems.Clear();
                ClaimDetails.Items.Refresh();

            }

            ////Display wait window
            //wait.Visibility = Visibility.Visible;
            //ForceUIToUpdate();

            
            // print provider receipt if needed
            if (!QuoteMode && Settings.Default.PrintProviderClaimReceiptLocal)
            {
                //Display wait window
                wait.Visibility = Visibility.Visible;
                ForceUIToUpdate();
                var rec = new PharmaceuticalReceipt(response, ReceiptFor.Provider);
                //Close wait window
                wait.Visibility = Visibility.Collapsed;
                rec.printReport(true);
            }

            
            // print customer receipt if needed
            bool shouldPrint = false;
            PharmaceuticalReceipt r;
            if (QuoteMode){

                shouldPrint = Settings.Default.PrintQuoteReceiptLocal;
                var quoteResponse = (HicapsConnectControl.HicapsConnectControl.QuotePharmResponse)response;
                r = new PharmaceuticalReceipt(quoteResponse);
            }
            else
            {
                shouldPrint = Settings.Default.PrintCustomerClaimReceiptLocal;
                r = new PharmaceuticalReceipt(response, ReceiptFor.Customer);
            }
            if (shouldPrint) r.printReport(false);
           
            

            if (response.ResponseCode == "00")
            {
                // process claim-to-sale
                // todo: is gap really calculated this way?
                decimal gap = response.TransactionAmount - response.BenefitAmount;
                if (gap > 0 && !QuoteMode)
                {
                    if (main.setup.ClaimToSale && main.setup.ClaimToSalePopup)
                    {
                        ClaimToSalePopup popup = new ClaimToSalePopup(gap);
                        popup.Show();
                    }
                    else if (main.setup.ClaimToSale && !main.setup.ClaimToSalePopup)
                    {
                        main.eftpos.doClaimToSale();
                    }
                    else
                    {
                        // ClaimToSale is not checked. Do nothing
                    }
                }
            }
           // main.hicaps.DisplayStatusWindow(false);
            SendButton.IsEnabled = true;
        }

        private static void dumpClaimResponseToDebug(
            HicapsConnectControl.HicapsConnectControl.ClaimResponse response)
        {
            Debug.WriteLine(Environment.NewLine + "Claim Response:");
            string textString = "";
            textString += "ResponseCode = " + (response.ResponseCode.NullTrim()) + Environment.NewLine;
            textString += "ResponseText = " + (response.ResponseText.NullTrim()) + Environment.NewLine;
            textString += "ExpiryDate = " + (response.ExpiryDate.NullTrim()) + Environment.NewLine;
            textString += "PrimaryAccountNumber = " + (response.PrimaryAccountNumber.NullTrim()) + Environment.NewLine;
            textString += "ProviderName = " + (response.ProviderName.NullTrim()) + Environment.NewLine;
            textString += "RrnNumber = " + (response.RrnNumber.NullTrim()) + Environment.NewLine;
            textString += "Merchant Id = " + (response.MerchantId.NullTrim()) + Environment.NewLine;
            textString += "Terminal Id = " + (response.TerminalId.NullTrim()) + Environment.NewLine;
            textString += "TransactionAmount = " + (response.TransactionAmount).ToCurrency() + Environment.NewLine;
            textString += "BenefitAmount = " + (response.BenefitAmount).ToCurrency() + Environment.NewLine;
            foreach (var ClaimDetailsLine in response.ClaimDetails)
            {
                textString += "ClaimDetailsLine = " + ClaimDetailsLine + Environment.NewLine;
            }
            foreach (var PatientNameDetailsLine in response.PatientNameDetails)
            {
                textString += "PatientNameDetailsLine = " + PatientNameDetailsLine + Environment.NewLine;
            }
            textString += "PatientNameDetailsStr = " + response.PatientNameDetailsStr.Aggregate("", (a, s) => a + s + ", ") + Environment.NewLine;
            textString += "Transaction Date = " + response.TransactionDate + Environment.NewLine;
            textString += "Response Time = " + response.ResponseTime + Environment.NewLine;
            textString += "Response Time = " + response.ResponseTime + Environment.NewLine;
            textString += "ServerURL = " + response.ServerUrl + Environment.NewLine;
            textString += "ClientVersion = " + response.ClientVersion + Environment.NewLine;
            textString += "ServerVersion = " + response.ServerVersion + Environment.NewLine;
            textString += "Membership Id = " + response.MembershipId + Environment.NewLine;

            Debug.Write(textString);
        }
               

        public static void ForceUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate(object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }        

        private void UpdateDescription()
        {
            DescriptionTextBox.Text = "";

            // look up item number in the item dictionary
            if (main != null && main.items != null
                && main.items.ContainsKey(ItemNumberInput.Text))
            {
                DescriptionTextBox.Text = main.items[ItemNumberInput.Text];
           
                if (string.IsNullOrEmpty(ItemFeeInput.Text))
                {
                    // Get fee from cache
                    ItemFeeInput.Text = ItemsFeeDictionary.getValue(ItemNumberInput.Text);

                  // ItemFeeInput.Text = main.ItemInfo[ItemNumberInput.Text].Amount.ToString();
                }
            }
        }

        private void UpdateDescription(string val)
        {
            // look up item number in the item dictionary
            if (main != null && main.items != null
                && main.items.ContainsKey(val))
            {
                DescriptionTextBox.Text = main.items[val];
                if (string.IsNullOrEmpty(ItemFeeInput.Text))
                {
                    // Get fee from cache
                    ItemFeeInput.Text = ItemsFeeDictionary.getValue(ItemNumberInput.Text);

                    //ItemFeeInput.Text = main.ItemInfo[ItemNumberInput.Text].Amount.ToString();
                }
            }
        }

        private void ItemNumberInput_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = (TextBox)ItemNumberInput.Template.FindName("Text", ItemNumberInput);
            if (t != null)
            {
                t.MaxLength = 6;
                t.PreviewMouseLeftButtonDown += SelectivelyIgnoreMouseButton;
                t.GotKeyboardFocus += SelectAllText;
                t.MouseDoubleClick += SelectAllText;
            }
        }

        private void ItemFeeInput_GotFocus(object sender, RoutedEventArgs e)
        {
            Xceed.Wpf.Toolkit.WatermarkTextBox t 
                = (Xceed.Wpf.Toolkit.WatermarkTextBox)ItemFeeInput.Template.FindName("PART_TextBox", ItemFeeInput);
            if (t != null)
            {
                t.TextChanged += new TextChangedEventHandler(ItemFeeInput_TextChanged);
            }
        }

        // a regex we can check as the user types (matches partially typed currency)
        Regex currency = new Regex(@"^\$?(\d*)(\.(\d*))?$");
        private void ItemFeeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Xceed.Wpf.Toolkit.WatermarkTextBox t
                = (Xceed.Wpf.Toolkit.WatermarkTextBox)ItemFeeInput.Template.FindName("PART_TextBox", ItemFeeInput);
            Match m = currency.Match(Utils.stripCurrencyFormatting(t.Text));
            GroupCollection g = m.Groups;

            if (t != null && m.Success)
            {
                // auto-add decimal place 
                if (g[1].Length > 4 || g[3].Length > 2)
                {
                    t.Text =  "$" + g[1].Value.LimitLength(4) + "." + (g[1].Value.SubstringOrEmpty(4) + g[3].Value).LimitLength(2);
                    t.CaretIndex = t.Text.Length;
                }
            }

            // limit length to 8 to prevent typing long sequences of invalid characters
            if (t != null)
            {
                t.Text = t.Text.LimitLength(8);
            }
        }

        private void ItemFeeInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.OnlyCurrencyCharacters();
        }

        private void PatientIdInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int x;
            e.Handled = !int.TryParse(e.Text, out x);
            e.Handled = e.Handled || !e.Text.OnlyNumbers();
        }

        private void ItemNumberInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.Handled = !e.Text.OnlyNumbers();
        }

        private string prevItemNumberInputText = "";
        private void ItemNumberInput_TextChanged(object sender, RoutedEventArgs e)
        {
            // only allow item codes from list.
            if (!Settings.Default.AllowAnyItemNumber)
            {
                if (!ItemsDictionary.Keys.Any(key => key.StartsWith(ItemNumberInput.Text.JustItemNumber().ToUpper())))
                {
                    ItemNumberInput.Text = prevItemNumberInputText;
                }
            }
            prevItemNumberInputText = ItemNumberInput.Text;
            UpdateDescription();
        }

        internal void UpdateItems()
        {
            ItemNumberInput.ItemsSource = ItemsDictionary;
        }

        private void OpenItemNumberList_Click(object sender, RoutedEventArgs e)
        {
            ItemNumberInput.IsDropDownOpen = !ItemNumberInput.IsDropDownOpen;
            OpenItemNumberList.Content = ItemNumberInput.IsDropDownOpen ? "▲" : "▼";

        }

        public static bool StartsWithIncludingEmpty(string text, string value)
        {
            return value.StartsWith(text, StringComparison.CurrentCultureIgnoreCase)
                    || text == "";
        }

        private void ItemNumberInput_DropDownOpened(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            OpenItemNumberList.Content = ItemNumberInput.IsDropDownOpen ? "▲" : "▼";
        }

        private void ItemNumberInput_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            OpenItemNumberList.Content = ItemNumberInput.IsDropDownOpen ? "▲" : "▼";
        }

        private void ItemNumberInput_LostFocus(object sender, RoutedEventArgs e)
        {
            //if (ItemNumberInput.Text.Length > 0)
           // {
            //    ItemNumberInput.Text = ItemNumberInput.Text.PadLeft(4, '0');
            //}
        }

        private void PatientIdInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PatientIdInput.Text.Length > 0)
            {
                PatientIdInput.Text = PatientIdInput.Text.PadLeft(2, '0');
            }
            defaultPatientId = PatientIdInput.Text;
        }


        private static HicapsConnectControl.HicapsConnectControl.ClaimResponse CreateInitialLatestResponseForTesting()
        {
            HicapsConnectControl.HicapsConnectControl.ClaimResponse response
                = new HicapsConnectControl.HicapsConnectControl.ClaimResponse
                {
                    ResponseCode = "00",
                    ResponseText = "APPROVED",
                    ExpiryDate = "5012",
                    PrimaryAccountNumber = "9036999801234567891",
                    ProviderName = "DANIEL CHU",
                    ProviderNumberId = "2288249B",
                    RrnNumber = "000001006600",
                    MerchantId = "33123433",
                    TerminalId = "SE101A",
                    TransactionAmount = 260,
                    BenefitAmount = 115,
                    TransactionDate = DateTime.Now,
                    ResponseTime = DateTime.Now,
                    ServerUrl = "10.1.2.6:11000:SE101A:COM1",
                    ClientVersion = "1.0.3.26",
                    ServerVersion = "1.0.3.26",
                    MembershipId = "12345678"
                };
            List<string> claimDetails = new List<string>();
            claimDetails.Add("01002200240701000000250003");
            claimDetails.Add("02053100240701000000500000");
            claimDetails.Add("02053100240701000000500000");
            List<string> nameDetails = new List<string>();
            nameDetails.Add("01Peter Pig");
            nameDetails.Add("02Pearl Pig");
            nameDetails.Add("03Paddy Pig");
            nameDetails.Add("04Patsy Pig");

            response.PatientNameDetails = nameDetails;
            response.PatientNameDetailsStr = nameDetails.ToArray();
            response.ClaimDetails = claimDetails;
            return response;
        }

        private void DOSInput_Loaded(object sender, RoutedEventArgs e)
        {
            DOSInput.Value = DateTime.Today;
        }

        private void ScriptInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var c = Convert.ToChar(e.Text);
            if (Char.IsNumber(c))
                e.Handled = false;
            else
                e.Handled = true;

            base.OnPreviewTextInput(e);
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAll();
            clearEntryFields(true);
        }

        private string defaultPatientId = "01";
        private void PatientIdInput_TextChanged(object sender, TextChangedEventArgs e)
        {
           
        }

    }

    public class ItemRow
    {
        
        public ItemRow() { }
        public ItemRow(String patientId,
                       String itemNumber,
                       String description,
                       DateTime? dos,
                       String bodyPart,
                       String itemFee,
                       String scriptnumber,
                       String Compound)
        {
            this.PatientId = patientId;
            this.ItemNumber = itemNumber;
            this.Description = description;
            this.Dos = dos;
            this.ItemFee = itemFee;
            this.BodyPart = bodyPart;
            this.ScriptNumber = scriptnumber;
            this.Compound = Compound;
        }

        public String PatientId { get; set; }
        public String ItemNumber { get; set; }
        public String Description { get; set; }
        public DateTime? Dos { get; set; }
        public String ItemFee { get; set; }
        public String BodyPart { get; set; }
        public String ScriptNumber { get; set; }
        public String Compound { get; set; }
    }
}
