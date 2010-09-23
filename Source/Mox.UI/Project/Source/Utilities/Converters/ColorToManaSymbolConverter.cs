using System;
using System.Globalization;
using System.Windows.Data;

namespace Mox.UI
{
    public class ColorToManaSymbolConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color)
            {
                Color color = (Color) value;
                return ManaSymbolHelper.GetSymbol(color);
            }

            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}