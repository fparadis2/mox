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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Transactions
{
    /// <summary>
    /// A composite command.
    /// </summary>
    [Serializable]
    public class MultiCommand : ICommand, IEnumerable<ICommand>
    {
        #region Variables

        private readonly List<ICommand> m_commands = new List<ICommand>();

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the command is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_commands.All(command => command.IsEmpty); }
        }

        /// <summary>
        /// Commands in this multi command.
        /// </summary>
        public IList<ICommand> Commands
        {
            get { return m_commands.AsReadOnly(); }
        }

        /// <summary>
        /// Number of commands in the multi command.
        /// </summary>
        public int CommandCount
        {
            get { return m_commands.Count; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Pushes a new <see cref="command"/> on this multi-command.
        /// </summary>
        /// <param name="command"></param>
        public void Add(ICommand command)
        {
            Throw.IfNull(command, "command");
            m_commands.Add(command);
        }

        /// <summary>
        /// Executes all the commands in this multi-command.
        /// </summary>
        public void Execute(ObjectManager manager)
        {
            m_commands.ForEach(command => command.Execute(manager));
        }

        /// <summary>
        /// Unexecutes all the commands in this multi-command.
        /// </summary>
        public void Unexecute(ObjectManager manager)
        {
            m_commands.Reverse<ICommand>().ForEach(command => command.Unexecute(manager));
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<ICommand> GetEnumerator()
        {
            return m_commands.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
