using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace UntitledFinanceTracker
{
    abstract class Data
    {
        public static ObservableCollection<Account> Accounts = new();
        public static ObservableCollection<AccountType> AccountTypes = new();
        public static ObservableCollection<Category> Categories = new();
        public static ObservableCollection<Transaction> transactions = new();
    }
}
