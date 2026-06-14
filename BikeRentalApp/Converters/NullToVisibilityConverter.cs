using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BikeRentalApp.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string paramString && paramString.Contains("|"))
            {
                var options = paramString.Split('|');
                if (value is bool b) return b ? options[0] : options[1];
                if (value == null) return options[1];
                return options[0];
            }

            if (targetType == typeof(Visibility))
                return value == null ? Visibility.Collapsed : Visibility.Visible;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
