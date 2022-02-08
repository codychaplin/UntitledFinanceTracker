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
        /// Loads account types into dataGrid.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChanged event data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (Data.AccountTypes.Count > 0)
                dgAccountTypes.ItemsSource = Data.AccountTypes;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
