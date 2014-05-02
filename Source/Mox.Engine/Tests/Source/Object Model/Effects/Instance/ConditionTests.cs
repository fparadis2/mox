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
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ConditionTests : BaseGameTests
    {
        #region Mock Types

        private class ObjectWithColorProperty : GameObject
        {
            private Color m_color;
            public static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<ObjectWithColorProperty>("Color", o => o.m_color);

            public Color Color
            {
                get { return m_color; }
                set { SetValue(ColorProperty, value, ref m_color); }
            }

            private Color m_otherColor;
            public static readonly Property<Color> OtherColorProperty = Property<Color>.RegisterProperty<ObjectWithColorProperty>("OtherColor", o => o.m_otherColor);

            public Color OtherColor
            {
                get { return m_otherColor; }
                set { SetValue(OtherColorProperty, value, ref m_otherColor); }
            }
        }

        #endregion

        #region Tests

        #region True

        [Test]
        public void Test_True_returns_a_condition_that_always_matches()
        {
            Assert.That(Condition.True.Matches(m_card));
            Assert.IsFalse(Condition.True.Invalidate(Card.ZoneIdProperty));
        }

        [Test]
        public void Test_True_always_returns_the_same_hash()
        {
            Assert.HashIsEqual(Condition.True, Condition.True);
        }

        #endregion

        #region ControlledBySameController

        [Test]
        public void Test_ControlledBySameController_returns_a_condition_that_matches_only_cards_controlled_by_given_player()
        {
            Card sourceCard = CreateCard(m_playerA);

            m_card.Zone = m_game.Zones.Battlefield;
            sourceCard.Zone = m_game.Zones.Battlefield;

            m_card.Controller = m_playerA;
            Assert.IsTrue(Condition.ControlledBySameController(sourceCard).Matches(m_card));

            m_card.Controller = m_playerB;
            Assert.IsFalse(Condition.ControlledBySameController(sourceCard).Matches(m_card));

            sourceCard.Controller = m_playerB;
            Assert.IsTrue(Condition.ControlledBySameController(sourceCard).Matches(m_card));
        }

        [Test]
        public void Test_ControlledBySameController_is_invalidated_by_ControllerProperty()
        {
            Assert.IsTrue(Condition.ControlledBySameController(m_card).Invalidate(Card.ControllerProperty));
            Assert.IsFalse(Condition.ControlledBySameController(m_card).Invalidate(Card.ColorProperty));
        }

        [Test]
        public void Test_ControlledBySameController_hash_depends_on_the_card()
        {
            Card sourceCard = CreateCard(m_playerA);

            Assert.HashIsEqual(Condition.ControlledBySameController(sourceCard), Condition.ControlledBySameController(sourceCard));
            Assert.HashIsNotEqual(Condition.ControlledBySameController(sourceCard), Condition.ControlledBySameController(m_card));
        }

        #endregion

        #region Is (Type)

        [Test]
        public void Test_Is_returns_a_condition_that_matches_only_cards_of_the_given_type()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            m_card.Type = Type.Creature | Type.Artifact;
            Assert.IsTrue(Condition.Is(Type.Creature).Matches(m_card));

            m_card.Type = Type.Artifact;
            Assert.IsFalse(Condition.Is(Type.Creature).Matches(m_card));
        }

        [Test]
        public void Test_Is_is_invalidated_by_TypeProperty()
        {
            Assert.IsTrue(Condition.Is(Type.Creature).Invalidate(Card.TypeProperty));
            Assert.IsFalse(Condition.Is(Type.Creature).Invalidate(Card.ColorProperty));
        }

        [Test]
        public void Test_Is_hash_depends_on_the_type()
        {
            Assert.HashIsEqual(Condition.Is(Type.Creature), Condition.Is(Type.Creature));
            Assert.HashIsNotEqual(Condition.Is(Type.Creature), Condition.Is(Type.Artifact));
        }

        #endregion

        #region IsSameColorThan

        [Test]
        public void Test_IsSameColorThan_returns_a_condition_that_matches_only_cards_of_the_same_color()
        {
            ObjectWithColorProperty source = m_game.Create<ObjectWithColorProperty>();
            m_game.Objects.Add(source);

            Condition condition = Condition.IsSameColorThan(source, ObjectWithColorProperty.ColorProperty);

            m_card.Color = Color.Blue;
            source.Color = Color.Blue;

            Assert.IsTrue(condition.Matches(m_card));

            m_card.Color = Color.Red | Color.Black;
            Assert.IsFalse(condition.Matches(m_card));

            source.Color = Color.Red;
            Assert.IsTrue(condition.Matches(m_card));
        }

        [Test]
        public void Test_IsSameColorThan_is_invalidated_by_ColorProperty_and_the_given_property()
        {
            ObjectWithColorProperty source = m_game.Create<ObjectWithColorProperty>();
            m_game.Objects.Add(source);

            Assert.IsTrue(Condition.IsSameColorThan(source, ObjectWithColorProperty.ColorProperty).Invalidate(Card.ColorProperty));
            Assert.IsTrue(Condition.IsSameColorThan(source, ObjectWithColorProperty.ColorProperty).Invalidate(ObjectWithColorProperty.ColorProperty));
            Assert.IsFalse(Condition.IsSameColorThan(source, ObjectWithColorProperty.ColorProperty).Invalidate(Card.TypeProperty));
        }

        [Test]
        public void Test_IsSameColorThan_hash_depends_on_the_source_and_the_property()
        {
            ObjectWithColorProperty source1 = m_game.Create<ObjectWithColorProperty>();
            ObjectWithColorProperty source2 = m_game.Create<ObjectWithColorProperty>();

            Assert.HashIsEqual(Condition.IsSameColorThan(source1, ObjectWithColorProperty.ColorProperty), Condition.IsSameColorThan(source1, ObjectWithColorProperty.ColorProperty));
            Assert.HashIsNotEqual(Condition.IsSameColorThan(source1, ObjectWithColorProperty.ColorProperty), Condition.IsSameColorThan(source2, ObjectWithColorProperty.ColorProperty));
            Assert.HashIsNotEqual(Condition.IsSameColorThan(source1, ObjectWithColorProperty.ColorProperty), Condition.IsSameColorThan(source1, ObjectWithColorProperty.OtherColorProperty));
        }

        #endregion

        #endregion
    }
}