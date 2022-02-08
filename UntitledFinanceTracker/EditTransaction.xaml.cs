﻿using System;
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

            transaction = new();
            btnEdit.Content = "Add";
            btnCSV.Visibility = Visibility.Visible;
        }

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

                if (Title == "Edit Transaction")
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
                    Transaction trans = Data.Transactions.First(t => t.TransactionID == transaction.TransactionID);
                    trans = transaction;

                    // updates database
                    string query = "UPDATE Transactions SET Date = '" + transaction.Date.ToString("yyyy-MM-dd") + "'" +
                        ", Account_fk = " + transaction.AccountID +
                        ", Amount = '" + transaction.Amount + "'" +
                        ", Category_fk = " + transaction.CategoryID +
                        ", Subcategory_fk = " + transaction.SubcategoryID +
                        ", Payee = '" + transaction.Payee + "' " +
                        " WHERE TransactionID = " + transaction.TransactionID;

                    SqlCommand command = new(query, con);
                    command.ExecuteNonQuery();
                }
                else if (Title == "Add Transaction")
                {
                    transaction.Date = (DateTime)dpDate.SelectedDate;
                    transaction.AccountID = (int)cbAccounts.SelectedValue;
                    transaction.AccountName = cbAccounts.Text;
                    transaction.Amount = Convert.ToDecimal(txtAmount.Text);
                    transaction.CategoryID = (int)cbCategories.SelectedValue;
                    transaction.CategoryName = cbCategories.Text;
                    transaction.SubcategoryID = (int)cbSubcategories.SelectedValue;
                    transaction.SubcategoryName = cbSubcategories.Text;
                    transaction.Payee = txtPayee.Text;
                    
                    string query = "INSERT INTO Transactions (Date, Account_fk, Amount, Category_fk, Subcategory_fk, Payee)" +
                        " VALUES ('" + transaction.Date.ToString("yyyy-MM-dd") + "'" +
                        ", " + transaction.AccountID +
                        ", " + transaction.Amount +
                        ", " + transaction.CategoryID +
                        ", " + transaction.SubcategoryID +
                        ", '" + transaction.Payee + "')";

                    SqlCommand command = new(query, con);
                    command.ExecuteNonQuery();

                    Data.Transactions.Add(transaction);
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

        private void btnCSV_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
