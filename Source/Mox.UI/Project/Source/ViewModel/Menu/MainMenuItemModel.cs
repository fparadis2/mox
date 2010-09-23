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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Mox.UI
{
    public class MainMenuItemModel : ViewModel
    {
        #region Variables

        private readonly System.Action m_clickAction;
        private readonly ObservableCollection<MainMenuItemModel> m_childItems = new ObservableCollection<MainMenuItemModel>();

        private string m_text;
        private MainMenuItemModel m_selectedItem;
        private bool m_selected;

        #endregion

        #region Constructor

        protected MainMenuItemModel(System.Action clickAction)
        {
            m_clickAction = clickAction;
        }

        #endregion

        #region Properties

        public System.Action ClickAction
        {
            get { return m_clickAction; }
        }

        public ObservableCollection<MainMenuItemModel> Items
        {
            get { return m_childItems; }
        }

        public string Text
        {
            get { return m_text; }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public bool IsSelected
        {
            get { return m_selected; }
            internal set
            {
                if (m_selected != value)
                {
                    m_selected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public MainMenuItemModel SelectedItem
        {
            get { return m_selectedItem; }
            set
            {
                if (m_selectedItem != value)
                {
                    Debug.Assert(value == null || Items.Contains(value), "Unknown item");

                    if (m_selectedItem != null)
                    {
                        m_selectedItem.IsSelected = false;
                    }

                    if (value != null && value.ClickAction != null)
                    {
                        value.ClickAction();
                        m_selectedItem = null;
                    }
                    else
                    {
                        m_selectedItem = value;

                        if (m_selectedItem != null)
                        {
                            m_selectedItem.IsSelected = true;
                        }
                    }

                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        #endregion

        #region Static Creation

        public static MainMenuItemModel Create()
        {
            return Create(null);
        }

        public static MainMenuItemModel Create(System.Action clickAction)
        {
            return new MainMenuItemModel(clickAction);
        }

        #endregion
    }
}
