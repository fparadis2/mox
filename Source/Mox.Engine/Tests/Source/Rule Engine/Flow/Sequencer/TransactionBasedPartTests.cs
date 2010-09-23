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

using Mox.Transactions;
using NUnit.Framework;

namespace Mox.Flow
{
    [TestFixture]
    public class TransactionBasedPartTests : PartTestBase<Part<IGameController>>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => new BeginTransactionPart<IGameController>(null));
            Assert.Throws<ArgumentNullException>(() => new EndTransactionPart<IGameController>(null));
            Assert.Throws<ArgumentNullException>(() => new RollbackTransactionPart<IGameController>(null));
        }

        [Test]
        public void Test_BeginTransaction_begins_a_normal_transaction_and_returns_null()
        {
            Assert.IsNull(m_game.TransactionStack.CurrentTransaction, "Sanity check");

            Assert.IsNull(new BeginTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));

            Assert.IsNotNull(m_game.TransactionStack.CurrentTransaction);
            Assert.AreEqual(TransactionType.None, m_game.TransactionStack.CurrentTransaction.Type);
            Assert.AreEqual("Token", m_game.TransactionStack.CurrentTransaction.Token);
        }

        [Test]
        public void Test_EndTransaction_ends_the_current_transaction()
        {
            m_game.TransactionStack.BeginTransaction(TransactionType.None, "Token");

            Assert.AreNotEqual(m_playerB, m_game.State.ActivePlayer);
            m_game.State.ActivePlayer = m_playerB;
            Assert.AreEqual(m_playerB, m_game.State.ActivePlayer);

            Assert.IsNull(new EndTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));

            Assert.AreEqual(m_playerB, m_game.State.ActivePlayer);
        }

        [Test]
        public void Test_RollbackTransaction_rolls_back_the_current_transaction()
        {
            m_game.TransactionStack.BeginTransaction(TransactionType.None, "Token");

            Assert.AreNotEqual(m_playerB, m_game.State.ActivePlayer);
            m_game.State.ActivePlayer = m_playerB;
            Assert.AreEqual(m_playerB, m_game.State.ActivePlayer);

            Assert.IsNull(new RollbackTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));

            Assert.AreNotEqual(m_playerB, m_game.State.ActivePlayer);
        }

        [Test]
        public void Test_EndTransaction_and_RollbackTransaction_throw_if_there_is_no_current_transaction()
        {
            Assert.Throws<InvalidOperationException>(() => new EndTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));
            Assert.Throws<InvalidOperationException>(() => new RollbackTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));
        }

        [Test]
        public void Test_EndTransaction_and_RollbackTransaction_throw_if_the_current_transaction_is_not_a_user_transaction()
        {
            m_game.TransactionStack.BeginTransaction(TransactionType.Master);

            Assert.Throws<InvalidOperationException>(() => new EndTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));
            Assert.Throws<InvalidOperationException>(() => new RollbackTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));
        }

        [Test]
        public void Test_EndTransaction_and_RollbackTransaction_throw_if_the_current_transaction_doesnt_have_the_same_token()
        {
            m_game.TransactionStack.BeginTransaction(TransactionType.None, "OtherToken");

            Assert.Throws<InvalidOperationException>(() => new EndTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));
            Assert.Throws<InvalidOperationException>(() => new RollbackTransactionPart<IGameController>("Token").Execute(m_sequencerTester.Context));
        }

        #endregion
    }
}
