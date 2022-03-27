using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Collections.Generic;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
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
                    Data.Transactions = Data.Transactions.OrderBy(t => t.Order).ToList();
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
        /// Updates current account balances based on transactions
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains MouseButtonEventArgs data.</param>
        private void lviUpdateAccountBalances_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DateTime startTime = DateTime.Now;

                // updates current balance of account in database
                string connectionString = Properties.Settings.Default.connectionString;
                string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                        "WHERE AccountID=@AccountID";

                SqlConnection con = new(connectionString);
                con.Open();

                int count = 0;
                foreach (Account account in Data.Accounts)
                {
                    // get list of transactions for specified account
                    var transactions = Data.Transactions.Where(t => t.AccountID == account.AccountID);

                    // calculate sum and update current balance in memory
                    decimal sum = account.StartingBalance + transactions.Sum(t => t.Amount);

                    // if different, update
                    if (account.CurrentBalance != sum)
                    {
                        count++;
                        account.CurrentBalance = sum;
                    
                        // executes query
                        SqlCommand accUpdateCmd = new(accUpdateQuery, con);
                        accUpdateCmd.Parameters.AddWithValue("@CurrentBalance", account.CurrentBalance);
                        accUpdateCmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                        accUpdateCmd.ExecuteNonQuery();
                    }
                }

                con.Close();

                int endTime = (DateTime.Now - startTime).Milliseconds;
                MessageBox.Show($"Updated account balances for {count} records in {endTime} milliseconds");

                ((MainWindow)Application.Current.MainWindow).RefreshBalances();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Updates total running balance based on transactions
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains MouseButtonEventArgs data.</param>
        private void lviUpdateRunningBalance_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DateTime startTime = DateTime.Now;

                // updates current balance of account in database
                string connectionString = Properties.Settings.Default.connectionString;
                string accUpdateQuery = "UPDATE Transactions SET Balance=@Balance " +
                                        "WHERE TransactionID=@TransactionID";

                SqlConnection con = new(connectionString);
                con.Open();

                Data.Transactions = Data.Transactions.OrderBy(t => t.Order).ToList();

                int count = 0;
                decimal balance = Data.Accounts.Sum(a => a.StartingBalance);
                for (int i = 0; i < Data.Transactions.Count; i++)
                {
                    balance += Data.Transactions[i].Amount;
                    if (Data.Transactions[i].Balance != balance)
                    {
                        count++;
                        Data.Transactions[i].Balance = balance;

                        // executes query
                        SqlCommand accUpdateCmd = new(accUpdateQuery, con);
                        accUpdateCmd.Parameters.AddWithValue("@Balance", Data.Transactions[i].Balance);
                        accUpdateCmd.Parameters.AddWithValue("@TransactionID", Data.Transactions[i].TransactionID);
                        accUpdateCmd.ExecuteNonQuery();
                    }
                }

                con.Close();

                int endTime = (DateTime.Now - startTime).Milliseconds;
                MessageBox.Show($"Updated running balance for {count} records in {endTime} milliseconds");

                ((MainWindow)Application.Current.MainWindow).RefreshBalances();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Updates order of transactions in memory and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains MouseButtonEventArgs data.</param>
        private void lviUpdateTransactionOrder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DateTime startTime = DateTime.Now;

                Data.Transactions = Data.Transactions.
                OrderBy(t => t.Date).
                ThenBy(t => Math.Abs(t.Amount)).
                ThenBy(t => t.CategoryID).
                ThenBy(t => t.TransactionID).ToList();

                string connectionString = Properties.Settings.Default.connectionString;
                string orderUpdateQuery = "UPDATE Transactions SET DisplayOrder=@Order " +
                                          "WHERE TransactionID=@TransactionID";

                SqlConnection con = new(connectionString);
                con.Open();

                int count = 0;
                int index = 1;
                for (int i = 0; i < Data.Transactions.Count; i++, index++)
                {
                    if (Data.Transactions[i].Order != index)
                    {
                        count++;
                        Data.Transactions[i].Order = index;
                        SqlCommand accUpdateCmd = new(orderUpdateQuery, con);
                        accUpdateCmd.Parameters.AddWithValue("@Order", Data.Transactions[i].Order);
                        accUpdateCmd.Parameters.AddWithValue("@TransactionID", Data.Transactions[i].TransactionID);
                        accUpdateCmd.ExecuteNonQuery();
                    }
                }

                con.Close();

                int endTime = (DateTime.Now - startTime).Milliseconds;
                MessageBox.Show($"Updated transaction order for {count} records in {endTime} milliseconds");
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
