using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Controls;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Settings()
        {
            InitializeComponent();
            lvSettingsMenu.SelectedIndex = 0;
        }

        /// <summary>
        /// When a ListViewItem is selected, this event is triggered which opens the page corresponding to the selection.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChanged event data.</param>
        private void SettingsMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvSettingsMenu != null)
            {
                if (SettingsPanel != null)
                    SettingsPanel.Children.Clear();

                // general
                if (lvSettingsMenu.SelectedItem.Equals(lviGeneral)) // if selected item is specified listViewItem
                {
                    Control ctrlSettingsGeneral = new SettingsGeneral(); // insantiate control
                    SettingsPanel.Children.Add(ctrlSettingsGeneral); // add control to panel as child
                }

                // account types
                if (lvSettingsMenu.SelectedItem.Equals(lviAccountTypes))
                {
                    Control ctrlSettingsAccountTypes = new SettingsAccountTypes();
                    SettingsPanel.Children.Add(ctrlSettingsAccountTypes);
                }

                // accounts
                if (lvSettingsMenu.SelectedItem.Equals(lviAccounts))
                {
                    Control ctrlSettingsAccounts = new SettingsAccounts();
                    SettingsPanel.Children.Add(ctrlSettingsAccounts);
                }

                // categories
                if (lvSettingsMenu.SelectedItem.Equals(lviCategories))
                {
                    Control ctrlSettingsCategories = new SettingsCategories();
                    SettingsPanel.Children.Add(ctrlSettingsCategories);
                }
            }
        }
    }
}
