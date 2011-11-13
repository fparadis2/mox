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

namespace Mox.Flow
{
    public abstract class TransactionPart : NewPart
    {
        #region Variables

        private readonly object m_token;

        #endregion

        #region Constructor

        protected TransactionPart(object token)
        {
            Throw.IfNull(token, "token");
            m_token = token;
        }

        #endregion

        #region Properties

        protected object Token
        {
            get { return m_token; }
        }

        #endregion
    }

    public class BeginTransactionPart : TransactionPart
    {
        #region Constructor

        public BeginTransactionPart(object token)
            : base(token)
        {
        }

        #endregion

        #region Methods

        public override NewPart Execute(Context context)
        {
            context.Game.Controller.BeginTransaction(Token);
            return null;
        }

        #endregion
    }

    public class EndTransactionPart : TransactionPart
    {
        #region Variables

        private readonly bool m_rollback;

        #endregion

        #region Constructor

        public EndTransactionPart(bool rollback, object token)
            : base(token)
        {
            m_rollback = rollback;
        }

        public bool Rollback
        {
            get { return m_rollback; }
        }

        #endregion

        #region Methods

        public override NewPart Execute(Context context)
        {
            context.Game.Controller.EndTransaction(m_rollback, Token);
            return null;
        }

        #endregion
    }
}