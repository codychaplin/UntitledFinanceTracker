using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UntitledFinanceTracker.Models
{
    abstract class Data
    {
        public static ObservableCollection<Account> Accounts { get; set; }
        public static ObservableCollection<AccountType> AccountTypes { get; set; }
        public static ObservableCollection<Category> Categories { get; set; }
        public static ObservableCollection<Payee> Payees { get; set; }
        public static ObservableCollection<Transaction> Transactions { get; set; }

        // income categories
        public static readonly int TRANSFER_ID = 1;
        public static readonly int INCOME_ID = 3;

        /// <summary>
        /// Holds starting balance for each year of transactions
        /// </summary>
        /// <returns>
        /// Dictionary containing starting balances for each year (key = year, value = balance)
        /// </returns>
        public static Dictionary<int, decimal> YearStartBalances { get; set; }

        /// <summary>
        /// Debug purposes
        /// </summary>
        public static void Log(object log)
        {
            System.Diagnostics.Debug.WriteLine(log);
        }
    }
}