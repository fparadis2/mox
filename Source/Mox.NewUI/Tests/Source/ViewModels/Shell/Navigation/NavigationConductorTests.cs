using System;

using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class NavigationConductorTests
    {
        #region Variables

        private NavigationConductor<object> m_conductor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
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

        #endregion
    }
}
