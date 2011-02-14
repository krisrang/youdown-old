using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using YouDown.Models;

namespace YouDown.Presentation
{
    public class VideoStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (VideoState) value;

            SolidColorBrush brush;

            switch (state)
            {
                case VideoState.Error:
                    brush = Brushes.Red;
                    break;
                case VideoState.Cancelled:
                    brush = Brushes.Orange;
                    break;
                case VideoState.Downloaded:
                    brush = Brushes.Green;
                    break;
                default:
                    brush = Brushes.Transparent;
                    break;
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
