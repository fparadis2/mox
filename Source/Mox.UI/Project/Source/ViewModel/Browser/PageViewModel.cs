using System;
using System.Windows.Input;

namespace Mox.UI.Browser
{
    public class PageViewModel : ViewModel
    {
        #region Properties

        public virtual string Title
        {
            get { return "Browser"; }
        }

        #endregion

        #region Navigation

        public ICommand GoBackCommand
        {
            get { return new RelayCommand(o => CanGoBack, o => GoBack()); }
        }

        public virtual bool CanGoBack
        {
            get
            {
                return GameFlow.CanGoBack;
            }
        }

        public virtual string GoBackText
        {
            get { return "Back"; }
        }

        public virtual void GoBack()
        {
            GameFlow.GoBack();
        }

        public ICommand GoForwardCommand
        {
            get { return new RelayCommand(o => CanGoForward, o => GoForward()); }
        }

        public virtual bool CanGoForward
        {
            get
            {
                return false;
            }
        }

        public virtual string GoForwardText
        {
            get { return "Next"; }
        }

        public virtual void GoForward()
        {
        }

        #endregion
    }
}
