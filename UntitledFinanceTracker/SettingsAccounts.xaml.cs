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
    /// Interaction logic for SettingsAccounts.xaml
    /// </summary>
    public partial class SettingsAccounts : UserControl
    {
        public SettingsAccounts()
        {
            InitializeComponent();
        }
        
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                // SELECTS name, type ID (AS type using INNER JOIN), starting balance, enabled
                string query = "SELECT AccountID, AccountName, AccountTypes.AccountType, StartingBalance, Enabled FROM Accounts " +
                    "INNER JOIN AccountTypes ON Accounts.AccountType_fk = AccountTypes.AccountTypeID";
                DataTable dt = new DataTable("AccountTypes");

                using (SqlConnection con = new(connectionString))
                using (SqlCommand command = new(query, con))
                using (SqlDataAdapter adapter = new(command))
                    adapter.Fill(dt);

                dgAccounts.ItemsSource = dt.DefaultView;
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
