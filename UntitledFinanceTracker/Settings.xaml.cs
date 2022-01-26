using System;
using System.Collections.Generic;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
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
                    this.SettingsPanel.Children.Add(ctrlSettingsAccounts);
                }

                // categories
                if (lvSettingsMenu.SelectedItem.Equals(lviCategories))
                {
                    Control ctrlSettingsCategories = new SettingsCategories();
                    this.SettingsPanel.Children.Add(ctrlSettingsCategories);
                }
            }
        }
    }
}
