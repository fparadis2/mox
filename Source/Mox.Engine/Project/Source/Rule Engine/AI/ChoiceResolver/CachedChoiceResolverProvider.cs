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

namespace Mox.AI
{
    /// <summary>
    /// A <see cref="IChoiceResolverProvider"/> that caches its results.
    /// </summary>
    internal abstract class CachedChoiceResolverProvider : IChoiceResolverProvider
    {
        #region Variables

        private readonly AIParameters m_parameters;
        private readonly AISessionData m_sessionData;
        private readonly Dictionary<MethodBase, ChoiceResolver> m_resolvers = new Dictionary<MethodBase, ChoiceResolver>();

        #endregion

        #region Constructor

        protected CachedChoiceResolverProvider(AIParameters parameters)
        {
            Throw.IfNull(parameters, "parameters");

            m_parameters = parameters;
            m_sessionData = AISessionData.Create();
        }

        protected CachedChoiceResolverProvider(CachedChoiceResolverProvider other)
            : this(other.m_parameters)
        {
            // Session data is not cloned (this is wanted, each provider gets its own session data)
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the resolver corresponding to the given <paramref name="choiceMethod"/>.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <returns></returns>
        public ChoiceResolver GetResolver(MethodBase choiceMethod)
        {
            Throw.IfNull(choiceMethod, "choiceMethod");

            ChoiceResolver resolver;
            if (!m_resolvers.TryGetValue(choiceMethod, out resolver))
            {
                resolver = GetResolverImpl(choiceMethod);

                resolver.Parameters = m_parameters;
                resolver.SessionData = m_sessionData;

                m_resolvers.Add(choiceMethod, resolver);
            }
            Debug.Assert(resolver != null);
            return resolver;
        }

        public abstract IChoiceResolverProvider Clone();

        /// <summary>
        /// Gets the resolver corresponding to the given <paramref name="choiceMethod"/>.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <returns></returns>
        protected abstract ChoiceResolver GetResolverImpl(MethodBase choiceMethod);

        #endregion
    }
}
