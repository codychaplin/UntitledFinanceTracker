using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Data;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for SettingsAccountTypes.xaml
    /// </summary>
    public partial class SettingsAccountTypes : UserControl
    {
        public SettingsAccountTypes()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Queries database for account types and fills the DataTable with results.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChanged event data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                string query = "SELECT AccountTypeID, AccountType FROM AccountTypes";
                DataTable dt = new DataTable("AccountTypes");

                using (SqlConnection con = new(connectionString))
                    using (SqlCommand command = new(query, con))
                        using (SqlDataAdapter adapter = new(command))
                            adapter.Fill(dt);

                dgAccountTypes.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
