using System;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

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
            Window addAccountType = new EditAccountTypes();
            addAccountType.Title = "Add Account Type";
            addAccountType.ShowDialog();

            dgAccountTypes.Items.Refresh();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            AccountType row = (AccountType)(sender as Button).DataContext;
            int ID = row.AccountTypeID;

            Window editAccountType = new EditAccountTypes(ID);
            editAccountType.ShowDialog();

            dgAccountTypes.Items.Refresh();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
