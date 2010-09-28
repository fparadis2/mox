// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
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
                return GameFlow.Instance.CanGoBack;
            }
        }

        public virtual string GoBackText
        {
            get { return "Back"; }
        }

        public virtual void GoBack()
        {
            GameFlow.Instance.GoBack();
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
