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

                // transaction view
                if (lvMainMenu.SelectedItem.Equals(lviTransactionView))
                {
                    Control ctrlTransactionView = new TransactionView();
                    this.ContentPanel.Children.Add(ctrlTransactionView);
                }

                // week view
                if (lvMainMenu.SelectedItem.Equals(lviWeekView))
                {
                    Control ctrlWeekView = new WeekView();
                    this.ContentPanel.Children.Add(ctrlWeekView);
                }

                // month view
                if (lvMainMenu.SelectedItem.Equals(lviMonthView))
                {
                    Control ctrlMonthView = new MonthView();
                    this.ContentPanel.Children.Add(ctrlMonthView);
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
