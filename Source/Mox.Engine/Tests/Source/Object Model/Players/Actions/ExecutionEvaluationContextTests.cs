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

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class ExecutionEvaluationContextTests : BaseGameTests
    {
        #region Variables

        private ExecutionEvaluationContext m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal);
        }

        #endregion

        #region Utilities

        private void Assert_CanPlay(Predicate<EvaluationContextType> predicate)
        {
            foreach (EvaluationContextType type in Enum.GetValues(typeof(EvaluationContextType)))
            {
                var context = new ExecutionEvaluationContext(m_playerA, type);
                Assert.AreEqual(predicate(type), context.CanPlay(m_mockAbility));
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_user_mode()
        {
            Assert.IsFalse(m_context.UserMode);
            m_context.UserMode = true;
            Assert.IsTrue(m_context.UserMode);
        }

        [Test]
        public void Test_Can_get_set_AbilityContext()
        {
            object abilityContext = new object();

            Assert.IsNull(m_context.AbilityContext);
            m_context.AbilityContext = abilityContext;
            Assert.AreEqual(abilityContext, m_context.AbilityContext);
        }

        [Test]
        public void Test_Can_only_play_Normal_in_Normal_context()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.MockedIsManaAbility = false;

            Assert_CanPlay(type => type == EvaluationContextType.Normal);
        }

        [Test]
        public void Test_Can_play_Mana_in_both_normal_and_mana_payment_context()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.MockedIsManaAbility = true;

            Assert_CanPlay(type => type == EvaluationContextType.Normal || type == EvaluationContextType.ManaPayment);
        }

        [Test]
        public void Test_Can_only_play_Attack_in_Attack_context()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Attack;
            
            m_mockAbility.MockedIsManaAbility = false;
            Assert_CanPlay(type => type == EvaluationContextType.Attack);

            m_mockAbility.MockedIsManaAbility = true;
            Assert_CanPlay(type => type == EvaluationContextType.Attack);
        }

        [Test]
        public void Test_Can_only_play_Block_in_Block_context()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Block;

            m_mockAbility.MockedIsManaAbility = false;
            Assert_CanPlay(type => type == EvaluationContextType.Block);

            m_mockAbility.MockedIsManaAbility = true;
            Assert_CanPlay(type => type == EvaluationContextType.Block);
        }

        [Test]
        public void Test_Can_only_play_Triggered_in_Triggered_context()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Triggered;

            m_mockAbility.MockedIsManaAbility = false;
            Assert_CanPlay(type => type == EvaluationContextType.Triggered);

            m_mockAbility.MockedIsManaAbility = true;
            Assert_CanPlay(type => type == EvaluationContextType.Triggered);
        }

        [Test]
        public void Test_Can_never_play_Static_abilities()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Static;

            m_mockAbility.MockedIsManaAbility = false;
            Assert_CanPlay(type => false);

            m_mockAbility.MockedIsManaAbility = true;
            Assert_CanPlay(type => false);
        }

        #endregion
    }
}
