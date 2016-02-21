using System;
using System.Windows;
using System.Windows.Input;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckEditPageViewModel : PageViewModel
    {
        #region Constructor

        public DeckEditPageViewModel()
        {
            CanEditName = true;
        }

        #endregion

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

        private string m_saveText = "Save";

        public string SaveText
        {
            get { return m_saveText; }
            set
            {
                if (m_saveText != value)
                {
                    m_saveText = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        public void TryUseClipboardContent()
        {
            string clipboardContents = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardContents))
                return;

            DeckReader reader = new DeckReader("From Clipboard");
            var deck = reader.Read(clipboardContents);
            if (deck == null || reader.HasErrors)
                return;

            Contents = clipboardContents;
        }

        #endregion

        #region Commands

        public ICommand SaveCommand
        {
            get { return new RelayCommand(Save, CanSave); }
        }

        public Func<DeckEditPageViewModel, bool> SaveAction { get; set; }

        public bool CanSave()
        {
            return true;
        }

        public void Save()
        {
            if (SaveAction != null && !SaveAction(this))
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
