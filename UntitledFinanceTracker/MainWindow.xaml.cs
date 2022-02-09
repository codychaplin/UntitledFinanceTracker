using System;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;

namespace UntitledFinanceTracker
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
            InitializeComponent();

            // load database info into ObservableCollections
            Init();

            lvMainMenu.SelectedIndex = 0;
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

                // dashboard
                if (lvMainMenu.SelectedItem.Equals(lviDashboard)) // if selected item is specified listViewItem
                {
                    Control ctrlDashboard = new Dashboard(); // insantiate control
                    ContentPanel.Children.Add(ctrlDashboard); // add control to panel as child
                }

                // transactions
                if (lvMainMenu.SelectedItem.Equals(lviTransactions))
                {
                    Control ctrlTransactionView = new Transactions();
                    ContentPanel.Children.Add(ctrlTransactionView);
                }

                // calendar
                if (lvMainMenu.SelectedItem.Equals(lviCalendar))
                {
                    Control ctrlWeekView = new Calendar();
                    ContentPanel.Children.Add(ctrlWeekView);
                }

                // balance sheet
                if (lvMainMenu.SelectedItem.Equals(lviBalanceSheet))
                {
                    Control ctrlBalanceSheet = new BalanceSheet();
                    ContentPanel.Children.Add(ctrlBalanceSheet);
                }

                // statistics
                if (lvMainMenu.SelectedItem.Equals(lviStats))
                {
                    Control ctrlStats = new Statistics();
                    ContentPanel.Children.Add(ctrlStats);
                }

                // settings
                if (lvMainMenu.SelectedItem.Equals(lviSettings))
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
                Data.Transactions = new();

                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                InitializeAccountTypes(ref con);
                InitializeAccounts(ref con);
                InitializeCategories(ref con);
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
        /// Loads transactions from database into memory.
        /// </summary>
        /// <param name="con">Reference to database connection.</param>
        void InitializeTransactions(ref SqlConnection con)
        {
            string query = "SELECT TransactionID, Date, Account_fk, Accounts.AccountName, Amount, " +
                "Category_fk, cat.CategoryName, Subcategory_fk, sub.CategoryName AS SubcategoryName, Payee FROM Transactions " +
                "INNER JOIN Accounts ON Transactions.Account_fk = Accounts.AccountID " +
                "INNER JOIN Categories cat ON Transactions.Category_fk = cat.CategoryID " +
                "INNER JOIN Categories sub ON Transactions.Subcategory_fk = sub.CategoryID";
                
            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Data.Transactions.Add(new Transaction((int)reader[0], (DateTime)reader[1], (int)reader[2], reader[3].ToString(),
                     (decimal)reader[4], (int)reader[5], reader[6].ToString(), (int)reader[7], reader[8].ToString(), reader[9].ToString()));
            }

            reader.Close();
        }
    }
}
