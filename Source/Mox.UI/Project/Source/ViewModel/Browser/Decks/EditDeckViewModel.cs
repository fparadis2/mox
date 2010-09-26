using System;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class EditDeckViewModel : ViewModel, IDeckViewModelEditor
    {
        #region Variables

        private readonly CardDatabase m_database;

        private bool m_detailsExpanded;
        private bool m_isEnabled;

        #endregion

        #region Constructor

        public EditDeckViewModel(CardDatabase database)
        {
            Throw.IfNull(database, "database");
            m_database = database;
        }

        #endregion

        #region Properties

        public CardDatabase Database
        {
            get { return m_database; }
        }

        public bool DetailsExpanded
        {
            get { return m_detailsExpanded; }
            set
            {
                if (m_detailsExpanded != value)
                {
                    m_detailsExpanded = value;
                    OnPropertyChanged("DetailsExpanded");
                    OnPropertyChanged("DetailsExpanderText");
                }
            }
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
                    OnPropertyChanged("DetailsExpanderText");
                }
            }
        }

        public string UserName
        {
            get { return Environment.UserName; }
        }

        public string DetailsExpanderText
        {
            get 
            {
                if (DetailsExpanded)
                {
                    return "Hide Details";
                }

                return IsEnabled ? "Edit Details" : "Show Details";
            }
        }

        #endregion
    }
}
