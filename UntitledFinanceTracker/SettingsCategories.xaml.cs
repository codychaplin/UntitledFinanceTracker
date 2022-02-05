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
    /// Interaction logic for SettingsCategories.xaml
    /// </summary>
    public partial class SettingsCategories : UserControl
    {
        public SettingsCategories()
        {
            InitializeComponent();
        }
        
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                // SELECTS parent ID (AS name using LEFT JOIN), type, name, enabled WHERE type != Transfer
                string query = "SELECT par.CategoryID, sub.CategoryName AS Parent, par.CategoryType, par.CategoryName, par.Enabled FROM Categories par " +
                    "LEFT JOIN Categories sub ON par.ParentID_fk = sub.CategoryID WHERE par.CategoryType <> 'Transfer'";

                DataTable dt = new DataTable("Categories");

                using (SqlConnection con = new(connectionString))
                using (SqlCommand command = new(query, con))
                using (SqlDataAdapter adapter = new(command))
                    adapter.Fill(dt);

                dgCategories.ItemsSource = dt.DefaultView;
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
