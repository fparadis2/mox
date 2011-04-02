using System;
using Caliburn.Micro;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class NavigationConductorTests
    {
        #region Variables

        private MockRepository m_mockery;

        private IChild m_child;
        private NavigationConductor<object> m_conductor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_child = m_mockery.StrictMock<IChild>();

            m_conductor = new NavigationConductor<object>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNull(m_conductor.ActiveItem);
        }

        [Test]
        public void Test_Push_and_Pop_modify_the_ActiveItem()
        {
            object itemA = new object();
            Assert.ThatProperty(m_conductor, c => c.ActiveItem).RaisesChangeNotificationWhen(() => m_conductor.Push(itemA));
            {
                Assert.AreEqual(itemA, m_conductor.ActiveItem);

                object itemB = new object();
                Assert.ThatProperty(m_conductor, c => c.ActiveItem).RaisesChangeNotificationWhen(() => m_conductor.Push(itemB));
                {
                    Assert.AreEqual(itemB, m_conductor.ActiveItem);
                }
                Assert.ThatProperty(m_conductor, c => c.ActiveItem).RaisesChangeNotificationWhen(() => m_conductor.Pop());

                Assert.AreEqual(itemA, m_conductor.ActiveItem);
            }
            Assert.ThatProperty(m_conductor, c => c.ActiveItem).RaisesChangeNotificationWhen(() => m_conductor.Pop());
            Assert.IsNull(m_conductor.ActiveItem);
        }

        [Test]
        public void Test_Push_sets_Parent_on_IChild()
        {
            m_child.Parent = m_conductor;

            using (m_mockery.Test())
            {
                m_conductor.Push(m_child);
            }
        }

        [Test]
        public void Test_Pop_resets_Parent_on_IChild()
        {
            using (m_mockery.Ordered())
            {
                m_child.Parent = m_conductor;
                m_child.Parent = null;
            }

            using (m_mockery.Test())
            {
                m_conductor.Push(m_child);
                m_conductor.Pop();
            }
        }

        [Test]
        public void Test_Pop_will_throw_if_nothing_to_Pop()
        {
            Assert.Throws<InvalidOperationException>(() => m_conductor.Pop());
        }

        [Test]
        public void Test_Pop_will_fallback_to_parent_if_nothing_to_pop()
        {
            var childConductor = new NavigationConductor<object>();
            m_conductor.Push(childConductor);

            childConductor.Push(new object());
            childConductor.Pop();

            Assert.IsNull(m_conductor.ActiveItem);
        }

        #endregion
    }
}
