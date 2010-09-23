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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Mox.Flow;

namespace Mox.AI.Resolvers
{
    internal abstract class BaseMTGChoiceResolver : ChoiceResolver
    {
        #region Properties

        /// <summary>
        /// Expected method name, used for asserts...
        /// </summary>
        public abstract string ExpectedMethodName
        {
            get;
        }

        #endregion

        #region Methods

        [Conditional("DEBUG")]
        protected void ValidateMethod(MethodBase method)
        {
            Throw.IfNull(method, "method");
            Throw.InvalidOperationIf(method.Name != ExpectedMethodName, string.Format("Cannot use resolver {0} on method {1}", GetType().FullName, method.Name));
        }

        #endregion

        #region Implementation of ChoiceResolver

        public override Part<TController>.Context GetContext<TController>(MethodBase method, object[] args)
        {
            ValidateMethod(method);

            Part<TController>.Context context = (Part<TController>.Context)args[0];
#if DEBUG
            Player player = GetPlayer(method, args);
            Throw.InvalidProgramIf(context.Game != player.Manager, "Cross-game operation");
#endif

            return context;
        }

        /// <summary>
        /// Replaces the context argument with the given one.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="context"></param>
        public override void SetContext<TController>(MethodBase method, object[] args, Part<TController>.Context context)
        {
            ValidateMethod(method);
            args[0] = context;
            
            // Also set player
            args[1] = Resolvable<Player>.Resolve(context.Game, GetPlayer(method, args));
        }

        /// <summary>
        /// Returns the player associated with the given method call.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override Player GetPlayer(MethodBase choiceMethod, object[] args)
        {
            ValidateMethod(choiceMethod);
            return (Player)args[1];
        }

        #endregion
    }
}
