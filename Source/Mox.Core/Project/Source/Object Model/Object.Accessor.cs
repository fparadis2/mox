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
using PropertyKey = System.String;
using ObjectIdentifier = System.Int32;

namespace Mox
{
    /// <summary>
    /// Base object, allows easy serialization, transfer, transactions and replication of data.
    /// </summary>
    partial class Object
    {
        #region Inner Types

        /// <summary>
        /// Object used to access properties of an object.
        /// </summary>
        public class Accessor
        {
            #region Variables

            private readonly Object m_object;

            #endregion

            #region Constructor

            public Accessor(Object obj)
            {
                Throw.IfNull(obj, "obj");
                m_object = obj;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Gets all the properties associated with the object.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<PropertyBase> GetProperties()
            {
                foreach (Type type in GetBaseTypes(m_object.GetType()))
                {
                    foreach (var propertyBase in PropertyBase.AllProperties.GetPropertiesByName(type))
                    {
                        yield return propertyBase;
                    }
                }
            }

            private static IEnumerable<Type> GetBaseTypes(Type startingType)
            {
                Type type = startingType;
                while (type != typeof(object))
                {
                    yield return type;
                    type = type.BaseType;
                }
            }

            /// <summary>
            /// Gets the value of the given <paramref name="property"/> for the object.
            /// </summary>
            /// <param name="property"></param>
            /// <returns></returns>
            public object GetValue(PropertyBase property)
            {
                return m_object.GetValueInternal(property);
            }

            #endregion
        }

        #endregion
    }
}
