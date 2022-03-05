using System;
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
        public static MainWindow main;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            Init(); // load database info into ObservableCollections
            InitializeComponent();
            lvMainMenu.SelectedIndex = 0; // default to dashboard view
            main = this;
        }

        /// <summary>
        /// Main window initialization
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            // adds accounts to accounts lists
            lvAccounts.ItemsSource = Data.Accounts.Where(a => a.Enabled == true).OrderBy(a => a.AccountTypeID);
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvAccounts.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("AccountTypeName");
            view.GroupDescriptions.Add(groupDescription);

            RefreshBalances();
        }

        /// <summary>
        /// Gets updated sum of all current account balances and refreshes lvAccounts
        /// </summary>
        public void RefreshBalances()
        {
            txtNetWorth.Text = Data.Accounts.Sum(a => a.CurrentBalance).ToString("C");
            lvAccounts.Items.Refresh();
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

                Calculations();
                
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
                Data.Payees.Add(new Payee((int)reader[0], reader[1].ToString()));
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
                "pay.PayeeName, Balance, DisplayOrder FROM Transactions " +
                "INNER JOIN Accounts ON Transactions.Account_fk = Accounts.AccountID " +
                "INNER JOIN Categories cat ON Transactions.Category_fk = cat.CategoryID " +
                "INNER JOIN Categories sub ON Transactions.Subcategory_fk = sub.CategoryID " +
                "INNER JOIN Payees pay ON Transactions.Payee_fk = pay.PayeeID";
                
            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Data.Transactions.Add(new Transaction((int)reader[0], (DateTime)reader[1], // TransactionID, date
                    (int)reader[2], reader[3].ToString(), // accountID, accountName
                    (decimal)reader[4], (int)reader[5], reader[6].ToString(), // amount, categoryID, categoryName
                    (int)reader[7], reader[8].ToString(), // subcategoryID, subcategoryName
                    (int)reader[9], reader[10].ToString(), (decimal)reader[11], (int)reader[12])); // payeeID, payeeName, balance, order
            }

            reader.Close();
        }

        /// <summary>
        /// Performs calculations needed for data visualization.
        /// </summary>
        void Calculations()
        {
            CalulateYearStartBalances();
        }

        /// <summary>
        /// Calculate starting balance for each year and add to Dictionary
        /// </summary>
        void CalulateYearStartBalances()
        {
            Data.YearStartBalances = new();
            int startingYear = Data.Transactions.Min(x => x.Date).Year; // get starting year
            decimal startingBalance = Data.Accounts.Sum(a => a.StartingBalance); // get starting balance of first year
            Data.YearStartBalances.Add(startingYear, startingBalance); // add first year to dictionary

            // for each year balance = startingBalance + incomes - expenses
            for (int i = startingYear; i < DateTime.Now.Year;)
            {
                decimal transactions = Data.Transactions.Where(t => t.Date.Year == i).Sum(t => t.Amount);
                decimal balance = startingBalance + transactions;
                startingBalance = balance;
                Data.YearStartBalances.Add(++i, balance); // ++i because this year end balance = next year start balance
            }
        }
    }
}
