using System;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class StepViewModelTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_Name_after_setting_step()
        {
            Assert.AreEqual("Declare Blockers", new StepViewModel(Steps.DeclareBlockers).Name);
        }

        [Test]
        public void Test_Can_get_Name_after_setting_phase()
        {
            Assert.AreEqual("Precombat Main", new StepViewModel(Phases.PrecombatMain).Name);
        }

        [Test]
        public void Test_Nothing_happens_when_setting_a_CurrentPhase_with_no_associated_step()
        {
            Mox.Assert.Throws<ArgumentException>(() => new StepViewModel(Phases.Beginning));
        }

        [Test]
        public void Test_Equality()
        {
            var stepViewModel = new StepViewModel(Phases.PrecombatMain);

            Assert.AreCompletelyEqual(stepViewModel, new StepViewModel(Phases.PrecombatMain), false);
            Assert.AreCompletelyNotEqual(stepViewModel, new StepViewModel(Phases.PostcombatMain), false);
            Assert.AreCompletelyNotEqual(stepViewModel, new StepViewModel(Steps.DeclareAttackers), false);

            stepViewModel = new StepViewModel(Steps.CombatDamage);

            Assert.AreCompletelyEqual(stepViewModel, new StepViewModel(Steps.CombatDamage), false);
            Assert.AreCompletelyNotEqual(stepViewModel, new StepViewModel(Steps.DeclareAttackers), false);
            Assert.AreCompletelyNotEqual(stepViewModel, new StepViewModel(Phases.PostcombatMain), false);

            stepViewModel = new StepViewModel(Steps.Untap);
            Assert.AreCompletelyNotEqual(stepViewModel, new StepViewModel(Phases.PostcombatMain), false);
        }

        #endregion
    }
}
