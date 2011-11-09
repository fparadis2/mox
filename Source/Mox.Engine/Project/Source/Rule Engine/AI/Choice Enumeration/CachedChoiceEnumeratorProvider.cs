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

using Mox.Flow;

namespace Mox.AI
{
    /// <summary>
    /// A <see cref="IChoiceEnumeratorProvider"/> that caches its results.
    /// </summary>
    internal abstract class CachedChoiceEnumeratorProvider : IChoiceEnumeratorProvider
    {
        #region Variables

        private readonly AIParameters m_parameters;
        private readonly AISessionData m_sessionData;
        private readonly Dictionary<System.Type, ChoiceEnumerator> m_enumerators = new Dictionary<System.Type, ChoiceEnumerator>();

        #endregion

        #region Constructor

        protected CachedChoiceEnumeratorProvider(AIParameters parameters)
        {
            Throw.IfNull(parameters, "parameters");

            m_parameters = parameters;
            m_sessionData = AISessionData.Create();
        }

        protected CachedChoiceEnumeratorProvider(CachedChoiceEnumeratorProvider other)
            : this(other.m_parameters)
        {
            // Session data is not cloned (this is on purpose, each provider gets its own session data)
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the resolver corresponding to the given <paramref name="choice"/>.
        /// </summary>
        public ChoiceEnumerator GetEnumerator(Choice choice)
        {
            Throw.IfNull(choice, "choice");

            var choiceType = choice.GetType();

            ChoiceEnumerator resolver;
            if (!m_enumerators.TryGetValue(choiceType, out resolver))
            {
                resolver = GetResolverImpl(choiceType);

                resolver.Parameters = m_parameters;
                resolver.SessionData = m_sessionData;

                m_enumerators.Add(choiceType, resolver);
            }
            Debug.Assert(resolver != null);
            return resolver;
        }

        public abstract IChoiceEnumeratorProvider Clone();

        /// <summary>
        /// Gets the resolver corresponding to the given <paramref name="choiceType"/>.
        /// </summary>
        protected abstract ChoiceEnumerator GetResolverImpl(System.Type choiceType);

        #endregion
    }
}
