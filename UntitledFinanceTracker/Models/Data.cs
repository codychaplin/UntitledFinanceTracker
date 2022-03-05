using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UntitledFinanceTracker.Models
{
    abstract class Data
    {
        public static List<Account> Accounts { get; set; }
        public static List<AccountType> AccountTypes { get; set; }
        public static List<Category> Categories { get; set; }
        public static List<Payee> Payees { get; set; }
        public static List<Transaction> Transactions { get; set; }

        // income categories
        public static readonly int TRANSFER_ID = 1;
        public static readonly int DEBIT_ID = 2;
        public static readonly int CREDIT_ID = 3;
        public static readonly int INCOME_ID = 4;

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