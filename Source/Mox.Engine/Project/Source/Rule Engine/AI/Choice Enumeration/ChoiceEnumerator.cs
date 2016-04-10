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
using Mox.Flow;

namespace Mox.AI
{
    /// <summary>
    /// Provides meta-information about a user choice (for example, the possible answers).
    /// </summary>
    public abstract class ChoiceEnumerator
    {
        #region Properties

        public AISessionData SessionData
        {
            get;
            internal set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public abstract IEnumerable<object> EnumerateChoices(Game game, Choice choice);

        /// <summary>
        /// Returns the default choice for the choice context.
        /// </summary>
        /// <remarks>
        /// The actual value is not so important, only that it returns a valid value.
        /// </remarks>
        public virtual object GetDefaultChoice(Choice choice)
        {
            return choice.DefaultValue;
        }

        #endregion
    }
}
