using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI.Shell
{
	/// <summary>
	/// Interaction logic for MoxWorkspaceView.xaml
	/// </summary>
	public partial class MoxWorkspaceView : UserControl
	{
        public static readonly DependencyProperty BottomViewVisibilityProperty = DependencyProperty.Register("BottomViewVisibility", typeof(Visibility), typeof(MoxWorkspaceView), new FrameworkPropertyMetadata(Visibility.Visible, (PropertyChangedCallback)OnBottomViewVisibilityChanged));

        public Visibility BottomViewVisibility
        {
            get { return (Visibility)GetValue(BottomViewVisibilityProperty); }
            set { SetValue(BottomViewVisibilityProperty, value); }
        }

        private static void OnBottomViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MoxWorkspaceView)d).OnBottomViewVisibilityChanged((Visibility)e.NewValue);
        }

	    private GridLength m_lastBottomRowHeight = new GridLength(250, GridUnitType.Pixel);

        private void OnBottomViewVisibilityChanged(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                case Visibility.Hidden:
                    bottomRow.MinHeight = 150;
                    bottomRow.Height = m_lastBottomRowHeight;
                    break;

                case System.Windows.Visibility.Collapsed:
                    m_lastBottomRowHeight = bottomRow.Height;
                    bottomRow.MinHeight = 0;
                    bottomRow.Height = new GridLength(0, GridUnitType.Pixel);
                    break;
            }
        }

		public MoxWorkspaceView()
		{
			this.InitializeComponent();
		}
	}
}