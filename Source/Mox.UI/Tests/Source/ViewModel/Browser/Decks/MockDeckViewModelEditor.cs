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
    public class MockDeckViewModelEditor : IDeckViewModelEditor
    {
        private readonly CardDatabase m_database;
        private readonly DeckLibrary m_library;

        public MockDeckViewModelEditor(CardDatabase database, DeckLibrary library)
        {
            m_database = database;
            m_library = library;
        }

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
            get;
            set;
        }

        public bool IsDirty
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }
    }
}
