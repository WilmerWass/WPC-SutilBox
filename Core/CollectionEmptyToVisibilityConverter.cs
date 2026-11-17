using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Wpc_SutilBox.Core
{
    public class CollectionEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                bool isEmpty = collection.Count == 0;
                bool invert = parameter?.ToString() == "Invert";
                return (isEmpty != invert) ? Visibility.Collapsed : Visibility.Visible;
            }
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
