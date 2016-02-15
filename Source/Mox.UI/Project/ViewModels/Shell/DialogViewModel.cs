using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Caliburn.Micro;

namespace Mox.UI
{
    public interface IDialogViewModel
    {
        string Title { get; }
        object Content { get; }
        IEnumerable Commands { get; }
    }

    public class DialogViewModel : PropertyChangedBase, IDialogViewModel
    {
        #region Implementation

        internal static Func<IDialogViewModel, object> ShowImplementation;
        internal static Action<IDialogViewModel, object> CloseImplementation;

        #endregion

        #region Variables

        private object m_showToken;

        #endregion

        private string m_title;
        public string Title
        {
            get { return m_title; }
            set
            {
                if (m_title != value)
                {
                    m_title = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private object m_content;

        public object Content
        {
            get { return m_content; }
            set
            {
                if (m_content != value)
                {
                    m_content = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private readonly ObservableCollection<DialogCommandViewModel> m_commands = new ObservableCollection<DialogCommandViewModel>();
        public IEnumerable Commands
        {
            get { return m_commands; }
        }

        public void Show()
        {
            Debug.Assert(ReferenceEquals(m_showToken, null));

            m_showToken = ShowImplementation(this);

            Debug.Assert(!ReferenceEquals(m_showToken, null));
        }

        public void AddCommand(string name, System.Action action, DialogCommandOptions options = DialogCommandOptions.None)
        {
            var proxyCommand = new RelayCommand(o =>
            {
                Close();
                action();
            });

            m_commands.Add(new DialogCommandViewModel { Name = name, Command = proxyCommand, Options = options });
        }

        public void AddCommand(string name, ICommand command, DialogCommandOptions options = DialogCommandOptions.None)
        {
            var proxyCommand = new RelayCommand(o =>
            {
                Close();
                command.Execute(o);
            }, command.CanExecute);

            m_commands.Add(new DialogCommandViewModel { Name = name, Command = proxyCommand, Options = options });
        }

        public void AddCancelCommand(string name = "Cancel")
        {
            AddCommand(name, () => {}, DialogCommandOptions.IsCancel);
        }

        private void Close()
        {
            Debug.Assert(!ReferenceEquals(m_showToken, null));
            CloseImplementation(this, m_showToken);
            m_showToken = null;
        }
    }

    [Flags]
    public enum DialogCommandOptions
    {
        None = 0,
        IsDefault = 1,
        IsCancel = 2
    }

    public class DialogCommandViewModel
    {
        public string Name
        {
            get;
            set;
        }

        public ICommand Command
        {
            get;
            set;
        }

        public DialogCommandOptions Options
        {
            get;
            set;
        }

        public bool IsDefault
        {
            get { return Options.HasFlag(DialogCommandOptions.IsDefault); }
        }

        public bool IsCancel
        {
            get { return Options.HasFlag(DialogCommandOptions.IsCancel); }
        }
    }

    public class DialogViewModel_DesignTime : DialogViewModel
    {
        public DialogViewModel_DesignTime()
        {
            Title = "My Dialog";
            Content = "My Content";
            AddCommand("Ok", () => {}, DialogCommandOptions.IsDefault);
            AddCancelCommand();
        }
    }
}
