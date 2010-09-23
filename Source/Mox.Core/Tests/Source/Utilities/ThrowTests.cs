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

namespace Mox
{
    [TestFixture]
    public class ThrowTests
    {
        #region Tests

        [Test]
        public void Test_IfNull()
        {
            Throw.IfNull(new object(), "Doesn't throws");

            try
            {
                Throw.IfNull(null, "Throws");
                Assert.Fail("Should have thrown");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [Test]
        public void Test_IfEmpty()
        {
            Assert.DoesntThrow(() => Throw.IfEmpty("Non Empty", "Doesn't throws"));

            Assert.Throws<ArgumentNullException>(() => Throw.IfEmpty(null, "Should throw"));
            Assert.Throws<ArgumentNullException>(() => Throw.IfEmpty(string.Empty, "Should throw"));
        }

        [Test]
        public void Test_InvalidArgumentIf()
        {
            Assert.DoesntThrow(() => Throw.InvalidArgumentIf(false, "Message", "My param"));
            Assert.Throws<ArgumentException>(() => Throw.InvalidArgumentIf(true, "Message", "My param"));
        }

        [Test]
        public void Test_InvalidOperationIf()
        {
            Assert.DoesntThrow(() => Throw.InvalidOperationIf(false, "Message"));
            Assert.Throws<InvalidOperationException>(() => Throw.InvalidOperationIf(true, "Message"));
        }

        [Test]
        public void Test_InvalidProgramIf()
        {
            Assert.DoesntThrow(() => Throw.InvalidProgramIf(false, "Message"));
            Assert.Throws<InvalidProgramException>(() => Throw.InvalidProgramIf(true, "Message"));
        }

        [Test]
        public void Test_ArgumentOutOfRangeIf()
        {
            Assert.DoesntThrow(() => Throw.ArgumentOutOfRangeIf(false, "Message", "paramName"));
            Assert.Throws<ArgumentOutOfRangeException>(() => Throw.ArgumentOutOfRangeIf(true, "Message", "paramName"));
        }

        [Test]
        public void Test_DisposedIf()
        {
            Assert.DoesntThrow(() => Throw.DisposedIf(false, "Object"));
            Assert.Throws<ObjectDisposedException>(() => Throw.DisposedIf(true, "Object"));
        }

        #endregion
    }
}
