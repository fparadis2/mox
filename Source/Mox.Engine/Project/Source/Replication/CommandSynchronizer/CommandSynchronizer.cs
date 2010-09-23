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
using System.Linq;
using System.Text;
using Mox.Transactions;

namespace Mox.Replication
{
    public class CommandSynchronizer : ICommandSynchronizer
    {
        #region Inner Types

        private class SynchronizationContext : ISynchronizationContext
        {
            #region Variables

            private readonly CommandSynchronizer m_owner;
            private readonly ObjectManager m_manager;
            private readonly IVisibilityStrategy m_visibilityStrategy;
            private readonly Player m_player;

            private readonly MultiCommand m_command = new MultiCommand();

            #endregion

            #region Constructor

            public SynchronizationContext(CommandSynchronizer owner, ObjectManager manager, IVisibilityStrategy visibilityStrategy, Player player)
            {
                Throw.IfNull(owner, "owner");
                Throw.IfNull(manager, "manager");
                Throw.IfNull(visibilityStrategy, "visibilityStrategy");

                m_owner = owner;
                m_manager = manager;
                m_visibilityStrategy = visibilityStrategy;
                m_player = player;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The command created during synchronization.
            /// </summary>
            public ICommand Command
            {
                get { return m_command; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Synchronizes the given command.
            /// </summary>
            /// <param name="command"></param>
            public void Synchronize(ICommand command)
            {
                ICommand synchronizedCommand = SynchronizeImpl(command);
                if (synchronizedCommand != null)
                {
                    m_command.Push(synchronizedCommand);
                }
            }

            private ICommand SynchronizeImpl(ICommand command)
            {
                ISynchronizableCommand synchronizableCommand = command as ISynchronizableCommand;
                if (synchronizableCommand != null)
                {
                    ICommand result = synchronizableCommand.Synchronize(this);

                    Object obj;
                    bool isVisible = GetIsVisible(synchronizableCommand, out obj);
                    if (isVisible)
                    {
                        return result;
                    }
                    else
                    {
                        Debug.Assert(obj != null);
                        m_owner.PushUpdateCommand(obj, result);
                        return null;
                    }
                }

                return command;
            }

            private bool GetIsVisible(ISynchronizableCommand command, out Object obj)
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

                return m_visibilityStrategy.IsVisible(obj, m_player);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Dictionary<Object, MultiCommand> m_updateCommands = new Dictionary<Object, MultiCommand>();

        #endregion

        #region Methods

        public ICommand Synchronize(ObjectManager manager, IVisibilityStrategy visibilityStrategy, Player player, IEnumerable<ICommand> commands)
        {
            Throw.IfNull(manager, "manager");
            Throw.IfNull(visibilityStrategy, "visibilityStrategy");

            if (commands == null || !commands.Any())
            {
                return null;
            }

            SynchronizationContext context = new SynchronizationContext(this, manager, visibilityStrategy, player);

            commands.ForEach(context.Synchronize);

            return context.Command;
        }

        public ICommand Update(Object theObject)
        {
            ICommand result = m_updateCommands.SafeGetValue(theObject);
            m_updateCommands.Remove(theObject);
            return result;
        }

        private void PushUpdateCommand(Object theObject, ICommand command)
        {
            MultiCommand multiCommand;
            if (!m_updateCommands.TryGetValue(theObject, out multiCommand))
            {
                multiCommand = new MultiCommand();
                m_updateCommands.Add(theObject, multiCommand);
            }
            multiCommand.Push(command);
        }

        #endregion
    }
}
