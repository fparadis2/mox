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
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class DeckViewModelEditor : PropertyChangedBase, IDeckViewModelEditor
    {
        #region Variables

        private readonly CardDatabase m_database;
        private readonly IMasterCardFactory m_cardFactory;

        private bool m_isDirty;
        private string m_userName;

        #endregion

        #region Constructor

        public DeckViewModelEditor(CardDatabase database, IMasterCardFactory cardFactory)
        {
            Throw.IfNull(database, "database");

            m_database = database;
            m_cardFactory = cardFactory;
        }

        #endregion

        #region Properties

        public CardDatabase Database
        {
            get { return m_database; }
        }

        public IMasterCardFactory CardFactory
        {
            get { return m_cardFactory; }
        }

        public bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                if (m_isDirty != value)
                {
                    m_isDirty = value;
                    NotifyOfPropertyChange(() => IsDirty);
                }
            }
        }

        public string UserName
        {
            get { return m_userName; }
            set
            {
                if (m_userName != value)
                {
                    m_userName = value;
                    NotifyOfPropertyChange(() => UserName);
                }
            }
        }

        #endregion

        #region Methods

        public static DeckViewModelEditor FromMaster()
        {
            return new DeckViewModelEditor(MasterCardDatabase.Instance, MasterCardFactory.Instance)
            {
                UserName = Environment.UserName
            };
        }

        public static DeckViewModelEditor FromDesignTime()
        {
            return new DeckViewModelEditor(DesignTimeCardDatabase.Instance, null)
            {
                UserName = "John Smith"
            };
        }

        #endregion
    }
}
