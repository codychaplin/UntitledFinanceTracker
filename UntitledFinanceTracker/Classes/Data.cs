using System.Collections.ObjectModel;

namespace UntitledFinanceTracker
{
    abstract class Data
    {
        public static ObservableCollection<Account> Accounts { get; set; }
        public static ObservableCollection<AccountType> AccountTypes { get; set; }
        public static ObservableCollection<Category> Categories { get; set; }
        public static ObservableCollection<Transaction> Transactions { get; set; }

        /// <summary>
        /// Debug purposes
        /// </summary>
        public static void Log(object log)
        {
            System.Diagnostics.Debug.WriteLine(log);
        }
    }
}