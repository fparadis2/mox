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
using System.Diagnostics;

using Mox.Transactions;

namespace Mox.Replication
{
    internal class CommandSynchronizer<TUser>
    {
        #region Variables

        private readonly ObjectManager m_manager;
        private readonly IAccessControlStrategy<TUser> m_accessControlStrategy;
        private readonly TUser m_user;
        private readonly Dictionary<Object, MultiCommand> m_updateCommands = new Dictionary<Object, MultiCommand>();

        #endregion

        #region Constructor

        public CommandSynchronizer(ObjectManager manager, IAccessControlStrategy<TUser> accessControlStrategy, TUser user)
        {
            Throw.IfNull(manager, "manager");
            Throw.IfNull(accessControlStrategy, "accessControlStrategy");

            m_manager = manager;
            m_user = user;
            m_accessControlStrategy = accessControlStrategy;
        }

        #endregion

        #region Properties

        public TUser User
        {
            get { return m_user; }
        }

        #endregion

        #region Methods

        public ICommand PrepareImmediateSynchronization(ICommand command)
        {
            if (command == null)
            {
                return null;
            }

            MultiCommand syncCommand = new MultiCommand();
            Process(syncCommand, command);
            return syncCommand;
        }

        public ICommand PrepareDelayedSynchronization(Object theObject)
        {
            ICommand result = m_updateCommands.SafeGetValue(theObject);
            m_updateCommands.Remove(theObject);
            return result;
        }

        private void Process(MultiCommand synchronizationCommand, ICommand command)
        {
            MultiCommand multiCommand = command as MultiCommand;
            if (multiCommand != null)
            {
                foreach (ICommand subCommand in multiCommand.Commands)
                {
                    Process(synchronizationCommand, subCommand);
                }
            }
            else
            {
                ICommand synchronizedCommand = ProcessImpl(command);
                if (synchronizedCommand != null)
                {
                    synchronizationCommand.Add(synchronizedCommand);
                }
            }
        }

        private ICommand ProcessImpl(ICommand command)
        {
            ISynchronizableCommand synchronizableCommand = command as ISynchronizableCommand;
            if (synchronizableCommand != null)
            {
                ICommand result = synchronizableCommand.Synchronize();

                Object obj;
                bool isVisible = IsVisible(synchronizableCommand, out obj);
                if (isVisible)
                {
                    return result;
                }

                Debug.Assert(obj != null);
                PushUpdateCommand(obj, result);
                return null;
            }

            // Non-synchronizable commands are always considered visible.
            return command;
        }

        private bool IsVisible(ISynchronizableCommand command, out Object obj)
        {
            obj = null;
            if (command.IsPublic)
            {
                return true;
            }

            obj = command.GetObject(m_manager);
            if (obj == null)
            {
                return true;
            }

            Flags<UserAccess> access = m_accessControlStrategy.GetUserAccess(m_user, obj);
            return access.Contains(UserAccess.Read);
        }

        private void PushUpdateCommand(Object theObject, ICommand command)
        {
            MultiCommand multiCommand;
            if (!m_updateCommands.TryGetValue(theObject, out multiCommand))
            {
                multiCommand = new MultiCommand();
                m_updateCommands.Add(theObject, multiCommand);
            }
            multiCommand.Add(command);
        }

        #endregion
    }
}
