using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for EditAccounts.xaml
    /// </summary>
    public partial class EditAccounts : Window
    {
        Account account { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EditAccounts()
        {
            InitializeComponent();

            account = new();
            btnEdit.Content = "Add";
        }

        /// <summary>
        /// Parameterized constructor setting the ID
        /// </summary>
        /// <param name="ID">Account ID.</param>
        public EditAccounts(int ID)
        {
            InitializeComponent();

            SetAccount(ID);
        }

        /// <summary>
        /// Loads account data into input fields
        /// </summary>
        /// <param name="ID">Account Type ID.</param>
        void SetAccount(int ID)
        {
            try
            {
                IEnumerable<Account> a = Data.Accounts.Where(a => a.AccountID == ID);
                account = a.Count() == 1 ? a.First() : throw new Exception("ERROR: Could not find account");

                // sets input value from account
                txtName.Text = account.AccountName;
                cbAccountType.SelectedValue = account.AccountTypeID;
                txtBalance.Text = account.StartingBalanceString;
                chkEnabled.IsChecked = account.Enabled;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Populates Accounts and Categories ComboBoxes
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            cbAccountType.ItemsSource = Data.AccountTypes;
        }

        /// <summary>
        /// Updates/adds account to collection and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                // update account
                account.AccountName = txtName.Text;
                account.AccountTypeID = (int)cbAccountType.SelectedValue;
                account.AccountTypeName = cbAccountType.Text;
                account.StartingBalanceString = txtBalance.Text;
                account.Enabled = chkEnabled.IsChecked != null && chkEnabled.IsChecked != false;

                if (Title == "Edit Account")
                {
                    // updates database
                    string query = "UPDATE Accounts SET AccountName=@AccountName, AccountType_fk=@AccountTypeID, " +
                                   "StartingBalance=@StartingBalance, Enabled=@Enabled " +
                                   "WHERE AccountID=@AccountID";

                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@AccountName", account.AccountName);
                    command.Parameters.AddWithValue("@AccountTypeID", account.AccountTypeID);
                    command.Parameters.AddWithValue("@StartingBalance", account.StartingBalance);
                    command.Parameters.AddWithValue("@Enabled", account.Enabled);
                    command.Parameters.AddWithValue("@AccountID", account.AccountID);
                    command.ExecuteNonQuery();
                }
                else if (Title == "Add Account")
                {
                    // updates database
                    string query = "INSERT INTO Accounts (AccountName, AccountType_fk, StartingBalance, CurrentBalance, Enabled) " +
                                   "OUTPUT INSERTED.AccountID " +
                                   "VALUES (@AccountName, @AccountTypeID, @StartingBalance, @StartingBalance, @Enabled)";

                    // execute query and get ID of new account
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@AccountName", account.AccountName);
                    command.Parameters.AddWithValue("@AccountTypeID", account.AccountTypeID);
                    command.Parameters.AddWithValue("@StartingBalance", account.StartingBalance);
                    command.Parameters.AddWithValue("@Enabled", account.Enabled);
                    int ID = (int)command.ExecuteScalar();

                    // create and add newAccount to collection
                    Account newAccount = new(ID, account);
                    Data.Accounts.Add(newAccount);
                }
                else
                {
                    throw new Exception("How did this even happen");
                }

                con.Close();
                Close();
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
        /// Closes window
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
