using System.Globalization;

namespace EnterpriseTemplate.MauiApp.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = (bool)value;
        return isActive ? Color.FromArgb("#ECFDF5") : Color.FromArgb("#FEF2F2");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = (bool)value;
        return isActive ? "ATIVO" : "INATIVO";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StatusTextColorsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = (bool)value;
        return isActive ? Color.FromArgb("#059669") : Color.FromArgb("#DC2626");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
