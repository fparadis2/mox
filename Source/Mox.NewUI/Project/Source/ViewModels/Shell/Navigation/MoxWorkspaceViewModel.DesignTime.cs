using System;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mox.UI.Shell
{
    public class MoxWorkspaceViewModel_DesignTime : MoxWorkspaceViewModel
    {
        #region Constructor

        public MoxWorkspaceViewModel_DesignTime()
        {
            Push(new DesignTimeViewModel());
        }

        #endregion

        #region Methods

        protected override object TransformWorkspaceValue(object oldValue, object newValue)
        {
            return newValue;
        }

        #endregion

        #region Inner Types

        private class DesignTimeViewModel : INavigationViewModel<MoxWorkspace>
        {
            #region Implementation of INavigationViewModel<in MoxWorkspace>

            public void Fill(MoxWorkspace view)
            {
                view.LeftView = new Rectangle { Fill = Brushes.Red };
                view.CenterView = new Rectangle { Fill = Brushes.Yellow };
                view.RightView = new Rectangle { Fill = Brushes.Green, Width=200 };
                view.BottomView = new Rectangle { Fill = Brushes.CornflowerBlue };
                view.CommandView = new Rectangle { Fill = Brushes.Indigo };
            }

            #endregion
        }

        #endregion
    }
}