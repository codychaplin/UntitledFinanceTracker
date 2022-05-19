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

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            Init(); // load database info into ObservableCollections
            InitializeComponent();
            lvMainMenu.SelectedIndex = 0; // default to dashboard view
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

            // set default dates
            Data.FromDate = new DateTime(DateTime.Now.Year, 1, 1);
            Data.ToDate = DateTime.Now;
            dtFrom.SelectedDate = Data.FromDate;
            dtTo.SelectedDate = Data.ToDate;
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
                "pay.PayeeName, TransferID, Balance, DisplayOrder FROM Transactions " +
                "INNER JOIN Accounts ON Transactions.Account_fk = Accounts.AccountID " +
                "INNER JOIN Categories cat ON Transactions.Category_fk = cat.CategoryID " +
                "INNER JOIN Categories sub ON Transactions.Subcategory_fk = sub.CategoryID " +
                "LEFT JOIN Payees pay ON Transactions.Payee_fk = pay.PayeeID";
                
            SqlCommand command = new(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                // converts DBNull to null
                int? payeeID = (reader[9] is DBNull) ? null : (int)reader[9];
                int? transferID = (reader[11] is DBNull) ? null : (int)reader[11];

                Data.Transactions.Add(new Transaction((int)reader[0], (DateTime)reader[1], // TransactionID, date
                    (int)reader[2], reader[3].ToString(), // accountID, accountName
                    (decimal)reader[4], (int)reader[5], reader[6].ToString(), // amount, categoryID, categoryName
                    (int)reader[7], reader[8].ToString(), // subcategoryID, subcategoryName
                    payeeID, reader[10].ToString(), transferID, // payeeID, payeeName, transferID
                    (decimal)reader[12], (int)reader[13])); // balance, order
            }

            reader.Close();
        }

        private void dtFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeDates();
        }

        private void dtTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeDates();
        }

        void ChangeDates()
        {
            try
            {
                // validation
                if (dtFrom.SelectedDate == null || dtTo.SelectedDate == null)
                    return;
                if (dtFrom.SelectedDate >= dtTo.SelectedDate)
                    throw new Exception("Error: \"From\" date must be before \"To\" date.");

                // cache dates
                Data.FromDate = dtFrom.SelectedDate.Value;
                Data.ToDate = dtTo.SelectedDate.Value;

                if (Dashboard.dashboard != null)
                    Dashboard.dashboard.UpdateCharts(); // call UpdateCharts through singleton
            }
            catch (Exception ex)
            {
                // reset dates and display error
                dtFrom.SelectedDate = Data.FromDate;
                dtTo.SelectedDate = Data.ToDate;
                MessageBox.Show(ex.Message);
            }
        }
    }
}
