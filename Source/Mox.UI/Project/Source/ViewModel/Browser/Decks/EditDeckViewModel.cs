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
using Mox.Database;

namespace Mox.UI.Browser
{
    public class EditDeckViewModel : ViewModel, IDeckViewModelEditor
    {
        #region Variables

        private readonly CardDatabase m_database;
        private readonly DeckLibrary m_library;

        private bool m_isEnabled;
        private bool m_isDirty;

        #endregion

        #region Constructor

        public EditDeckViewModel(CardDatabase database, DeckLibrary library)
        {
            Throw.IfNull(database, "database");
            Throw.IfNull(library, "library");

            m_database = database;
            m_library = library;
        }

        #endregion

        #region Properties

        public CardDatabase Database
        {
            get { return m_database; }
        }

        public DeckLibrary Library
        {
            get { return m_library; }
        }

        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set
            {
                if (m_isEnabled != value)
                {
                    m_isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                if (m_isDirty != value)
                {
                    m_isDirty = value;
                    OnPropertyChanged("IsDirty");
                }
            }
        }

        public string UserName
        {
            get { return Environment.UserName; }
        }

        #endregion
    }
}
