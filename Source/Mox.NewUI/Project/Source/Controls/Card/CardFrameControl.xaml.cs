using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mox.AI;
using Mox.Database;
using Mox.UI.Browser;
using Mox.UI.ImageGenerator;

namespace Mox.UI
{
    public partial class CardFrameControl : UserControl
    {
        public CardFrameControl()
        {
            Width = CardFrameGenerator.Width;
            Height = CardFrameGenerator.Height;

            DataContext = this;

            InitializeComponent();
        }

        public static readonly DependencyProperty CardProperty = DependencyProperty.Register("Card", typeof (CardInstanceInfo), typeof (CardFrameControl), new PropertyMetadata(default(CardInstanceInfo)));

        public CardInstanceInfo Card
        {
            get { return (CardInstanceInfo) GetValue(CardProperty); }
            set { SetValue(CardProperty, value); }
        }

        public CardIdentifier CardIdentifier
        {
            get { return Card; }
            set { Card = MasterCardDatabase.Instance.GetCardInstance(value); }
        }
    }

    public class CardToBackgroundImageConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            CardInstanceInfo cardInstanceInfo = value as CardInstanceInfo;
            if (cardInstanceInfo != null)
            {
                return CardFrameGenerator.RenderFrame(cardInstanceInfo);
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
