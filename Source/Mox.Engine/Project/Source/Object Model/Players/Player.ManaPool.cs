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

namespace Mox
{
    /// <summary>
    /// Represents the mana pool of a player.
    /// </summary>
    public class PlayerManaPool : ManaPool
    {
        #region Inner Types

        [Serializable]
        private class ChangeManaCommand : Object.ObjectCommand
        {
            private readonly ManaAmount m_oldAmount;
            private readonly ManaAmount m_newAmount;

            public ChangeManaCommand(PlayerManaPool manaPool, ManaAmount newAmount)
                : base(manaPool.m_player.Identifier)
            {
                m_oldAmount = manaPool;
                m_newAmount = newAmount;
            }

            #region Overrides of Command

            /// <summary>
            ///  Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                Player player = (Player)GetObject(manager);
                player.ManaPool.SetManaInternal(m_newAmount);
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                Player player = (Player)GetObject(manager);
                player.ManaPool.SetManaInternal(m_oldAmount);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Player m_player;

        #endregion

        #region Constructor

        internal PlayerManaPool(Player player)
        {
            Throw.IfNull(player, "player");
            m_player = player;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Player owning this mana pool.
        /// </summary>
        public Player Player
        {
            get { return m_player; }
        }

        #endregion

        #region Methods

        protected override void SetMana(ManaAmount value)
        {
            m_player.Manager.Controller.Execute(new ChangeManaCommand(this, value));
        }

        private void SetManaInternal(ManaAmount mana)
        {
            base.SetMana(mana);
            OnChanged(EventArgs.Empty);
        }

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the mana pool changes.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Triggers the Changed event.
        /// </summary>
        protected void OnChanged(EventArgs e)
        {
            Changed.Raise(this, e);
        }

        #endregion
    }
}
