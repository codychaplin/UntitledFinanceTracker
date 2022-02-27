﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Data.SqlClient;
using System.Windows.Controls;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            // load database info into ObservableCollections
            Init();

            InitializeComponent();

            lvMainMenu.SelectedIndex = 0;
        }

        /// <summary>
        /// Main window initialization
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            // gets sum of all current account balances
            txtNetWorth.Text = Data.Accounts.Sum(a => a.CurrentBalance).ToString("C");

            // adds accounts to accounts lists
            lvAccounts.ItemsSource = Data.Accounts.Where(a => a.Enabled == true).OrderBy(a => a.AccountTypeID);
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvAccounts.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("AccountTypeName");
            view.GroupDescriptions.Add(groupDescription);
        }

        /// <summary>
        /// When a ListViewItem is selected, this event is triggered which opens the page corresponding to the selection.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChangedEventArgs data.</param>
        private void MainMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvMainMenu != null)
            {
                if (ContentPanel != null)
                    ContentPanel.Children.Clear();
                
                if (lvMainMenu.SelectedItem.Equals(lviDashboard)) // dashboard
                {
                    Control ctrlDashboard = new Dashboard(); // insantiate control
                    ContentPanel.Children.Add(ctrlDashboard); // add control to panel as child
                }
                else if (lvMainMenu.SelectedItem.Equals(lviTransactions)) // transactions
                {
                    Control ctrlTransactionView = new Transactions();
                    ContentPanel.Children.Add(ctrlTransactionView);
                }
                else if (lvMainMenu.SelectedItem.Equals(lviCalendar)) // calendar
                {
                    Control ctrlWeekView = new Calendar();
                    ContentPanel.Children.Add(ctrlWeekView);
                }
                else if (lvMainMenu.SelectedItem.Equals(lviBalanceSheet)) // balance sheet
                {
                    Control ctrlBalanceSheet = new BalanceSheet();
                    ContentPanel.Children.Add(ctrlBalanceSheet);
                }
                else if (lvMainMenu.SelectedItem.Equals(lviStats)) // statistics
                {
                    Control ctrlStats = new Statistics();
                    ContentPanel.Children.Add(ctrlStats);
                }
                else if (lvMainMenu.SelectedItem.Equals(lviSettings)) // settings
                {
                    Control ctrlSettings = new Settings();
                    ContentPanel.Children.Add(ctrlSettings);
                }
            }
        }

        /// <summary>
        /// loads database info into ObservableCollections.
        /// </summary>
        void Init()
        {
            try
            {
                Data.AccountTypes = new();
                Data.Accounts = new();
                Data.Categories = new();
                Data.Payees = new();
                Data.Transactions = new();

                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                InitializeAccountTypes(ref con);
                InitializeAccounts(ref con);
                InitializeCategories(ref con);
                InitializePayees(ref con);
                InitializeTransactions(ref con);

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Loads account types from database into memory.
        /// </summary>
        /// <param name="con">Reference to database connection.</param>
        void InitializeAccountTypes(ref SqlConnection con)
        {
            string query = "SELECT * FROM AccountTypes";

            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Data.AccountTypes.Add(new AccountType((int)reader[0], reader[1].ToString()));
            }

            reader.Close();
        }

        /// <summary>
        /// Loads accounts from database into memory.
        /// </summary>
        /// <param name="con">Reference to database connection.</param>
        void InitializeAccounts(ref SqlConnection con)
        {
            string query = "SELECT Accounts.*, AccountTypes.AccountType FROM Accounts " +
                "INNER JOIN AccountTypes ON Accounts.AccountType_fk = AccountTypes.AccountTypeID";

            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Data.Accounts.Add(new Account((int)reader[0], reader[1].ToString(), (int)reader[2], reader[6].ToString(),
                    (decimal)reader[3], (decimal)reader[4], (bool)reader[5]));
            }

            reader.Close();
        }

        /// <summary>
        /// Loads categories from database into memory.
        /// </summary>
        /// <param name="con">Reference to database connection.</param>
        void InitializeCategories(ref SqlConnection con)
        {
            string query = "SELECT par.*, sub.CategoryName AS ParentName FROM Categories par " +
                "LEFT JOIN Categories sub ON par.ParentID_fk = sub.CategoryID";

            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // converts DBNull to null
                int? parentID = (reader[1] is DBNull) ? null : (int)reader[1];
                //string parentName = (reader[4] is DBNull) ? "" : reader[4].ToString();

                Data.Categories.Add(new Category((int)reader[0], parentID, reader[4].ToString(),
                    reader[2].ToString(), (bool)reader[3]));
            }

            reader.Close();
        }

        /// <summary>
        /// Loads payees from database into memory.
        /// </summary>
        /// <param name="con">Reference to database connection.</param>
        void InitializePayees(ref SqlConnection con)
        {
            string query = "SELECT * FROM Payees";

            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // converts DBNull to null
                int? AccountID = (reader[1] is DBNull) ? null : (int)reader[1];

                Data.Payees.Add(new Payee((int)reader[0], AccountID, reader[2].ToString()));
            }

            reader.Close();
        }

        /// <summary>
        /// Loads transactions from database into memory.
        /// </summary>
        /// <param name="con">Reference to database connection.</param>
        void InitializeTransactions(ref SqlConnection con)
        {
            string query = "SELECT TransactionID, Date, Account_fk, Accounts.AccountName, Amount, " +
                "Category_fk, cat.CategoryName, Subcategory_fk, sub.CategoryName AS SubcategoryName, Payee_fk, " +
                "pay.AccountID_fk AS PayeeAccountID, pay.PayeeName FROM Transactions " +
                "INNER JOIN Accounts ON Transactions.Account_fk = Accounts.AccountID " +
                "INNER JOIN Categories cat ON Transactions.Category_fk = cat.CategoryID " +
                "INNER JOIN Categories sub ON Transactions.Subcategory_fk = sub.CategoryID " +
                "INNER JOIN Payees pay ON Transactions.Payee_fk = pay.PayeeID";
                
            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // converts DBNull to null
                int? PayeeAccountID = (reader[10] is DBNull) ? null : (int)reader[10];

                Data.Transactions.Add(new Transaction((int)reader[0], (DateTime)reader[1], (int)reader[2],
                     reader[3].ToString(), (decimal)reader[4], (int)reader[5], reader[6].ToString(),
                     (int)reader[7], reader[8].ToString(), (int)reader[9], PayeeAccountID, reader[11].ToString()));
            }

            reader.Close();
        }
    }
}