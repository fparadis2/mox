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
#warning remove
    public partial class CardFrameControl : UserControl
    {
        public CardFrameControl()
        {
            DataContext = this;

            InitializeComponent();
        }

        public static readonly DependencyProperty CardProperty = DependencyProperty.Register("Card", typeof (CardInstanceInfo), typeof (CardFrameControl), new PropertyMetadata(default(CardInstanceInfo), OnCardChanged));

        private static void OnCardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CardFrameControl)d).UpdateContents();
        }

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

        private void UpdateContents()
        {
            Content = CardFrameGenerator.Generate(Card);
        }
    }
}
