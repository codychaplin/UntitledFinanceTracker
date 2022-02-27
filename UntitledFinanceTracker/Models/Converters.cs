using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace UntitledFinanceTracker.Models
{
    class AccountTypeTotalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // get account type from header name
            var accountType = from accType in Data.AccountTypes
                                where accType.AccountTypeName == value.ToString()
                                select accType.AccountTypeID;

            // if account type exists, return the sum of all enabled accounts with matching accountTypeID
            if (accountType.Any())
            {
                return Data.Accounts.Where(a => a.AccountTypeID == accountType.First() && a.Enabled).Sum(a => a.CurrentBalance).ToString("C");
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
    }

    class ColourValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int ID = (int)value;
            switch (ID)
            {
                case 1:
                    return "#ffffff"; // transfer = white
                case 3:
                    return "#a3ff9b"; // income = green
                default:
                    return "#ff8a89"; // expense = red
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
