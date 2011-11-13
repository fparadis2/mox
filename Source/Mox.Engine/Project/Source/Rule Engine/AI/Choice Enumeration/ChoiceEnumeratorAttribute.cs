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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mox.AI
{
    /// <summary>
    /// Maps a choice to a choice enumerator
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ChoiceEnumeratorAttribute : Attribute
    {
        #region Variables

        private readonly System.Type m_type;

        #endregion

        #region Constructor

        public ChoiceEnumeratorAttribute(System.Type type)
        {
            Throw.IfNull(type, "type");
            m_type = type;
        }

        #endregion

        #region Properties

        public System.Type Type
        {
            get { return m_type; }
        }

        #endregion
    }
}
