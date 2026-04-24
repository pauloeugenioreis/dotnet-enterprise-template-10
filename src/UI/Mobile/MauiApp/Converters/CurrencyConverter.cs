using System.Globalization;

namespace EnterpriseTemplate.MauiApp.Converters;

public class CurrencyConverter : IValueConverter
{
    private static readonly CultureInfo _brCulture = new CultureInfo("pt-BR");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return d.ToString("C", _brCulture);
        if (value is decimal m)
            return m.ToString("C", _brCulture);
        if (value is float f)
            return f.ToString("C", _brCulture);

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
