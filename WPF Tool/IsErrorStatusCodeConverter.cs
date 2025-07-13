using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF_Tool
{
    public class IsErrorStatusCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int code)
                return code < 200 || code > 299;
            if (value is string str && int.TryParse(str, out int code2))
                return code2 < 200 || code2 > 299;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}