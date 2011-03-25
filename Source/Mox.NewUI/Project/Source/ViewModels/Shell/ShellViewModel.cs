using System;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    public class ShellViewModel : PropertyChangedBase
    {
        #region Variables

        private string m_name;

        #endregion

        #region Properties

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        #endregion
    }

}
