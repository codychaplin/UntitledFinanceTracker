using System;
using System.Linq;
using System.Globalization;
using System.Windows.Controls;
using System.Collections.Generic;
using UntitledFinanceTracker.Models;
using LiveCharts.Wpf;
using LiveCharts;
using System.Windows.Media;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Dashboard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Performs calculations for data visualization and assigns values to charts
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs event data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            dtFrom.SelectedDate = new DateTime(DateTime.Now.Year, 1, 1);
            dtTo.SelectedDate = DateTime.Now;
            CalculateWeeklyBalance();
        }

        /// <summary>
        /// Calculates weekly total balance to use in chart
        /// </summary>
        void CalculateWeeklyBalance()
        {
            
        }
    }
}
