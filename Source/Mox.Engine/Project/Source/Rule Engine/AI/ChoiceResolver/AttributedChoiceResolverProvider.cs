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

namespace Mox.AI
{
    /// <summary>
    /// A <see cref="IChoiceResolverProvider"/> that uses a <see cref="ChoiceResolverAttribute"/> to find the correct resolver.
    /// </summary>
    internal sealed class AttributedChoiceResolverProvider : CachedChoiceResolverProvider
    {
        #region Overrides of CachedChoiceResolverProvider

        public AttributedChoiceResolverProvider(AIParameters parameters)
            : base(parameters)
        {
        }

        private AttributedChoiceResolverProvider(AttributedChoiceResolverProvider other)
            : base(other)
        {
        }

        public override IChoiceResolverProvider Clone()
        {
            return new AttributedChoiceResolverProvider(this);
        }

        /// <summary>
        /// Gets the resolver corresponding to the given <paramref name="choiceMethod"/>.
        /// </summary>
        /// <param name="choiceMethod"></param>
        /// <returns></returns>
        protected override ChoiceResolver GetResolverImpl(MethodBase choiceMethod)
        {
            ChoiceResolverAttribute[] attributes = (ChoiceResolverAttribute[])choiceMethod.GetCustomAttributes(typeof(ChoiceResolverAttribute), false);

            if (attributes == null || attributes.Length != 1)
            {
                throw new ArgumentException(string.Format("Method {0} is not decorated with ChoiceResolverAttribute", choiceMethod.Name), "choiceMethod");
            }

            System.Type choiceResolverType = attributes[0].Type;

            if (!typeof(ChoiceResolver).IsAssignableFrom(choiceResolverType))
            {
                throw new ArgumentException(string.Format("Type {0} is not assignable to ChoiceResolver type on method {1}.", choiceResolverType.FullName, choiceMethod.Name), "choiceMethod");
            }

            return (ChoiceResolver)Activator.CreateInstance(choiceResolverType);
        }

        #endregion
    }
}
