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
    /// A <see cref="IChoiceEnumeratorProvider"/> that uses a <see cref="ChoiceEnumeratorAttribute"/> to find the correct resolver.
    /// </summary>
    internal sealed class AttributedChoiceEnumeratorProvider : CachedChoiceEnumeratorProvider
    {
        #region Overrides of CachedChoiceResolverProvider

        public AttributedChoiceEnumeratorProvider(AIParameters parameters)
            : base(parameters)
        {
        }

        private AttributedChoiceEnumeratorProvider(AttributedChoiceEnumeratorProvider other)
            : base(other)
        {
        }

        public override IChoiceEnumeratorProvider Clone()
        {
            return new AttributedChoiceEnumeratorProvider(this);
        }

        /// <summary>
        /// Gets the resolver corresponding to the given <paramref name="choiceType"/>.
        /// </summary>
        protected override ChoiceEnumerator GetResolverImpl(System.Type choiceType)
        {
            ChoiceEnumeratorAttribute[] attributes = (ChoiceEnumeratorAttribute[])choiceType.GetCustomAttributes(typeof(ChoiceEnumeratorAttribute), false);

            if (attributes == null || attributes.Length != 1)
            {
                throw new ArgumentException(string.Format("Choice {0} is not decorated with a ChoiceEnumeratorAttribute", choiceType.Name), "choiceType");
            }

            System.Type choiceResolverType = attributes[0].Type;

            if (!typeof(ChoiceEnumerator).IsAssignableFrom(choiceResolverType))
            {
                throw new ArgumentException(string.Format("Type {0} is not assignable to ChoiceEnumerator type on choice {1}.", choiceResolverType.FullName, choiceType.Name), "choiceType");
            }

            return (ChoiceEnumerator)Activator.CreateInstance(choiceResolverType);
        }

        #endregion
    }
}
