using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Mox.UI
{
    public class CenterToolTipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(v => v == DependencyProperty.UnsetValue))
            {
                return double.NaN;
            }

            double placementTargetSize = (double)values[0];
            double toolTipSize = (double)values[1];
            return (placementTargetSize / 2) - (toolTipSize / 2);
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
