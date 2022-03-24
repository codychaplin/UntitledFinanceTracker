using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace UntitledFinanceTracker.Models
{
    abstract class Data
    {
        public static List<Account> Accounts { get; set; }
        public static List<AccountType> AccountTypes { get; set; }
        public static List<Category> Categories { get; set; }
        public static List<Payee> Payees { get; set; }
        public static List<Transaction> Transactions { get; set; }

        public static readonly int TRANSFER_ID = 1; // parent transfer category
        public static readonly int DEBIT_ID = 2; // transfer debit subcategory
        public static readonly int CREDIT_ID = 3; // transfer credit subcategory
        public static readonly int INCOME_ID = 4; // parent income category

        /// <summary>
        /// Holds starting balance for each year of transactions
        /// </summary>
        /// <returns>
        /// Dictionary containing starting balances for each year (key = year, value = balance)
        /// </returns>
        public static Dictionary<int, decimal> YearStartBalances { get; set; }

        /// <summary>
        /// Updates order and running balance of transactions in memory and database
        /// </summary>
        /// <param name="date">Object that raised the event.</param>
        public static void UpdateOrderAndRunningBalance(Transaction trans)
        {
            try
            {
                int originalIndex = Transactions.IndexOf(trans); // get index before sort

                // sorts Transactions list
                Transactions = Transactions.
                    OrderBy(t => t.Date).
                    ThenBy(t => Math.Abs(t.Amount)).
                    ThenBy(t => t.CategoryID).
                    ThenBy(t => t.TransactionID).ToList();

                string connectionString = Properties.Settings.Default.connectionString;
                string orderUpdateQuery = "UPDATE Transactions SET DisplayOrder=@Order, Balance=@Balance " +
                                          "WHERE TransactionID=@TransactionID";

                SqlConnection con = new(connectionString);
                con.Open();

                decimal balance = -1; // set default balance
                int newIndex = Transactions.IndexOf(trans); // get index after sort
                
                if (newIndex == -1 || originalIndex == -1) // if can't find, throw exception
                    throw new Exception("Could not find index of transaction");

                if (newIndex > originalIndex) // if newIndex > originalIndex, keep originalIndex
                    newIndex = originalIndex;

                if (trans.SubcategoryID == CREDIT_ID) // if credit, get position before debit (-2)
                     newIndex -= 2;

                if (newIndex <= 0) // if 0, set starting balance
                    balance = Accounts.Sum(a => a.StartingBalance);
                else // balance = previous transaction's balance
                    balance = Transactions[newIndex - 1].Balance;

                

                int order = newIndex + 1;
                int counter = 0;
                
                // starting at specified transaction, calculate order and running balance
                for (int i = newIndex; i < Transactions.Count; i++, order++)
                {
                    if (Transactions[i].Order != order || Transactions[i].Balance != balance)
                    {
                        counter++;
                        Transactions[i].Order = order; // assign order
                        balance += Transactions[i].Amount; // update running balance
                        Transactions[i].Balance = balance; // assign running balance

                        // updates database
                        SqlCommand accUpdateCmd = new(orderUpdateQuery, con);
                        accUpdateCmd.Parameters.AddWithValue("@Order", Transactions[i].Order);
                        accUpdateCmd.Parameters.AddWithValue("@Balance", Transactions[i].Balance);
                        accUpdateCmd.Parameters.AddWithValue("@TransactionID", Transactions[i].TransactionID);
                        accUpdateCmd.ExecuteNonQuery();
                    }
                }

                Log(counter + " records updated");
                con.Close();
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
        /// Debug purposes
        /// </summary>
        public static void Log(object log)
        {
            System.Diagnostics.Debug.WriteLine(log);
        }
    }
}