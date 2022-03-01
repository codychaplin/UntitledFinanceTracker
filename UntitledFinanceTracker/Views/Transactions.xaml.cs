﻿using System;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Collections.Generic;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class Transactions : UserControl
    {
        List<Account> accounts { get; set; }
        List<Category> categories { get; set; }
        List<Category> subcategories { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Transactions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads transactions into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            // enables all accounts
            accounts = Data.Accounts.ToList();
            foreach (var acc in accounts)
                acc.Enabled = true;
            cbAccounts.ItemsSource = accounts;

            // enables all categories
            categories = Data.Categories.Where(c => c.ParentID == null).ToList();
            foreach (var cat in categories)
                cat.Enabled = true;
            cbCategories.ItemsSource = categories;

            // enables all subcategories
            subcategories = Data.Categories.Where(c => c.ParentID != null).ToList();
            foreach (var subcat in subcategories)
                subcat.Enabled = true;
            cbSubcategories.ItemsSource = subcategories;

            // filters transactions to current year
            if (Data.Transactions.Count > 0)
            {
                int startingDate = Data.Transactions.Min(x => x.Date).Year;
                int endDate = DateTime.Now.Year;
                List<int> dates = new();
                for (int i = startingDate; i <= endDate; i++)
                    dates.Add(i);
                cbYears.ItemsSource = dates;
                cbYears.SelectedValue = dates.Last();
            }
        }

        void Filter()
        {
            dgTransactions.ItemsSource = Data.Transactions.
                    Where(t => t.Date.Year == (int)cbYears.SelectedValue).
                    Where(t => accounts.Where(a => a.Enabled == true).Select(a => a.AccountID).ToList().Contains(t.AccountID)).
                    Where(t => categories.Where(c => c.Enabled == true).Select(c => c.CategoryID).ToList().Contains(t.CategoryID)).
                    Where(t => subcategories.Where(s => s.Enabled == true).Select(s => s.CategoryID).ToList().Contains(t.SubcategoryID)).
                    OrderBy(t => t.Date).
                    ThenBy(t => t.Amount);

            ((MainWindow)Application.Current.MainWindow).RefreshBalances();
        }

        private void cbYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void ChkAccount_Click(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        private void ChkCategory_Click(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        private void ChkSubcategory_Click(object sender, RoutedEventArgs e)
        {
            Filter();
        }

        /// <summary>
        /// Adds a transaction to the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Window addTransaction = new EditTransaction();
            addTransaction.Title = "Add Transaction";
            addTransaction.ShowDialog();

            Filter();
        }

        /// <summary>
        /// Edits a transaction and updates the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Transaction row = (Transaction)(sender as Button).DataContext;
            int ID = row.TransactionID;

            Window editTransaction = new EditTransaction(ID);
            editTransaction.ShowDialog();

            Filter();
        }

        /// <summary>
        /// Deletes a transaction from the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // prompts user to confirm deletion
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this transaction?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Transaction transaction = (Transaction)(sender as Button).DataContext; // gets transaction from dataGrid

                    // gets reference to account and updates current balance
                    Account acc = Data.Accounts.First(a => a.AccountID == transaction.AccountID);
                    Account payAcc = null;

                    if (transaction.CategoryID == Data.TRANSFER_ID)
                    {
                        payAcc = Data.Accounts.First(a => a.AccountID ==
                                 Data.Payees.Where(p => p.AccountID == transaction.PayeeAccountID)
                                 .Select(p => p.AccountID).First());

                        acc.CurrentBalance += transaction.Amount;
                        payAcc.CurrentBalance -= transaction.Amount;
                    }
                    else
                    {
                        acc.CurrentBalance = transaction.CategoryID == Data.INCOME_ID
                            ? acc.CurrentBalance - transaction.Amount : acc.CurrentBalance + transaction.Amount;
                    }
                    
                    Data.Transactions.Remove(transaction); // deletes transaction from collection

                    // deletes transaction from database
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    con.Open();

                    string transDeleteQuery = "DELETE FROM Transactions WHERE TransactionID = @ID";
                    SqlCommand transDeleteCmd = new(transDeleteQuery, con);
                    transDeleteCmd.Parameters.AddWithValue("@ID", transaction.TransactionID);
                    transDeleteCmd.ExecuteNonQuery();

                    // updates current account balance in database
                    string accUpdateQuery = "UPDATE Accounts SET CurrentBalance=@CurrentBalance " +
                                            "WHERE AccountID=@AccountID";
                    UpdateAccountBalance(accUpdateQuery, ref con, acc);

                    if (payAcc != null) // if transfer
                        UpdateAccountBalance(accUpdateQuery, ref con, payAcc);

                    con.Close();

                    Filter();
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
    }
}
