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
    /// Interaction logic for ReceiptSetup.xaml
    /// </summary>
    public partial class ReceiptSetup : Window
    {
        private PharmaceuticalReceipt receipt;

        public ReceiptSetup(PharmaceuticalReceipt r)
        {
            receipt = r;
            InitializeComponent();
            SiteName.Text = r.SiteName;
            SiteAddress.Text = r.SiteAddress;
            SiteFax.Text = r.Fax;
            SitePhone.Text = r.Phone;
            SiteEmail.Text = r.Email;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SiteName_TextChanged(object sender, TextChangedEventArgs e)
        {
            receipt.SiteName = SiteName.Text;
        }

        private void SiteAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            receipt.SiteAddress = SiteAddress.Text;
        }

        private void SiteFax_TextChanged(object sender, TextChangedEventArgs e)
        {
            receipt.Fax = SiteFax.Text;
        }

        private void SitePhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            receipt.Phone = SitePhone.Text;
        }

        private void SiteEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            receipt.Email = SiteEmail.Text;
        }
    }
}
