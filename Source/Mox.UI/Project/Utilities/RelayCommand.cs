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
using System.Windows.Input;

namespace Mox.UI
{
    public class RelayCommand : ICommand
    {
        #region Variables

        private static readonly Func<bool> ms_canAlwaysExecute = () => true;

        private readonly Func<bool> m_canExecute;
        private readonly System.Action m_execute;

        #endregion

        #region Constructor

        public RelayCommand(System.Action execute, Func<bool> canExecute = null)
        {
            Throw.IfNull(execute, "execute");

            m_canExecute = canExecute ?? ms_canAlwaysExecute;
            m_execute = execute;
        }

        #endregion

        #region Implementation of ICommand

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            m_execute();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter)
        {
            return m_canExecute();
        }

        #endregion
    }
}
