using System;

namespace UntitledFinanceTracker
{
    class Transaction
    {
        public int TransactionID { get; private set; }
        public DateTime Date { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int SubcategoryID { get; set; }
        public string SubcategoryName { get; set; }
        public string Payee { get; set; }

        /// <summary>
        /// Initializes a new instance of the Transaction class with no parameters.
        /// </summary>
        public Transaction() { }

        /// <summary>
        /// Initializes a new instance of the Transaction class only setting the ID.
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        public Transaction(int ID)
        {
            TransactionID = ID;
        }

        /// <summary>
        /// Initializes a new instance of the Transaction class.
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        /// <param name="date">Transaction date.</param>
        /// <param name="accountID">Transaction account ID.</param>
        /// <param name="account">Transaction account name.</param>
        /// <param name="amount">Transaction amount.</param>
        /// <param name="categoryID">Transaction category ID.</param>
        /// <param name="category">Transaction category name.</param>
        /// <param name="subcategoryID">Transaction subcategory ID.</param>
        /// <param name="subcategory">Transaction subcategory name.</param>
        /// <param name="payee">Transaction payee.</param>
        public Transaction(int ID, DateTime date, int accountID, string account, decimal amount,
            int categoryID, string category, int subcategoryID, string subcategory, string payee) : this(ID)
        {
            Date = date;
            AccountID = accountID;
            AccountName = account;
            Amount = amount;
            CategoryID = categoryID;
            CategoryName = category;
            SubcategoryID = subcategoryID;
            SubcategoryName = subcategory;
            Payee = payee;
        }

        // debugging purposes
        public override string ToString()
        {
            return "\nTransactionID: " + TransactionID +
                "\nDate: " + Date +
                "\nAccountID: " + AccountID +
                "\nAccount: " + AccountName +
                "\nAmount: " + Amount +
                "\nCategoryID: " + CategoryID +
                "\nCategory: " + CategoryName +
                "\nSubcategoryID: " + SubcategoryID +
                "\nSubcategory: " + SubcategoryName +
                "\nPayee: " + Payee;
        }
    }
}
