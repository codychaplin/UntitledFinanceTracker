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

        public EditAccountTypes()
        {
            InitializeComponent();

            accountType = new();
            btnEdit.Content = "Add";
        }

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

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                if (Title == "Edit Account Type")
                {
                    // update accountType
                    accountType.AccountTypeName = txtAccountType.Text;

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
                    // update accountType
                    accountType.AccountTypeName = txtAccountType.Text;

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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
