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
using System.Text;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ScopeTests
    {
        #region Variables

        private Scope m_scope;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_scope = new Scope();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_InScope_returns_false_normally()
        {
            Assert.IsFalse(m_scope.InScope);
        }

        [Test]
        public void Test_InScope_returns_true_when_in_scope()
        {
            using (m_scope.Begin())
            {
                Assert.IsTrue(m_scope.InScope);
            }
            Assert.IsFalse(m_scope.InScope);
        }

        [Test]
        public void Test_Scope_supports_nested_scopes()
        {
            using (m_scope.Begin())
            {
                using (m_scope.Begin())
                {
                    Assert.IsTrue(m_scope.InScope);
                }
                Assert.IsTrue(m_scope.InScope);
            }
            Assert.IsFalse(m_scope.InScope);
        }

        #endregion
    }
}
