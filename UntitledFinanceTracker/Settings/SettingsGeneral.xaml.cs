using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Controls;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for SettingsGeneral.xaml
    /// </summary>
    public partial class SettingsGeneral : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsGeneral()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Exports all transactions in memory to a CSV file
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains MouseButtonEventArgs data.</param>
        private void lviExportCSV_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFile = new();
            saveFile.Filter = "CSV Files (*.csv)|*.csv";

            if (saveFile.ShowDialog() == true)
            {
                FileStream fs = new(saveFile.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new(fs);

                try
                {
                    foreach (Transaction trans in Data.Transactions)
                        sw.WriteLine(trans.ToString());

                    MessageBox.Show("Transactions have successfully been exported");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                sw.Close();
            }
        }

        /// <summary>
        /// Updates switch on click
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains MouseButtonEventArgs data.</param>
        private void lviDeveloperMode_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tsDeveloperMode.IsChecked = !tsDeveloperMode.IsChecked;
        }

        /// <summary>
        /// Updates switch on click
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains MouseButtonEventArgs data.</param>
        private void lviTestSwitch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tsTestSwitch.IsChecked = !tsTestSwitch.IsChecked;
        }
    }
}
