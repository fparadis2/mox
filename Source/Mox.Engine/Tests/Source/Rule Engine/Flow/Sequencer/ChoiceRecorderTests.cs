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

namespace Mox.Flow
{
    [TestFixture]
    public class ChoiceRecorderTests
    {
        #region Variables

        private ChoiceRecorder m_choiceRecorder;

        private const string m_choice1 = "1";
        private const string m_choice2 = "2";

        #endregion

        #region Constructor

        [SetUp]
        public void Setup()
        {
            m_choiceRecorder = new ChoiceRecorder();
        }

        #endregion

        #region Utilities

        private void Assert_Replay(object expected)
        {
            Assert_Replay(m_choiceRecorder, expected);
        }

        private static void Assert_Replay(ChoiceRecorder recorder, object expected)
        {
            object actual;
            Assert.That(recorder.TryReplay(out actual));
            Assert.AreEqual(expected, actual);
        }

        private void Assert_NoReplay()
        {
            Assert_NoReplay(m_choiceRecorder);
        }

        private static void Assert_NoReplay(ChoiceRecorder recorder)
        {
            object actual;
            Assert.IsFalse(recorder.TryReplay(out actual));
        }

        private void Clone()
        {
            m_choiceRecorder = m_choiceRecorder.Clone();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_What_is_recorded_cannot_be_replayed_on_the_same_recorder()
        {
            m_choiceRecorder.Record(m_choice1);
            m_choiceRecorder.Record(m_choice2);

            Assert_NoReplay();
        }

        [Test]
        public void Test_Can_record_choices_and_replay_them_on_a_clone()
        {
            m_choiceRecorder.Record(m_choice1);

            Clone();

            Assert_Replay(m_choice1);
        }

        [Test]
        public void Test_Can_record_multiple_choices()
        {
            m_choiceRecorder.Record(m_choice1);
            m_choiceRecorder.Record(m_choice2);

            Clone();

            Assert_Replay(m_choice1);
            Assert_Replay(m_choice2);
            Assert_NoReplay();
        }

        [Test]
        public void Test_Can_only_replay_recorded_choices()
        {
            m_choiceRecorder.Record(m_choice1);

            Clone();

            Assert_Replay(m_choice1);
            Assert_NoReplay();
        }

        [Test]
        public void Test_Cannot_record_before_replaying()
        {
            m_choiceRecorder.Record(m_choice1);

            Clone();

            Assert.Throws<InvalidOperationException>(() => m_choiceRecorder.Record(m_choice2));
        }

        [Test]
        public void Test_Can_continue_recording_on_clone()
        {
            m_choiceRecorder.Record(m_choice1);

            Clone();

            Assert_Replay(m_choice1);
            m_choiceRecorder.Record(m_choice2);

            Clone();

            Assert_Replay(m_choice1);
            Assert_Replay(m_choice2);
            Assert_NoReplay();
        }

        [Test]
        public void Test_Can_continue_recording_on_clone_without_affecting_original()
        {
            m_choiceRecorder.Record(m_choice1);

            Clone();
            ChoiceRecorder clone = m_choiceRecorder.Clone();

            Assert_Replay(m_choice1);
            Assert_NoReplay();

            Assert_Replay(clone, m_choice1);
            Assert_NoReplay(clone);
        }

        [Test]
        public void Test_Can_continue_recording_on_original_without_affecting_clone()
        {
            m_choiceRecorder.Record(m_choice1);

            ChoiceRecorder clone = m_choiceRecorder.Clone();

            m_choiceRecorder.Record(m_choice2);

            Assert_Replay(clone, m_choice1);
            Assert_NoReplay(clone);

            Clone();

            Assert_Replay(m_choice1);
            Assert_Replay(m_choice2);
        }

        [Test]
        public void Test_Recorded_choices_after_cloning_are_not_kept_alive_by_original_recorder()
        {
            object obj = new object();
            WeakReference reference = new WeakReference(obj);

            {
                ChoiceRecorder clone = m_choiceRecorder.Clone();
                clone.Record(obj);
                obj = null;
                clone = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(reference.IsAlive);
        }

        #endregion
    }
}
