﻿// Copyright (c) François Paradis
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
using System.ComponentModel;
using System.Linq.Expressions;

namespace Mox.UI
{
    public class Assert : Mox.Assert
    {
        #region Property Validation

        public static SinglePropertyAssertion<T> ThatProperty<T, K>(T propertyOwner, Expression<Func<T, K>> property)
            where T : class, INotifyPropertyChanged
        {
            var assertion = new SinglePropertyAssertion<T>(propertyOwner);
            assertion.AndProperty(property);
            return assertion;
        }

        public static AllPropertiesAssertion<T> ThatAllPropertiesOn<T>(T propertyOwner)
            where T : class, INotifyPropertyChanged
        {
            return new AllPropertiesAssertion<T>(propertyOwner);
        }

        #endregion
    }
}
