using Mox.Abilities;
using Mox.Flow;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Database.Sets
{
    [TestFixture]
    public class FactoryArtifactTests : BaseFactoryTests
    {
        #region Angel's feather

        private PlayCardAbility CreateWhiteCreature(Player owner)
        {
            Card whiteCreature = CreateCard(owner);
            whiteCreature.Color = Color.White;
            whiteCreature.Zone = m_game.Zones.Hand;

            owner.Manager.CreateAbility<FlashAbility>(whiteCreature);
            return owner.Manager.CreateAbility<PlayCardAbility>(whiteCreature);
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

            ActivatedAbility ability = m_game.CreateAbility<ActivatedAbility>(myCreature);

            Play(m_playerB, ability);
            HandleTriggeredAbilities(m_playerA);
            Assert.AreEqual(1, m_game.SpellStack.Count());
        }

        #endregion

        #region Leonin Scimitar

        [Test]
        public void Test_Leonin_Scimitar()
        {
            Card creature1 = CreateCreatureOnBattlefield(m_playerA, 1, 1);
            Card creature2 = CreateCreatureOnBattlefield(m_playerA, 2, 3);

            Card card = InitializeCard("Leonin Scimitar");
            card.Zone = m_game.Zones.Battlefield;

            var equipAbility = card.Abilities.OfType<ActivatedAbility>().Single();

            // -- Can play a first time
            Assert.IsTrue(CanPlay(m_playerA, equipAbility));
            Expect_Target(m_playerA, creature1);
            Expect_PayManaCost(m_playerA, "1");
            PlayAndResolve(m_playerA, equipAbility);

            Assert.AreEqual(creature1, card.AttachedTo);
            Assert_PT(creature1, 2, 2);
            Assert_PT(creature2, 2, 3);

            // -- Can play a second time
            Assert.IsTrue(CanPlay(m_playerA, equipAbility));
            Expect_Target(m_playerA, creature2);
            Expect_PayManaCost(m_playerA, "1");
            PlayAndResolve(m_playerA, equipAbility);

            Assert.AreEqual(creature2, card.AttachedTo);
            Assert_PT(creature1, 1, 1);
            Assert_PT(creature2, 3, 4);
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

            ActivatedAbility tapAbility = artifact.Abilities.OfType<ActivatedAbility>().Single();

            Assert.IsTrue(CanPlay(m_playerA, tapAbility));
            Expect_Target(m_playerA, sacrificed, TargetContextType.SacrificeCost);
            Expect_PayManaCost(m_playerA, "2");
            PlayAndResolve(m_playerA, tapAbility);

            Assert.That(artifact.Tapped);
            Assert.AreEqual(m_game.Zones.Graveyard, sacrificed.Zone);
        }

        #endregion
    }
}
