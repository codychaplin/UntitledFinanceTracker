using System;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;

namespace UntitledFinanceTracker
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
            accounts = Data.Accounts.ToList();
            foreach (var acc in accounts)
                acc.Enabled = true;
            cbAccounts.ItemsSource = accounts;

            // enables all categories
            categories = Data.Categories.Where(c => c.ParentID == null).ToList();
            foreach (var cat in categories)
                cat.Enabled = true;
            cbCategories.ItemsSource = categories;

            // enables all subcategories
            subcategories = Data.Categories.Where(c => c.ParentID != null).ToList();
            foreach (var subcat in subcategories)
                subcat.Enabled = true;
            cbSubcategories.ItemsSource = subcategories;

            // filters transactions to current year
            if (Data.Transactions.Count > 0)
            {
                int startingDate = Data.Transactions.OrderBy(x => x.Date).First().Date.Year;
                int endDate = DateTime.Now.Year;
                List<int> dates = new();
                for (int i = startingDate; i <= endDate; i++)
                    dates.Add(i);
                cbYears.ItemsSource = dates;
                cbYears.SelectedValue = dates.Last();
            }
        }

        void Filter()
        {
            dgTransactions.ItemsSource = Data.Transactions.
                    Where(t => t.Date.Year == (int)cbYears.SelectedValue).
                    Where(t => accounts.Where(a => a.Enabled == true).Select(a => a.AccountID).ToList().Contains(t.AccountID)).
                    Where(t => categories.Where(c => c.Enabled == true).Select(c => c.CategoryID).ToList().Contains(t.CategoryID)).
                    Where(t => subcategories.Where(s => s.Enabled == true).Select(s => s.CategoryID).ToList().Contains(t.SubcategoryID)).
                    OrderBy(t => t.Date).
                    ThenBy(t => t.Amount).
                    ThenBy(t => t.CategoryName).
                    ThenBy(t => t.TransactionID);
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

            dgTransactions.ItemsSource = Data.Transactions;
            //dgTransactions.Items.Refresh();
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

            //dgTransactions.Items.Refresh();
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
                    Transaction row = (Transaction)(sender as Button).DataContext; // gets transaction from dataGrid

                    // gets reference to account and updates current balance
                    Account acc = Data.Accounts.First(a => a.AccountID == row.AccountID);
                    acc.CurrentBalance -= row.Amount;

                    Data.Transactions.Remove(row); // deletes transaction from collection

                    // deletes transaction from database
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    con.Open();

                    string query = "DELETE FROM Transactions WHERE TransactionID = @ID";
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@ID", row.TransactionID);
                    command.ExecuteNonQuery();

                    // updates current account balance in database
                    string query1 = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                    "WHERE AccountID=@AccountID";
                    SqlCommand command1 = new(query1, con);
                    command1.Parameters.AddWithValue("@CurrentBalance", acc.CurrentBalance);
                    command1.Parameters.AddWithValue("@AccountID", acc.AccountID);
                    command1.ExecuteNonQuery();

                    con.Close();

                    //dgTransactions.Items.Refresh();
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
    }
}
