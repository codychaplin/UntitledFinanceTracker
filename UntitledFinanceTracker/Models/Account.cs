using System;

namespace UntitledFinanceTracker.Models
{
    class Account
    {
        int _accountID;
        string _accountName;
        int _accountTypeID;
        string _accountTypeName;

        /// <summary>
        /// Initializes a new instance of the Account class with no parameters.
        /// </summary>
        public Account() { }

        /// <summary>
        /// Initializes a new instance of the Account class only setting the ID.
        /// </summary>
        /// <param name="ID">Account ID.</param>
        public Account(int ID)
        {
            AccountID = ID;
        }

        /// <summary>
        /// Initializes a new instance of the Account class.
        /// </summary>
        /// <param name="ID">Account ID.</param>
        /// <param name="accountName">Account name.</param>
        /// <param name="accountTypeID">Account type ID.</param>
        /// <param name="accountTypeName">Account type.</param>
        /// <param name="startingBalance">Starting balance.</param>
        /// <param name="currentBalance">Current balance.</param>
        /// <param name="enabled">Enabled.</param>
        public Account(int ID, string accountName, int accountTypeID, string accountTypeName,
            decimal startingBalance, decimal currentBalance, bool enabled) : this(ID)
        {
            AccountName = accountName;
            AccountTypeID = accountTypeID;
            AccountTypeName = accountTypeName;
            StartingBalance = startingBalance;
            CurrentBalance = currentBalance;
            Enabled = enabled;
        }

        /// <summary>
        /// Initializes a new instance of the Account class using a copy constructor
        /// </summary>
        /// <param name="acc">Account object.</param>
        public Account(Account acc) : this(acc.AccountID, acc.AccountName, acc.AccountTypeID, acc.AccountTypeName,
            acc.StartingBalance, acc.CurrentBalance, acc.Enabled)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Account class using a copy constructor and a new account ID
        /// </summary>
        /// <param name="ID">Account ID.</param>
        /// <param name="acc">Account object.</param>
        public Account(int ID, Account acc) : this(ID, acc.AccountName, acc.AccountTypeID, acc.AccountTypeName,
            acc.StartingBalance, acc.CurrentBalance, acc.Enabled)
        {

        }

        /// <summary>
        /// Account ID
        /// </summary>
        public int AccountID
        {
            get { return _accountID; }
            private set
            {
                if (value > 0)
                    _accountID = value;
                else
                    throw new Exception("Error: Account ID must be greater than 0");
            }
        }

        /// <summary>
        /// Account Name
        /// </summary>
        public string AccountName
        {
            get { return _accountName; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _accountName = value;
                else
                    throw new Exception("Error: Account name cannot be blank");
            }
        }

        /// <summary>
        /// Account Type ID
        /// </summary>
        public int AccountTypeID
        {
            get { return _accountTypeID; }
            set
            {
                if (value > 0)
                    _accountTypeID = value;
                else
                    throw new Exception("Error: Account type ID must be greater than 0");
            }
        }

        /// <summary>
        /// Account Type Name
        /// </summary>
        public string AccountTypeName
        {
            get { return _accountTypeName; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _accountTypeName = value;
                else
                    throw new Exception("Error: Account type name cannot be blank");
            }
        }

        /// <summary>
        /// Starting Balance of Account
        /// </summary>
        public decimal StartingBalance { get; set; }

        /// <summary>
        /// Starting Balance of Account represented as a String
        /// </summary>
        public string StartingBalanceString
        {
            get { return StartingBalance.ToString(); }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (decimal.TryParse(value, out decimal val))
                        StartingBalance = val;
                    else
                        throw new Exception("Error: Starting balance could not be converted to decimal");
                }
                else
                    StartingBalance = 0;
            }
        }

        /// <summary>
        /// Current Balance of Account
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Current Balance of Account represented as a String
        /// </summary>
        public string CurrentBalanceString
        {
            get { return CurrentBalance.ToString(); }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (decimal.TryParse(value, out decimal val))
                        CurrentBalance = val;
                    else
                        throw new Exception("Error: Current balance could not be converted to decimal");
                }
                else
                    CurrentBalance = 0;
            }
        }

        /// <summary>
        /// Account Status
        /// </summary>
        public bool Enabled { get; set; }
    }
}