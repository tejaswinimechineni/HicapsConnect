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
using HicapsConnectControl;
using System.Diagnostics;
using HicapsConnectClient12.Properties;
using System.Windows.Threading;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HicapsConnectClient12
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string currentTerminalField;
        public string currentTerminal 
        {
            get
            {
                return currentTerminalField;
            }
            set
            {
                currentTerminalField = value;
                // update the status area with new selection
                Debug.WriteLine("changing current terminal to " + value);
                SelectionStatus(value == "" || value == null
                    ? "No terminal selected"
                    : value + " selected");

                if (value != null && value != "" && cancel != null)
                {
                    cancel.UpdateTransactions();
                }

                if (setup != null && setup.SelectedTerminalLabel != null)
                {
                    // update the setup tab with new selection
                    setup.SelectedTerminalLabel.Content = value == "" || value == null
                        ? "Selected terminal: none"
                        : "Selected terminal: " + value;

                }

                // update settings with new last used terminal
                Settings.Default.LastTerminal = value;
                Settings.Default.Save();
            }
        }
        private ConnectToTerminal chooser;

        public void SelectionStatus(string s)
        {
            Debug.WriteLine("changing selection status to " + s);
            if (StatusLabel != null)
            {
                StatusLabel.Content = s;
            }
        }

        public void SyncStatus(string s)
        {
            if (LastSyncLabel != null)
            {
                LastSyncLabel.Content = s;
            }
        }

        public HicapsConnectControl.HicapsConnectControl hicaps { get; set; }

        public ItemHttpSource itemSource { get; set; }
        
        // Sync-related data.
        // Some of these properties automatically update 
        // the GUI when they are set.
        public HicapsCacheManager cache { get; set; }
        private String[] providersField;
        public String[] providers
        {
            get
            {
                return providersField;
            }
            set
            {
                providersField = (from p in value
                                  select formatProvider(p)).ToArray();
                Array.Sort<string>(providersField);

                if (value != null)
                {
                    // update the providers dropdowns automatically
                    // when this property gets updated
                    int? selectedIndexClaim = null;
                    string selectedProviderClaim = claim.ProviderCombo.Text;
                    claim.ProviderCombo.Items.Clear();

                    int? selectedIndexQuote = null;
                    string selectedProviderQuote = quote.ProviderCombo.Text;
                    quote.ProviderCombo.Items.Clear();

                    int? selectedIndexCancel = null;
                    string selectedProviderCancel = cancel.ProviderCombo.Text;
                    cancel.ProviderCombo.Items.Clear();

                    foreach (string provider in providersField)
                    {
                        int claimIndex = claim.ProviderCombo.Items.Add(provider);
                        int quoteIndex = quote.ProviderCombo.Items.Add(provider);
                        int cancelIndex = cancel.ProviderCombo.Items.Add(provider);
                        if (provider == selectedProviderClaim) selectedIndexClaim = claimIndex;
                        if (provider == selectedProviderQuote) selectedIndexQuote = quoteIndex;
                        if (provider == selectedProviderCancel) selectedIndexCancel = cancelIndex;
                    }

                    if (selectedIndexClaim != null)
                    {
                        claim.ProviderCombo.SelectedIndex = (int)selectedIndexClaim;
                    }
                    if (selectedIndexQuote != null)
                    {
                        quote.ProviderCombo.SelectedIndex = (int)selectedIndexQuote;
                    }
                    if (selectedIndexCancel != null)
                    {
                        cancel.ProviderCombo.SelectedIndex = (int)selectedIndexCancel;
                    }

                    LastSync = DateTime.Now;
                }
                
            }
        }
        private String[] merchantsField;
        public String[] merchants
        {
            get
            {
                return merchantsField;
            }
            set
            {
                merchantsField = value;

                // update the merchants field in the Eftpos tab
                if (value != null)
                {
                    int? selectedIndex = null;
                    string selectedMerchant = eftpos.MerchantCombo.Text;
                    eftpos.MerchantCombo.Items.Clear();

                    foreach (string merchant in value)
                    {
                        string _merchant = formatMerchant(merchant);
                        int index = eftpos.MerchantCombo.Items.Add(_merchant);
                        if (_merchant == selectedMerchant) selectedIndex = index;
                    }

                    if (selectedIndex != null)
                    {
                        eftpos.MerchantCombo.SelectedIndex = (int) selectedIndex;
                    }
                }
            }
        }
        private Dictionary<string, string> cardsField;
        public Dictionary<string, string> cards
        {
            get { return cardsField; }
            set { cardsField = value; }
        }
        private DateTime LastSyncField;
        public DateTime LastSync
        {
            get
            {
                return LastSyncField;
            }
            set
            {
                LastSyncField = value;

                // also update the status area
                SyncStatus("Last synced " + LastSync.ToString());
            }
        }
        private Dictionary<String, String> itemsField;
        public Dictionary<String, String> items
        {
            get
            {
                return itemsField;
            }
            set
            {
                itemsField = value;                
            }
        }
        public IScheduleProvider ItemCache { get; set; }
        public Dictionary<String, Item> ItemInfo;
        private List<HicapsConnectControl.HicapsConnectControl.ClaimResponse> transactionsField;
        public List<HicapsConnectControl.HicapsConnectControl.ClaimResponse> transactions
        {
            get { return transactionsField; }
            set { transactionsField = value; }
        }
        public ClaimLog claimLog { get; set; }

        // sub-pages (tabs)
        internal Setup setup;
        internal Claim claim;
        internal Eftpos eftpos;
        internal Claim quote;
        internal Cancel cancel;

        public MainWindow()
        {
            InitializeComponent();
            chooser = null;
            currentTerminalField = null;
            items = null;

            // todo: temp debug solution
            /*Debug.WriteLine("Setting up file debug");
            TextWriterTraceListener myWriter = new
                TextWriterTraceListener(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "HicapsConnect" + Path.DirectorySeparatorChar + "debug.txt");
            myWriter.Flush();
            Debug.Listeners.Add(myWriter);
            Debug.WriteLine("sup");
            Debug.WriteLine("Set up file debug");*/
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // set up references to sub-pages (i.e. the tabs)
            setup  = (Setup)  SetupFrame.Content;
            claim  = (Claim)  ClaimFrame.Content;
            eftpos = (Eftpos) EftposFrame.Content;
            quote  = (Claim)  QuoteFrame.Content;
            cancel = (Cancel) CancelFrame.Content;

            Debug.WriteLine("PrintReceiptCancelClaim: " + Settings.Default.PrintReceiptCancelClaim);

            // put the quote tab into quote mode
            quote.enterQuoteMode();

            // create a cache object
            cache = new HicapsCacheManager();
            
            claimLog = new ClaimLog(null);

            // create the HicapsConnectControl.
            hicaps = new HicapsConnectControl.HicapsConnectControl();
         //   hicaps.DisplayStatusWindow(false);
            hicaps.TerminalListChanged += new HicapsConnectControl.TerminalListChangedEventHandler(hicaps_termListChanged);
            

            // Create a host for the winforms control
           // var host = new System.Windows.Forms.Integration.WindowsFormsHost {Child = hicaps};

            // add HicapsConnectControl to window via host
            //this.HiddenHicapsControl.Children.Add(host);

            if (!PharmaceuticalReceipt.siteInfoExists())
            {
                new PharmaceuticalReceipt().receiptSetup();
            }
            

            // select terminal from dispatcher rather than Window_Loaded
            // to prevent weird combobox behaviour. Select after 5 seconds 
            // to allow time for terminal auto-selection.
            var timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher) {Interval = new TimeSpan(0, 0, 5)};
            timer.Tick += (s, a) => 
            {
                if (currentTerminal == null) SelectTerminal();
                var dispatcherTimer = s as DispatcherTimer;
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                //NewUpdateItemFields();
                //SyncTerminal(true);

            };
            timer.Start();
        }

        

        

        

        /// <summary>
        /// New method to bring the items list using HicapsConnectControl.AllItemListRequest, this replaces the old one which was handling local files
        /// </summary>
        private void NewUpdateItemFields(bool silent = true)
        {
            if (string.IsNullOrEmpty(currentTerminal))
            {
                //SelectTerminal();
            }
            var myRequest = new HicapsConnectControl.HicapsConnectControl.AllPharmItemListRequest
            {
                ServerUrl = currentTerminal
            };

            var myResponse = hicaps.sendAllPharmItemList(myRequest);
            items = new Dictionary<String, String>();
            if (myResponse.ResponseCode != "00")
            {
                MessageBox.Show("Error downloading new item schedule: " + myResponse.ResponseText);
            }

            foreach (var merchantList in myResponse.ItemListDescription)
            {
                items.Add(merchantList.Substring(0, 4), merchantList.Substring(4));
            }
            
            UpdateItemList();
            if (!silent)
            {
                if (myResponse.ResponseCode == "00")
                {
                    MessageBox.Show(items.Count + " Pharmacy Items Updated");
                }
                else
                {
                    {
                        MessageBox.Show("Error downloading new item schedule: " + myResponse.ResponseText);
                    }
                }
            }
        }

        private void UpdateItemFields()
        {
            // check expiry date on cached data
            Schedule s = ItemCache.GetCurrentSchedule();
            if (s.Expiry > DateTime.Now)
            {
                // schedule is in-date, update the fields
                List<Item> l = s.Items;
                ItemInfo = new Dictionary<string, Item>();
                l.ForEach(it => ItemInfo.Add(it.Number, it));
                itemsField = new Dictionary<string,string>();
                l.ForEach(it => itemsField.Add(it.Number, it.Description));

                // update GUI
                UpdateItemList();
            }
        }

        public void DownloadItemsCompleted(object sender,
                                            DownloadCompletedEventArgs e)
        {
            try
            {

                ItemCache.SetCurrentSchedule(e.schedule);

                // success
                Debug.WriteLine("++++!!! items downloaded: " + e.schedule.Items.Aggregate("", (s, i) => s + i.Number + ", "));

                // update fields (and GUI)
                UpdateItemFields();
            }
            catch (Exception ex)
            {
                // failure
                Debug.WriteLine("Item download did not succeed. There may have been a problem with the data received");
            } 
        }   

        public void SyncTerminal(bool force)
        {
            if (hicaps != null && currentTerminal != null && currentTerminal != "")
            {
                //Test the terminal before getting the providers, merchants and Items, if test is successful it proceeds
                HicapsConnectControl.HicapsConnectControl.TerminalTestResponse response;
                response = hicaps.sendTerminalTest(currentTerminal);

                if (response.ResponseCode == "00")
                {
                // providers, merchants, and card details are
                // 'cached' to disk. That functionality comes from 
                // the cache object.
                
             //   hicaps.DisplayStatusWindow(false);
                Debug.WriteLine("++++++++++++++ Syncing");
                SyncStatus("Syncing providers...");
                providers = cache.syncProviderCache(force, currentTerminal, hicaps);
             //   hicaps.DisplayStatusWindow(false);
                SyncStatus("Syncing merchants...");
                merchants = cache.syncMerchantCache(force, currentTerminal, hicaps);
               // hicaps.DisplayStatusWindow(false);
                SyncStatus("Syncing card details...");
                string[] cardDetails = cache.syncCardCache(force, currentTerminal, hicaps);
                cards = new Dictionary<string, string>();
                foreach (string card in cardDetails)
                {
                    var fields = card.Split('|');
                    if (fields.Length > 1)
                    {
                        string fund = fields[0];
                        string cardNo = fields[1];
                        cards[cardNo] = fund;
                    }
                }
               // hicaps.DisplayStatusWindow(false);

                //Items request
                SyncStatus("Syncing items...");
                NewUpdateItemFields();
               // hicaps.DisplayStatusWindow(false);
                

                if (Properties.Settings.Default.SyncTransactions)
                {
                    // this doesn't happen in current builds
                    // since SyncTransactions is turned off
                    // and client-side logging is used.
                    SyncStatus("Syncing transactions...");
                    SyncTransactions();
                }
               // hicaps.DisplayStatusWindow(false);
                // updating this property will update SyncStatus as well
                LastSync = DateTime.Now;
                }
                else
                {
                    SelectTerminal();
                }
            }
        }

        private void SyncTransactions()
        {

            // loop through all available providers and download 
            // the transaction list for that provider
            // todo: caching
            if (currentTerminal != null)
            {
                transactions = new List<HicapsConnectControl.HicapsConnectControl.ClaimResponse>();
                foreach (string provider in providers)
                {
                    HicapsConnectControl.HicapsConnectControl.HicapsTransListingRequest request
                        = new HicapsConnectControl.HicapsConnectControl.HicapsTransListingRequest();
                    request.ServerUrl = currentTerminal;
                    request.ProviderNumberId = Utils.getProviderNumberFromRow(provider);
                    HicapsConnectControl.HicapsConnectControl.HicapsTransListingResponse response 
                        = hicaps.sendHicapsTransListing(request);

                    if (response.ClaimTransactionList != null)
                    {
                        foreach (var claimResponse in response.ClaimTransactionList)
                        {
                            transactions.Add(claimResponse);
                            Debug.WriteLine("tr>> " + claimResponse.ToString());
                        }
                    }
                }

                if (cancel != null)
                {
                    cancel.UpdateTransactions();
                }
            }
        }

        private void LoadItemFile()
        {
            items = new Dictionary<String, String>();
            string[] lines = Properties.Resources.Items.Split('\n');

            foreach (string line in lines)
            {
                string[] fields = line.Split(',');
                if (fields.Length == 2)
                {
                    fields[0] = fields[0].Trim();
                    fields[1] = fields[1].Trim();
                    items.Add(fields[0], fields[1]);
                }
            }

            // force items property to update gui
            UpdateItemList();
        }

        // updates the item list used for autocompletion in claim/quote
        public void UpdateItemList()
        {
            // update both claim and quote
            foreach (Claim c in new Claim[] {claim, quote})
            {
                // update claim.ItemsDictionary
                if (c != null)
                {
                    c.ItemsDictionary = items;
                    c.UpdateItems();
                }
            }
        }

        public void hicaps_termListChanged(string param)
        {
            // If no terminal is selected, try to select "last used" terminal.
            // We don't want to do this with the chooser open since it would 
            // cause the terminal to be selected while the user is potentially trying 
            // to select a terminal manually. Dispatch it to the UI thread since this  
            // gets called from other threads
            Dispatcher.Invoke((Action) (() => 
            {
                if (currentTerminal == null && chooser == null)
                {
                   // hicaps.DisplayStatusWindow(false);
                    string[] terms = hicaps.getTerminalList();

                    bool found = false;
                    foreach (string term in terms)
                    {
                        if (term == Settings.Default.LastTerminal)
                        {
                            currentTerminal = term;
                            found = true;
                        }
                    }
                    if (found)
                    {
                        // sync the terminal
                        if (Properties.Settings.Default.FirstAccess)
                        {
                            
                            SyncTerminal(true);
                            Properties.Settings.Default.FirstAccess = false;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            SyncTerminal(true);
                        }
                    }
                }
            }));

            // if chooser is open, call the update method
            // in the UI thread (since this event handler 
            // seems to get called from a different thread)
            Dispatcher.BeginInvoke((Action)(() => { if (chooser != null) chooser.RefreshTerminalList(); }));
        }

        public void ChooserClosed(object sender, System.EventArgs e)
        {
            chooser = null;
            
            // sync the terminal if possible
            if (Settings.Default.FirstAccess)
            {
                SyncTerminal(true);
            }
            else
            {
                SyncTerminal(true);
            }
        }

        public void SelectTerminal()
        {
            // create brand new window
            chooser = new ConnectToTerminal();
            chooser.Closed += ChooserClosed;
            chooser.ShowDialog();
        }

        public void ResetStatus()
        {
            // force redisplay of last sync
            LastSync = LastSync;
        }

        private void Clicked_StatusArea(object sender, MouseButtonEventArgs e)
        {
            SelectTerminal();
        }
        
        private static string formatProvider(string provider)
        {
            string[] fields = provider.Trim().Split('|');
            return fields[2] + " (" + fields[3] + ")";
        }

        private string formatMerchant(string merchant)
        {
            string[] fields = merchant.Split('|');
            return fields[1] + " (" + fields[2] + ")";
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            foreach (Claim c in new Claim[] { claim, quote })
            {
                if (c != null && c.ItemNumberInput != null)
                {
                    // close the dropdown menu so that it doesn't awkwardly keep its position.
                    c.ItemNumberInput.IsDropDownOpen = false;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Debug.Flush();
            Application.Current.Shutdown(0);
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabItem selected = e.AddedItems[0] as TabItem;
                if (selected.Name == "Eftpos")
                {
                    eftpos.reEnableMerchantMenu();
                }
            }
        }
    }
}
