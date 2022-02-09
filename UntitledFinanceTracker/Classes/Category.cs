using System;

namespace UntitledFinanceTracker
{
    class Category
    {
        public int CategoryID { get; private set; }
        public int? ParentID { get; set; }
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
        /// <param name="ID">Category ID.</param>
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
        /// <param name="categoryName">Category name.</param>
        /// <param name="enabled">Enabled.</param>
        public Category(int ID, int? parentID, string parentName, string categoryName, bool enabled) : this(ID)
        {
            ParentID = parentID;
            ParentName = parentName;
            CategoryName = categoryName;
            Enabled = enabled;
        }

        /// <summary>
        /// Initializes a new instance of the Category class using a copy constructor
        /// </summary>
        /// <param name="ID">Category ID.</param>
        /// <param name="cat">Category object.</param>
        public Category(int ID, Category cat) : this(ID, cat.ParentID, cat.ParentName, cat.CategoryName, cat.Enabled)
        {

        }
    }
}
