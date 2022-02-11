﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for EditAccounts.xaml
    /// </summary>
    public partial class EditCategories : Window
    {
        Category category { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EditCategories()
        {
            InitializeComponent();

            category = new();
            btnEdit.Content = "Add";
        }

        /// <summary>
        /// Parameterized constructor setting the ID
        /// </summary>
        /// <param name="ID">Account ID.</param>
        public EditCategories(int ID)
        {
            InitializeComponent();

            SetCategory(ID);
        }

        /// <summary>
        /// Loads category data into input fields
        /// </summary>
        /// <param name="ID">Account Type ID.</param>
        void SetCategory(int ID)
        {
            try
            {
                IEnumerable<Category> c = from cat in Data.Categories
                                         where cat.CategoryID == ID
                                         select cat;

                category = c.Count() == 1 ? c.First() : throw new Exception("ERROR: Could not find record");

                // sets input value from category
                cbParent.SelectedValue = category.ParentID;
                txtName.Text = category.CategoryName;
                chkEnabled.IsChecked = category.Enabled;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Populates parent categories ComboBox
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains EventArgs data.</param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            // gets parents categories
            IEnumerable<Category> cats = from cat in Data.Categories
                                         where cat.ParentID == null && cat.CategoryID != 1
                                         select cat;

            // prepends empty object to beginning
            List<Category> categories = cats.ToList();
            categories.Insert(0, new Category());
            cbParent.ItemsSource = categories;
        }

        /// <summary>
        /// Updates/adds category to collection and database
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = Properties.Settings.Default.connectionString;
                SqlConnection con = new(connectionString);
                con.Open();

                // update category
                category.ParentID = (int?)cbParent.SelectedValue;
                category.ParentName = cbParent.Text;
                category.CategoryName = txtName.Text;
                category.Enabled = chkEnabled.IsChecked != null && chkEnabled.IsChecked != false;

                if (Title == "Edit Category")
                {
                    // updates collection
                    Category cat = Data.Categories.First(c => c.CategoryID == category.CategoryID);
                    cat = category;
                    
                    // updates database
                    string query = "UPDATE Categories SET ParentID_fk = " + category.ParentID +
                        ", CategoryName = '" + category.CategoryName + "'" +
                        ", Enabled = '" + category.Enabled + "'" +
                        " WHERE CategoryID = " + category.CategoryID;
                    SqlCommand command = new(query, con);
                    command.ExecuteNonQuery();
                }
                else if (Title == "Add Category")
                {
                    // updates database
                    string query = "";

                    if (category.ParentID.HasValue && category.ParentID != 0)
                    {
                        query = "INSERT INTO Categories (ParentID_fk, CategoryName, Enabled)" +
                            " OUTPUT INSERTED.CategoryID" +
                            " VALUES (" + category.ParentID +
                            ", '" + category.CategoryName + "'" +
                            ", '" + category.Enabled + "')";
                    }
                    else
                    {
                        query = "INSERT INTO Categories (ParentID_fk, CategoryName, Enabled)" +
                            " OUTPUT INSERTED.CategoryID" +
                            " VALUES (null" +
                            ", '" + category.CategoryName + "'" +
                            ", '" + category.Enabled + "')";
                    }

                    // execute query and get ID of new category
                    SqlCommand command = new(query, con);
                    int ID = (int)command.ExecuteScalar();

                    // create and add newCategory to collection
                    Category newCategory = new(ID, category);
                    Data.Categories.Add(newCategory);
                }
                else
                {
                    throw new Exception("How did this even happen");
                }

                con.Close();
                Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Closes window
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Contains RoutedEventArgs data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}