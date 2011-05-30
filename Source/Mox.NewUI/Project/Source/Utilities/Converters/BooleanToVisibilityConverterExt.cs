using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mox.UI.Converters
{
    public class BooleanToVisibilityConverterExt : IValueConverter
    {
        #region Properties

        public bool HideWhenFalse
        {
            get;
            set;
        }

        #endregion

        #region Implementation of IValueConverter

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            if (boolValue)
            {
                return Visibility.Visible;
            }

            return HideWhenFalse ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
