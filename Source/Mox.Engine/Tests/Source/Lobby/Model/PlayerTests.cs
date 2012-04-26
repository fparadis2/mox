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

namespace Mox.Lobby
{
    [TestFixture]
    public class PlayerTests
    {
        #region Variables

        private User m_user;
        private Player m_player;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_user = new User("MyName");
            m_player = new Player(m_user);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Player(null); });
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_user, m_player.User);
        }

        [Test]
        public void Test_Equality_and_serialization()
        {
            Player other = Assert.IsSerializable(m_player);
            Assert.AreEqual(m_player.User, other.User);
        }

        [Test]
        public void Test_Can_clone_Players()
        {
            User newUser = new User("John");
            Player other = m_player.AssignUser(newUser);

            Assert.AreEqual(newUser, other.User);
            Assert.AreEqual(other.Id, m_player.Id);
            Assert.AreEqual(other.Data, m_player.Data);
        }

        [Test]
        public void Test_Can_ChangeData()
        {
            var data = m_player.Data;
            data.Deck = "3 Plains";
            Player other = m_player.ChangeData(data);

            Assert.AreEqual(m_player.Id, other.Id);
            Assert.AreEqual(data, other.Data);
        }

        #endregion
    }
}
