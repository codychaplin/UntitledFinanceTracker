using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for TransactionView.xaml
    /// </summary>
    public partial class Transactions : UserControl
    {
        public Transactions()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads database information into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                // SELECT * including foreign key named counterparts
                string query = "SELECT TransactionID, Date, Account_fk, Accounts.AccountName, Type, Amount, " +
                    "Category_fk, cat.CategoryName, Subcategory_fk, sub.CategoryName AS SubcategoryName, Payee FROM Transactions " +
                    "INNER JOIN Accounts ON Transactions.Account_fk = Accounts.AccountID " +
                        "INNER JOIN Categories cat ON Transactions.Category_fk = cat.CategoryID " +
                            "INNER JOIN Categories sub ON Transactions.Subcategory_fk = sub.CategoryID";

                DataTable dt = new DataTable("Transactions");
                using (SqlConnection con = new(connectionString))
                    using (SqlCommand command = new(query, con))
                        using (SqlDataAdapter adapter = new(command))
                            adapter.Fill(dt);

                dgTransactions.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Adds a transaction to the database and dataGrid
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Edits a transaction and updates the database and dataGrid
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // gets Transaction as array from dataGridRow
                DataRowView row = (sender as Button).DataContext as DataRowView;
                object[] rowArray = row.Row.ItemArray;

                Window editTransaction = new EditTransaction();
                editTransaction.ShowDialog();
                
                /*// instantiates new Transaction using dataGridRow info
                Transaction transaction = new Transaction((int)rowArray[0], (DateTime)rowArray[1], // ID, Date
                    (int)rowArray[2], rowArray[3].ToString(), // Account ID, Account
                    (CategoryType)Enum.Parse(typeof(CategoryType), rowArray[4].ToString()), (decimal)rowArray[5], // Type, Amount
                    (int)rowArray[6], rowArray[7].ToString(), // Category ID, Category
                    (int)rowArray[8], rowArray[9].ToString(), rowArray[7].ToString()); // Subcategory ID, Subcategory, Payee

                string connectionString = Properties.Settings.Default.connectionString;
                using (SqlConnection con = new(connectionString))
                {
                    con.Open();
                    
                    string setQuery = "UPDATE Transactions SET Date = '" + transaction.Date + "'" +
                        ", Account_fk = " + transaction.Account[0] +
                        ", Type = '" + transaction.Type + "'" +
                        ", Category_fk = " + transaction.Category[0] +
                        ", Subcategory_fk = " + transaction.Subcategory[0] +
                        ", Payee = '" + transaction.Payee + "' " +
                        " WHERE TransactionID = " + transaction.TransactionID;

                    using (SqlCommand command = new(setQuery, con))
                        command.ExecuteNonQuery();

                    con.Close();
                }*/
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("InvalidOperation: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("General: " + ex.Message);
            }
        }

        /// <summary>
        /// Deletes a transaction from the database and dataGrid
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // gets transactionID from dataGridRow
                DataRowView row = (sender as Button).DataContext as DataRowView;
                int ID = (int)row.Row.ItemArray[0];

                string connectionString = Properties.Settings.Default.connectionString;
                string query = "DELETE FROM Transactions WHERE TransactionID = " + ID;

                using (SqlConnection con = new(connectionString))
                {
                    con.Open();
                    using (SqlCommand command = new(query, con))
                        command.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
