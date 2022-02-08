using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for SettingsCategories.xaml
    /// </summary>
    public partial class SettingsCategories : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SettingsCategories()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads categories into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (Data.Categories.Count > 0)
                dgCategories.ItemsSource = Data.Categories;
        }

        /// <summary>
        /// Adds a category to the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Window addCategory = new EditCategories();
            addCategory.Title = "Add Category";
            addCategory.ShowDialog();

            dgCategories.Items.Refresh();
        }

        /// <summary>
        /// Edits a category and updates the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Category row = (Category)(sender as Button).DataContext;
            int ID = row.CategoryID;

            Window editCategory = new EditCategories(ID);
            editCategory.ShowDialog();

            dgCategories.Items.Refresh();
        }

        /// <summary>
        /// Deletes a category from the dataGrid and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // prompts user to confirm deletion
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this category?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // gets category ID
                    Category row = (Category)(sender as Button).DataContext;
                    int ID = row.CategoryID;

                    // deletes category from collection
                    Category cat = Data.Categories.First(c => c.CategoryID == ID);
                    Data.Categories.Remove(cat);

                    // deletes category from database
                    string connectionString = Properties.Settings.Default.connectionString;
                    string query = "DELETE FROM Categories WHERE CategoryID = " + ID;

                    SqlConnection con = new(connectionString);
                    con.Open();
                    SqlCommand command = new(query, con);
                    command.ExecuteNonQuery();
                    con.Close();

                    dgCategories.Items.Refresh();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
