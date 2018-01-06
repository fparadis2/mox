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
using System.IO;
using System.Linq;
using Mox.Transactions;
using NUnit.Framework;
using Rhino.Mocks;

using Mox.Abilities;
using Mox.Flow.Parts;

namespace Mox.Replication
{
    [TestFixture]
    public class ReplicationTests : BaseGameTests
    {
        #region Inner Types

        private class ReplicationTester : MarshalByRefObject
        {
            private readonly BaseGameTests m_baseTester = new BaseGameTests();
            private readonly ReplicationSource<Player> m_source;

            private Game Game
            {
                get { return m_baseTester.m_game; }
            }

            public ReplicationTester()
            {
                m_baseTester.Setup();
                m_source = new ReplicationSource<Player>(Game, new MTGAccessControlStrategy(Game));
            }

            public void Register(Resolvable<Player> player, IReplicationClient client)
            {
                m_source.Register(player.Resolve(Game), client);
            }

            public Resolvable<Card> CreateCard(Resolvable<Player> player)
            {
                return CreateCard(player.Resolve(Game), "B");
            }

            public void Put_card_in_player_hand()
            {
                Game.Cards.First().Zone = Game.Zones.Hand;
            }

            private Card CreateCard(Player owner, string name)
            {
                Card card = Game.CreateCard(owner, new CardIdentifier { Card = name });
                card.Zone = Game.Zones.Hand;
                return card;
            }

            public IEnumerable<Resolvable<Card>> Put_cards_in_player_hand_in_specific_order(Resolvable<Player> playerHandle)
            {
                Player player = playerHandle.Resolve(Game);

                Card card1 = CreateCard(player, "Card1");
                Card card2 = CreateCard(player, "Card2");
                Card card3 = CreateCard(player, "Card3");
                player.Hand.MoveToBottom(new[] {card3});

                var cards = new[] { card3, card1, card2 };

                Assert.Collections.AreEqual(cards, player.Hand); // Sanity check

                object token = new object();
                Game.Controller.BeginTransaction(token);
                {
                    card1.Zone = Game.Zones.PhasedOut;
                }
                Game.Controller.EndTransaction(true, token);
                
                Assert.Collections.AreEqual(cards, player.Hand); // Sanity check
                return cards.Select(c => (Resolvable<Card>)c).ToList();
            }

            public void Put_card_in_play(Resolvable<Card> card)
            {
                card.Resolve(Game).Zone = Game.Zones.Battlefield;
            }

            public void Create_Ability()
            {
                CreateVisibleAbility();
            }

            private Ability CreateVisibleAbility()
            {
                Game.Cards.First().Zone = Game.Zones.Battlefield;
                return Game.CreateAbility<PlayCardAbility>(Game.Cards.First());
            }

            public void AddRedManaTo(Resolvable<Player> player, byte mana)
            {
                player.Resolve(Game).ManaPool.Red += mana;
            }

            public void ChangePlayerLifeInANonAtomicTransaction(int life, bool rollback)
            {
                object token = new object();

                Game.Controller.BeginTransaction(token);
                Game.Players[0].Life = life;
                Game.Controller.EndTransaction(rollback, token);
            }

            public void PushSpell(Resolvable<Player> player)
            {
                Player resolvedPlayer = player.Resolve(Game);

                Game.Cards.First().Zone = Game.Zones.Battlefield;
                var ability = Game.CreateAbility<GainLifeAbility>(Game.Cards.First());

                Game.SpellStack.Push(new Spell(ability, resolvedPlayer));
            }

            public void AddPlusOnePlusOneEffect(Resolvable<Card> card, int initialPW)
            {
                Card resolvedCard = card.Resolve(Game);

                resolvedCard.Power = resolvedCard.Toughness = initialPW;
                Game.CreateLocalEffect(resolvedCard, new PlusOnePlusOneEffect());
            }

            public void AddPlusOnePlusOneGlobalEffect()
            {
                AddEffect.OnCards(Game, Condition.True).ModifyPowerAndToughness(+1, +1).Forever();
            }

            public void RemoveAllEffects()
            {
                foreach (EffectInstance effectInstance in Game.Objects.Where(obj => obj is EffectInstance).ToList())
                {
                    effectInstance.Remove();
                }
            }

            #region Inner Types

            [Serializable]
            private class PlusOnePlusOneEffect : Effect<PowerAndToughness>
            {
                public PlusOnePlusOneEffect()
                    : base(Card.PowerAndToughnessProperty)
                {
                }

                public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
                {
                    value.Power += 1;
                    value.Toughness += 1;
                    return value;
                }
            }

