using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace UntitledFinanceTracker
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
                IEnumerable<AccountType> a = from accType in Data.AccountTypes
                                             where accType.AccountTypeID == ID
                                             select accType;

                accountType = a.Count() == 1 ? a.First() : throw new Exception("ERROR: ID returned more than 1 row");

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
                    // updates collection
                    AccountType accType = Data.AccountTypes.First(a => a.AccountTypeID == accountType.AccountTypeID);
                    accType = accountType;
                    
                    // updates database
                    string query = "UPDATE AccountTypes SET AccountType = '" + accountType.AccountTypeName + "'" +
                        " WHERE AccountTypeID = " + accountType.AccountTypeID;
                    SqlCommand command = new(query, con);
                    command.ExecuteNonQuery();
                }
                else if (Title == "Add Account Type")
                {
                    // updates database
                    string query = "INSERT INTO AccountTypes (AccountType)" +
                        " VALUES ('" + accountType.AccountTypeName + "')";
                    SqlCommand command = new(query, con);
                    command.ExecuteNonQuery();

                    Data.AccountTypes.Add(accountType);
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
