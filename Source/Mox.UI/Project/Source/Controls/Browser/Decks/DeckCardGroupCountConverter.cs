using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Mox.UI.Browser
{
    public class DeckCardGroupCountConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            var viewGroup = value as CollectionViewGroup;

            if (viewGroup != null)
            {
                return viewGroup.Items.OfType<DeckCardViewModel>().Sum(card => card.Quantity);
            }

            return 0;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
