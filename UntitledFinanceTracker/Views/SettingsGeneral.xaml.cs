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
                    Data.Transactions = Data.Transactions.
                        OrderBy(t => t.Date).
                        ThenBy(t => t.CategoryID).
                        ThenBy(t => Math.Abs(t.Amount)).
                        ThenBy(t => t.TransactionID).ToList();

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

                foreach (Account account in Data.Accounts)
                {
                    // get list of transactions for specified account
                    var transactions = Data.Transactions.Where(t => t.AccountID == account.AccountID);

                    // calculate sum and update current balance in memory
                    decimal sum = account.StartingBalance + transactions.Sum(t => t.Amount);
                    account.CurrentBalance = sum;
                    
                    // executes query
                    SqlCommand accUpdateCmd = new(accUpdateQuery, con);
                    accUpdateCmd.Parameters.AddWithValue("@CurrentBalance", account.CurrentBalance);
                    accUpdateCmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    accUpdateCmd.ExecuteNonQuery();
                }

                con.Close();

                int endTime = (DateTime.Now - startTime).Milliseconds;
                MessageBox.Show($"Account balances successfully updated in {endTime} milliseconds");

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

                decimal balance = Data.Accounts.Sum(a => a.StartingBalance);
                foreach (Transaction trans in Data.Transactions)
                {
                    balance += trans.Amount;
                    if (trans.Balance != balance)
                    {
                        trans.Balance = balance;

                        // executes query
                        SqlCommand accUpdateCmd = new(accUpdateQuery, con);
                        accUpdateCmd.Parameters.AddWithValue("@Balance", trans.Balance);
                        accUpdateCmd.Parameters.AddWithValue("@TransactionID", trans.TransactionID);
                        accUpdateCmd.ExecuteNonQuery();
                    }
                }

                con.Close();

                int endTime = (DateTime.Now - startTime).Milliseconds;
                MessageBox.Show($"Running balance successfully updated in {endTime} milliseconds");

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

                int i = 1;
                foreach (Transaction trans in Data.Transactions)
                {
                    if (trans.Order != i)
                    {
                        trans.Order = i;
                        SqlCommand accUpdateCmd = new(orderUpdateQuery, con);
                        accUpdateCmd.Parameters.AddWithValue("@Order", trans.Order);
                        accUpdateCmd.Parameters.AddWithValue("@TransactionID", trans.TransactionID);
                        accUpdateCmd.ExecuteNonQuery();
                    }

                    i++;
                }

                con.Close();

                int endTime = (DateTime.Now - startTime).Milliseconds;
                MessageBox.Show($"Running balance successfully updated in {endTime} milliseconds");
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
