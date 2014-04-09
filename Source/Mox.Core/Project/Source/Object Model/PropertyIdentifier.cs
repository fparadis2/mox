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
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Mox
{
    [Serializable]
    public class PropertyIdentifier : ISerializable
    {
        #region Variables

        private readonly PropertyBase m_property;

        #endregion

        #region Constructor

        public PropertyIdentifier(PropertyBase property)
        {
            Debug.Assert(property != null);
            m_property = property;
        }

        #endregion

        #region Properties

        public PropertyBase Property
        {
            get { return m_property; }
        }

        #endregion

        #region Serialization

        private const string OwnerType = "Type";
        private const string PropertyName = "Name";

        protected PropertyIdentifier(SerializationInfo info, StreamingContext context)
        {
            string propertyName = info.GetString(PropertyName);
            Type ownerType = (Type)info.GetValue(OwnerType, typeof(Type));

            // Make sure static ctors have run
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(ownerType.TypeHandle);

            m_property = PropertyBase.GetProperty(ownerType, propertyName);
            Throw.IfNull(m_property, "Invalid property");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(PropertyName, Property.Name);
            info.AddValue(OwnerType, Property.OwnerType);
        }

        #endregion
    }
}
