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
    /// Interaction logic for TerminalTestWindow.xaml
    /// </summary>
    public partial class TerminalTestWindow : Window
    {
        private HicapsConnectControl.HicapsConnectControl hicaps;
        private String terminal;

        public TerminalTestWindow(HicapsConnectControl.HicapsConnectControl hicaps, 
                                  String terminal)
        {
            InitializeComponent();
            this.hicaps = hicaps;
            this.terminal = terminal;
            HicapsConnectControl.HicapsConnectControl.TerminalTestResponse response
                = null;
            
            if (terminal != null && terminal != "")
            {
                this.StatusLabel.Text = "Connecting to " + terminal + "...";

                // send TerminalTest request
                response = hicaps.sendTerminalTest(terminal);
            }
            else
            {
                this.StatusLabel.Text = "Error: no terminal selected";
            }

            // check response
            if (response != null)
            {
                StringBuilder result = new StringBuilder();
                result.Append("Response code: " + response.ResponseCode.NullTrim() + Environment.NewLine);
                result.Append("Response text: " + response.ResponseText.NullTrim() + Environment.NewLine);
                result.Append("Response time: " + response.ResponseTime + Environment.NewLine);
                if (response.ResponseCode == "00")
                {
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    StatusLabel.Text = "Connection to terminal " + terminal + " was successful.";
                }
                else
                {
                    StatusLabel.Text = "Connection to terminal " + terminal + " FAILED";
                    StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                }
                ResponseLabel.Text = result.ToString();
            }

        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
