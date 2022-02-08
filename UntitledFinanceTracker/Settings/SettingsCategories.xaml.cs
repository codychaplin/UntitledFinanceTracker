using System;
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

        }
    }
}
