using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Mox.Lobby;
using Mox.Threading;
using Mox.UI.Browser;
using Mox.UI.Lobby;

namespace Mox.UI.Shell
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly ObservableCollection<MainMenuItemViewModel> m_childItems = new ObservableCollection<MainMenuItemViewModel>();
        private MainMenuItemViewModel m_selectedItem;

        #endregion

        #region Constructor

        public MainMenuViewModel(IShellViewModel shellViewModel)
        {
            Items.Add(CreateFromWorkspacePage("Single Player", shellViewModel, CreateLocalLobby));
            Items.Add(CreateFromWorkspacePage("Join", shellViewModel, JoinLobby));
            Items.Add(CreateFromWorkspacePage("Browse Decks", shellViewModel, () => new BrowseDecksPageViewModel()));
            Items.Add(CreateFromAction("Exit", () => Application.Current.Shutdown()));
        }

        public static LobbyPageViewModel CreateLocalLobby()
        {
            var server = Server.CreateLocal(new LogContext()); // TODO: Find better place for this? Where to log?
            
            var client = Client.CreateLocal(server);
            client.Dispatcher = WPFDispatcher.FromCurrentThread();

            client.Connect();
            client.CreateLobby(Environment.UserName);

            // TODO: Restore user settings (last used deck, etc)

            return new ConnectedLobbyPageViewModel(client);
        }

        private static LobbyPageViewModel JoinLobby()
        {
            var client = Client.CreateNetwork();
            client.Dispatcher = WPFDispatcher.FromCurrentThread();
            client.Connect();

            var lobbies = client.GetLobbies();

            if (lobbies.Any())
            {
                client.EnterLobby(lobbies.First(), Environment.UserName);
            }
            else
            {
                client.CreateLobby(Environment.UserName);
            }

            // TODO: Restore user settings (last used deck, etc)

            return new ConnectedLobbyPageViewModel(client);
        }

        private static MainMenuItemViewModel CreateFromWorkspacePage(string text, IShellViewModel shellViewModel, Func<INavigationViewModel<MoxWorkspace>> viewModelCreator)
        {
            return CreateFromAction(text, () =>
            {
                MoxWorkspaceViewModel conductor = new MoxWorkspaceViewModel();
                conductor.Push(viewModelCreator());
                shellViewModel.Push(conductor);
            });
        }

        private static MainMenuItemViewModel CreateFromAction(string text, System.Action action)
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

        #region Inner Types

        public class ConnectedLobbyPageViewModel : LobbyPageViewModel
        {
            private readonly Client m_client;

            public ConnectedLobbyPageViewModel(Client client)
                : base(client.Lobby)
            {
                m_client = client;
            }

            public override void Deactivate()
            {
                base.Deactivate();

                m_client.Disconnect();
            }
        }

        #endregion
    }
}
