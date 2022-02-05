using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntitledFinanceTracker
{
    class Transaction
    {
        public int TransactionID { get; private set; }
        public DateTime Date { get; set; }
        public object[] Account = new object[2];
        public CategoryType Type { get; set; }
        public decimal Amount { get; set; }
        public object[] Category = new object[2];
        public object[] Subcategory = new object[2];
        public string Payee { get; set; }

        /// <summary>
        /// Initializes a new instance of the Transaction class with no parameters.
        /// </summary>
        public Transaction() { }

        /// <summary>
        /// Initializes a new instance of the Transaction class.
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        /// <param name="date">Transaction date.</param>
        /// <param name="AccountID">Transaction account ID.</param>
        /// <param name="type">Transaction type.</param>
        /// <param name="amount">Transaction amount.</param>
        /// <param name="categoryID">Transaction category ID.</param>
        /// <param name="category">Transaction category.</param>
        /// <param name="subcategoryID">Transaction subcategory ID.</param>
        /// <param name="subcategory">Transaction subcategory.</param>
        /// <param name="payee">Transaction payee.</param>
        public Transaction(int ID, DateTime date, int accountID, string account, CategoryType type, decimal amount,
            int categoryID, string category, int subcategoryID, string subcategory, string payee)
        {
            TransactionID = ID;
            Date = date;
            Account[0] = accountID;
            Account[1] = account;
            Type = type;
            Amount = amount;
            Category[0] = categoryID;
            Category[1] = category;
            Subcategory[0] = subcategoryID;
            Subcategory[1] = subcategory;
            Payee = payee;
        }

        // debugging purposes
        public override string ToString()
        {
            return "TransactionID: " + TransactionID +
                "\nDate: " + Date +
                "\nAccountID: " + Account[0] +
                "\nAccount: " + Account[1] +
                "\nType: " + Type +
                "\nAmount: " + Amount +
                "\nCategoryID: " + Category[0] +
                "\nCategory: " + Category[1] +
                "\nSubcategoryID: " + Subcategory[0] +
                "\nSubcategory: " + Subcategory[1] +
                "\nPayee: " + Payee;
        }
    }

    public enum CategoryType { Transfer, Income, Expense }
}
