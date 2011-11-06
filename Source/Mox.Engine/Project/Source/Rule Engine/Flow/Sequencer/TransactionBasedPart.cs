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
namespace Mox.Flow
{
    public abstract class TransactionBasedPart : NewPart
    {
        #region Variables

        private readonly object m_token;

        #endregion

        #region Constructor

        protected TransactionBasedPart(object token)
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

    public class BeginTransactionPart : TransactionBasedPart
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

    public class EndTransactionPart : TransactionBasedPart
    {
        #region Constructor

        public EndTransactionPart(object token)
            : base(token)
        {
        }

        #endregion

        #region Methods

        public override NewPart Execute(Context context)
        {
            context.Game.Controller.EndTransaction(false, Token);
            return null;
        }

        #endregion
    }

    public class RollbackTransactionPart : TransactionBasedPart
    {
        #region Constructor

        public RollbackTransactionPart(object token)
            : base(token)
        {
        }

        #endregion

        #region Methods

        public override NewPart Execute(Context context)
        {
            context.Game.Controller.EndTransaction(true, Token);
            return null;
        }

        #endregion
    }
}