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
using Rhino.Mocks;
using System.Windows.Input;

namespace Mox.UI
{
    [TestFixture]
    public class RelayCommandTests
    {
        #region Variables

        private MockRepository m_mockery;

        private ICommand m_mockCommand;
        private RelayCommand m_relayCommand;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_mockCommand = m_mockery.StrictMock<ICommand>();
            m_relayCommand = new RelayCommand(m_mockCommand.CanExecute, m_mockCommand.Execute);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new RelayCommand(null, m_mockCommand.Execute); });
            Assert.Throws<ArgumentNullException>(delegate { new RelayCommand(m_mockCommand.CanExecute, null); });
        }

        [Test]
        public void Test_CanExecute_relays_to_the_CanExecute_delegate()
        {
            object parameter = new object();
            Expect.Call(m_mockCommand.CanExecute(parameter)).Return(true);
            m_mockery.Test(() => Assert.IsTrue(m_relayCommand.CanExecute(parameter)));
        }

        [Test]
        public void Test_Execute_relays_to_the_Execute_delegate()
        {
            object parameter = new object();
            m_mockCommand.Execute(parameter);
            m_mockery.Test(() => m_relayCommand.Execute(parameter));
        }

        #endregion
    }
}
