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

namespace Mox
{
    /// <summary>
    /// Base class for effect conditions.
    /// </summary>
    [Serializable]
    public abstract class Condition : IHashable
    {
        #region Variables

        private static readonly Condition m_true = new TrueCondition();

        #endregion

        #region Methods

        public abstract bool Matches(Card card);

        protected internal virtual bool Invalidate(PropertyBase property)
        {
            return false;
        }

        public virtual void ComputeHash(Hash hash)
        {
#warning Should not have to hash conditions... only hash the modifications incurred by the effects
            hash.Add(GetType().MetadataToken);
        }

        #endregion

        #region Operators

        public static Condition operator &(Condition a, Condition b)
        {
            return new AndCondition(a, b);
        }

        #endregion

        #region Static conditions

        /// <summary>
        /// Always true.
        /// </summary>
        public static Condition True
        {
            get { return m_true; }
        }

        /// <summary>
        /// True for cards that are controlled by the same controller as the given card.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static Condition ControlledBySameController(Card card)
        {
            return new ControlledBySameControllerCondition(card);
        }
        
        /// <summary>
        /// True for cards of that type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Condition Is(Type type)
        {
            return new IsTypeCondition(type);
        }

        /// <summary>
        /// True for cards of that color.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="colorProperty"></param>
        /// <returns></returns>
        public static Condition IsSameColorThan(Object obj, Property<Color> colorProperty)
        {
            return new IsSameColorThanCondition(obj, colorProperty);
        }

        #endregion

        #region Inner Types

        [Serializable]
        private class TrueCondition : Condition
        {
            public override bool Matches(Card card)
            {
                return true;
            }
        }

        [Serializable]
        private class ControlledBySameControllerCondition : Condition
        {
            private readonly Resolvable<Card> m_card;

            public ControlledBySameControllerCondition(Card card)
            {
                m_card = card;
            }

            public override bool Matches(Card card)
            {
                return m_card.Resolve(card.Manager).Controller == card.Controller;
            }

            public override void ComputeHash(Hash hash)
            {
                base.ComputeHash(hash);

                hash.Add(m_card.Identifier);
            }

            protected internal override bool Invalidate(PropertyBase property)
            {
                return property == Card.ControllerProperty || base.Invalidate(property);
            }
        }

        [Serializable]
        private class IsTypeCondition : Condition
        {
            private readonly Type m_type;

            public IsTypeCondition(Type type)
            {
                m_type = type;
            }

            public override bool Matches(Card card)
            {
                return card.Is(m_type);
            }

            public override void ComputeHash(Hash hash)
            {
                base.ComputeHash(hash);

                hash.Add((int)m_type);
            }

            protected internal override bool Invalidate(PropertyBase property)
            {
                return property == Card.TypeProperty || base.Invalidate(property);
            }
        }

        [Serializable]
        private class IsSameColorThanCondition : Condition
        {
            private readonly Resolvable<Object> m_object;
            private readonly PropertyIdentifier m_colorProperty;

            public IsSameColorThanCondition(Object obj, Property<Color> colorProperty)
            {
                m_object = obj;
                m_colorProperty = new PropertyIdentifier(colorProperty);
            }

            public override bool Matches(Card card)
            {
                Property<Color> property = (Property<Color>)m_colorProperty.Property;
                Color color = m_object.Resolve(card.Manager).GetValue(property);
                return card.Is(color);
            }

            public override void ComputeHash(Hash hash)
            {
                base.ComputeHash(hash);

                hash.Add(m_object.Identifier);
                hash.Add(m_colorProperty.Property.Name);
            }

            protected internal override bool Invalidate(PropertyBase property)
            {
                return property == Card.ColorProperty || property == m_colorProperty.Property || base.Invalidate(property);
            }
        }

        #endregion
    }
}
