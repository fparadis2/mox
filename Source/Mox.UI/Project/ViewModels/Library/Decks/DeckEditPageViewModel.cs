using System;
using System.Windows.Input;

namespace Mox.UI.Library
{
    public class DeckEditPageViewModel : PageViewModel
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

        #region Commands

        public ICommand CreateCommand
        {
            get { return new RelayCommand(Create, CanCreate); }
        }

        public Func<DeckEditPageViewModel, bool> CreateAction { get; set; }

        public bool CanCreate()
        {
            return true;
        }

        public void Create()
        {
            if (CreateAction != null && !CreateAction(this))
            {
                return;
            }

            Close();
        }

        #endregion
    }

    public class DeckEditPageViewModel_DesignTime : DeckEditPageViewModel
    {
        public DeckEditPageViewModel_DesignTime()
        {
            DisplayName = "Edit this deck!";
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
