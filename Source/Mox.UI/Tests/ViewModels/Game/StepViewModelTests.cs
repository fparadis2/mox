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
            Assert.AreEqual("Block", new StepViewModel(Steps.DeclareBlockers).Name);
        }

        [Test]
        public void Test_Can_get_Name_after_setting_phase()
        {
            Assert.AreEqual("Main", new StepViewModel(Phases.PrecombatMain).Name);
        }

        [Test]
        public void Test_Nothing_happens_when_setting_a_CurrentPhase_with_no_associated_step()
        {
            Mox.Assert.Throws<ArgumentException>(() => new StepViewModel(Phases.Beginning));
        }

        #endregion
    }
}
