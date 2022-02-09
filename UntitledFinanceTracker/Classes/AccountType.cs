using System;

namespace UntitledFinanceTracker
{
    class AccountType
    {
        public int AccountTypeID { get; private set; }
        public string AccountTypeName { get; set; }

        /// <summary>
        /// Initializes a new instance of the AccountType class with no parameters.
        /// </summary>
        public AccountType() { }

        /// <summary>
        /// Initializes a new instance of the AccountType class only setting the ID.
        /// </summary>
        /// <param name="ID">Account ID.</param>
        public AccountType(int ID)
        {
            AccountTypeID = ID;
        }

        /// <summary>
        /// Initializes a new instance of the AccountType class.
        /// </summary>
        /// <param name="ID">Account type ID.</param>
        /// <param name="accountTypeName">Account type.</param>
        public AccountType(int ID, string accountTypeName) : this(ID)
        {
            AccountTypeName = accountTypeName;
        }

        /// <summary>
        /// Initializes a new instance of the AccountType class using a copy constructor
        /// </summary>
        /// <param name="ID">Account ID.</param>
        /// <param name="accType">AccountType object.</param>
        public AccountType(int ID, AccountType accType) : this(ID, accType.AccountTypeName)
        {

        }
    }
}
