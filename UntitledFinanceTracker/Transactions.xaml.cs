using System;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Linq;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class Transactions : UserControl
    {
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
            if (Data.Transactions.Count > 0)
            {
                dgTransactions.ItemsSource = Data.Transactions.
                    OrderBy(x => x.Date).
                    ThenBy(x => x.Amount).
                    ThenBy(x => x.CategoryName).
                    ThenBy(x => x.TransactionID);
            }
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
            dgTransactions.Items.Refresh();
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

            dgTransactions.Items.Refresh();
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

                    dgTransactions.Items.Refresh();
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
