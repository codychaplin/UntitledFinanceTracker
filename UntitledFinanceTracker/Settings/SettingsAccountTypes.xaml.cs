using System;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Linq;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for SettingsAccountTypes.xaml
    /// </summary>
    public partial class SettingsAccountTypes : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsAccountTypes()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads account types into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (Data.AccountTypes.Count > 0)
                dgAccountTypes.ItemsSource = Data.AccountTypes;
        }

        /// <summary>
        /// Adds an account type to the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Window addAccountType = new EditAccountTypes();
            addAccountType.Title = "Add Account Type";
            addAccountType.ShowDialog();

            dgAccountTypes.Items.Refresh();
        }

        /// <summary>
        /// Edits an account type and updates the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            AccountType row = (AccountType)(sender as Button).DataContext;
            int ID = row.AccountTypeID;

            Window editAccountType = new EditAccountTypes(ID);
            editAccountType.ShowDialog();

            dgAccountTypes.Items.Refresh();
        }

        /// <summary>
        /// Deletes an account type from the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // prompts user to confirm deletion
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this account type?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    AccountType row = (AccountType)(sender as Button).DataContext; // gets accountType from dataGrid

                    Data.AccountTypes.Remove(row); // deletes accountType from collection

                    // deletes account type from database
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    con.Open();
                    string query = "DELETE FROM AccountTypes WHERE AccountTypeID = @ID";
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@ID", row.AccountTypeID);
                    command.ExecuteNonQuery();
                    con.Close();

                    dgAccountTypes.Items.Refresh();
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
