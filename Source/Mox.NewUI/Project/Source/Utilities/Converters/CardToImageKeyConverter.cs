using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Mox.Database;

namespace Mox.UI
{
#warning remove
    public class CardToCardFrameImageKeyConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            CardInstanceInfo cardInstanceInfo = value as CardInstanceInfo;
            if (cardInstanceInfo != null)
            {
                return ImageKey.ForCardFrameImage(cardInstanceInfo);
            }

            Debug.Assert(value == null);
            return null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
