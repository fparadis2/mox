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
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class DisposableHelperTests
    {
        #region Tests

        [Test]
        public void Test_Invalid_constructor_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new DisposableHelper(null); });
        }

        [Test]
        public void Test_Dispose_calls_the_action_passed_in_constructor()
        {
            bool called = false;

            new DisposableHelper(delegate
                {
                    called = true;
                }).Dispose();

            Assert.IsTrue(called);
        }

        [Test]
        public void Test_SafeDispose_disposes_the_object()
        {
            MockRepository mockery = new MockRepository();
            IDisposable disposable = mockery.StrictMock<IDisposable>();

            disposable.Dispose();

            mockery.Test(() => DisposableHelper.SafeDispose(disposable));
        }

        [Test]
        public void Test_SafeDispose_disposes_the_object_and_sets_the_reference_to_null()
        {
            MockRepository mockery = new MockRepository();
            IDisposable disposable = mockery.StrictMock<IDisposable>();

            disposable.Dispose();

            mockery.Test(() => DisposableHelper.SafeDispose(ref disposable));
            Assert.IsNull(disposable);
        }

        [Test]
        public void Test_SafeDispose_does_nothing_if_object_is_null()
        {
            IDisposable nullDisposable = null;
            DisposableHelper.SafeDispose(nullDisposable);
            DisposableHelper.SafeDispose(ref nullDisposable);
        }

        #endregion
    }
}
