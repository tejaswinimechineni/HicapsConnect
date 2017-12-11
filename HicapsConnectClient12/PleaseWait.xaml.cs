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
using System.Diagnostics;
using HicapsConnectClient12.Properties;

namespace HicapsConnectClient12
{
    /// <summary>
    /// Interaction logic for ConnectToTerminal.xaml
    /// </summary>
    public partial class PleaseWait : Window
    {
        private MainWindow main;

        public PleaseWait()
        {
            InitializeComponent();
            //main = (MainWindow) Application.Current.MainWindow;
            
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
    }
}
