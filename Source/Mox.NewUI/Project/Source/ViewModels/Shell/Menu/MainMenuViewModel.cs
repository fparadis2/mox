using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly ObservableCollection<MainMenuItemViewModel> m_childItems = new ObservableCollection<MainMenuItemViewModel>();
        private MainMenuItemViewModel m_selectedItem;

        #endregion

        #region Constructor

        public MainMenuViewModel()
        {
            Items.Add(Create("Single Player", null));
            Items.Add(Create("Browse Decks", null));
            Items.Add(Create("Exit", () => Application.Current.Shutdown()));
        }

        private static MainMenuItemViewModel Create(string text, System.Action action)
        {
            return new MainMenuItemViewModel(action) { Text = text };
        }

        #endregion

        #region Properties

        public ObservableCollection<MainMenuItemViewModel> Items
        {
            get { return m_childItems; }
        }

        public MainMenuItemViewModel SelectedItem
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

                    m_selectedItem = value;

                    if (m_selectedItem != null)
                    {
                        m_selectedItem.IsSelected = true;
                    }

                    NotifyOfPropertyChange(() => SelectedItem);
                }
            }
        }

        #endregion
    }
}
