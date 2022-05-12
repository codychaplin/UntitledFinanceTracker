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
        public string[] Labels { get; set; }
        DateTime fromDate { get; set; }
        DateTime ToDate { get; set; }

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
        /// Updates running balance chart.
        /// </summary>
        void UpdateRunningBalanceChart()
        {
            // cache start and end dates, get list of properties from Transactions
            fromDate = dtFrom.SelectedDate.Value;
            ToDate = dtTo.SelectedDate.Value;
            var results = Data.Transactions.Where(t => t.Date >= fromDate && t.Date <= ToDate)
                                           .Where(t => t.CategoryID != Data.TRANSFER_ID)
                                           .OrderBy(t => t.Order)
                                           .Select(t => new { Date = t.Date, Amount = t.Amount, Balance = t.Balance }).ToList();

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

            // sets X axis values to dates
            Labels = dates;
            chrtBalanceAxisX.Labels = Labels;
        }

        /// <summary>
        /// Updates expenses chart.
        /// </summary>
        void UpdateExpensesChart()
        {
            // cache start and end dates
            fromDate = dtFrom.SelectedDate.Value;
            ToDate = dtTo.SelectedDate.Value;
            
            // get transactions between dates, group by category, return objects containing category name and summed amount
            var query = Data.Transactions.Where(t => t.Date >= fromDate && t.Date <= ToDate)
                                         .Where(t => t.CategoryID > Data.INCOME_ID)
                                         .GroupBy(t => t.CategoryName)
                                         .Select(t => new
                                         {
                                             Category = t.First().CategoryName,
                                             Amount = t.Sum(a => a.Amount)
                                         });
            
            SeriesCollection seriesCollection = new();
            foreach (var item in query)
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

            // sets balances as chart values in series
            chrtExpenses.Series = seriesCollection;

            (chrtExpenses.DataTooltip as DefaultTooltip).SelectionMode = TooltipSelectionMode.OnlySender;
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

                UpdateRunningBalanceChart();
                UpdateExpensesChart();
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

                UpdateRunningBalanceChart();
                UpdateExpensesChart();
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
