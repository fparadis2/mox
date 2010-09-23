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

namespace Mox.Collections
{
    [TestFixture]
    public class ObservableCollectionTests
    {
        #region Variables

        protected ObservableCollection<int> m_mutableCollection;
        protected IObservableCollection<int> m_testCollection;

        #endregion

        #region Setup

        [SetUp]
        public virtual void Setup()
        {
            m_testCollection = m_mutableCollection = new ObservableCollection<int>();

            m_mutableCollection.Add(1);
            m_mutableCollection.Add(2);
        }

        #endregion

        #region Utilities

        private void Assert_Triggers_CollectionChanged(Action<IObservableCollection<int>> action, CollectionChangeAction changeAction, params int[] expectedItems)
        {
            Test_CollectionChanged(action, sink =>
            {
                Assert.AreEqual(1, sink.TimesCalled);
                Assert.AreEqual(changeAction, sink.LastEventArgs.Action);
                Assert.Collections.AreEqual(expectedItems, sink.LastEventArgs.Items);
            });
        }

        private void Assert_Doesnt_Trigger_CollectionChanged(Action<IObservableCollection<int>> action)
        {
            Test_CollectionChanged(action, sink => Assert.AreEqual(0, sink.TimesCalled));
        }

        private void Test_CollectionChanged(Action<IObservableCollection<int>> action, Action<EventSink<CollectionChangedEventArgs<int>>> testAction)
        {
            EventSink<CollectionChangedEventArgs<int>> collectionChangedSink = new EventSink<CollectionChangedEventArgs<int>>(m_testCollection);
            m_testCollection.CollectionChanged += collectionChangedSink;

            action(m_mutableCollection);
            testAction(collectionChangedSink);

            m_testCollection.CollectionChanged -= collectionChangedSink;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Add()
        {
            Assert.IsFalse(m_testCollection.Contains(5));
            m_mutableCollection.Add(5);
            Assert.IsTrue(m_testCollection.Contains(5));
        }

        [Test]
        public void Test_Remove()
        {
            m_mutableCollection.Add(5);
            m_mutableCollection.Add(5);
            Assert.IsTrue(m_testCollection.Contains(5));

            Assert.IsTrue(m_mutableCollection.Remove(5));
            Assert.IsTrue(m_mutableCollection.Remove(5));
            Assert.IsFalse(m_mutableCollection.Remove(5));

            Assert.IsFalse(m_testCollection.Contains(5));
        }

        [Test]
        public void Test_Clear()
        {
            m_mutableCollection.Clear();
            Assert.Collections.IsEmpty(m_testCollection);
        }

        [Test]
        public void Test_GetEnumerator()
        {
            Assert.Collections.AreEqual(new [] { 1, 2 }, m_testCollection);
        }

        [Test]
        public void Test_Count_returns_the_number_of_items_in_the_collection()
        {
            Assert.AreEqual(2, m_testCollection.Count);
            m_mutableCollection.Add(99);
            Assert.AreEqual(3, m_testCollection.Count);
        }

        [Test]
        public void Test_CollectionChanged_is_triggered_when_adding_an_item()
        {
            Assert_Triggers_CollectionChanged(collection => collection.Add(1), CollectionChangeAction.Add, 1);
        }

        [Test]
        public void Test_CollectionChanged_is_triggered_when_removing_an_item()
        {
            Assert_Triggers_CollectionChanged(collection => collection.Remove(2), CollectionChangeAction.Remove, 2);
            Assert_Doesnt_Trigger_CollectionChanged(collection => collection.Remove(2));
        }

        [Test]
        public void Test_CollectionChanged_is_triggered_when_clearing_the_collection()
        {
            Assert_Triggers_CollectionChanged(collection => collection.Clear(), CollectionChangeAction.Clear, 1, 2);
        }

        [Test]
        public void Test_During_the_Clear_event_the_element_of_the_collection_are_already_cleared()
        {
            EventSink<CollectionChangedEventArgs<int>> collectionChangedSink = new EventSink<CollectionChangedEventArgs<int>>(m_testCollection);
            m_testCollection.CollectionChanged += collectionChangedSink;

            collectionChangedSink.Callback += ((sender, e) => Assert.AreEqual(m_testCollection.Count, 0));

            m_mutableCollection.Clear();
            Assert.AreEqual(1, collectionChangedSink.TimesCalled);

            m_testCollection.CollectionChanged -= collectionChangedSink;
        }

        #endregion
    }
}
