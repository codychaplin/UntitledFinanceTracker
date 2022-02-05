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
using System.Windows.Shapes;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for EditTransaction.xaml
    /// </summary>
    public partial class EditTransaction : Window
    {
        public EditTransaction()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                string accountsQuery = "SELECT AccountID, AccountName FROM Accounts";
                string categoriesQuery = "SELECT CategoryID, CategoryName FROM Categories " +
                    "WHERE ParentID_fk IS NULL AND CategoryType <> 'Transfer'";
                DataSet dsA = new DataSet("Accounts");
                DataSet dsC = new DataSet("Categories");

                using (SqlConnection con = new(connectionString))
                {
                    using (SqlCommand cmdAccounts = new(accountsQuery, con))
                    using (SqlDataAdapter adapter = new(cmdAccounts))
                        adapter.Fill(dsA, "Accounts");

                    using (SqlCommand cmdCategories = new(categoriesQuery, con))
                    using (SqlDataAdapter adapter = new(cmdCategories))
                        adapter.Fill(dsC, "Categories");
                }

                cbAccounts.ItemsSource = dsA.Tables["Accounts"].DefaultView;
                cbCategories.ItemsSource = dsC.Tables["Categories"].DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void cbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int ID = (int)cbCategories.SelectedValue;

                string connectionString = Properties.Settings.Default.connectionString;
                string query = "SELECT CategoryID, CategoryName FROM Categories " +
                    "WHERE ParentID_fk = " + ID;
                DataSet ds = new DataSet("Subcategories");

                using (SqlConnection con = new(connectionString))
                using (SqlCommand cmdAccounts = new(query, con))
                using (SqlDataAdapter adapter = new(cmdAccounts))
                    adapter.Fill(ds, "Subcategories");

                cbSubcategories.ItemsSource = ds.Tables["Subcategories"].DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
