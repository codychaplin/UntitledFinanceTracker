using System;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Collections.Generic;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class Transactions : UserControl
    {
        List<Account> accounts { get; set; }
        List<Category> categories { get; set; }
        List<Category> subcategories { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Transactions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads transactions into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            // enables all accounts
            accounts = new();
            for (int i = 0; i < Data.Accounts.Count; i++)
            {
                accounts.Add(new Account(Data.Accounts[i]));
                //accounts[i].Enabled = true;
            }
            cbAccounts.ItemsSource = accounts;

            // enables all categories
            categories = new();
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                categories.Add(new Category(Data.Categories[i]));
                //categories[i].Enabled = true;
            }
            cbCategories.ItemsSource = categories.Where(c => c.ParentID == null);

            // enables all subcategories
            subcategories = new();
            for (int i = 0; i < Data.Categories.Count; i++)
            {
                subcategories.Add(new Category(Data.Categories[i]));
                //subcategories[i].Enabled = true;
            }
            cbSubcategories.ItemsSource = subcategories.Where(c => c.ParentID != null);

            // filters transactions to current year
            if (Data.Transactions.Count > 0)
            {
                int startingDate = Data.Transactions.Min(x => x.Date).Year;
                int endDate = DateTime.Now.Year;
                List<int> dates = new();
                for (int i = startingDate; i <= endDate; i++)
                    dates.Add(i);
                cbYears.ItemsSource = dates;
                cbYears.SelectedValue = dates.Last();
            }
            else
            {
                cbYears.ItemsSource = new List<int> { DateTime.Now.Year };
                cbYears.SelectedValue = DateTime.Now.Year;
            }
        }

        void Filter()
        {
            // order by year, active accounts, active sub/categories, then by Order
            int year = (int)cbYears.SelectedValue;
            dgTransactions.ItemsSource = Data.Transactions.
                Where(t => t.Date.Year == year).
                Where(t => accounts.Where(a => a.Enabled == true).Select(a => a.AccountID).Contains(t.AccountID)).
                Where(t => categories.Where(c => c.Enabled == true).Select(c => c.CategoryID).Contains(t.CategoryID)).
                Where(t => subcategories.Where(s => s.Enabled == true).Select(s => s.CategoryID).Contains(t.SubcategoryID)).
                OrderBy(t => t.Order);

            ((MainWindow)Application.Current.MainWindow).RefreshBalances();
        }

        private void cbYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void ChkAccount_Click(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        private void ChkCategory_Click(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        private void ChkSubcategory_Click(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        /// <summary>
        /// Adds a transaction to the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Window addTransaction = new EditTransaction();
            addTransaction.Title = "Add Transaction";
            addTransaction.ShowDialog();

            Filter();
        }

        /// <summary>
        /// Edits a transaction and updates the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Transaction row = (Transaction)(sender as Button).DataContext;
            int ID = row.TransactionID;

            Window editTransaction = new EditTransaction(ID);
            editTransaction.ShowDialog();

            Filter();
        }

        /// <summary>
        /// Deletes a transaction from the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // prompts user to confirm deletion
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this transaction?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    con.Open();

                    Transaction transaction = (Transaction)(sender as Button).DataContext; // gets transaction from dataGrid

                    // gets reference to account and updates current balance
                    Account acc = Data.Accounts.First(a => a.AccountID == transaction.AccountID);
                    acc.CurrentBalance -= transaction.Amount;

                    int index = Data.Transactions.IndexOf(transaction); // get index of transaction before removal
                    if (index >= Data.Transactions.Count - 1) // if last in collection, set to count - 2 to avoid OutOfRange Exception
                        index = Data.Transactions.Count - 2;
                    if (transaction.CategoryID == Data.TRANSFER_ID) // if deleting credit, decrement by 1 again to avoid OutOfRange Exception
                        index--;

                    // transaction delete query
                    string transDeleteQuery = "DELETE FROM Transactions WHERE TransactionID = @ID";
                    // account balance update query
                    string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance WHERE AccountID=@AccountID";

                    // if transfer, delete other side of transfer from memory and database too
                    if (transaction.CategoryID == Data.TRANSFER_ID)
                    {
                        // delete other transaction
                        Transaction otherTransaction = Data.Transactions.First(t => t.TransactionID == transaction.TransferID);
                        Data.Transactions.Remove(otherTransaction);
                        SqlCommand otherTransDeleteCmd = new(transDeleteQuery, con);
                        otherTransDeleteCmd.Parameters.AddWithValue("@ID", otherTransaction.TransactionID);
                        otherTransDeleteCmd.ExecuteNonQuery();

                        // updates other account's balance
                        Account otherAccount = Data.Accounts.First(a => a.AccountID == otherTransaction.AccountID);
                        otherAccount.CurrentBalance -= otherTransaction.Amount;
                        UpdateAccountBalance(accUpdateQuery, ref con, otherAccount);
                    }

                    // delete transaction
                    Data.Transactions.Remove(transaction); // deletes transaction from collection
                    SqlCommand transDeleteCmd = new(transDeleteQuery, con);
                    transDeleteCmd.Parameters.AddWithValue("@ID", transaction.TransactionID);
                    transDeleteCmd.ExecuteNonQuery();

                    UpdateAccountBalance(accUpdateQuery, ref con, acc); // updates current account balance in database
                    Data.UpdateOrderAndRunningBalance(Data.Transactions[index]); // update balance and order after removal
                    Filter();

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
        }

        /// <summary>
        /// Updates Accounts table in database, using provided query and Account
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="con">reference to database connection.</param>
        /// <param name="account">Account object.</param>
        void UpdateAccountBalance(string query, ref SqlConnection con, Account account)
        {
            SqlCommand accUpdateCmd = new(query, con);
            accUpdateCmd.Parameters.AddWithValue("@CurrentBalance", account.CurrentBalance);
            accUpdateCmd.Parameters.AddWithValue("@AccountID", account.AccountID);
            accUpdateCmd.ExecuteNonQuery();
        }
    }
}
