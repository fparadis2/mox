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

namespace Mox.Database
{
    /// <summary>
    /// Uses the <see cref="CardFactoryAttribute"/> to discover card factories.
    /// </summary>
    public class AssemblyCardFactory
    {
        #region Variables

        private readonly Dictionary<string, System.Type> m_types = new Dictionary<string, System.Type>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor

        public AssemblyCardFactory(Assembly assembly)
            : this(assembly.GetTypes())
        {
        }

        public AssemblyCardFactory(IEnumerable<System.Type> types)
        {
            foreach (System.Type type in types)
            {
                if (!type.IsAbstract && typeof(CardFactory).IsAssignableFrom(type))
                {
                    CardFactoryAttribute[] attributes = (CardFactoryAttribute[])type.GetCustomAttributes(typeof(CardFactoryAttribute), false);

                    foreach (CardFactoryAttribute attribute in attributes)
                    {
                        m_types.Add(attribute.CardName, type);
                    }
                }
            }
        }

        #endregion

        #region Properties

        public int Count => m_types.Count;

        #endregion

        #region Methods

        public ICardFactory CreateFactory(string name, CardInfo cardInfo)
        {
            if (m_types.TryGetValue(name, out System.Type type))
            {
                CardFactory factory = (CardFactory)Activator.CreateInstance(type);
                factory.CardInfo = cardInfo;
                factory.Build();
                return factory;
            }

            return null;
        }

        public bool IsDefined(string name)
        {
            return m_types.ContainsKey(name);
        }

        #endregion
    }
}
