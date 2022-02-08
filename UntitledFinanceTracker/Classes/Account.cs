using System;

namespace UntitledFinanceTracker
{
    class Account
    {
        public int AccountID { get; private set; }
        public string AccountName { get; set; }
        public int AccountTypeID { get; set; }
        public string AccountTypeName { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool Enabled { get; set; }

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
    }
}