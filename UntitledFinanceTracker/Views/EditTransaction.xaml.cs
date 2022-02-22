using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
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
                txtPayee.Text = transaction.PayeeName;
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
                                       where cat.ParentID == null
                                       select cat;

            dpDate.SelectedDate = DateTime.Now.Date;
            
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
                int originalCategoryID = transaction.CategoryID;
                int? originalPayeeAccountID = transaction.PayeeAccountID;
                Payee payee = (from pay in Data.Payees
                              where pay.PayeeName == txtPayee.Text
                              select pay).First();

                // update transaction
                transaction.Date = (DateTime)dpDate.SelectedDate;
                transaction.AccountID = (int)cbAccounts.SelectedValue;
                transaction.AccountName = cbAccounts.Text;
                transaction.AmountString = txtAmount.Text;
                transaction.CategoryID = (int)cbCategories.SelectedValue;
                transaction.CategoryName = cbCategories.Text;
                transaction.SubcategoryID = (int)cbSubcategories.SelectedValue;
                transaction.SubcategoryName = cbSubcategories.Text;
                transaction.PayeeID = payee.PayeeID;
                transaction.PayeeAccountID = payee.AccountID;
                transaction.PayeeName = payee.PayeeName;

                // gets account(s) from transaction
                Account account = Data.Accounts.First(a => a.AccountID == transaction.AccountID);
                Account payeeAccount = null;

                // updates current account balance in database
                string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                        "WHERE AccountID=@AccountID";

                if (Title == "Edit Transaction")
                {
                    // updates database
                    string transUpdateQuery = "UPDATE Transactions SET Date=@Date, Account_fk=@AccountID, Amount=@Amount, " +
                                              "Category_fk=@CategoryID, Subcategory_fk=@SubcategoryID, Payee_fk =@Payee " +
                                              "WHERE TransactionID=@TransactionID";

                    SqlCommand transUpdateCmd = new(transUpdateQuery, con);
                    transUpdateCmd.Parameters.AddWithValue("@Date", transaction.DateString);
                    transUpdateCmd.Parameters.AddWithValue("@AccountID", transaction.AccountID);
                    transUpdateCmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    transUpdateCmd.Parameters.AddWithValue("@CategoryID", transaction.CategoryID);
                    transUpdateCmd.Parameters.AddWithValue("@SubcategoryID", transaction.SubcategoryID);
                    transUpdateCmd.Parameters.AddWithValue("@Payee", transaction.PayeeID);
                    transUpdateCmd.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                    transUpdateCmd.ExecuteNonQuery();

                    // update current account balance in collection
                    account.CurrentBalance = (originalCategoryID == 3) // 3 = income
                        ? account.CurrentBalance - originalAmount : account.CurrentBalance + originalAmount;

                    account.CurrentBalance = (transaction.CategoryID == 3) // 3 = income
                        ? account.CurrentBalance + transaction.Amount : account.CurrentBalance - transaction.Amount;

                    UpdateAccountBalance(accUpdateQuery, ref con, account);

                    if (originalCategoryID == 1) // 1 = transfer
                    {
                        // gets account from payee account ID
                        payeeAccount = Data.Accounts.First(a => a.AccountID ==
                            Data.Payees.Where(p => p.AccountID == originalPayeeAccountID)
                            .Select(p => p.AccountID).First());

                        // update current account balance in collection
                        payeeAccount.CurrentBalance -= originalAmount;
                        
                        UpdateAccountBalance(accUpdateQuery, ref con, payeeAccount);
                    }

                    if (transaction.CategoryID == 1) // 1 = transfer
                    {
                        // gets account from payee account ID
                        payeeAccount = Data.Accounts.First(a => a.AccountID ==
                            Data.Payees.Where(p => p.AccountID == transaction.PayeeAccountID)
                            .Select(p => p.AccountID).First());

                        // update current account balance in collection
                        payeeAccount.CurrentBalance += transaction.Amount;

                        UpdateAccountBalance(accUpdateQuery, ref con, payeeAccount);
                    }
                }
                else if (Title == "Add Transaction")
                {
                    string transInsertQuery = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee_fk) " +
                                              "OUTPUT INSERTED.TransactionID " +
                                              "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @Payee)";

                    SqlCommand transInsertCmd = new(transInsertQuery, con);
                    transInsertCmd.Parameters.AddWithValue("@Date", transaction.DateString);
                    transInsertCmd.Parameters.AddWithValue("@AccountID", transaction.AccountID);
                    transInsertCmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    transInsertCmd.Parameters.AddWithValue("@CategoryID", transaction.CategoryID);
                    transInsertCmd.Parameters.AddWithValue("@SubcategoryID", transaction.SubcategoryID);
                    transInsertCmd.Parameters.AddWithValue("@Payee", transaction.PayeeID);
                    int ID = (int)transInsertCmd.ExecuteScalar();

                    // create and add newTransaction to collection
                    Transaction newTransaction = new(ID, transaction);
                    Data.Transactions.Add(newTransaction);

                    if (transaction.CategoryID == 1) // 1 = transfer
                    {
                        // update current account balance in collection
                        account.CurrentBalance -= transaction.Amount;

                        UpdateAccountBalance(accUpdateQuery, ref con, account);

                        // gets account from payee account ID
                        var acc = Data.Accounts.First(a => a.AccountID ==
                                  Data.Payees.Where(p => p.AccountID == transaction.PayeeAccountID)
                                  .Select(p => p.AccountID).First());
                        
                        acc.CurrentBalance += transaction.Amount;

                        UpdateAccountBalance(accUpdateQuery, ref con, acc);
                    }
                    else
                    {
                        account.CurrentBalance = transaction.CategoryID == 3 // 3 = income
                            ? account.CurrentBalance + transaction.Amount : account.CurrentBalance - transaction.Amount;

                        UpdateAccountBalance(accUpdateQuery, ref con, account);
                    }
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
        /// Updates Accounts table in database, using provided query and Account
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="con">reference to database connection.</param>
        /// <param name="account">Account object.</param>
        void UpdateAccountBalance(string query, ref SqlConnection con, Account account)
        {
            SqlCommand accUpdateCmd = new(query, con);
            accUpdateCmd.Parameters.AddWithValue("@CurrentBalance", account.CurrentBalance);
            accUpdateCmd.Parameters.AddWithValue("@AccountID", account.AccountID);
            accUpdateCmd.ExecuteNonQuery();
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

            // if only 1 subcategory, select it
            if (categories.Count() == 1)
                cbSubcategories.SelectedIndex = 0;
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
                        if (!accountID.Any())
                            throw new Exception(column[1] + " is not a valid account");

                        var categoryID = from cat in Data.Categories
                                         where cat.CategoryName == column[3]
                                         select cat.CategoryID;
                        if (!categoryID.Any())
                            throw new Exception(column[3] + " is not a valid category");

                        var subCategoryID = from cat in Data.Categories
                                            where cat.CategoryName == column[4] && cat.ParentID == categoryID.First()
                                            select cat.CategoryID;
                        if (!subCategoryID.Any())
                            throw new Exception(column[4] + " is not a valid subcategory");

                        var payeeID = from pay in Data.Payees
                                      where pay.PayeeName == column[5]
                                      select pay.PayeeID;

                        // set transaction attributes
                        Transaction trans = new();
                        trans.DateString = column[0];
                        trans.AccountID = accountID.First();
                        trans.AccountName = column[1];
                        trans.AmountString = column[2];
                        trans.CategoryID = categoryID.First();
                        trans.CategoryName = column[3];
                        trans.SubcategoryID = subCategoryID.First();
                        trans.SubcategoryName = column[4];

                        // if category is "Transfer"
                        if (categoryID.First() == 1)
                        {
                            // get AccountID from account name in payee field
                            var account = from acc in Data.Accounts
                                          where acc.AccountName == column[5]
                                          select acc.AccountID;

                            // if account exists
                            if (account.Any())
                            {
                                trans.PayeeAccountID = account.First();
                                trans.PayeeName = column[5];
                            }
                            else
                                throw new Exception(column[5] + " is not a valid account");
                        }
                        else
                        {
                            trans.PayeeAccountID = null;
                            trans.PayeeName = column[5];
                        }

                        if (payeeID.Any())
                            trans.PayeeID = payeeID.First();
                        else
                        {
                            string payeeInsertQuery = "INSERT INTO Payees (AccountID_fk, PayeeName) " +
                                                      "OUTPUT INSERTED.PayeeID " +
                                                      "VALUES (@AccountID, @PayeeName)";

                            SqlCommand payeeInsertCmd = new(payeeInsertQuery, con);

                            payeeInsertCmd.Parameters.AddWithValue("@AccountID", (trans.PayeeAccountID == null) ? DBNull.Value : trans.PayeeAccountID);
                            payeeInsertCmd.Parameters.AddWithValue("@PayeeName", trans.PayeeName);
                            int ID = (int)payeeInsertCmd.ExecuteScalar();

                            trans.PayeeID = ID;
                            Data.Payees.Add(new Payee(ID, trans.PayeeAccountID, trans.PayeeName));
                        }

                        transactions.Add(trans);
                    }

                    // after all data passes validation, insert each into collection/database
                    foreach (Transaction trans in transactions)
                    {

                        string transInsertQuery = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee_fk) " +
                                                  "OUTPUT INSERTED.TransactionID " +
                                                  "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @Payee)";

                        SqlCommand transInsertCmd = new(transInsertQuery, con);
                        transInsertCmd.Parameters.AddWithValue("@Date", trans.DateString);
                        transInsertCmd.Parameters.AddWithValue("@AccountID", trans.AccountID);
                        transInsertCmd.Parameters.AddWithValue("@Amount", trans.Amount);
                        transInsertCmd.Parameters.AddWithValue("@CategoryID", trans.CategoryID);
                        transInsertCmd.Parameters.AddWithValue("@SubcategoryID", trans.SubcategoryID);
                        transInsertCmd.Parameters.AddWithValue("@Payee", trans.PayeeID);
                        int ID = (int)transInsertCmd.ExecuteScalar();

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