            private class GainLifeAbility : InPlayAbility
            {
                public override void Play(Spell spell)
                {
                    spell.Effect = s =>
                    {
                        s.Controller.GainLife(44);
                    };
                }
            }

            #endregion
        }

        #endregion

        #region Variables

        private AppDomain m_testDomain;

        private ReplicationClient<Game> m_client;
        private ReplicationTester m_tester;

        private Game m_synchronizedGame;
        private Player m_synchronizedPlayerA;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_client = new ReplicationClient<Game>();

            SetupListenerWithRealGame();
        }

        public override void Teardown()
        {
            base.Teardown();

            TestDomain = null;
        }

        #endregion

        #region Utilities

        private AppDomain TestDomain
        {
            get { return m_testDomain; }
            set 
            {
                if (m_testDomain != null)
                {
                    AppDomain.Unload(m_testDomain);
                }
                m_testDomain = value;
            }
        }

        private void SetupListenerWithRealGame()
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(typeof(ReplicationTester).Assembly.CodeBase).Substring(6);
            TestDomain = AppDomain.CreateDomain("ReplicationTests Domain", null, setup);

            m_tester = (ReplicationTester)TestDomain.CreateInstanceAndUnwrap(typeof(ReplicationTester).Assembly.FullName, typeof(ReplicationTester).FullName);

