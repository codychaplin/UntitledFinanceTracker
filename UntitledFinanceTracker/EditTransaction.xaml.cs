using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Collections.Generic;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for EditTransaction.xaml
    /// </summary>
    public partial class EditTransaction : Window
    {
        Transaction transaction { get; set; }

        public EditTransaction()
        {
            InitializeComponent();
        }

        public EditTransaction(int ID)
        {
            InitializeComponent();

            SetTransaction(ID);
        }

        void SetTransaction(int ID)
        {
            try
            {
                IEnumerable<Transaction> t = from trans in Data.transactions where trans.TransactionID == ID select trans;
                
                transaction = t.Count() == 1 ? t.First() : throw new Exception("ERROR: ID returned more than 1 row");

                // sets input values from transaction
                dpDate.SelectedDate = transaction.Date;
                cbAccounts.SelectedValue = transaction.AccountID;
                cbTypes.Text = transaction.Type.ToString();
                txtAmount.Text = transaction.Amount.ToString();
                cbCategories.SelectedValue = transaction.CategoryID;
                cbSubcategories.SelectedValue = transaction.SubcategoryID;
                txtPayee.Text = transaction.Payee;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                string accountsQuery = "SELECT AccountID, AccountName FROM Accounts";
                string categoriesQuery = "SELECT CategoryID, CategoryName FROM Categories " +
                    "WHERE ParentID_fk IS NULL AND CategoryType <> 'Transfer'";
                DataSet dsA = new DataSet("Accounts");
                DataSet dsC = new DataSet("Categories");

                using (SqlConnection con = new(connectionString))
                {
                    using (SqlCommand cmdAccounts = new(accountsQuery, con))
                        using (SqlDataAdapter adapter = new(cmdAccounts))
                            adapter.Fill(dsA, "Accounts");

                    using (SqlCommand cmdCategories = new(categoriesQuery, con))
                        using (SqlDataAdapter adapter = new(cmdCategories))
                            adapter.Fill(dsC, "Categories");
                }

                cbAccounts.ItemsSource = dsA.Tables["Accounts"].DefaultView;
                cbCategories.ItemsSource = dsC.Tables["Categories"].DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(cbTypes.Text);

                /*// sets input values from transaction
                transaction.Date = (DateTime)dpDate.SelectedDate;
                transaction.AccountID = (int)cbAccounts.SelectedValue;
                transaction.Type = (cbTypes.SelectedValue.ToString() == "Income") ? CategoryType.Income : CategoryType.Expense;
                transaction.Amount = Convert.ToDecimal(txtAmount.Text);
                transaction.CategoryID = (int)cbCategories.SelectedValue;
                transaction.SubcategoryID = (int)cbSubcategories.SelectedValue;
                transaction.Payee = txtPayee.Text;

                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                string setQuery = "UPDATE Transactions SET Date = '" + transaction.Date + "'" +
                    ", Account_fk = " + transaction.AccountID +
                    ", Type = '" + transaction.Type + "'" +
                    ", Amount = '" + transaction.Amount + "'" +
                    ", Category_fk = " + transaction.CategoryID +
                    ", Subcategory_fk = " + transaction.SubcategoryID +
                    ", Payee = '" + transaction.Payee + "' " +
                    " WHERE TransactionID = " + transaction.TransactionID;

                SqlCommand command = new(setQuery, con);
                command.ExecuteNonQuery();
                con.Close();

                Tag = transaction;*/

                //Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int ID = (int)cbCategories.SelectedValue;

                string connectionString = Properties.Settings.Default.connectionString;
                string query = "SELECT CategoryID, CategoryName FROM Categories " +
                    "WHERE ParentID_fk = " + ID;
                DataSet ds = new DataSet("Subcategories");

                using (SqlConnection con = new(connectionString))
                    using (SqlCommand cmdAccounts = new(query, con))
                        using (SqlDataAdapter adapter = new(cmdAccounts))
                            adapter.Fill(ds, "Subcategories");

                cbSubcategories.ItemsSource = ds.Tables["Subcategories"].DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
