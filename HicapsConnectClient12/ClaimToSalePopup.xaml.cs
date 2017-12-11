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
using System.Windows.Shapes;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for ClaimToSalePopup.xaml
    /// </summary>
    public partial class ClaimToSalePopup : Window
    {
        private Decimal _amount;
        private MainWindow main;

        public ClaimToSalePopup()
        {
            main = (MainWindow) Application.Current.MainWindow;
            _amount = 0;
            InitializeComponent();
        }

        public ClaimToSalePopup(Decimal amount) : this()
        {
            _amount = amount;
            AmountLabel.Content = "Gap: " + amount.ToCurrency();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            if (main != null)
            {
                main.eftpos.doClaimToSale();
            }
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
