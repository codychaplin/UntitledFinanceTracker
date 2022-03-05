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

            // blocks user from changing transaction to/from transfer
            if (transaction.CategoryID == Data.TRANSFER_ID)
                cbCategories.ItemsSource = Data.Categories.Where(c => c.CategoryID == Data.TRANSFER_ID);
            else
                cbCategories.ItemsSource = Data.Categories.Where(c => c.ParentID == null && c.CategoryID != Data.TRANSFER_ID);
        }

        /// <summary>
        /// Loads transaction data into input fields
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        void SetTransaction(int ID)
        {
            try
            {
                IEnumerable<Transaction> t = Data.Transactions.Where(t => t.TransactionID == ID);
                transaction = t.Count() == 1 ? t.First() : throw new Exception("ERROR: Could not find transaction");

                if (transaction.CategoryID > Data.INCOME_ID || transaction.SubcategoryID == Data.DEBIT_ID)
                    txtAmount.Text = (transaction.Amount * -1).ToString();

                // sets input values from transaction
                dpDate.SelectedDate = transaction.Date;
                cbAccounts.SelectedValue = transaction.AccountID;
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
            // load dropdown menu items
            cbAccounts.ItemsSource = Data.Accounts;
            cbTransferAccount.ItemsSource = Data.Accounts;
            cbCategories.ItemsSource = Data.Categories.Where(c => c.ParentID == null);
            cbSubcategories.ItemsSource = null;
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

                // validate fields
                if (dpDate.SelectedDate == null || cbAccounts.SelectedValue == null || txtAmount.Text == ""
                    || cbCategories.SelectedValue == null || cbSubcategories.SelectedValue == null
                    || ((int)cbCategories.SelectedValue == Data.TRANSFER_ID && cbTransferAccount.SelectedValue == null))
                    throw new Exception("Please fill out all required fields");

                // make sure user isn't transferring to same account
                if ((int)cbCategories.SelectedValue == Data.TRANSFER_ID && (int)cbAccounts.SelectedValue == (int)cbTransferAccount.SelectedValue)
                    throw new Exception("Cannot transfer to same account");

                // update transaction
                transaction.Date = (DateTime)dpDate.SelectedDate;
                transaction.AccountID = (int)cbAccounts.SelectedValue;
                transaction.AccountName = cbAccounts.Text;
                transaction.AmountString = txtAmount.Text;
                transaction.CategoryID = (int)cbCategories.SelectedValue;
                transaction.CategoryName = cbCategories.Text;
                transaction.SubcategoryID = (int)cbSubcategories.SelectedValue;
                transaction.SubcategoryName = cbSubcategories.Text;
                
                GetPayee(ref con);

                //TODO: Update running balance

                // if transfer, get transfer payee info
                int transferAccountID = (transaction.CategoryID == Data.TRANSFER_ID) ? (int)cbTransferAccount.SelectedValue : -1;

                // if expense or transfer debit, value = value * -1
                if (transaction.CategoryID > Data.INCOME_ID || transaction.SubcategoryID == Data.DEBIT_ID)
                    transaction.Amount *= -1;

                // gets account from transaction
                Account account = Data.Accounts.First(a => a.AccountID == transaction.AccountID);

                // updates current account balance in database
                string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                        "WHERE AccountID=@AccountID";

                if (Title == "Edit Transaction")
                {
                    // updates database
                    string transUpdateQuery = "UPDATE Transactions SET Date=@Date, Account_fk=@AccountID, Amount=@Amount, " +
                                              "Category_fk=@CategoryID, Subcategory_fk=@SubcategoryID, Payee_fk =@PayeeID, Balance=0, Order=0 " +
                                              "WHERE TransactionID=@TransactionID";

                    /*// if expense or transfer debit, value = value * -1
                    if (transaction.CategoryID > Data.INCOME_ID || transaction.SubcategoryID == Data.DEBIT_ID)
                        transaction.Amount *= -1;*/

                    SqlCommand transUpdateCmd = new(transUpdateQuery, con);
                    transUpdateCmd.Parameters.AddWithValue("@Date", transaction.DateString);
                    transUpdateCmd.Parameters.AddWithValue("@AccountID", transaction.AccountID);
                    transUpdateCmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    transUpdateCmd.Parameters.AddWithValue("@CategoryID", transaction.CategoryID);
                    transUpdateCmd.Parameters.AddWithValue("@SubcategoryID", transaction.SubcategoryID);
                    transUpdateCmd.Parameters.AddWithValue("@PayeeID", transaction.PayeeID);
                    transUpdateCmd.Parameters.AddWithValue("@TransactionID", transaction.TransactionID);
                    transUpdateCmd.ExecuteNonQuery();

                    // update current account balance in collection
                    account.CurrentBalance -= originalAmount;
                    account.CurrentBalance += transaction.Amount;

                    UpdateAccountBalance(accUpdateQuery, ref con, account);
                    UpdateOrder();
                    UpdateRunningBalance();

                    // if transfer, edit other side of transaction as well
                    if (transaction.CategoryID == Data.TRANSFER_ID)
                    {
                        // if debit, check next transaction for match, if not, check next + 1, if still not, throw exception
                        if (transaction.SubcategoryID == Data.DEBIT_ID)
                            UpdateTransfer(1, originalAmount, Data.CREDIT_ID, accUpdateQuery, ref con);
                        else if (transaction.SubcategoryID == Data.CREDIT_ID)
                            UpdateTransfer(-1, originalAmount, Data.DEBIT_ID, accUpdateQuery, ref con);
                    }
                }
                else if (Title == "Add Transaction")
                {
                    string transInsertQuery = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee_fk, Balance, DisplayOrder) " +
                                              "OUTPUT INSERTED.TransactionID " +
                                              "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @PayeeID, @Balance, @Order)";

                    UpdateOrder();
                    UpdateRunningBalance();
                    AddTransaction(transInsertQuery, ref con, transaction);

                    account.CurrentBalance += transaction.Amount;
                    UpdateAccountBalance(accUpdateQuery, ref con, account);

                    // if transfer, create matching credit transfer transaction
                    if (transaction.CategoryID == Data.TRANSFER_ID)
                    {
                        if (transferAccountID != -1)
                            account = Data.Accounts.Where(a => a.AccountID == transferAccountID).First();
                        else
                            throw new Exception("Error: Invalid transfer payee");

                        Transaction newTransaction = new(transaction.TransactionID + 1, transaction);
                        newTransaction.Amount = transaction.Amount * -1;
                        newTransaction.AccountID = account.AccountID;
                        newTransaction.AccountName = account.AccountName;
                        newTransaction.SubcategoryID = Data.CREDIT_ID;
                        newTransaction.Order = transaction.Order + 1;

                        AddTransaction(transInsertQuery, ref con, newTransaction);
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
        /// Gets Payee from input field, if it is new, add it to database, if existing, assign value to transaction
        /// </summary>
        /// <param name="con">SQL Connection.</param>
        void GetPayee(ref SqlConnection con)
        {
            // if existing payee, assign values, otherwise, create new payee
            var potentialPayee = Data.Payees.Where(p => p.PayeeName == txtPayee.Text);
            if (potentialPayee.Any())
            {
                Payee payee = potentialPayee.First();
                transaction.PayeeID = payee.PayeeID;
                transaction.PayeeName = payee.PayeeName;
            }
            else if (txtPayee.Text != "")
            {
                // insert new payee into database
                string addPayeeQuery = "INSERT INTO Payees (PayeeName) OUTPUT INSERTED.PayeeID VALUES (@PayeeName)";
                SqlCommand payeeInsertCmd = new(addPayeeQuery, con);
                payeeInsertCmd.Parameters.AddWithValue("@PayeeName", txtPayee.Text);
                int ID = (int)payeeInsertCmd.ExecuteScalar();

                // insert payee into collection
                Payee payee = new(ID, txtPayee.Text);
                Data.Payees.Add(payee);

                transaction.PayeeID = payee.PayeeID;
                transaction.PayeeName = payee.PayeeName;
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
        /// Updates transaction running balance starting at specified transaction
        /// </summary>
        void UpdateRunningBalance()
        {

        }

        /// <summary>
        /// Attempts to find matching transfer
        /// </summary>
        /// <param name="offset">search offset.</param>
        /// <param name="originalAmount">original transfer amount.</param>
        /// <param name="transferSubcatID">ID of debit/credit transfer subcategory.</param>
        /// <param name="accUpdateQuery">SQL query.</param>
        /// <param name="con">SQL Connection.</param>
        void UpdateTransfer(int offset, decimal originalAmount, int transferSubcatID, string accUpdateQuery, ref SqlConnection con)
        {
            Account acc = UpdateConnectedTransfer(1 * offset, originalAmount, transferSubcatID);
            if (acc != null)
                UpdateAccountBalance(accUpdateQuery, ref con, acc);
            else
            {
                acc = UpdateConnectedTransfer(2 * offset, originalAmount, transferSubcatID);
                if (acc != null)
                    UpdateAccountBalance(accUpdateQuery, ref con, acc);
                else
                    throw new Exception($"Could not find matching transfer on { transaction.Date } for ${ transaction.Amount }");
            }
        }

        /// <summary>
        /// Updates transaction Amount of matching transfer and returns account
        /// </summary>
        /// <param name="offset">index offset where matching transfer should be located.</param>
        /// <param name="originalAmount">original transfer amount.</param>
        /// <param name="transferSubcatID">SubcategoryID of matching transfer.</param>
        /// <returns>Account of matching transfer</returns>
        Account UpdateConnectedTransfer(int offset, decimal originalAmount, int transferSubcatID)
        {
            int transID = Data.Transactions.IndexOf(transaction);
            Transaction trans = Data.Transactions[transID + offset];
            if (trans.SubcategoryID == transferSubcatID && Math.Abs(originalAmount) == Math.Abs(trans.Amount))
            {
                // updates amount and gets account from trans
                trans.Amount = transaction.Amount;
                Account acc = Data.Accounts.First(a => a.AccountID == trans.AccountID);

                // update current account balance in collection
                acc.CurrentBalance -= originalAmount;
                acc.CurrentBalance += trans.Amount;

                return acc;
            }
            else
                return null;
        }

        /// <summary>
        /// Updates Order of transaction in list
        /// </summary>
        void UpdateOrder()
        {
            for (int i = Data.Transactions.Count() - 1; i >= 0; i--)
            {
                // go until transaction date is >= date in transactions list
                if (transaction.Date >= Data.Transactions[i].Date)
                {
                    // if dates are equal, create list of transactions, sort, and assign Order
                    if (Data.Transactions[i].Date == transaction.Date)
                    {
                        List<Transaction> transactionsOfDay = new();
                        transactionsOfDay.Add(transaction);
                        for (int j = i; transaction.Date == Data.Transactions[j].Date; j--)
                            transactionsOfDay.Add(Data.Transactions[j]);

                        transactionsOfDay = transactionsOfDay.
                            OrderBy(t => Math.Abs(t.Amount)).
                            ThenBy(t => t.CategoryID).
                            ThenBy(t => t.TransactionID).ToList();

                        int index = transactionsOfDay.IndexOf(transaction);
                        if (index == 0)
                            transaction.Order = transactionsOfDay[index + 1].Order;
                        else
                            transaction.Order = transactionsOfDay[index - 1].Order + 1;

                        break;
                    }
                    else
                    {
                        transaction.Order = Data.Transactions[i - 1].Order + 1;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a transaction into memory and database
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="con">reference to database connection.</param>
        /// <param name="trans">Transaction object.</param>
        void AddTransaction(string query, ref SqlConnection con, Transaction trans)
        {
            if (trans.CategoryID == Data.TRANSFER_ID)
            {
                trans.PayeeID = null;
                trans.PayeeName = "";
            }
            
            SqlCommand transInsertCmd = new(query, con);
            transInsertCmd.Parameters.AddWithValue("@Date", trans.DateString);
            transInsertCmd.Parameters.AddWithValue("@AccountID", trans.AccountID);
            transInsertCmd.Parameters.AddWithValue("@Amount", trans.Amount);
            transInsertCmd.Parameters.AddWithValue("@CategoryID", trans.CategoryID);
            transInsertCmd.Parameters.AddWithValue("@SubcategoryID", trans.SubcategoryID);
            transInsertCmd.Parameters.AddWithValue("@PayeeID", trans.PayeeID);
            transInsertCmd.Parameters.AddWithValue("@Balance", trans.Balance);
            transInsertCmd.Parameters.AddWithValue("@Order", trans.Order);
            int ID = (int)transInsertCmd.ExecuteScalar();

            // create and add newTransaction to collection
            Transaction newTransaction = new(ID, trans);
            int index = newTransaction.Order - 1;
            Data.Transactions.Insert(index, newTransaction);
            
            // update order values, starting at specified index
            string orderUpdateQuery = "UPDATE Transactions SET DisplayOrder=@Order " +
                                      "WHERE TransactionID=@TransactionID";

            for (int i = index + 1; i < Data.Transactions.Count(); i++)
            {
                if (Data.Transactions[i].Order != index)
                {
                    Data.Transactions[i].Order = index;
                    SqlCommand accUpdateCmd = new(orderUpdateQuery, con);
                    accUpdateCmd.Parameters.AddWithValue("@Order", Data.Transactions[i].Order);
                    accUpdateCmd.Parameters.AddWithValue("@TransactionID", Data.Transactions[i].TransactionID);
                    accUpdateCmd.ExecuteNonQuery();
                }

                index++;
            }
        }

        /// <summary>
        /// Updates Subcategories ComboBox based on currently selected Category
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChangedEventArgs data.</param>
        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ID = (int)cbCategories.SelectedValue;

            IEnumerable<Category> categories = (ID == Data.TRANSFER_ID)
                ? Data.Categories.Where(c => c.CategoryID == Data.DEBIT_ID)
                : Data.Categories.Where(c => c.ParentID == ID);
            
            cbSubcategories.ItemsSource = categories;

            // if only 1 subcategory, select it
            if (categories.Count() == 1)
                cbSubcategories.SelectedIndex = 0;

            if (ID == Data.TRANSFER_ID)
            {
                txtPayee.Visibility = Visibility.Collapsed;
                cbTransferAccount.Visibility = Visibility.Visible;
            }
            else
            {
                cbTransferAccount.Visibility = Visibility.Collapsed;
                txtPayee.Visibility = Visibility.Visible;
            }
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
                        var accountID = Data.Accounts.Where(a => a.AccountName == column[1]).Select(a => a.AccountID);
                        if (!accountID.Any())
                            throw new Exception(column[1] + " is not a valid account");

                        var categoryID = Data.Categories.Where(c => c.CategoryName == column[3]).Select(c => c.CategoryID);
                        if (!categoryID.Any())
                            throw new Exception(column[3] + " is not a valid category");

                        var subCategoryID = Data.Categories.Where(s => s.CategoryName == column[4] && s.ParentID == categoryID.First())
                                                           .Select(s => s.CategoryID);
                        if (!subCategoryID.Any())
                            throw new Exception(column[4] + " is not a valid subcategory");

                        var payeeID = Data.Payees.Where(p => p.PayeeName == column[5]).Select(p => p.PayeeID);

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
                        trans.PayeeName = column[5];

                        // if payee exists, add to transaction, if not, add to Payee table, then to transaction
                        if (payeeID.Any())
                            trans.PayeeID = payeeID.First();
                        else
                        {
                            string payeeInsertQuery = "INSERT INTO Payees (PayeeName) " +
                                                      "OUTPUT INSERTED.PayeeID " +
                                                      "VALUES (@PayeeName)";

                            SqlCommand payeeInsertCmd = new(payeeInsertQuery, con);

                            payeeInsertCmd.Parameters.AddWithValue("@PayeeName", trans.PayeeName);
                            int ID = (int)payeeInsertCmd.ExecuteScalar();

                            trans.PayeeID = ID;
                            Data.Payees.Add(new Payee(ID, trans.PayeeName));
                        }

                        transactions.Add(trans);
                    }

                    // order transactions
                    transactions = transactions.OrderBy(t => t.Date).
                        ThenBy(t => t.CategoryID).
                        ThenBy(t => Math.Abs(t.Amount)).ToList();

                    // after all data passes validation, insert each into collection/database
                    for (int i = 0; i < transactions.Count(); i++)
                    {

                        string transInsertQuery = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee_fk, Balance, DisplayOrder) " +
                                                  "OUTPUT INSERTED.TransactionID " +
                                                  "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @Payee, @Balance, @Order)";

                        SqlCommand transInsertCmd = new(transInsertQuery, con);
                        transInsertCmd.Parameters.AddWithValue("@Date", transactions[i].DateString);
                        transInsertCmd.Parameters.AddWithValue("@AccountID", transactions[i].AccountID);
                        transInsertCmd.Parameters.AddWithValue("@Amount", transactions[i].Amount);
                        transInsertCmd.Parameters.AddWithValue("@CategoryID", transactions[i].CategoryID);
                        transInsertCmd.Parameters.AddWithValue("@SubcategoryID", transactions[i].SubcategoryID);
                        transInsertCmd.Parameters.AddWithValue("@Payee", transactions[i].PayeeID);
                        transInsertCmd.Parameters.AddWithValue("@Balance", 0);
                        transInsertCmd.Parameters.AddWithValue("@Order", i + 1); // order starts at 1, not 0
                        int ID = (int)transInsertCmd.ExecuteScalar();

                        // create and add newTransaction to collection
                        Transaction newTransaction = new(ID, transactions[i]);
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
