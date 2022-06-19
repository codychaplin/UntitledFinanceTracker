using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace UntitledFinanceTracker.Models
{
    class AccountType
    {
        int _accountTypeID;
        string _accountTypeName;

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

        /// <summary>
        /// Account Type ID
        /// </summary>
        public int AccountTypeID
        {
            get { return _accountTypeID; }
            private set
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

        public override string ToString()
        {
            return $"{AccountTypeID},{AccountTypeName}";
        }
    }
}
