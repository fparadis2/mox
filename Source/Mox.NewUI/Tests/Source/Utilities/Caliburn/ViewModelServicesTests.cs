using System;
using Caliburn.Micro;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class ViewModelServicesTests
    {
        #region Variables

        private MockRepository m_mockery;
        private IChild m_child;
        private IChild m_clonableChild;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_child = m_mockery.StrictMock<IChild>();
            m_clonableChild = m_mockery.StrictMultiMock<IChild>(typeof (ICloneable));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_FindParent_returns_null_if_no_parent_satisfies_the_query()
        {
            Expect.Call(m_child.Parent).Return(m_child);
            Expect.Call(m_child.Parent).Return(m_child);
            Expect.Call(m_child.Parent).Return(null);

            using (m_mockery.Test())
            {
                Assert.IsNull(m_child.FindParent<ICloneable>());
            }
        }

        [Test]
        public void Test_FindParent_returns_null_if_one_of_the_parent_is_not_a_child()
        {
            Expect.Call(m_child.Parent).Return(m_child);
            Expect.Call(m_child.Parent).Return(new object());

            using (m_mockery.Test())
            {
                Assert.IsNull(m_child.FindParent<ICloneable>());
            }
        }

        [Test]
        public void Test_FindParent_returns_the_parent_when_found()
        {
            Expect.Call(m_child.Parent).Return(m_child);
            Expect.Call(m_child.Parent).Return(m_clonableChild);

            using (m_mockery.Test())
            {
                Assert.AreSame(m_clonableChild, m_child.FindParent<ICloneable>());
            }
        }

        #endregion
    }
}