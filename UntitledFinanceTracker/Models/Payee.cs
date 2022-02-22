using System;

namespace UntitledFinanceTracker.Models
{
    class Payee
    {
        int _payeeID;
        int? _accountID;

        /// <summary>
        /// Initializes a new instance of the Payee class with no parameters.
        /// </summary>
        public Payee() { }

        /// <summary>
        /// Initializes a new instance of the Payee class only setting the ID.
        /// </summary>
        /// <param name="ID">Payee ID.</param>
        public Payee(int ID)
        {
            PayeeID = ID;
        }

        /// <summary>
        /// Initializes a new instance of the AccountType class.
        /// </summary>
        /// <param name="ID">Payee ID.</param>
        /// <param name="AccountID">Account ID.</param>
        /// <param name="payeeName">Payee.</param>
        public Payee(int ID, int? accountID, string payeeName) : this(ID)
        {
            AccountID = accountID;
            PayeeName = payeeName;
        }

        /// <summary>
        /// Initializes a new instance of the Payee class using a copy constructor
        /// </summary>
        /// <param name="ID">Payee ID.</param>
        /// <param name="payee">Payee object.</param>
        public Payee(int ID, Payee payee) : this(ID, payee.AccountID, payee.PayeeName)
        {

        }

        /// <summary>
        /// Payee ID
        /// </summary>
        public int PayeeID
        {
            get { return _payeeID; }
            private set
            {
                if (value > 0)
                    _payeeID = value;
                else
                    throw new Exception("Error: Payee ID must be greater than 0");
            }
        }

        /// <summary>
        /// Account ID
        /// </summary>
        public int? AccountID
        {
            get { return _accountID; }
            private set
            {
                if (value > 0 || value == null)
                    _accountID = value;
                else
                    throw new Exception("Error: If Account ID  is not null, it must be greater than 0");
            }
        }

        /// <summary>
        /// Payee Name
        /// </summary>
        public string PayeeName { get; set; }
    }
}
