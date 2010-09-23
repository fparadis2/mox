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

namespace Mox.Replication
{
    /// <summary>
    /// Concrete implementation of the <see cref="IGameListener"/> interface.
    /// </summary>
    /// <remarks>
    /// Maintains a synchronized instance of a <see cref="Game"/>.
    /// </remarks>
    public class GameListener : MarshalByRefObject, IGameListener
    {
        #region Variables

        private readonly Game m_game = new Game();

        #endregion

        #region Constructor

        public GameListener()
        {
            m_game.ChangeControlMode(GameControlMode.Synchronized);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The game synchronized by this listener.
        /// </summary>
        public Game Game
        {
            get 
            {
                return m_game; 
            }
        }

        #endregion

        #region IGameListener Members

        public void Synchronize(ICommand command)
        {
            Game.EnsureControlModeIs(GameControlMode.Synchronized);
            Game.TransactionStack.PushAndExecute(command);
        }

        public void BeginTransaction(TransactionType type)
        {
            Game.EnsureControlModeIs(GameControlMode.Synchronized);
            Game.TransactionStack.BeginTransaction(type);
        }

        public void EndCurrentTransaction(bool rollback)
        {
            Game.EnsureControlModeIs(GameControlMode.Synchronized);

            ITransaction transaction = Game.TransactionStack.CurrentTransaction;

            if (rollback)
            {
                transaction.Rollback();
            }
            else
            {
                transaction.Dispose();
            }
        }

        #endregion
    }
}
