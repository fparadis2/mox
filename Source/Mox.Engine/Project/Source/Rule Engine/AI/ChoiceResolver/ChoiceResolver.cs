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
using Mox.Flow;

namespace Mox.AI
{
    /// <summary>
    /// Provides meta-information about a user choice (for example, the possible answers).
    /// </summary>
    public abstract class ChoiceResolver
    {
        #region Properties

        public AIParameters Parameters
        {
            get;
            internal set;
        }

        public AISessionData SessionData
        {
            get;
            internal set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the part context for the given choice context.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract Part<TController>.Context GetContext<TController>(MethodBase method, object[] args);

        /// <summary>
        /// Replaces the context argument with the given one.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="context"></param>
        public abstract void SetContext<TController>(MethodBase method, object[] args, Part<TController>.Context context);

        /// <summary>
        /// Returns the player associated with the given method call.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract Player GetPlayer(MethodBase choiceMethod, object[] args);

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract IEnumerable<object> ResolveChoices(MethodBase choiceMethod, object[] args);

        /// <summary>
        /// Returns the default choice for the choice context.
        /// </summary>
        /// <remarks>
        /// The actual value is not so important, only that it returns a valid value.
        /// </remarks>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract object GetDefaultChoice(MethodBase choiceMethod, object[] args);

        #endregion
    }
}
