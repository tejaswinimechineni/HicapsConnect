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
    /// Interaction logic for ResponseError.xaml
    /// </summary>
    public partial class ResponseError : Window
    {
        public ResponseError(string code, string description)
        {
            InitializeComponent();
            ResponseCodeLabel.Text = "Error code: " + code;

            ResponseTextLabel.Text = "Description: " + Environment.NewLine
                + description.Split('|').Aggregate("", (str, next) => str + Environment.NewLine + next);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape
                || e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
