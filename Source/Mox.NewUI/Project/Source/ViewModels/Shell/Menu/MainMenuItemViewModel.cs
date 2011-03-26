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

using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MainMenuItemViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly System.Action m_clickAction;
        
        private string m_text;
        private bool m_selected;

        #endregion

        #region Constructor

        public MainMenuItemViewModel(System.Action clickAction)
        {
            m_clickAction = clickAction;
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return m_text; }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

        public bool IsSelected
        {
            get { return m_selected; }
            internal set
            {
                if (m_selected != value)
                {
                    m_selected = value;
                    NotifyOfPropertyChange(() => IsSelected);
                }
            }
        }

        #endregion

        #region Methods

        public void Activate()
        {
            if (m_clickAction != null)
            {
                m_clickAction();
            }
        }

        #endregion
    }
}
