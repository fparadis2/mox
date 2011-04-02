using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mox.UI.Converters
{
    public class VisibilityToGridLengthConverter : IValueConverter
    {
        #region Constructor

        public VisibilityToGridLengthConverter()
        {
            DefaultGridLength = new GridLength(1, GridUnitType.Star);
        }

        #endregion

        #region Properties

        public GridLength DefaultGridLength
        {
            get;
            set;
        }

        #endregion

        #region Implementation of IValueConverter

        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            switch (visibility)
            {
                case Visibility.Visible:
                case Visibility.Hidden:
                    return DefaultGridLength;

                case Visibility.Collapsed:
                    return new GridLength(0, GridUnitType.Pixel);

                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
