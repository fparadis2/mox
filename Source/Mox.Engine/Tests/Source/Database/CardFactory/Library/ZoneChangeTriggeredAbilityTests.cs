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

namespace Mox.Database.Library
{
    [TestFixture]
    public class ZoneChangeTriggeredAbilityTests : BaseFactoryTests
    {
        #region Mock Types

        private class MyAbility : ZoneChangeTriggeredAbility
        {
            #region Inner Types

            public enum AbilityState
            {
                None,
                Triggered,
                PushedOnStack,
                Resolved
            }

            #endregion

            #region Properties

            public Zone MyTriggerZone
            {
                get;
                set;
            }

            protected override Zone.Id TriggerZone
            {
                get
                {
                    return MyTriggerZone.ZoneId;
                }
            }

            public AbilityState State
            {
                get;
                private set;
            }

            public new Card Target
            {
                get;
                private set;
            }

            public Zone ChangeZone
            {
                get;
                set;
            }

            public bool TriggerOnAllCards
            {
                get;
                set;
            }

            #endregion

            #region Methods

            public void Assert_State_Is(AbilityState state, Card target)
            {
                Assert.AreEqual(state, State);
                Assert.AreEqual(target, Target);
            }

            public override bool CanPushOnStack(Game game, object zoneChangeContext)
            {
                bool result = base.CanPushOnStack(game, zoneChangeContext);
                if (result)
                {
                    Assert.AreEqual(AbilityState.Triggered, State, "Sanity check");
                    Assert.AreEqual(((ZoneChangeContext) zoneChangeContext).Card.Resolve(game), Target, "Sanity check");
                    State = AbilityState.PushedOnStack;
                }
                return result;
            }

            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                if (State == AbilityState.None || State == AbilityState.Resolved)
                {
                    State = AbilityState.Triggered;
                    Target = card.Resolve(spell.Game);
                }

                spell.Effect = s =>
                {
                    Assert.AreEqual(AbilityState.PushedOnStack, State, "Sanity check");
                    Assert.AreEqual(card.Resolve(s.Game), Target, "Sanity check");
                    State = AbilityState.Resolved;
                };
            }

            public override void Push(Spell spell)
            {
                if (ChangeZone != null)
                {
                    ((ZoneChangeContext)spell.Context).Card.Resolve(spell.Game).Zone = ChangeZone;
                }

                base.Push(spell);
            }

            protected override bool IsValidCard(Card card)
            {
                if (TriggerOnAllCards)
                {
                    return true;
                }

                return base.IsValidCard(card);
            }

            protected override bool CanTriggerWhenSourceIsNotVisible
            {
                get
                {
                    return !TriggerOnAllCards;
                }
            }

            #endregion
        }

        #endregion

        #region Variables

        private MyAbility m_comesIntoPlayAbility;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Zone = m_game.Zones.Hand;
            m_comesIntoPlayAbility = m_game.CreateAbility<MyAbility>(m_card);
            m_comesIntoPlayAbility.MyTriggerZone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Ability_doesnt_trigger_normally()
        {
            m_card.Zone = m_game.Zones.Graveyard;
            m_card.Zone = m_game.Zones.Library;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.None, null);
        }

        [Test]
        public void Test_Ability_triggers_when_playing_it()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.Resolved, m_card);
        }

        [Test]
        public void Test_Ability_is_not_pushed_if_card_changes_zone_before_ability_is_pushed()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Zone = m_game.Zones.Hand;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.Triggered, m_card);
        }

        [Test]
        public void Test_Ability_wont_do_anything_if_card_has_changed_zone_between_trigger_and_resolve()
        {
            m_comesIntoPlayAbility.ChangeZone = m_game.Zones.Graveyard;

            m_card.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.PushedOnStack, m_card);
        }

        [Test]
        public void Test_Ability_doesnt_trigger_for_other_cards()
        {
            Card otherCard = CreateCard(m_playerA);
            otherCard.Zone = m_game.Zones.Hand;
            otherCard.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.None, null);
        }

        [Test]
        public void Test_Ability_triggers_for_other_cards_if_needed()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.TriggerOnAllCards = true;

            Card otherCard = CreateCard(m_playerA);
            otherCard.Zone = m_game.Zones.Hand;
            otherCard.Zone = m_game.Zones.Battlefield;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.Resolved, otherCard);
        }

        [Test]
        public void Test_Can_trigger_abilities_on_other_zones()
        {
            m_comesIntoPlayAbility.TriggerOnAllCards = true;
            m_comesIntoPlayAbility.MyTriggerZone = m_game.Zones.Exile;

            m_card.Zone = m_game.Zones.Battlefield;
            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            Card otherCard = CreateCard(m_playerA);
            otherCard.Zone = m_game.Zones.Hand;
            otherCard.Zone = m_game.Zones.Exile;

            Expect_AllPlayersPass(m_playerA);
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);

            m_comesIntoPlayAbility.Assert_State_Is(MyAbility.AbilityState.Resolved, otherCard);
        }

        #endregion
    }
}
