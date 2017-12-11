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
using System.Drawing.Printing;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for ReportPreview.xaml
    /// </summary>
    public partial class ReportPreview : Window
    {
        private PharmaceuticalReceipt receipt;

        public ReportPreview(PharmaceuticalReceipt r)
        {
            receipt = r;
            InitializeComponent();
            _reportViewer.Load += ReportViewer_Load;
        }

        private void ReportViewer_Load(object sender, EventArgs e)
        {
            receipt.setUpReport(_reportViewer.LocalReport, false);
            var set = new PageSettings();
            var mar = new Margins(25, 25, 30, 30);
            set.Margins = mar;
            _reportViewer.SetPageSettings(set);
            _reportViewer.SetDisplayMode(Microsoft.Reporting.WinForms.DisplayMode.PrintLayout);
            _reportViewer.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.PageWidth;
            _reportViewer.ShowPrintButton = false;
            _reportViewer.ShowRefreshButton = false;
            _reportViewer.RefreshReport();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
