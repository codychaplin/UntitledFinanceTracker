using System;
using System.Linq;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using UntitledFinanceTracker.Models;

namespace UntitledFinanceTracker.Views
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
                GetSource();
        }

        /// <summary>
        /// Reloads dataGrid itemsSource
        /// </summary>
        void GetSource()
        {
            dgCategories.ItemsSource = from cat in Data.Categories
                                       where cat.CategoryID > 4
                                       select cat;
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

            GetSource();
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
                    Category row = (Category)(sender as Button).DataContext; // gets category from dataGrid

                    Data.Categories.Remove(row); // deletes category from collection

                    // deletes category from database
                    string connectionString = Properties.Settings.Default.connectionString;
                    SqlConnection con = new(connectionString);
                    con.Open();
                    string query = "DELETE FROM Categories WHERE CategoryID = @ID";
                    SqlCommand command = new(query, con);
                    command.Parameters.AddWithValue("@ID", row.CategoryID);
                    command.ExecuteNonQuery();
                    con.Close();

                    GetSource();
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
