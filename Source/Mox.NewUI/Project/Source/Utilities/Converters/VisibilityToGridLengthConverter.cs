using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mox.UI.Converters
{
    public class VisibilityToGridLengthConverter : IValueConverter, IMultiValueConverter
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
            return Convert((Visibility)value, null);
        }

        public object Convert(object[] values, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((Visibility)values[0], values[1] as TransitionPresenter);
        }

        private GridLength Convert(Visibility visibility, TransitionPresenter element)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                case Visibility.Hidden:
                    if (element != null && element.Content is IDefinedSizePartView)
                    {
                        return GridLength.Auto;
                    }

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

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
