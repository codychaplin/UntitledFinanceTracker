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
        /// <param name="e">Contains SelectionChanged event data.</param>
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

        void Init()
        {
            try
            {
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

        void InitializeCategories(ref SqlConnection con)
        {
            string query = "SELECT par.*, sub.CategoryName AS ParentName FROM Categories par " +
                "LEFT JOIN Categories sub ON par.ParentID_fk = sub.CategoryID";

            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int? parentID = (reader[1] is DBNull) ? null : (int)reader[1]; // coverts DBNull to null

                Data.Categories.Add(new Category((int)reader[0], parentID, reader[5].ToString(),
                    (CategoryType)Enum.Parse(typeof(CategoryType), reader[2].ToString()), reader[3].ToString(), (bool)reader[4]));
            }

            reader.Close();
        }

        void InitializeTransactions(ref SqlConnection con)
        {
            string query = "SELECT TransactionID, Date, Account_fk, Accounts.AccountName, Type, Amount, " +
                "Category_fk, cat.CategoryName, Subcategory_fk, sub.CategoryName AS SubcategoryName, Payee FROM Transactions " +
                "INNER JOIN Accounts ON Transactions.Account_fk = Accounts.AccountID " +
                "INNER JOIN Categories cat ON Transactions.Category_fk = cat.CategoryID " +
                "INNER JOIN Categories sub ON Transactions.Subcategory_fk = sub.CategoryID";
                
            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Data.transactions.Add(new Transaction((int)reader[0], (DateTime)reader[1], (int)reader[2], reader[3].ToString(),
                    (CategoryType)Enum.Parse(typeof(CategoryType), reader[4].ToString()), (decimal)reader[5],
                    (int)reader[6], reader[7].ToString(), (int)reader[8], reader[9].ToString(), reader[10].ToString()));
            }

            reader.Close();
        }
    }
}
