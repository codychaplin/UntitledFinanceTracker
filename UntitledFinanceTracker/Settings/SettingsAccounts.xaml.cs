using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UntitledFinanceTracker
{
    /// <summary>
    /// Interaction logic for SettingsAccounts.xaml
    /// </summary>
    public partial class SettingsAccounts : UserControl
    {
        public SettingsAccounts()
        {
            InitializeComponent();
        }
        
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            if (Data.Accounts.Count > 0)
                dgAccounts.ItemsSource = Data.Accounts;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
