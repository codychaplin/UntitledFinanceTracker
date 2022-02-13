using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for EditTransaction.xaml
    /// </summary>
    public partial class EditTransaction : Window
    {
        Transaction transaction { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EditTransaction()
        {
            InitializeComponent();

            transaction = new();
            btnEdit.Content = "Add";
            btnCSV.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Parameterized constructor setting the ID
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        public EditTransaction(int ID)
        {
            InitializeComponent();

            SetTransaction(ID);
            btnCSV.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Loads transaction data into input fields
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        void SetTransaction(int ID)
        {
            try
            {
                IEnumerable<Transaction> t = from trans in Data.Transactions
                                             where trans.TransactionID == ID
                                             select trans;
                
                transaction = t.Count() == 1 ? t.First() : throw new Exception("ERROR: Could not find record");

                // sets input values from transaction
                dpDate.SelectedDate = transaction.Date;
                cbAccounts.SelectedValue = transaction.AccountID;
                txtAmount.Text = transaction.AmountString;
                cbCategories.SelectedValue = transaction.CategoryID;
                cbSubcategories.SelectedValue = transaction.SubcategoryID;
                txtPayee.Text = transaction.Payee;
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
            cbAccounts.ItemsSource = Data.Accounts;
            cbCategories.ItemsSource = from cat in Data.Categories
                                       where cat.ParentID == null && cat.CategoryID != 1
                                       select cat;
        }

        /// <summary>
        /// Updates/adds Transaction to collection and database
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

                decimal originalAmount = transaction.Amount;

                // update transaction
                transaction.Date = (DateTime)dpDate.SelectedDate;
                transaction.AccountID = (int)cbAccounts.SelectedValue;
                transaction.AccountName = cbAccounts.Text;
                transaction.AmountString = txtAmount.Text;
                transaction.CategoryID = (int)cbCategories.SelectedValue;
                transaction.CategoryName = cbCategories.Text;
                transaction.SubcategoryID = (int)cbSubcategories.SelectedValue;
                transaction.SubcategoryName = cbSubcategories.Text;
                transaction.Payee = txtPayee.Text;

                Account acc = Data.Accounts.First(a => a.AccountID == transaction.AccountID);

                if (Title == "Edit Transaction")
                {
                    // updates database
                    string query = "UPDATE Transactions SET Date=@Date, Account_fk=@AccountID, Amount=@Amount, " +
                                   "Category_fk=@CategoryID, Subcategory_fk=@SubcategoryID, Payee=@Payee " +
                                   "WHERE TransactionID=@TransactionID";

                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@Date", transaction.DateString);
                    command.Parameters.AddWithValue("@AccountID", transaction.AccountID);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@CategoryID", transaction.CategoryID);
                    command.Parameters.AddWithValue("@SubcategoryID", transaction.SubcategoryID);
                    command.Parameters.AddWithValue("@Payee", transaction.Payee);
                    command.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                    command.ExecuteNonQuery();

                    // update current account balance in collection
                    acc.CurrentBalance += transaction.Amount - originalAmount;

                    // updates current account balance in database
                    string query1 = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                    "WHERE AccountID=@AccountID";

                    SqlCommand command1 = new(query1, con);
                    command1.Parameters.AddWithValue("@CurrentBalance", acc.CurrentBalance);
                    command1.Parameters.AddWithValue("@AccountID", acc.AccountID);
                    command1.ExecuteNonQuery();
                }
                else if (Title == "Add Transaction")
                {
                    string query = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee) " +
                                   "OUTPUT INSERTED.TransactionID " +
                                   "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @Payee)";

                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@Date", transaction.DateString);
                    command.Parameters.AddWithValue("@AccountID", transaction.AccountID);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@CategoryID", transaction.CategoryID);
                    command.Parameters.AddWithValue("@SubcategoryID", transaction.SubcategoryID);
                    command.Parameters.AddWithValue("@Payee", transaction.Payee);
                    int ID = (int)command.ExecuteScalar();

                    // update current account balance in collection
                    acc.CurrentBalance += transaction.Amount;

                    // updates current account balance in database
                    string query1 = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                    "WHERE AccountID=@AccountID";

                    SqlCommand command1 = new(query1, con);
                    command1.Parameters.AddWithValue("@CurrentBalance", acc.CurrentBalance);
                    command1.Parameters.AddWithValue("@AccountID", acc.AccountID);
                    command1.ExecuteNonQuery();

                    // create and add newTransaction to collection
                    Transaction newTransaction = new(ID, transaction);
                    Data.Transactions.Add(newTransaction);
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

        /// <summary>
        /// Updates Subcategories ComboBox based on currently selected Category
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChangedEventArgs data.</param>
        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ID = (int)cbCategories.SelectedValue;
            IEnumerable<Category> categories = from cat in Data.Categories
                                               where cat.ParentID == ID
                                               select cat;

            cbSubcategories.ItemsSource = categories;
        }

        /// <summary>
        /// Uploads CSV file and imports data into collection/database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog upload = new();
            upload.Filter = "CSV Files (*.csv)|*.csv";

            // if file is uplaoded
            if (upload.ShowDialog() == true)
            {
                FileStream fs = new(upload.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new(fs);

                try
                {
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    con.Open();

                    List<Transaction> transactions = new();

                    // for each line, convert to Transaction object
                    while (sr.Peek() != -1)
                    {
                        string row = sr.ReadLine();
                        string[] column = row.Split(',');

                        // get IDs from names
                        var accountID = from acc in Data.Accounts
                                        where acc.AccountName == column[1]
                                        select acc.AccountID;
                        if (accountID.Count() < 1)
                            throw new Exception(column[1] + " is not a valid account");

                        var categoryID = from cat in Data.Categories
                                         where cat.CategoryName == column[3]
                                         select cat.CategoryID;
                        if (categoryID.Count() < 1)
                            throw new Exception(column[3] + " is not a valid category");

                        var subCategoryID = from cat in Data.Categories
                                            where cat.CategoryName == column[4]
                                            select cat.CategoryID;
                        if (subCategoryID.Count() < 1)
                            throw new Exception(column[4] + " is not a valid subcategory");

                        Transaction trans = new();
                        trans.DateString = column[0];
                        trans.AccountID = accountID.First();
                        trans.AccountName = column[1];
                        trans.AmountString = column[2];
                        trans.CategoryID = categoryID.First();
                        trans.CategoryName = column[3];
                        trans.SubcategoryID = subCategoryID.First();
                        trans.SubcategoryName = column[4];
                        trans.Payee = column[5];

                        transactions.Add(trans);
                    }

                    // after all data passes validation, insert each into collection/database
                    foreach (Transaction trans in transactions)
                    {
                        string query = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee) " +
                                       "OUTPUT INSERTED.TransactionID " +
                                       "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @Payee)";

                        SqlCommand command = new(query, con);
                        command.Parameters.AddWithValue("@Date", trans.DateString);
                        command.Parameters.AddWithValue("@AccountID", trans.AccountID);
                        command.Parameters.AddWithValue("@Amount", trans.Amount);
                        command.Parameters.AddWithValue("@CategoryID", trans.CategoryID);
                        command.Parameters.AddWithValue("@SubcategoryID", trans.SubcategoryID);
                        command.Parameters.AddWithValue("@Payee", trans.Payee);
                        int ID = (int)command.ExecuteScalar();

                        // create and add newTransaction to collection
                        Transaction newTransaction = new(ID, trans);
                        Data.Transactions.Add(newTransaction);
                    }

                    sr.Close();
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
        }
    }
}