            m_tester.Register(m_playerA, m_client);
            m_synchronizedGame = m_client.Host;
            m_synchronizedPlayerA = m_synchronizedGame.Players[0];
        }

        #endregion

        #region Functional Tests

        [Test]
        public void Test_the_game_is_synchronized_at_start()
        {
            Assert.AreEqual(2, m_synchronizedGame.Players.Count);
            Assert.AreEqual(1, m_synchronizedGame.Cards.Count);

            Assert_Objects_have_same_identifier(m_playerA, m_synchronizedGame.Players[0]);
            Assert.AreEqual(m_playerA.Name, m_synchronizedGame.Players[0].Name);

            Assert_Objects_have_same_identifier(m_playerB, m_synchronizedGame.Players[1]);
            Assert.AreEqual(m_playerB.Name, m_synchronizedGame.Players[1].Name);

            Assert_Objects_have_same_identifier(m_card, m_synchronizedGame.Cards.First());
            Assert.AreEqual(m_synchronizedGame.Players[0], m_synchronizedGame.Cards.First().Owner);
        }

        [Test]
        public void Test_Adding_cards_is_synchronized()
        {
            Resolvable<Card> createdCard = m_tester.CreateCard(m_playerA);

            Assert.AreEqual(2, m_synchronizedGame.Cards.Count);
            Card synchronizedCard = m_synchronizedGame.Cards.Skip(1).First();

            Assert.IsNotNull(synchronizedCard);
            Assert.AreEqual(synchronizedCard, createdCard.Resolve(m_synchronizedGame));
            Assert.AreEqual(m_synchronizedPlayerA, synchronizedCard.Owner);
        }

        [Test]
        public void Test_Zones_are_correctly_synchronized()
        {
            m_tester.Put_card_in_player_hand();

            Assert.AreEqual(1, m_synchronizedGame.Cards.Count);

            Assert.AreEqual(m_synchronizedGame.Zones.Hand, m_synchronizedGame.Cards.First().Zone);
        }

        [Test]
        public void Test_Position_in_zones_is_correctly_synchronized()
        {
            var cardHandles = m_tester.Put_cards_in_player_hand_in_specific_order(m_synchronizedPlayerA);

            IList<Card> cards = cardHandles.Select(c => c.Resolve(m_synchronizedGame)).ToList();

            Assert.Collections.AreEqual(cards, m_synchronizedPlayerA.Hand);
        }

        [Test]
        public void Test_Polymorphic_abilities_are_correctly_replicated()
        {
            m_tester.Create_Ability();

            Assert.AreEqual(2, m_synchronizedGame.Abilities.Count);
            Assert.IsInstanceOf<PlayCardAbility>(m_synchronizedGame.Abilities.Last());
            Assert.AreEqual(m_synchronizedGame.Cards.First(), m_synchronizedGame.Abilities.Last().Source);
        }

        [Test]
        public void Test_ManaPool_is_correctly_replicated()
        {
            m_tester.AddRedManaTo(m_playerA, 3);
            Assert.AreEqual(3, m_synchronizedPlayerA.ManaPool.Red);
        }

        [Test]
        public void Test_NonAtomic_transactions_are_correctly_replicated()
        {
            m_tester.ChangePlayerLifeInANonAtomicTransaction(99, false);
            Assert.AreEqual(99, m_synchronizedPlayerA.Life);
        }

        [Test]
        public void Test_Rollbacking_NonAtomic_transactions_is_correctly_replicated()
        {
            m_tester.ChangePlayerLifeInANonAtomicTransaction(99, true);
            Assert.AreNotEqual(99, m_synchronizedPlayerA.Life);
        }

        [Test]
        public void Test_Delayed_synchronization_works_with_identifier()
        {
            Resolvable<Card> createdCard = m_tester.CreateCard(m_playerB);

            Assert.AreEqual(2, m_synchronizedGame.Cards.Count);
            Card synchronizedCard = m_synchronizedGame.Cards.Skip(1).First();

            Assert.IsNotNull(synchronizedCard);
            Assert.AreEqual(synchronizedCard, createdCard.Resolve(m_synchronizedGame));
            Assert.IsNull(synchronizedCard.Name);

            m_tester.Put_card_in_play(createdCard);

            Assert.AreEqual("B", synchronizedCard.Name);
        }

        [Test]
        public void Test_Spell_stack_is_correctly_replicated()
        {
            m_tester.PushSpell(m_playerA);

            Assert.IsFalse(m_synchronizedGame.SpellStack.IsEmpty);
            Spell topSpell = m_synchronizedGame.SpellStack.Peek();

            Assert.AreEqual(m_synchronizedGame, topSpell.Game);
            Assert.AreEqual(m_synchronizedPlayerA, topSpell.Controller);
            Assert.AreEqual(m_synchronizedGame.Cards.First(), topSpell.Source);
            Assert.AreEqual(m_synchronizedGame.Cards.First().Abilities.Last(), topSpell.Ability);

            // Can resolve the spell
            m_playerA.Life = 20;

            MockRepository mockery = new MockRepository();
            NewSequencerTester tester = new NewSequencerTester(mockery, m_game);
            tester.Run(new ResolveSpell(topSpell));

            Assert.AreEqual(64, m_playerA.Life);
        }

        [Test]
        public void Test_Effects_are_correctly_synchronized()
        {
            Resolvable<Card> createdCard = m_tester.CreateCard(m_playerA);
            m_tester.AddPlusOnePlusOneEffect(createdCard, 3);

            Card synchronizedCard = createdCard.Resolve(m_synchronizedGame);
            Assert.AreEqual(4, synchronizedCard.Power);
            Assert.AreEqual(4, synchronizedCard.Toughness);

            m_tester.RemoveAllEffects();

            Assert.AreEqual(3, synchronizedCard.Power);
            Assert.AreEqual(3, synchronizedCard.Toughness);
        }

        [Test]
        public void Test_Global_Effects_are_correctly_synchronized()
        {
            Resolvable<Card> createdCard1 = m_tester.CreateCard(m_playerA);
            Resolvable<Card> createdCard2 = m_tester.CreateCard(m_playerA);

            m_tester.Put_card_in_play(createdCard1);
            m_tester.Put_card_in_play(createdCard2);

            m_tester.AddPlusOnePlusOneGlobalEffect();

            Card synchronizedCard1 = createdCard1.Resolve(m_synchronizedGame);
            Card synchronizedCard2 = createdCard2.Resolve(m_synchronizedGame);

            Assert.AreEqual(1, synchronizedCard1.Power);
            Assert.AreEqual(1, synchronizedCard2.Power);

            m_tester.RemoveAllEffects();

            Assert.AreEqual(0, synchronizedCard1.Power);
            Assert.AreEqual(0, synchronizedCard2.Power);
        }

        [Test, Ignore("This kind of case should assert. You can't synchronize on a modified game")]
        public void Test_Identifiers_are_globally_unique_1()
        {
            Card card1 = m_synchronizedGame.CreateCard(m_synchronizedPlayerA, new CardIdentifier { Card = "My Card" });
            Card card2 = m_tester.CreateCard(m_playerA).Resolve(m_synchronizedGame);

            Assert.AreNotSame(card1, card2);
            Assert.AreNotEqual(card1.Identifier, card2.Identifier);
        }

        [Test]
        public void Test_Identifiers_are_globally_unique_2()
        {
            Card card1 = m_tester.CreateCard(m_playerA).Resolve(m_synchronizedGame);

            using (m_synchronizedGame.UpgradeController(new ObjectController(m_synchronizedGame)))
            {
                Card card2 = m_synchronizedGame.CreateCard(m_synchronizedPlayerA, new CardIdentifier { Card = "My Card" });

                Assert.AreNotSame(card1, card2);
                Assert.AreNotEqual(card1.Identifier, card2.Identifier);
            }
        }

        #endregion
    }
}
