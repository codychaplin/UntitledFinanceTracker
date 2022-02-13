using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Controls;
using System.Data.SqlClient;

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
                string query = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                               "WHERE AccountID=@AccountID";

                SqlConnection con = new(connectionString);
                con.Open();

                foreach (Account account in Data.Accounts)
                {
                    // get income/expense categories
                    var incomeCatIDs = from cat in Data.Categories
                                       where cat.CategoryID == 4 // 4 = income
                                       select cat.CategoryID;
                    if (incomeCatIDs.Count() != 1)
                        throw new Exception("Could not find income category");

                    var expenseCatIDs = from cat in Data.Categories
                                        where cat.CategoryID > 4 && cat.ParentID == null // >4 = all expense categories
                                        select cat.CategoryID;
                    if (expenseCatIDs.Count() < 1)
                        throw new Exception("No expense catgories found");

                    // get list of transactions grouped by income/expense categories
                    var incomes = Data.Transactions.Where(t => t.AccountID == account.AccountID &&
                        (t.SubcategoryID == 3 || incomeCatIDs.Contains(t.CategoryID)));
                    var expenses = Data.Transactions.Where(t => t.AccountID == account.AccountID &&
                        (t.SubcategoryID == 2 || expenseCatIDs.Contains(t.CategoryID)));

                    // calculate sum
                    decimal sum = account.StartingBalance + incomes.Sum(t => t.Amount) - expenses.Sum(t => t.Amount);
                    account.CurrentBalance = sum;
                    
                    // executes query
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@CurrentBalance", account.CurrentBalance);
                    command.Parameters.AddWithValue("@AccountID", account.AccountID);
                    command.ExecuteNonQuery();
                }

                con.Close();
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
