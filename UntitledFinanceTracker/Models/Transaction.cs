using System;

namespace UntitledFinanceTracker.Models
{
    class Transaction
    {
        int _transactionID;
        int _accountID;
        string _accountName;
        int _categoryID;
        string _categoryName;
        int _subcategoryID;
        string _subcategoryName;
        int? _payeeID;

        /// <summary>
        /// Current highest transaction ID
        /// </summary>
        public static int HighestID { get; private set; }

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
        /// <param name="payeeID">Transaction payee ID.</param>
        /// <param name="payee">Transaction payee.</param>
        /// <param name="balance">Running total balance.</param>
        /// <param name="order">Display order.</param>
        public Transaction(int ID, DateTime date, int accountID, string account, decimal amount,
            int categoryID, string category, int subcategoryID, string subcategory, int? payeeID, string payee, decimal balance, int order) : this(ID)
        {
            Date = date;
            AccountID = accountID;
            AccountName = account;
            Amount = amount;
            CategoryID = categoryID;
            CategoryName = category;
            SubcategoryID = subcategoryID;
            SubcategoryName = subcategory;
            PayeeID = payeeID;
            PayeeName = payee;
            Balance = balance;
            Order = order;
        }

        /// <summary>
        /// Initializes a new instance of the Transaction class using a copy constructor
        /// </summary>
        /// <param name="ID">Transaction ID.</param>
        /// <param name="trans">Transaction object.</param>
        public Transaction(int ID, Transaction trans) : this(ID, trans.Date, trans.AccountID, trans.AccountName, trans.Amount,
            trans.CategoryID, trans.CategoryName, trans.SubcategoryID, trans.SubcategoryName, trans.PayeeID, trans.PayeeName, trans.Balance, trans.Order)
        {

        }

        public bool Compare(Transaction trans1, Transaction trans2)
        {
            if (trans1.Date != trans2.Date || trans1.AccountID != trans2.AccountID || trans1.Amount != trans2.Amount
                || trans1.CategoryID != trans2.CategoryID || trans1.SubcategoryID != trans2.SubcategoryID
                || trans1.PayeeID != trans2.PayeeID)
            {
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public int TransactionID
        {
            get { return _transactionID; }
            private set
            {
                if (value > 0)
                {
                    _transactionID = value;
                    HighestID = (value > HighestID) ? value : HighestID;
                }
                else
                    throw new Exception("Error: Transaction ID must be greater than 0");
            }
        }

        /// <summary>
        /// Transaction Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Transaction Date representing as a String
        /// </summary>
        public string DateString
        {
            get { return Date.ToString("yyyy-MM-dd"); }
            set
            {
                if (DateTime.TryParse(value, out DateTime date))
                    Date = date;
                else
                    throw new Exception("Error: Transaction Date String cannot be converted to a Date");
            }
        }

        /// <summary>
        /// Transaction Account ID
        /// </summary>
        public int AccountID
        {
            get { return _accountID; }
            set
            {
                if (value > 0)
                    _accountID = value;
                else
                    throw new Exception("Error: Transaction Account ID must be greater than 0");
            }
        }

        /// <summary>
        /// Transaction Account Name
        /// </summary>
        public string AccountName
        {
            get { return _accountName; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _accountName = value;
                else
                    throw new Exception("Error: Transaction Account Name cannot be blank");
            }
        }

        /// <summary>
        /// Transaction Amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Transaction Amount representing as a String
        /// </summary>
        public string AmountString
        {
            get { return Amount.ToString(); }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (decimal.TryParse(value, out decimal val))
                        Amount = val;
                    else
                        throw new Exception("Error: Transaction Amount could not be converted to decimal");
                }
                else
                    throw new Exception("Error: Transaction Amount cannot be blank");
            }
        }

        /// <summary>
        /// Transaction Category ID
        /// </summary>
        public int CategoryID
        {
            get { return _categoryID; }
            set
            {
                if (value > 0)
                    _categoryID = value;
                else
                    throw new Exception("Error: Transaction Category ID must be greater than 0");
            }
        }

        /// <summary>
        /// Transaction Category Name
        /// </summary>
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _categoryName = value;
                else
                    throw new Exception("Error: Transaction Category Name cannot be blank");
            }
        }

        /// <summary>
        /// Transaction Subcategory ID
        /// </summary>
        public int SubcategoryID
        {
            get { return _subcategoryID; }
            set
            {
                if (value > 0)
                    _subcategoryID = value;
                else
                    throw new Exception("Error: Transaction Subcategory ID must be greater than 0");
            }
        }

        /// <summary>
        /// Transaction Subcategory Name
        /// </summary>
        public string SubcategoryName
        {
            get { return _subcategoryName; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _subcategoryName = value;
                else
                    throw new Exception("Error: Transaction Subcategory Name cannot be blank");
            }
        }

        /// <summary>
        /// Transaction Payee ID
        /// </summary>
        public int? PayeeID
        {
            get { return _payeeID; }
            set
            {
                if (value > 0 || value == null)
                    _payeeID = value;
                else
                    throw new Exception("Error: If Transaction Payee ID is not null, it must be greater than 0");


            }
        }

        /// <summary>
        /// Transaction Payee Name
        /// </summary>
        public string PayeeName { get; set; }

        /// <summary>
        /// Running total account balance
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Overridden ToString() that outputs data into CSV friendly format
        /// </summary>
        public override string ToString()
        {
            return $"{DateString},{AccountName},{AmountString},{CategoryName},{SubcategoryName},{PayeeName}";
        }
    }
}
