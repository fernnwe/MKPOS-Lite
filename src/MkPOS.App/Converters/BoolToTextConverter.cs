using System.Globalization;
using System.Windows.Data;

namespace MKPOS.App.Converters;

public sealed class BoolToTextConverter : IValueConverter
{
    public string TrueText { get; set; } = "Sí";
    public string FalseText { get; set; } = "No";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? TrueText : FalseText;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() == TrueText;
    }
}