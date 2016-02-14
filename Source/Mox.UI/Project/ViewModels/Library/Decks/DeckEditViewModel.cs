using System;

using Caliburn.Micro;

namespace Mox.UI.Library
{
    public class DeckEditViewModel : PropertyChangedBase
    {
        #region Properties

        public bool CanEditName { get; set; }

        private string m_name;
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private string m_contents;

        public string Contents
        {
            get { return m_contents; }
            set
            {
                if (m_contents != value)
                {
                    m_contents = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion
    }

    public class DeckEditViewModel_DesignTime : DeckEditViewModel
    {
        public DeckEditViewModel_DesignTime()
        {
            CanEditName = true;
            Name = "My Deck Name";
            Contents = @"// This is my deck
1 Plains
2 Mountains
3 Goblin
4 Gargoyle
5 Something
";
        }
    }
}
