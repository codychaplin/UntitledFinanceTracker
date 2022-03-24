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

        int? originalAccountTransferID { get; set; }

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

                // if expense or transfer debit, amount *= -1 for readability
                if (transaction.CategoryID > Data.INCOME_ID || transaction.SubcategoryID == Data.DEBIT_ID)
                    txtAmount.Text = (transaction.Amount * -1).ToString();
                else
                    txtAmount.Text = transaction.AmountString;

                // sets input values from transaction
                dpDate.SelectedDate = transaction.Date;
                cbAccounts.SelectedValue = transaction.AccountID;
                cbCategories.SelectedValue = transaction.CategoryID;
                cbSubcategories.SelectedValue = transaction.SubcategoryID;
                txtPayee.Text = transaction.PayeeName;

                // if transfer, select account for cbTransferAccount and save current transferID
                if (transaction.CategoryID == Data.TRANSFER_ID)
                {
                    Transaction trans = Data.Transactions.First(t => t.TransactionID == transaction.TransferID);
                    originalAccountTransferID = trans.AccountID;
                    cbTransferAccount.SelectedValue = originalAccountTransferID;
                }
                else
                    originalAccountTransferID = -1;
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
            cbAccounts.ItemsSource = Data.Accounts.Where(a => a.Enabled);
            cbTransferAccount.ItemsSource = Data.Accounts.Where(a => a.Enabled);
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

                // saves original transaction amount and accountID
                decimal originalAmount = transaction.Amount;
                int originalAccountID = transaction.AccountID;

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

                // set default values for order and balance
                transaction.Order = -1;
                transaction.Balance = -1;

                GetPayee(ref con);

                // if transfer, get transfer payee info
                int transferAccountID = (transaction.CategoryID == Data.TRANSFER_ID) ? (int)cbTransferAccount.SelectedValue : -1;

                // if expense or transfer debit, value = value * -1
                if (transaction.CategoryID > Data.INCOME_ID || transaction.SubcategoryID == Data.DEBIT_ID)
                    transaction.Amount *= -1;

                // gets account from transaction
                Account account = Data.Accounts.First(a => a.AccountID == transaction.AccountID);

                // updates current account balance in database
                string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance WHERE AccountID=@AccountID";

                if (Title == "Edit Transaction")
                {
                    // update transaction query
                    string transUpdateQuery = "UPDATE Transactions SET Date=@Date, Account_fk=@AccountID, Amount=@Amount, " +
                                              "Category_fk=@CategoryID, Subcategory_fk=@SubcategoryID, Payee_fk=@PayeeID, " +
                                              "Balance=@Balance, DisplayOrder=@Order WHERE TransactionID=@ID";

                    SqlCommand transUpdateCmd = new(transUpdateQuery, con);
                    transUpdateCmd.Parameters.AddWithValue("@Date", transaction.DateString);
                    transUpdateCmd.Parameters.AddWithValue("@AccountID", transaction.AccountID);
                    transUpdateCmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                    transUpdateCmd.Parameters.AddWithValue("@CategoryID", transaction.CategoryID);
                    transUpdateCmd.Parameters.AddWithValue("@SubcategoryID", transaction.SubcategoryID);
                    transUpdateCmd.Parameters.AddWithValue("@PayeeID", (transaction.PayeeID == null) ? DBNull.Value : transaction.PayeeID);
                    transUpdateCmd.Parameters.AddWithValue("@TransferID", (transaction.TransferID == null) ? DBNull.Value : transaction.TransferID);
                    transUpdateCmd.Parameters.AddWithValue("@Balance", transaction.Balance);
                    transUpdateCmd.Parameters.AddWithValue("@Order", transaction.Order);
                    transUpdateCmd.Parameters.AddWithValue("@ID", transaction.TransactionID);
                    transUpdateCmd.ExecuteNonQuery();

                    // if account is same, only update current account balance
                    if (account.AccountID == originalAccountID)
                    {
                        account.CurrentBalance -= originalAmount;
                        account.CurrentBalance += transaction.Amount;
                        UpdateAccountBalance(accUpdateQuery, ref con, account);
                    }
                    else // if account has changed, update both accounts' balances
                    {
                        // get original account from transaction and update balance
                        Account originalAccount = Data.Accounts.First(a => a.AccountID == originalAccountID);
                        originalAccount.CurrentBalance -= originalAmount;
                        UpdateAccountBalance(accUpdateQuery, ref con, originalAccount);

                        // update new account balance
                        account.CurrentBalance += transaction.Amount;
                        UpdateAccountBalance(accUpdateQuery, ref con, account);
                    }

                    // if transfer, edit other side of transaction as well
                    if (transaction.CategoryID == Data.TRANSFER_ID)
                    {
                        // gets other transaction and updates date, accountID, and amount
                        Transaction otherTransaction = Data.Transactions.First(t => t.TransactionID == transaction.TransferID);
                        otherTransaction.Date = transaction.Date;
                        otherTransaction.AccountID = transferAccountID;
                        otherTransaction.Amount = transaction.Amount * -1;

                        // get other account and set account name in other transaction
                        Account otherAccount = Data.Accounts.First(a => a.AccountID == otherTransaction.AccountID);
                        otherTransaction.AccountName = otherAccount.AccountName;

                        // if payee account is same, update account balance
                        if (otherTransaction.AccountID == originalAccountTransferID)
                        {
                            otherAccount.CurrentBalance -= originalAmount * -1;
                            otherAccount.CurrentBalance += otherTransaction.Amount;
                            UpdateAccountBalance(accUpdateQuery, ref con, otherAccount);
                        }
                        else // if changed, update original and new account balances
                        {
                            Account originalAccount = Data.Accounts.First(a => a.AccountID == originalAccountTransferID);
                            originalAccount.CurrentBalance -= originalAmount * -1;
                            UpdateAccountBalance(accUpdateQuery, ref con, originalAccount);

                            Account newAccount = Data.Accounts.First(a => a.AccountID == transferAccountID);
                            newAccount.CurrentBalance += otherTransaction.Amount;
                            UpdateAccountBalance(accUpdateQuery, ref con, newAccount);
                        }

                        // update other transaction's date, accountID, and amount
                        string otherTransUpdateQuery = "UPDATE Transactions SET Date=@Date, Account_fk=@AccountID, Amount=@Amount WHERE TransactionID=@ID";
                        SqlCommand otherTransUpdateCmd = new(otherTransUpdateQuery, con);
                        otherTransUpdateCmd.Parameters.AddWithValue("@Date", otherTransaction.Date);
                        otherTransUpdateCmd.Parameters.AddWithValue("@AccountID", otherTransaction.AccountID);
                        otherTransUpdateCmd.Parameters.AddWithValue("@Amount", otherTransaction.Amount);
                        otherTransUpdateCmd.Parameters.AddWithValue("@ID", otherTransaction.TransactionID);
                        otherTransUpdateCmd.ExecuteNonQuery();
                    }
                    
                    Data.UpdateOrderAndRunningBalance(transaction);
                }
                else if (Title == "Add Transaction")
                {
                    string transInsertQuery = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee_fk, TransferID, Balance, DisplayOrder) " +
                                              "OUTPUT INSERTED.TransactionID " +
                                              "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @PayeeID, null, -1, -1)";
                    
                    Transaction trans = AddTransaction(transInsertQuery, ref con, transaction);

                    account.CurrentBalance += transaction.Amount;
                    UpdateAccountBalance(accUpdateQuery, ref con, account);

                    // if transfer, create matching credit transfer transaction
                    if (transaction.CategoryID == Data.TRANSFER_ID)
                    {
                        Account otherAccount;
                        if (transferAccountID != -1)
                            otherAccount = Data.Accounts.First(a => a.AccountID == transferAccountID);
                        else
                            throw new Exception("Error: Invalid transfer payee");

                        // set attributes of new transaction
                        Transaction newTransaction = new(trans);
                        newTransaction.Amount = trans.Amount * -1;
                        newTransaction.AccountID = otherAccount.AccountID;
                        newTransaction.AccountName = otherAccount.AccountName;
                        newTransaction.SubcategoryID = Data.CREDIT_ID;
                        newTransaction.SubcategoryName = "Credit";
                        newTransaction.TransferID = trans.TransactionID;

                        // add transaction
                        newTransaction = AddTransaction(transInsertQuery, ref con, newTransaction);
                        trans.TransferID = newTransaction.TransactionID;

                        // update transferIDs of both transactions
                        string TransIDUpdateQuery = "UPDATE Transactions SET TransferID=@TransferID WHERE TransactionID=@ID";
                        UpdateTransferID(TransIDUpdateQuery, ref con, newTransaction.TransactionID, trans.TransactionID);
                        UpdateTransferID(TransIDUpdateQuery, ref con, trans.TransactionID, newTransaction.TransactionID);

                        // update account balance
                        otherAccount.CurrentBalance += newTransaction.Amount;
                        UpdateAccountBalance(accUpdateQuery, ref con, otherAccount);
                    }

                    Data.UpdateOrderAndRunningBalance(trans);
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
            else if (string.IsNullOrEmpty(txtPayee.Text.Trim()))
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
            else
            {
                transaction.PayeeID = null;
                transaction.PayeeName = "";
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
        /// Inserts a transaction into memory and database
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="con">reference to database connection.</param>
        /// <param name="trans">Transaction object.</param>
        Transaction AddTransaction(string query, ref SqlConnection con, Transaction trans)
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
            transInsertCmd.Parameters.AddWithValue("@PayeeID", (trans.PayeeID == null) ? DBNull.Value : trans.PayeeID);
            int ID = (int)transInsertCmd.ExecuteScalar();

            // create and add newTransaction to collection
            Transaction newTransaction = new(ID, trans);
            Data.Transactions.Add(newTransaction);

            return newTransaction;
        }

        /// <summary>
        /// Inserts a transaction into memory and database
        /// </summary>
        /// <param name="query">SQL query.</param>
        /// <param name="con">reference to database connection.</param>
        /// <param name="transferID">transfer ID.</param>
        /// <param name="ID">Transaction ID.</param>
        void UpdateTransferID(string query, ref SqlConnection con, int transferID, int ID)
        {
            SqlCommand TransIDUpdateCmd = new(query, con);
            TransIDUpdateCmd.Parameters.AddWithValue("@TransferID", transferID);
            TransIDUpdateCmd.Parameters.AddWithValue("@ID", ID);
            TransIDUpdateCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates Subcategories ComboBox based on currently selected Category
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChangedEventArgs data.</param>
        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ID = (int)cbCategories.SelectedValue;

            IEnumerable<Category> categories = Data.Categories.Where(c => c.ParentID == ID);
            
            cbSubcategories.ItemsSource = categories;

            // if only 1 subcategory, select it
            if (categories.Count() == 1)
                cbSubcategories.SelectedIndex = 0;

            // if category is transfer, select 'debit' and show cbTransferAccount, else, show txtPayee instead
            if (ID == Data.TRANSFER_ID)
            {
                cbSubcategories.SelectedIndex = 0;
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

                        var payee = Data.Payees.Where(p => p.PayeeName == column[5]);

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
                        trans.TransferID = null;

                        // if payee exists, add to transaction, if not, add to Payee table, then to transaction
                        if (payee.Any())
                        {
                            trans.PayeeID = payee.First().PayeeID;
                            trans.PayeeName = payee.First().PayeeName;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(column[5]))
                            {
                                trans.PayeeName = column[5];
                                string payeeInsertQuery = "INSERT INTO Payees (PayeeName) " +
                                                          "OUTPUT INSERTED.PayeeID " +
                                                          "VALUES (@PayeeName)";

                                SqlCommand payeeInsertCmd = new(payeeInsertQuery, con);
                                payeeInsertCmd.Parameters.AddWithValue("@PayeeName", trans.PayeeName);
                                int ID = (int)payeeInsertCmd.ExecuteScalar();

                                trans.PayeeID = ID;
                                Data.Payees.Add(new Payee(ID, trans.PayeeName));
                            }
                        }

                        transactions.Add(trans);
                    }

                    // after all data passes validation, insert each into collection/database
                    for (int i = 0; i < transactions.Count(); i++)
                    {
                        string transInsertQuery = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee_fk, TransferID, Balance, DisplayOrder) " +
                                                  "OUTPUT INSERTED.TransactionID " +
                                                  "VALUES (@Date, @AccountID, @Amount, @CategoryID, @SubcategoryID, @Payee, null, -1, -1)";

                        SqlCommand transInsertCmd = new(transInsertQuery, con);
                        transInsertCmd.Parameters.AddWithValue("@Date", transactions[i].DateString);
                        transInsertCmd.Parameters.AddWithValue("@AccountID", transactions[i].AccountID);
                        transInsertCmd.Parameters.AddWithValue("@Amount", transactions[i].Amount);
                        transInsertCmd.Parameters.AddWithValue("@CategoryID", transactions[i].CategoryID);
                        transInsertCmd.Parameters.AddWithValue("@SubcategoryID", transactions[i].SubcategoryID);
                        transInsertCmd.Parameters.AddWithValue("@Payee", (transactions[i].PayeeID) == null ? DBNull.Value : transactions[i].PayeeID);
                        int ID = (int)transInsertCmd.ExecuteScalar();
                        
                        Transaction newTransaction = new(ID, transactions[i]); // create newTransaction 

                        // if transaction is a transfer credit, set TransferIDs of debit and credit
                        if (newTransaction.SubcategoryID == Data.CREDIT_ID)
                        {
                            Transaction previousTrans = transactions[i - 1];
                            if (previousTrans.SubcategoryID != Data.DEBIT_ID)
                                MessageBox.Show("Transaction: " + previousTrans.TransactionID + " is not placed correctly");

                            newTransaction.TransferID = previousTrans.TransactionID;
                            previousTrans.TransferID = newTransaction.TransactionID;

                            string transUpdateQuery = "UPDATE Transactions SET TransferID=@TransferID WHERE TransactionID=@ID";

                            SqlCommand creditTransUpdateCmd = new(transUpdateQuery, con);
                            creditTransUpdateCmd.Parameters.AddWithValue("@TransferID", newTransaction.TransferID);
                            creditTransUpdateCmd.Parameters.AddWithValue("@ID", newTransaction.TransactionID);
                            creditTransUpdateCmd.ExecuteNonQuery();

                            SqlCommand debitTransUpdateCmd = new(transUpdateQuery, con);
                            debitTransUpdateCmd.Parameters.AddWithValue("@TransferID", previousTrans.TransferID);
                            debitTransUpdateCmd.Parameters.AddWithValue("@ID", previousTrans.TransactionID);
                            debitTransUpdateCmd.ExecuteNonQuery();
                        }

                        transactions[i] = newTransaction; // update transaction in collection
                    }

                    Data.Transactions.AddRange(transactions); // add new transactions to Transactions
                    Data.UpdateOrderAndRunningBalance(Data.Transactions[0]);

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
