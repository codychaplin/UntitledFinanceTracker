using System;

namespace UntitledFinanceTracker.Models
{
    class Category
    {
        int _categoryID;
        int? _parentID;
        string _categoryName;

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
        /// <param name="cat">Category object.</param>
        public Category(Category cat) : this(cat.CategoryID, cat.ParentID, cat.ParentName, cat.CategoryName, cat.Enabled)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Category class using a copy constructor and a new category ID
        /// </summary>
        /// <param name="ID">Category ID.</param>
        /// <param name="cat">Category object.</param>
        public Category(int ID, Category cat) : this(ID, cat.ParentID, cat.ParentName, cat.CategoryName, cat.Enabled)
        {

        }

        /// <summary>
        /// Category ID
        /// </summary>
        public int CategoryID
        {
            get { return _categoryID; }
            private set
            {
                if (value > 0)
                    _categoryID = value;
                else
                    throw new Exception("Error: Category ID must be greater than 0");
            }
        }

        /// <summary>
        /// Parent Category ID
        /// </summary>
        public int? ParentID
        {
            get { return _parentID; }
            set
            {
                if (value >= 0 || value == null)
                    _parentID = value;
                else
                    throw new Exception("Error: Parent ID must be empty or greater than 0");
            }
        }

        /// <summary>
        /// Parent Category Name
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// Category Name
        /// </summary>
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    _categoryName = value;
                else
                    throw new Exception("Error: Category name cannot be blank");
            }
        }

        /// <summary>
        /// Category Status
        /// </summary>
        public bool Enabled { get; set; }

        public override string ToString()
        {
            return $"{ParentID},{CategoryID},{CategoryName}";
        }
    }
}
