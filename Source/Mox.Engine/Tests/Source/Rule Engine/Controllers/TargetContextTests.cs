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
    public class TargetContextTests : BaseGameTests
    {
        #region Variables

        private TargetContext m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new TargetContext(true, new [] { 1, 2, 3 }, TargetContextType.Discard);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsTrue(m_context.AllowCancel);
            Assert.Collections.AreEqual(new[] { 1, 2, 3 }, m_context.Targets);
            Assert.AreEqual(TargetContextType.Discard, m_context.Type);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new TargetContext(false, null, TargetContextType.Discard); });
        }

        [Test]
        public void Test_IsSerializable()
        {
            Assert.IsSerializable(m_context);
        }

        #endregion
    }
}
