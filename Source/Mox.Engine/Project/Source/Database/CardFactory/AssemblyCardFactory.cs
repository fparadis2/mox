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

namespace Mox
{
    /// <summary>
    /// A card factory that uses the <see cref="CardFactoryAttribute"/> to discover card factories.
    /// </summary>
    public class AssemblyCardFactory : CompoundCardFactory
    {
        #region Constructor

        public AssemblyCardFactory(Assembly assembly)
            : this(assembly.GetTypes())
        {
        }

        public AssemblyCardFactory(IEnumerable<System.Type> types)
        {
            foreach (System.Type type in types)
            {
                CardFactoryAttribute[] attributes = (CardFactoryAttribute[])type.GetCustomAttributes(typeof(CardFactoryAttribute), false);

                if (attributes.Length > 0)
                {
                    ICardFactory instance = (ICardFactory)Activator.CreateInstance(type);

                    foreach (CardFactoryAttribute attribute in attributes)
                    {
                        Register(attribute.CardName, instance);
                    }
                }
            }
        }

        #endregion
    }
}
