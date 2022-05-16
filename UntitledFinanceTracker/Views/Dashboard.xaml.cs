using System;
using System.Linq;
using System.Windows.Controls;
using UntitledFinanceTracker.Models;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Helpers;
using LiveCharts.Geared;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Globalization;

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        DateTime fromDate { get; set; }
        DateTime ToDate { get; set; }

        BrushConverter converter = new BrushConverter();

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
            // set default dates
            fromDate = new DateTime(DateTime.Now.Year, 1, 1);
            ToDate = DateTime.Now;
            dtFrom.SelectedDate = fromDate;
            dtTo.SelectedDate = ToDate;
        }

        /// <summary>
        /// Updates all charts.
        /// </summary>
        void UpdateCharts()
        {
            // cache dates
            fromDate = dtFrom.SelectedDate.Value;
            ToDate = dtTo.SelectedDate.Value;

            // get list of Transactions within date, excluding transfers, and ordered by Order
            var query = Data.Transactions.Where(t => t.Date >= fromDate && t.Date <= ToDate)
                                           .Where(t => t.CategoryID != Data.TRANSFER_ID)
                                           .OrderBy(t => t.Order);

            // update charts
            UpdateRunningBalanceChart(query);
            UpdateExpensesChart(query);
            UpdateIncomeExpensesChart(query);
            UpdateValues(query);
        }

        /// <summary>
        /// Updates running balance chart.
        /// </summary>
        /// <param name="query">Filtered list of Transactions.</param>
        void UpdateRunningBalanceChart(IOrderedEnumerable<Transaction> query)
        {
            // get list of dates and balances from Transactions
            var results = query.Select(t => new { Date = t.Date, Balance = t.Balance }).ToList();

            // split results
            string[] dates = results.Select(r => r.Date.ToString("yy-MM-dd")).ToArray();
            var balances = results.Select(r => r.Balance).AsGearedValues();
            balances.WithQuality(Quality.Medium); // reduce quality to increase performance for larger ranges

            // sets balances as chart values in series
            SeriesCollection seriesCollection = new()
            {
                new LineSeries
                {
                    Title = "Running Balance",
                    Values = balances,
                    PointGeometry = null,
                    LineSmoothness = 0.5
                }
            };

            chrtBalance.Series = seriesCollection;

            // formats Y axis values as currency
            Func<double, string> YFormatter = value => value.ToString("C0");
            chrtBalanceAxisY.LabelFormatter = YFormatter;

            chrtBalanceAxisX.Labels = dates; // sets X axis values to dates
        }

        /// <summary>
        /// Updates expenses chart.
        /// </summary>
        /// <param name="query">Filtered list of Transactions.</param>
        void UpdateExpensesChart(IOrderedEnumerable<Transaction> query)
        {
            // get transactions between dates, group by category, return objects containing category name and summed amount
            var results = query.Where(t => t.CategoryID > Data.INCOME_ID)
                             .GroupBy(t => t.CategoryName)
                             .Select(t => new { Category = t.First().CategoryName,
                                                Amount = t.Sum(a => a.Amount) });
            
            SeriesCollection seriesCollection = new();
            foreach (var item in results)
            {
                seriesCollection.Add(
                    new PieSeries
                    {
                        Title = item.Category,
                        Values = new ChartValues<decimal> { item.Amount },
                        DataLabels = true,
                        LabelPoint = point => Math.Abs(point.Y).ToString("C"),
                        LabelPosition = PieLabelPosition.InsideSlice
                    });
            };

            chrtExpenses.Series = seriesCollection;

            // on hover, only show details for selection
            (chrtExpenses.DataTooltip as DefaultTooltip).SelectionMode = TooltipSelectionMode.OnlySender;
        }

        /// <summary>
        /// Updates Incomes/Expenses chart.
        /// </summary>
        /// <param name="query">Filtered list of Transactions.</param>
        void UpdateIncomeExpensesChart(IOrderedEnumerable<Transaction> query)
        {
            // split query into incomes/expenses grouped by month
            var incomes = query.Where(t => t.CategoryID == Data.INCOME_ID)
                               .GroupBy(t => t.Date.Month)
                               .Select(t => t.Sum(t => t.Amount)).AsChartValues();
            // select month as well for axis label
            var expenses = query.Where(t => t.CategoryID > Data.INCOME_ID)
                                .GroupBy(t => t.Date.Month)
                                .Select(t => new { Month = t.First().Date, Amount = t.Sum(t => t.Amount) });

            // flip expense sums to positive unless already positive, in that case make them negative
            var expenseBalances = expenses.Select(i => (i.Amount <= 0) ? Math.Abs(i.Amount) : i.Amount * -1).AsChartValues();
            var dates = expenses.Select(i => i.Month.ToString("MMM yy")).ToList();

            SeriesCollection seriesCollection = new()
            {
                new ColumnSeries
                {
                    Title = "Income",
                    Values = incomes,
                    Fill = (Brush)converter.ConvertFromString("#54de42")
                },
                new ColumnSeries
                {
                    Title = "Expense",
                    Values = expenseBalances,
                    Fill = Brushes.Red
                }
            };

            chrtIncExp.Series = seriesCollection;

            // formats Y axis values as currency
            Func<double, string> YFormatter = value => value.ToString("C0");
            chrtIncExpAxisY.LabelFormatter = YFormatter;

            chrtIncExpAxisX.Labels = dates; // sets X axis values to dates

            // on hover, only show details for selection
            (chrtIncExp.DataTooltip as DefaultTooltip).SelectionMode = TooltipSelectionMode.OnlySender;
        }

        void UpdateValues(IOrderedEnumerable<Transaction> query)
        {
            // income sum
            decimal incomeSum = query.Where(t => t.CategoryID == Data.INCOME_ID).Select(t => t.Amount).Sum();
            txtIncomeSum.Text = incomeSum.ToString("C");

            // if expense sum is negative (expected), convert to positive, otherwise, make negative
            decimal expenseSum = query.Where(t => t.CategoryID > Data.INCOME_ID).Select(t => t.Amount).Sum();
            txtExpenseSum.Text = ((expenseSum < 0) ? Math.Abs(expenseSum) : expenseSum * -1).ToString("C");

            // transaction count
            txtTransCount.Text = query.Count().ToString();

            // net income
            decimal net = incomeSum + expenseSum;

            // if positive, text is green, if negative, text is red
            if (net > 0)
                txtNet.Foreground = (Brush)converter.ConvertFromString("#a3ff9b");
            else if (net < 0)
                txtNet.Foreground = (Brush)converter.ConvertFromString("#ff8a89");

            txtNet.Text = net.ToString("C");
        }

        private void dtFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // validation
                if (dtFrom.SelectedDate == null || dtTo.SelectedDate == null)
                    return;
                if (dtFrom.SelectedDate >= dtTo.SelectedDate)
                    throw new Exception("Error: \"From\" date must be before \"To\" date.");

                UpdateCharts();
                
            }
            catch (Exception ex)
            {
                // reset dates and display error
                dtFrom.SelectedDate = fromDate;
                dtTo.SelectedDate = ToDate;
                MessageBox.Show(ex.Message);
            }
        }

        private void dtTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // validation
                if (dtFrom.SelectedDate == null || dtTo.SelectedDate == null)
                    return;
                if (dtFrom.SelectedDate >= dtTo.SelectedDate)
                    throw new Exception("Error: \"From\" date must be before \"To\" date.");

                UpdateCharts();
            }
            catch (Exception ex)
            {
                // reset dates and display error
                dtFrom.SelectedDate = fromDate;
                dtTo.SelectedDate = ToDate;
                MessageBox.Show(ex.Message);
            }
        }
    }
}
