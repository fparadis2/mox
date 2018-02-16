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

using Mox.Abilities;
using Mox.Database;
using Mox.Flow;
using Mox.Flow.Parts;

namespace Mox
{
    public class BaseFactoryTests : BaseGameTests
    {
        #region Variables

        private IDisposable m_summoningSicknessBypass;
        protected NewSequencerTester m_sequencerTester;

        #endregion

        #region Setup

        public override void Setup()
        {
            m_summoningSicknessBypass = Rules.SummoningSickness.Bypass();

            base.Setup();

            m_sequencerTester = new NewSequencerTester(m_mockery, m_game);
            m_sequencerTester.MockPlayerChoices(m_playerA);

            m_game.State.CurrentPhase = Phases.PrecombatMain;
            m_game.State.ActivePlayer = m_playerA;

            Assert.AreEqual(20, m_playerA.Life, "Sanity check");
            Assert.AreEqual(20, m_playerB.Life, "Sanity check");
        }

        public override void Teardown()
        {
            base.Teardown();

            DisposableHelper.SafeDispose(m_summoningSicknessBypass);
        }

        #endregion

        #region Utilities

        protected Card InitializeCard(string cardName, Player owner = null)
        {
            Card card = CreateCard(owner ?? m_playerA, cardName);

            var result = MasterCardDatabase.Instance.Factory.InitializeCard(card);
            Assert.AreEqual(CardFactoryResult.ResultType.Success, result.Type, result.Error);

            card.Zone = m_game.Zones.Hand;
            return card;
        }

        protected PlayCardAbility GetPlayCardAbility(Card card)
        {
            return card.Abilities.OfType<PlayCardAbility>().First();
        }

        #region Assertions

        protected static void Assert_PT(Card card, int power, int tougness)
        {
            Assert.AreEqual(power, card.Power);
            Assert.AreEqual(tougness, card.Toughness);
        }

        #endregion

        #region Setup

        protected Card CreateCreatureOnBattlefield(int power, int toughness)
        {
            return CreateCreatureOnBattlefield(m_playerA, power, toughness);
        }

        protected Card CreateCreatureOnBattlefield(Player player, int power, int toughness)
        {
            Card card = CreateCard(player);
            card.Type = Type.Creature;
            card.Power = power;
            card.Toughness = toughness;
            card.Zone = m_game.Zones.Battlefield;
            return card;
        }

        #endregion

        #region Sequencing

        protected void PlayUntilAllPlayersPassAndTheStackIsEmpty(Player player)
        {
            m_sequencerTester.Run(new PlayUntilAllPlayersPassAndTheStackIsEmpty(player));
        }

        protected void Play(Player player, SpellAbility ability)
        {
            m_sequencerTester.Run(new Mox.Flow.Parts.PlayAbility(player, ability, null));
        }

        protected void HandleTriggeredAbilities(Player player)
        {
            m_sequencerTester.Run(new Mox.Flow.Parts.HandleTriggeredAbilities(player));
        }

        protected void PlayAndResolve(Player player, SpellAbility ability)
        {
            Assert.That(ability.UseStack);
            m_sequencerTester.RunWithoutMock(new Mox.Flow.Parts.PlayAbility(player, ability, null));
            m_sequencerTester.RunWithoutMock(new Mox.Flow.Parts.ResolveTopSpell());
        }

        protected bool CanPlay(Player player, Ability ability)
        {
            var context = new AbilityEvaluationContext(player, AbilityEvaluationContextType.Normal);
            return ability.CanPlay(context);
        }

        #endregion

        #region Expectations

        protected void Expect_AllPlayersPass(Player startingPlayer)
        {
            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                if (m_sequencerTester.IsMocked(player))
                {
                    m_sequencerTester.Expect_Player_GivePriority(player, null).RepeatAny();
                }
            }
        }

        protected void Expect_AllPlayersPass()
        {
            m_sequencerTester.Expect_All_Players_Pass();
        }

        protected void Expect_Target(Player controller, Filter filter, GameObject target, TargetContextType type = TargetContextType.Normal)
        {
            List<GameObject> targetables = new List<GameObject>();
            filter.EnumerateObjects(controller.Manager, controller, targetables);
            m_sequencerTester.Expect_Player_Target(controller, true, targetables, target, type);
        }

        protected void Expect_Target(Player controller, IEnumerable<GameObject> targetables, GameObject target, TargetContextType type = TargetContextType.Normal)
        {
            m_sequencerTester.Expect_Player_Target(controller, true, targetables, target, type);
        }

        protected void Expect_Target(Player controller, GameObject target, TargetContextType type = TargetContextType.Normal)
        {
            Expect_Target(controller, (IEnumerable<GameObject>)null, target, type);
        }

        protected void Expect_PayManaCost(Player controller, string manaCost)
        {
            m_sequencerTester.Expect_Player_PayDummyMana(controller, ManaCost.Parse(manaCost));
        }

        protected void Expect_AskModalChoice(Player controller, ModalChoiceContext context, ModalChoiceResult result)
        {
            m_sequencerTester.Expect_Player_AskModalChoice(controller, context, result);
        }

        protected void Expect_GainManaChoice(Player controller, int result, params ManaAmount[] amounts)
        {
            m_sequencerTester.Expect_Player_GainManaChoice(controller, result, amounts);
        }

        #endregion

        #endregion
    }
}
