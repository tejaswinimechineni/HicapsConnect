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
using System.Reflection;
using System.Diagnostics;
using HicapsConnectClient12.Properties;


namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Page
    {
        private MainWindow main;

        public bool ClaimToSale
        {
            get
            {
                return claimToSaleCheckBox.IsChecked ?? false;
            }
        }

        public bool ClaimToSalePopup
        {
            get
            {
                return claimToSalePromptCheckBox.IsChecked ?? false;
            }
        }

        public Setup()
        {
            InitializeComponent();
            main = (MainWindow) Application.Current.MainWindow;
            ClientVersionLabel.Content = Assembly.GetExecutingAssembly().GetName().Version;
           
                //Assembly.GetAssembly(typeof (HicapsConnectControl.HicapsConnectControl)).GetName().Version;
        }

        // Terminal test
        private void terminalTest_Click(object sender, RoutedEventArgs e)
        {
            terminalTest.IsEnabled = false;
            // Run the test in a new window
            if (main.hicaps != null)
            {
                new TerminalTestWindow(main.hicaps, main.currentTerminal).ShowDialog();
            }
            terminalTest.IsEnabled = true;
        }

        private void SelectTerminalButton_Click(object sender, RoutedEventArgs e)
        {
            main.SelectTerminal();
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            SyncButton.IsEnabled = false;
            main.SyncTerminal(true);
            SyncButton.IsEnabled = true;
        }

        private void AboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutUs().Show();
        }

        private void useApiCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (main != null && Settings.Default.UseApi)
            {
                main.itemSource.StartUpdate();
            }
        }

        private void ReceiptSetupButton_Click(object sender, RoutedEventArgs e)
        {
            new PharmaceuticalReceipt().receiptSetup();
        }

        private void useApiCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                Debug.WriteLine("Assem name: " + Assembly.GetExecutingAssembly().GetName().Name);
                //int result = VersionUpdate.RunCheckForUpdates(Assembly.GetExecutingAssembly(), Process.GetCurrentProcess().MainWindowHandle);
                //if (result != 0)
                //{
                //    main.Close();
                //}
                //else
                //{
                //    //MessageBox.Show("This version is already up to date");
                //}
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HicapsVersionLabel.Content = main.hicaps.getVersion();
        }
    }
}
