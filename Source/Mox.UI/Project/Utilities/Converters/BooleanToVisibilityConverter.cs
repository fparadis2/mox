using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mox.UI
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }
        public bool Hide { get; set; }

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = System.Convert.ToBoolean(value);

            if (Invert)
                boolValue = !boolValue;

            if (boolValue)
                return Visibility.Visible;

            return Hide ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
