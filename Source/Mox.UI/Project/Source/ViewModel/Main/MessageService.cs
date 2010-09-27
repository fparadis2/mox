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

namespace Mox.UI
{
    /// <summary>
    /// Provides access to modal message boxes.
    /// </summary>
    public static class MessageService
    {
        #region Variables

        private static IMessageService ms_instance = new Default();

        #endregion

        #region Properties

        public static IMessageService Instance
        {
            get { return ms_instance; }
        }

        #endregion

        #region Methods

        public static IDisposable Use(IMessageService instance)
        {
            var oldInstance = ms_instance;
            ms_instance = instance;

            return new DisposableHelper(() =>
            {
                ms_instance = oldInstance;
            });
        }

        #endregion

        #region Inner Types

        public class Default : IMessageService
        {
        }

        #endregion
    }
}
