using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for EditAccountTypes.xaml
    /// </summary>
    public partial class EditAccountTypes : Window
    {
        AccountType accountType { get; set; }

        /// <summary>
        /// Interaction logic for EditTransaction.xaml
        /// </summary>
        public EditAccountTypes()
        {
            InitializeComponent();

            accountType = new();
            btnEdit.Content = "Add";
        }

        /// <summary>
        /// Parameterized constructor setting the ID
        /// </summary>
        /// <param name="ID">Account Type ID.</param>
        public EditAccountTypes(int ID)
        {
            InitializeComponent();

            SetAccountType(ID);
        }

        /// <summary>
        /// Loads account type data into input field
        /// </summary>
        /// <param name="ID">Account Type ID.</param>
        void SetAccountType(int ID)
        {
            try
            {
                IEnumerable<AccountType> at = Data.AccountTypes.Where(at => at.AccountTypeID == ID);
                accountType = at.Count() == 1 ? at.First() : throw new Exception("ERROR: Could not find account type");

                // sets input value from account type
                txtAccountType.Text = accountType.AccountTypeName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Updates/adds account type to collection and database
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

                // update accountType
                accountType.AccountTypeName = txtAccountType.Text;

                if (Title == "Edit Account Type")
                {
                    // updates database
                    string query = "UPDATE AccountTypes SET AccountType=@AccountTypeName " +
                                   "WHERE AccountTypeID=@AccountTypeID";

                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@AccountTypeName", accountType.AccountTypeName);
                    command.Parameters.AddWithValue("@AccountTypeID", accountType.AccountTypeID);
                    command.ExecuteNonQuery();
                }
                else if (Title == "Add Account Type")
                {
                    // updates database
                    string query = "INSERT INTO AccountTypes (AccountType) " +
                                   "OUTPUT INSERTED.AccountTypeID " +
                                   "VALUES (@AccountTypeName)";

                    // execute query and get ID of new accountType
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@AccountTypeName", accountType.AccountTypeName);
                    int ID = (int)command.ExecuteScalar();

                    // create and add newAccountType to collection
                    AccountType newAccountType = new(ID, accountType);
                    Data.AccountTypes.Add(newAccountType);
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
