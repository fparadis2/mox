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
using System.IO;
using System.Linq;
using System.Text;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class ImportDeckViewModel : ViewModel
    {
        #region Variables

        private readonly CardDatabase m_database;

        private string m_text;
        private bool m_canImport;
        private string m_error;

        #endregion

        #region Constructor

        public ImportDeckViewModel(CardDatabase database)
        {
            m_database = database;
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return m_text; }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    OnPropertyChanged("Text");
                    Update();
                }
            }
        }

        public bool CanImport
        {
            get { return m_canImport; }
            protected set
            {
                if (m_canImport != value)
                {
                    m_canImport = value;
                    OnPropertyChanged("CanImport");
                }
            }
        }

        public string Error
        {
            get { return m_error; }
            protected set
            {
                if (m_error != value)
                {
                    m_error = value;
                    OnPropertyChanged("Error");
                }
            }
        }

        #endregion

        #region Methods

        public Deck Import()
        {
            Throw.InvalidOperationIf(!string.IsNullOrEmpty(Error), "Cannot import when there's an error");

            string error;
            return DeckImporter.Import(m_database, Text, out error);
        }

        private void Update()
        {
            string error;
            var deck = DeckImporter.Import(m_database, Text, out error);
            CanImport = deck != null && string.IsNullOrEmpty(error);
            Error = error;
        }

        #endregion
    }
}
