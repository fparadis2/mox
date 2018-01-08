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
using System.Linq;

using Mox.Abilities;
using Mox.Flow;
using NUnit.Framework;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class Factory10EArtifactsTests : BaseFactoryTests
    {
        #region Utilities

        private Card CreateCreature(Player owner, int power, int toughness)
        {
            Card creature = CreateCard(owner);
            creature.Type = Type.Creature;
            creature.Power = power;
            creature.Toughness = toughness;
            creature.Zone = m_game.Zones.Battlefield;
            return creature;
        }

        #endregion

        #region Tests

        #region Angel's feather

        private class Empty_Activated_Ability : InPlayAbility
        {
            public override void Play(Spell spell)
            {
            }
        }

        private PlayCardAbility CreateWhiteCreature(Player owner)
        {
            Card whiteCreature = InitializeCard("Wild Griffin", owner);
            Assert.AreEqual(Color.White, whiteCreature.Color, "Sanity check");
            whiteCreature.Zone = m_game.Zones.Hand;
            owner.Manager.CreateAbility<FlashAbility>(whiteCreature);
            PlayCardAbility ability = GetPlayCardAbility(whiteCreature);
            ability.ManaCost = ManaCost.Empty;
            return ability;
        }

        [Test]
        public void Test_When_in_hand_Angels_feather_does_nothing()
        {
            Card card = InitializeCard("Angel's Feather");
            card.Zone = m_game.Zones.Hand;

            PlayCardAbility whiteAbility = CreateWhiteCreature(m_playerA);

            Play(m_playerA, whiteAbility);
            HandleTriggeredAbilities(m_playerA);
            Assert.AreEqual(1, m_game.SpellStack.Count());
        }

        [Test]
        public void Test_When_in_play_Angels_feather_triggers_on_white_spells()
        {
            Card card = InitializeCard("Angel's Feather");
            card.Zone = m_game.Zones.Battlefield;

            PlayCardAbility whiteAbility = CreateWhiteCreature(m_playerA);

            Play(m_playerA, whiteAbility);
            HandleTriggeredAbilities(m_playerA);
            Assert.AreEqual(2, m_game.SpellStack.Count());
            Assert.AreNotEqual(whiteAbility, m_game.SpellStack.First().Ability);

            Expect_AllPlayersPass();

            Expect_AskModalChoice(m_playerA, ModalChoiceContext.YesNo("Gain 1 life?", ModalChoiceResult.Yes, ModalChoiceImportance.Trivial), ModalChoiceResult.Yes);

            int initialLife = m_playerA.Life;
            Expect_AllPlayersPass();
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            Assert.AreEqual(initialLife + 1, m_playerA.Life);
        }

        [Test]
        public void Test_Angels_feather_triggers_even_when_opponent_plays_white_spell()
        {
            Card card = InitializeCard("Angel's Feather");
            card.Zone = m_game.Zones.Battlefield;

            PlayCardAbility whiteAbility = CreateWhiteCreature(m_playerB);

            Play(m_playerB, whiteAbility);
            HandleTriggeredAbilities(m_playerA);
            Assert.AreEqual(2, m_game.SpellStack.Count());

            Expect_AllPlayersPass();

            Expect_AskModalChoice(m_playerA, ModalChoiceContext.YesNo("Gain 1 life?", ModalChoiceResult.Yes, ModalChoiceImportance.Trivial), ModalChoiceResult.No);

            int initialLife = m_playerA.Life;
            Expect_AllPlayersPass();
            PlayUntilAllPlayersPassAndTheStackIsEmpty(m_playerA);
            Assert.AreEqual(initialLife, m_playerA.Life);
        }

        [Test]
        public void Test_Angels_feather_doesnt_trigger_on_other_spells()
        {
            Card card = InitializeCard("Angel's Feather");
            card.Zone = m_game.Zones.Battlefield;

            PlayCardAbility redAbility = CreateWhiteCreature(m_playerB);
            redAbility.Source.Color = Color.Red;

            Play(m_playerB, redAbility);
            HandleTriggeredAbilities(m_playerA);
            Assert.AreEqual(1, m_game.SpellStack.Count());
        }

        [Test]
        public void Test_Angels_feather_doesnt_trigger_on_activated_abilities()
        {
            Card card = InitializeCard("Angel's Feather");
            card.Zone = m_game.Zones.Battlefield;

            Card myCreature = CreateCard(m_playerA);
            myCreature.Color = Color.White;
            myCreature.Zone = m_game.Zones.Battlefield;

            Ability ability = m_game.CreateAbility <Empty_Activated_Ability>(myCreature);

            Play(m_playerB, ability);
            HandleTriggeredAbilities(m_playerA);
            Assert.AreEqual(1, m_game.SpellStack.Count());
        }

        #endregion

        #region Leonin Scimitar

        [Test]
        public void Test_Leonin_Scimitar()
        {
            Card creature1 = CreateCreature(m_playerB, 1, 1);
            Card creature2 = CreateCreature(m_playerA, 2, 3);

            Card card = InitializeCard("Leonin Scimitar");
            card.Zone = m_game.Zones.Battlefield;

            EquipAbility equipAbility = card.Abilities.OfType<EquipAbility>().Single();

            // -- Can play a first time
            Assert.IsTrue(CanPlay(m_playerA, equipAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), creature1);
                Expect_PayManaCost(m_playerA, "1");
            }
            PlayAndResolve(m_playerA, equipAbility);

            Assert.AreEqual(creature1, card.AttachedTo);
            Assert.AreEqual(2, creature1.Power);
            Assert.AreEqual(2, creature1.Toughness);
            Assert.AreEqual(2, creature2.Power);
            Assert.AreEqual(3, creature2.Toughness);

            // -- Can play a second time
            Assert.IsTrue(CanPlay(m_playerA, equipAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), creature2);
                Expect_PayManaCost(m_playerA, "1");
            }
            PlayAndResolve(m_playerA, equipAbility);

            Assert.AreEqual(creature2, card.AttachedTo);
            Assert.AreEqual(1, creature1.Power);
            Assert.AreEqual(1, creature1.Toughness);
            Assert.AreEqual(3, creature2.Power);
            Assert.AreEqual(4, creature2.Toughness);
        }

        #endregion

        #region Phyrexian Vault

        [Test]
        public void Test_Phyrexian_Vault()
        {
            Card artifact = InitializeCard("Phyrexian Vault");
            artifact.Zone = m_game.Zones.Battlefield;

            Card sacrificed = CreateCard(m_playerA);
            sacrificed.Type = Type.Creature;
            sacrificed.Toughness = 1;
            sacrificed.Zone = m_game.Zones.Battlefield;

            InPlayAbility tapAbility = artifact.Abilities.OfType<InPlayAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, tapAbility));
            using (m_mockery.Ordered())
            {
                Expect_Target(m_playerA, TargetCost.Creature(), sacrificed);
                Expect_PayManaCost(m_playerA, "2");
            }
            PlayAndResolve(m_playerA, tapAbility);

            Assert.That(artifact.Tapped);
            Assert.AreEqual(m_game.Zones.Graveyard, sacrificed.Zone);
        }

        #endregion

        #endregion
    }
}
