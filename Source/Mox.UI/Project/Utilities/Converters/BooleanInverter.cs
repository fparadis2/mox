using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Mox.UI
{
    public class BooleanInverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return !(bool)value;
            return value;
        }
        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
