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
using System.Collections.Generic;
using Rhino.Mocks;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class DefaultTurnFactoryTests : BaseGameTests
    {
        #region Variables

        private Turn m_turn;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_turn = new DefaultTurnFactory().CreateTurn();
        }

        #endregion

        #region Utilities

        private static void Assert_Is_Beginning_Phase(Phase phase)
        {
            Assert.AreEqual(Mox.Phases.Beginning, phase.Type);

            Assert.AreEqual(3, phase.Steps.Count);

            Step untapStep = phase.Steps[0];
            Assert.AreEqual(Steps.Untap, untapStep.Type);
            Assert.IsInstanceOf<UntapStep>(untapStep);

            Step upkeepStep = phase.Steps[1];
            Assert.AreEqual(Steps.Upkeep, upkeepStep.Type);
            Assert.IsInstanceOf<UpkeepStep>(upkeepStep);

            Step drawStep = phase.Steps[2];
            Assert.AreEqual(Steps.Draw, drawStep.Type);
            Assert.IsInstanceOf<DrawStep>(drawStep);
        }

        private static void Assert_Is_Main_Phase(Phase phase, bool precombat)
        {
            Assert.AreEqual(precombat ? Mox.Phases.PrecombatMain : Mox.Phases.PostcombatMain, phase.Type);

            Assert.Collections.IsEmpty(phase.Steps);
        }

        private static void Assert_Is_Combat_Phase(Phase phase)
        {
            Assert.AreEqual(Mox.Phases.Combat, phase.Type);

            Assert.AreEqual(5, phase.Steps.Count);

            Step beginningOfCombatStep = phase.Steps[0];
            Assert.AreEqual(Steps.BeginningOfCombat, beginningOfCombatStep.Type);
            Assert.IsInstanceOf<BeginningOfCombatStep>(beginningOfCombatStep);

            Step declareAttackersStep = phase.Steps[1];
            Assert.AreEqual(Steps.DeclareAttackers, declareAttackersStep.Type);
            Assert.IsInstanceOf<DeclareAttackersStep>(declareAttackersStep);

            Step declareBlockersStep = phase.Steps[2];
            Assert.AreEqual(Steps.DeclareBlockers, declareBlockersStep.Type);
            Assert.IsInstanceOf<DeclareBlockersStep>(declareBlockersStep);

            Step combatDamageStep = phase.Steps[3];
            Assert.AreEqual(Steps.CombatDamage, combatDamageStep.Type);
            Assert.IsInstanceOf<CombatDamageStep>(combatDamageStep);

            Step endOfCombatStep = phase.Steps[4];
            Assert.AreEqual(Steps.EndOfCombat, endOfCombatStep.Type);
            Assert.IsInstanceOf<EndOfCombatStep>(endOfCombatStep);
        }

        private static void Assert_Is_End_Phase(Phase phase)
        {
            Assert.AreEqual(Mox.Phases.End, phase.Type);

            Assert.AreEqual(2, phase.Steps.Count);

            Step endOfTurnStep = phase.Steps[0];
            Assert.AreEqual(Steps.End, endOfTurnStep.Type);
            Assert.IsInstanceOf<EndOfTurnStep>(endOfTurnStep);

            Step cleanupStep = phase.Steps[1];
            Assert.AreEqual(Steps.Cleanup, cleanupStep.Type);
            Assert.IsInstanceOf<CleanupStep>(cleanupStep);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Contains_correct_phases()
        {
            Assert.IsNotNull(m_turn);

            Assert.AreEqual(5, m_turn.Phases.Count);

            Assert_Is_Beginning_Phase(m_turn.Phases[0]);
            Assert_Is_Main_Phase(m_turn.Phases[1], true);
            Assert_Is_Combat_Phase(m_turn.Phases[2]);
            Assert_Is_Main_Phase(m_turn.Phases[3], false);
            Assert_Is_End_Phase(m_turn.Phases[4]);

        }

        #endregion
    }
}
