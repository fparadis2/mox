using System;

using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class TargetDataTests : BaseGameTests
    {
        #region Variables

        private TargetData m_targetData;
        private TargetCost m_target;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_targetData = m_game.TargetData;

            m_target = TargetCost.Player();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Cannot_get_a_target_result_before_its_been_set()
        {
            Assert.Throws<InvalidOperationException>(() => m_targetData.GetTargetResult(m_target));
        }

        [Test]
        public void Test_Can_get_a_target_result_after_its_been_set()
        {
            Resolvable<ITargetable> result = m_playerA;

            Assert.IsUndoRedoable(m_game.Controller, 
                () => Assert.Throws<InvalidOperationException>(() => m_targetData.GetTargetResult(m_target)),
                () => m_targetData.SetTargetResult(m_target, result),
                () => Assert.AreEqual(result, m_targetData.GetTargetResult(m_target)));
        }

        [Test]
        public void Test_Clear_removes_all_target_results()
        {
            Resolvable<ITargetable> result = m_playerA;
            m_targetData.SetTargetResult(m_target, result);

            Assert.IsUndoRedoable(m_game.Controller,
                () => Assert.AreEqual(result, m_targetData.GetTargetResult(m_target)),
                () => m_targetData.Clear(),
                () => Assert.Throws<InvalidOperationException>(() => m_targetData.GetTargetResult(m_target)));
        }

        #endregion
    }
}
