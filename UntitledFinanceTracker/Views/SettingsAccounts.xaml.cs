using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for SettingsAccounts.xaml
    /// </summary>
    public partial class SettingsAccounts : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsAccounts()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads accounts into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (Data.Accounts.Count > 0)
                dgAccounts.ItemsSource = Data.Accounts;
        }

        /// <summary>
        /// Adds an account to the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Window addAccount = new EditAccounts();
            addAccount.Title = "Add Account";
            addAccount.ShowDialog();

            dgAccounts.Items.Refresh();
        }

        /// <summary>
        /// Edits an account and updates the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Account row = (Account)(sender as Button).DataContext;
            int ID = row.AccountID;

            Window editAccount = new EditAccounts(ID);
            editAccount.ShowDialog();

            dgAccounts.Items.Refresh();
        }

        /// <summary>
        /// Deletes an account from the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // prompts user to confirm deletion
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this account?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Account row = (Account)(sender as Button).DataContext; // gets account from dataGrid

                    Data.Accounts.Remove(row); // deletes account from collection

                    // deletes account from database
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    string query = "DELETE FROM Accounts WHERE AccountID = @ID";
                    con.Open();
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@ID", row.AccountID);
                    command.ExecuteNonQuery();
                    con.Close();

                    dgAccounts.Items.Refresh();
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