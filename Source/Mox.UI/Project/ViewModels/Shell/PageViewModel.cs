using System.Windows.Input;
using Caliburn.Micro;

namespace Mox.UI
{
    public class PageViewModel : PropertyChangedBase, IChild, IHaveDisplayName
    {
        private string m_displayName;

        public string DisplayName
        {
            get { return m_displayName; }
            set
            {
                if (m_displayName != value)
                {
                    m_displayName = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public object Parent
        {
            get { return ((IChild) this).Parent; }
        }

        object IChild.Parent { get; set; }

        protected void Close()
        {
            INavigationConductor conductor = (INavigationConductor)Parent;
            if (conductor != null)
            {
                conductor.Pop(this);
            }
        }

        public ICommand GoBackCommand
        {
            get { return new RelayCommand(Close); }
        }

        public void Show(IChild owner)
        {
            INavigationConductor conductor = owner.FindParent<INavigationConductor>();
            conductor.Push(this);
        }
    }
}