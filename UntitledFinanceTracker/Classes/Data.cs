using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace UntitledFinanceTracker
{
    abstract class Data
    {
        public static ObservableCollection<Account> Accounts { get; set; }
        public static ObservableCollection<AccountType> AccountTypes { get; set; }
        public static ObservableCollection<Category> Categories { get; set; }
        public static ObservableCollection<Transaction> Transactions { get; set; }
    }
}