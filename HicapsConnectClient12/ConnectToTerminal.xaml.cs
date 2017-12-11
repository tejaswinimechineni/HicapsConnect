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
    public partial class ConnectToTerminal : Window
    {
        private MainWindow main;

        public ConnectToTerminal()
        {
            InitializeComponent();
            try
            {
                main = (MainWindow) Application.Current.MainWindow;
            }
            catch
            {
            }
            RefreshTerminalList();
        }

        public void RefreshTerminalList()
        {
            if (main != null && main.hicaps != null)
            {
                string[] terms = main.hicaps.getTerminalListById();
                terms = terms.Distinct().ToArray();
                
                bool wasEmpty = TerminalCombo.Items.IsEmpty;
                TerminalCombo.Items.Clear();

                int selectedIndex = 0;
                
                string selectedTerm = TerminalCombo.Text;
                bool foundDefault = false;
                foreach (string term in terms)
                {
                    int index = TerminalCombo.Items.Add(term.Trim());
                    
                    // keep selected if it was previously selected
                    if (term.Trim() == selectedTerm && !foundDefault) selectedIndex = index;
                    
                    // favour terminals that match
                    if (term.Trim() == Settings.Default.LastTerminal.Trim())
                    {
                        selectedIndex = index;
                        foundDefault = true;
                    }
                }

                if (!TerminalCombo.Items.IsEmpty)
                {
                    TerminalCombo.SelectedIndex = selectedIndex;
                    if (wasEmpty && !foundDefault)
                    {
                        TerminalCombo.SelectedItem = TerminalCombo.Items[0];
                    }
                }
                TerminalCombo.Items.Refresh();

                // update main window (selection may have changed)
                main.currentTerminal = TerminalCombo.Text;
            }
        }

        private static void debugShowStringArray(string[] strings)
        {
            Debug.Write("strings: ");
            foreach (string s in strings)
            {
                Debug.Write(s + ",");
            }
            Debug.Write(Environment.NewLine);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTerminalList();
        }

        private void TerminalCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("> " + e.AddedItems);
            foreach (string s in e.AddedItems)
            {
                if (s != null && s != "") main.currentTerminal = s;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TerminalCombo_DropDownOpened(object sender, EventArgs e)
        {
            // print some debug info
            Debug.Write("Menu: ");
            foreach (var item in TerminalCombo.Items)
            {
                Debug.Write(item.ToString() + ",");
            }
            Debug.WriteLine("");
        }
    }
}
