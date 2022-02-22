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
        private void lviUpdateBalances_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // updates current balance of account in database
                string connectionString = Properties.Settings.Default.connectionString;
                string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                        "WHERE AccountID=@AccountID";

                SqlConnection con = new(connectionString);
                con.Open();

                foreach (Account account in Data.Accounts)
                {
                    // get income/expense categories
                    int incomeCatID = 3;
                    var expenseCatIDs = from cat in Data.Categories
                                        where cat.CategoryID > 3 && cat.ParentID == null // >3 = all expense categories
                                        select cat.CategoryID;
                    if (!expenseCatIDs.Any())
                        throw new Exception("No expense catgories found");

                    // get list of transactions grouped by income/expense categories
                    var incomes = Data.Transactions.Where(t => t.AccountID == account.AccountID
                        && t.CategoryID == incomeCatID);
                    var expenses = Data.Transactions.Where(t => t.AccountID == account.AccountID
                        && expenseCatIDs.Contains(t.CategoryID));
                    var transferDebits = Data.Transactions.Where(t => t.CategoryID == 1
                        && t.AccountID == account.AccountID);
                    var transferCredits = Data.Transactions.Where(t => t.CategoryID == 1
                        && t.PayeeAccountID == account.AccountID);

                    // calculate sum
                    decimal sum = account.StartingBalance + incomes.Sum(t => t.Amount) + transferCredits.Sum(t => t.Amount)
                                - expenses.Sum(t => t.Amount) - transferDebits.Sum(t => t.Amount);
                    account.CurrentBalance = sum;
                    
                    // executes query
                    SqlCommand accUpdateCmd = new(accUpdateQuery, con);
                    accUpdateCmd.Parameters.AddWithValue("@CurrentBalance", account.CurrentBalance);
                    accUpdateCmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    accUpdateCmd.ExecuteNonQuery();
                }

                con.Close();

                MessageBox.Show("Account balances successfully updated");
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
