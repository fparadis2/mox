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

namespace Mox.UI.Game
{
    /// <summary>
    /// Type of choice.
    /// </summary>
    public enum UserChoiceType
    {
        None,
        Yes,
        No,
        Cancel,
    }

    public class UserChoiceModel : PropertyChangedBase
    {
        #region Variables

        private string m_text;
        private UserChoiceType m_type;

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

        public UserChoiceType Type
        {
            get { return m_type; }
            set
            {
                if (m_type != value)
                {
                    m_type = value;
                    NotifyOfPropertyChange(() => Type);
                }
            }
        }

        #endregion
    }
}
