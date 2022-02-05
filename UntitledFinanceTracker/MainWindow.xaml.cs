using System;
using System.Windows;
using System.Windows.Controls;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            lvMainMenu.SelectedIndex = 0;
        }

        /// <summary>
        /// When a ListViewItem is selected, this event is triggered which opens the page corresponding to the selection.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SelectionChanged event data.</param>
        private void MainMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (lvMainMenu != null)
            {
                if (ContentPanel != null)
                    ContentPanel.Children.Clear();

                // dashboard
                if (lvMainMenu.SelectedItem.Equals(lviDashboard)) // if selected item is specified listViewItem
                {
                    Control ctrlDashboard = new Dashboard(); // insantiate control
                    this.ContentPanel.Children.Add(ctrlDashboard); // add control to panel as child
                }

                // transactions
                if (lvMainMenu.SelectedItem.Equals(lviTransactions))
                {
                    Control ctrlTransactionView = new Transactions();
                    this.ContentPanel.Children.Add(ctrlTransactionView);
                }

                // calendar
                if (lvMainMenu.SelectedItem.Equals(lviCalendar))
                {
                    Control ctrlWeekView = new Calendar();
                    this.ContentPanel.Children.Add(ctrlWeekView);
                }

                // balance sheet
                if (lvMainMenu.SelectedItem.Equals(lviBalanceSheet))
                {
                    Control ctrlBalanceSheet = new BalanceSheet();
                    this.ContentPanel.Children.Add(ctrlBalanceSheet);
                }

                // statistics
                if (lvMainMenu.SelectedItem.Equals(lviStats))
                {
                    Control ctrlStats = new Statistics();
                    this.ContentPanel.Children.Add(ctrlStats);
                }

                // settings
                if (lvMainMenu.SelectedItem.Equals(lviSettings))
                {
                    Control ctrlSettings = new Settings();
                    this.ContentPanel.Children.Add(ctrlSettings);
                }
            }
        }
    }
}
