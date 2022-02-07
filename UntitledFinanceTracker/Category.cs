using System;

namespace UntitledFinanceTracker
{
    class Category
    {
        public int CategoryID { get; private set; }
        public int? ParentID { get; set; }
        public CategoryType Type { get; set; }
        public string ParentName { get; set; }
        public string CategoryName { get; set; }
        public bool Enabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the Category class with no parameters.
        /// </summary>
        public Category() { }

        /// <summary>
        /// Initializes a new instance of the Category class only setting the ID.
        /// </summary>
        /// <param name="ID">Account ID.</param>
        public Category(int ID)
        {
            CategoryID = ID;
        }

        /// <summary>
        /// Initializes a new instance of the Category class.
        /// </summary>
        /// <param name="ID">Category ID.</param>
        /// <param name="parentID">Parent category ID.</param>
        /// <param name="parentName">Parent category name.</param>
        /// <param name="categoryType">Parent category name.</param>
        /// <param name="categoryName">Category name.</param>
        /// <param name="enabled">Enabled.</param>
        public Category(int ID, int? parentID, string parentName, CategoryType type, string categoryName, bool enabled) : this(ID)
        {
            ParentID = parentID;
            ParentName = parentName;
            Type = type;
            CategoryName = categoryName;
            Enabled = enabled;
        }
    }

    /// <summary>
    /// Enum representing the type of transaction
    /// </summary>
    public enum CategoryType { Transfer, Income, Expense }
}
