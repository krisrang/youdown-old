using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lemon
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reverse = parameter != null ? Boolean.Parse((string)parameter) : false;

            if (reverse)
                return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
            
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reverse = parameter != null ? Boolean.Parse((string)parameter) : false;

            if (reverse)
                return ((Visibility)value) != Visibility.Visible;
            
            return ((Visibility)value) == Visibility.Visible;
        }
    }
}