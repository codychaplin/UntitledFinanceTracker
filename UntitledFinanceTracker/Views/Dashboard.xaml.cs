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

namespace UntitledFinanceTracker.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public static Dashboard dashboard; // singleton

        string SelectedExpenseCategory { get; set; }

        BrushConverter converter = new BrushConverter();

        /// <summary>
        /// Default constructor
        /// </summary>
        public Dashboard()
        {
            InitializeComponent();
            dashboard = this;
        }

        /// <summary>
        /// Performs calculations for data visualization and assigns values to charts
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs event data.</param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            DataContext = this;
            chrtSubExpenses.Visibility = Visibility.Visible;
            UpdateCharts();
        }

        /// <summary>
        /// Updates all charts
        /// </summary>
        public void UpdateCharts()
        {
            // get list of Transactions within date, excluding transfers, and ordered by Order
            var query = Data.Transactions.Where(t => t.Date >= Data.FromDate && t.Date <= Data.ToDate)
                                           .Where(t => t.CategoryID != Data.TRANSFER_ID)
                                           .OrderBy(t => t.Order);

            // update charts
            UpdateRunningBalanceChart(query);
            UpdateExpensesChart(query);
            UpdateIncomeExpensesChart(query);
            UpdateValues(query);

            if (chrtSubExpenses.Visibility == Visibility.Visible)
                UpdateSubExpensesChart(query);
        }

        /// <summary>
        /// Updates running balance chart
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
        /// Updates expenses chart
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
        /// Updates sub expenses chart
        /// </summary>
        /// <param name="query">Filtered list of Transactions.</param>
        void UpdateSubExpensesChart(IOrderedEnumerable<Transaction> query)
        {
            // get transactions between dates, group by category, return objects containing category name and summed amount
            var results = query.Where(t => t.CategoryName == SelectedExpenseCategory)
                             .GroupBy(t => t.SubcategoryName)
                             .Select(t => new {
                                 Category = t.First().SubcategoryName,
                                 Amount = t.Sum(a => a.Amount)
                             });

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

            chrtSubExpenses.Series = seriesCollection;

            // on hover, only show details for selection
            (chrtSubExpenses.DataTooltip as DefaultTooltip).SelectionMode = TooltipSelectionMode.OnlySender;
        }

        /// <summary>
        /// Updates Incomes/Expenses chart
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

        /// <summary>
        /// Updates summary values
        /// </summary>
        /// <param name="query">Filtered list of Transactions.</param>
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

        /// <summary>
        /// Updates Expense subcategory pie chart data
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="chartPoint">ChartPoint object associated with data.</param>
        private void chrtExpenses_DataClick(object sender, ChartPoint chartPoint)
        {
            // get list of Transactions within date, excluding transfers, and ordered by Order
            var query = Data.Transactions.Where(t => t.Date >= Data.FromDate && t.Date <= Data.ToDate)
                                           .Where(t => t.CategoryID != Data.TRANSFER_ID)
                                           .OrderBy(t => t.Order);

            SelectedExpenseCategory = chartPoint.SeriesView.Title;
            chrtSubExpenses.Visibility = Visibility.Visible;
            UpdateSubExpensesChart(query);
        }

        /// <summary>
        /// Scales properties with size of window
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains SizeChangedEventArgs event data.</param>
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // scales pie chart doughnut hole size
            chrtExpenses.InnerRadius = Application.Current.MainWindow.Height / 22;
            chrtSubExpenses.InnerRadius = Application.Current.MainWindow.Height / 50;

            // scales summary values based on height/width ratio (favouring width)
            double fontSize = (Application.Current.MainWindow.Height / 80) + (Application.Current.MainWindow.Width / 60);
            txtIncomeSum.FontSize = fontSize;
            txtExpenseSum.FontSize = fontSize;
            txtNet.FontSize = fontSize;
            txtTransCount.FontSize = fontSize;

            // based on width
            if (Application.Current.MainWindow.Width < 1000)
            {
                chrtExpenses.LegendLocation = LegendLocation.None;
                chrtSubExpenses.LegendLocation = LegendLocation.None;
            }
            else if (Application.Current.MainWindow.Width < 1200)
            {
                chrtExpenses.LegendLocation = LegendLocation.Right;
                chrtSubExpenses.LegendLocation = LegendLocation.None;
            }
            else
            {
                chrtExpenses.LegendLocation = LegendLocation.Right;
                chrtSubExpenses.LegendLocation = LegendLocation.Right;
            }

            // when window is maximized
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                // expenses pie chart inner radius and legend visibility
                chrtExpenses.InnerRadius = 50;
                chrtSubExpenses.InnerRadius = 30;
                chrtExpenses.LegendLocation = LegendLocation.Right;
                chrtSubExpenses.LegendLocation = LegendLocation.Right;

                // summary values
                txtIncomeSum.FontSize = 40;
                txtExpenseSum.FontSize = 40;
                txtNet.FontSize = 40;
                txtTransCount.FontSize = 40;
            }
        }
    }
}
