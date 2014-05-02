using System;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class StepViewModelTests
    {
        #region Variables

        private StepViewModel m_stepViewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_stepViewModel = new StepViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_Name_after_setting_step()
        {
            m_stepViewModel.CurrentStep = Steps.DeclareBlockers;
            Assert.AreEqual("Declare Blockers", m_stepViewModel.Name);
        }

        [Test]
        public void Test_Can_get_Name_after_setting_phase()
        {
            m_stepViewModel.CurrentPhase = Phases.PrecombatMain;
            Assert.AreEqual("Precombat Main", m_stepViewModel.Name);
        }

        [Test]
        public void Test_Nothing_happens_when_setting_a_CurrentPhase_with_no_associated_step()
        {
            m_stepViewModel.CurrentPhase = Phases.PostcombatMain;
            m_stepViewModel.CurrentPhase = Phases.Beginning;
            Assert.AreEqual("Postcombat Main", m_stepViewModel.Name);
        }

        [Test]
        public void Test_Equality()
        {
            m_stepViewModel = new StepViewModel { CurrentPhase = Phases.PrecombatMain };

            Assert.AreCompletelyEqual(m_stepViewModel, new StepViewModel { CurrentPhase = Phases.PrecombatMain }, false);
            Assert.AreCompletelyNotEqual(m_stepViewModel, new StepViewModel { CurrentPhase = Phases.PostcombatMain }, false);
            Assert.AreCompletelyNotEqual(m_stepViewModel, new StepViewModel { CurrentStep = Steps.DeclareAttackers }, false);

            m_stepViewModel = new StepViewModel { CurrentStep = Steps.CombatDamage };

            Assert.AreCompletelyEqual(m_stepViewModel, new StepViewModel { CurrentStep = Steps.CombatDamage }, false);
            Assert.AreCompletelyNotEqual(m_stepViewModel, new StepViewModel { CurrentStep = Steps.DeclareAttackers }, false);
            Assert.AreCompletelyNotEqual(m_stepViewModel, new StepViewModel { CurrentPhase = Phases.PostcombatMain }, false);

            m_stepViewModel = new StepViewModel { CurrentStep = Steps.Untap };
            Assert.AreCompletelyNotEqual(m_stepViewModel, new StepViewModel { CurrentPhase = Phases.PostcombatMain }, false);
        }

        #endregion
    }
}
