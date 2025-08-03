using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace WPF_Tool
{
    public class IsErrorStatusCodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Handles both int and HttpStatusCode
            if (value is int code)
                return code != 200;
            if (value is HttpStatusCode status)
                return status != HttpStatusCode.OK;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}