using System;
using System.Linq;
using System.Globalization;
using System.Windows.Controls;
using System.Collections.Generic;
using UntitledFinanceTracker.Models;
using LiveCharts.Wpf;
using LiveCharts;

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
            CalculateWeeklyBalance();
        }

        /// <summary>
        /// Calculates weekly total balance to use in chart
        /// </summary>
        void CalculateWeeklyBalance()
        {
            int currentYear = DateTime.Now.Year;
            int currentWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            decimal balance = Data.YearStartBalances[currentYear];
            var weeks = Enumerable.Range(1, currentWeek);

            ChartValues<decimal> points = new();
            points.Add(balance);

            foreach (int week in weeks)
            {
                // get transactions in year and in specified week
                var transactionsInYear = Data.Transactions.Where(t => t.Date.Year == currentYear);
                var transactionsInWeek = transactionsInYear.Where(t => ISOWeek.GetWeekOfYear(t.Date) == week);

                // on first week, add transactions that were technically in last week of previous year
                if (week == 1)
                {
                    var other = transactionsInYear.Where(t => t.Date.Month == 1 && ISOWeek.GetWeekOfYear(t.Date) > 50);
                    transactionsInWeek = transactionsInWeek.Concat(other);
                }

                decimal incomes = transactionsInWeek.Where(t => t.CategoryID == Data.INCOME_ID).Sum(t => t.Amount);
                decimal expenses = transactionsInWeek.Where(t => t.CategoryID > Data.INCOME_ID).Sum(t => t.Amount);
                decimal sum = balance + incomes - expenses;
                balance = sum;
                points.Add(balance);
            }

            SeriesCollection series = new SeriesCollection { new LineSeries
            {
                Values = points,
                LineSmoothness = 0.5,
                PointGeometry = null
            }};
            
            Func<double, string> Formatter = value => value.ToString("C0");
            chrtBalance.Series = series;
            chrtBalanceAxisY.LabelFormatter = Formatter;
            chrtBalanceAxisX.Separator.Step = points.Count / 7;
        }
    }
}
