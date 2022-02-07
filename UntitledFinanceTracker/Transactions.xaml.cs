using System;
using System.Data;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;

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
        /// Loads transactions into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (Data.Transactions.Count > 0)
                dgTransactions.ItemsSource = Data.Transactions;
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
                Transaction row = (Transaction)(sender as Button).DataContext;
                int ID = row.TransactionID;

                Window editTransaction = new EditTransaction(ID);
                editTransaction.ShowDialog();
                dgTransactions.Items.Refresh();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
