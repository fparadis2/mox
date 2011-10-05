using System;
using System.Globalization;
using System.Windows.Data;

namespace Mox.UI.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        #endregion
    }
}
