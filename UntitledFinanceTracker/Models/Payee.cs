using System;

namespace UntitledFinanceTracker.Models
{
    class Payee
    {
        int? _payeeID;

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
        /// <param name="payeeName">Payee.</param>
        public Payee(int ID, string payeeName) : this(ID)
        {
            PayeeName = payeeName;
        }

        /// <summary>
        /// Initializes a new instance of the Payee class using a copy constructor
        /// </summary>
        /// <param name="ID">Payee ID.</param>
        /// <param name="payee">Payee object.</param>
        public Payee(int ID, Payee payee) : this(ID, payee.PayeeName)
        {

        }

        /// <summary>
        /// Payee ID
        /// </summary>
        public int? PayeeID
        {
            get { return _payeeID; }
            private set
            {
                if (value > 0 || value == null)
                    _payeeID = value;
                else
                    throw new Exception("Error: If Payee ID is not null, it must be greater than 0");
            }
        }

        /// <summary>
        /// Payee Name
        /// </summary>
        public string PayeeName { get; set; }
    }
}
