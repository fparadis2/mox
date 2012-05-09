using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mox.Database;
using Mox.UI.ImageGenerator;

namespace Mox.TestCardRendering.Source
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();

            var database = MasterCardDatabase.Instance;
            var set = database.Sets["10E"];

            const double CardWidth = 312;
            const double CardHeight = 445;

            foreach (var card in set.CardInstances.Take(20))
            {
                Image image = new Image
                {
                    Margin = new Thickness(4),
                    Width = CardWidth,
                    Height = CardHeight,
                    Stretch = Stretch.Uniform,
                    Source = CardFrameGenerator.RenderFrame(card),
                    SnapsToDevicePixels = true
                };

                panel.Children.Add(image);
            }
        }
    }
}
