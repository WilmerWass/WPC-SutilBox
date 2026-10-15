using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable enable

namespace Wpc_SutilBox.Core
{
    public class EnumToVisibilityConverter : IValueConverter
    {
        private static ILogService? _logger;

        private static ILogService? Logger
        {
            get
            {
                if (_logger == null && Application.Current is App app && app.Services != null)
                {
                    _logger = app.Services.GetService(typeof(ILogService)) as ILogService;
                }
                return _logger;
            }
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                Logger?.Info($"[EnumToVisibilityConverter] Null value ({value}) or parameter ({parameter}) -> Collapsed");
                return Visibility.Collapsed;
            }

            string? checkValue = value.ToString();
            string? targetValue = parameter.ToString();
            
            bool isMatch = checkValue?.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase) == true;
            Visibility result = isMatch ? Visibility.Visible : Visibility.Collapsed;

            Logger?.Info($"[EnumToVisibilityConverter] Current='{checkValue}', Target='{targetValue}' => {result}");

            return result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Visibility visibility && visibility == Visibility.Visible && parameter != null)
            {
                string? targetValue = parameter.ToString();
                if (targetValue != null)
                {
                    try
                    {
                        return Enum.Parse(targetType, targetValue);
                    }
                    catch 
                    { 
                        // Return DoNothing if parsing fails
                    }
                }
            }
            return Binding.DoNothing;
        }
    }
}

