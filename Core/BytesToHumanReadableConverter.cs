using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpc_SutilBox.Core;

public sealed class BytesToHumanReadableConverter : IValueConverter
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB"];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IConvertible convertible || value is string)
            return Binding.DoNothing;

        try
        {
            var bytes = convertible.ToDouble(CultureInfo.InvariantCulture);
            if (double.IsNaN(bytes) || double.IsInfinity(bytes))
                return Binding.DoNothing;

            var unit = 0;
            while (Math.Abs(bytes) >= 1024 && unit < Units.Length - 1)
            {
                bytes /= 1024;
                unit++;
            }

            return $"{bytes.ToString(unit == 0 ? "0" : "0.##", culture)} {Units[unit]}";
        }
        catch (FormatException)
        {
            return Binding.DoNothing;
        }
        catch (InvalidCastException)
        {
            return Binding.DoNothing;
        }
        catch (OverflowException)
        {
            return Binding.DoNothing;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
