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
                IEnumerable<Transaction> t = from trans in Data.Transactions
                                             where trans.TransactionID == ID
                                             select trans;
                
                transaction = t.Count() == 1 ? t.First() : throw new Exception("ERROR: ID returned more than 1 row");

                // sets input values from transaction
                dpDate.SelectedDate = transaction.Date;
                cbAccounts.SelectedValue = transaction.AccountID;
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
            cbAccounts.ItemsSource = Data.Accounts;
            cbCategories.ItemsSource = from cat in Data.Categories
                                       where (cat.ParentID == null && cat.CategoryID != 1)
                                       select cat;
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // update transaction
                transaction.Date = (DateTime)dpDate.SelectedDate;
                transaction.AccountID = (int)cbAccounts.SelectedValue;
                transaction.AccountName = cbAccounts.Text;
                transaction.Amount = Convert.ToDecimal(txtAmount.Text);
                transaction.CategoryID = (int)cbCategories.SelectedValue;
                transaction.CategoryName = cbCategories.Text;
                transaction.SubcategoryID = (int)cbSubcategories.SelectedValue;
                transaction.SubcategoryName = cbSubcategories.Text;
                transaction.Payee = txtPayee.Text;

                // updates collection
                var trans = Data.Transactions.FirstOrDefault(t => t.TransactionID == transaction.TransactionID);
                trans = transaction;

                // updates database
                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                string setQuery = "UPDATE Transactions SET Date = '" + transaction.Date + "'" +
                    ", Account_fk = " + transaction.AccountID +
                    ", Amount = '" + transaction.Amount + "'" +
                    ", Category_fk = " + transaction.CategoryID +
                    ", Subcategory_fk = " + transaction.SubcategoryID +
                    ", Payee = '" + transaction.Payee + "' " +
                    " WHERE TransactionID = " + transaction.TransactionID;

                SqlCommand command = new(setQuery, con);
                command.ExecuteNonQuery();
                con.Close();
                
                Close();
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
            int ID = (int)cbCategories.SelectedValue;
            IEnumerable<Category> categories = from cat in Data.Categories
                                               where cat.ParentID == ID
                                               select cat;

            cbSubcategories.ItemsSource = categories;
        }
    }
}
