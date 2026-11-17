using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable enable

namespace Wpc_SutilBox.Core
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            string? checkValue = value.ToString();
            string? targetValue = parameter.ToString();

            bool isMatch = checkValue?.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase) == true;
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Visibility visibility && visibility == Visibility.Visible && parameter != null)
            {
                string? targetValue = parameter.ToString();
                if (targetType.IsEnum && targetValue != null && Enum.TryParse(targetType, targetValue, true, out object? parsed))
                {
                    return parsed;
                }
            }
            return Binding.DoNothing;
        }
    }
}
