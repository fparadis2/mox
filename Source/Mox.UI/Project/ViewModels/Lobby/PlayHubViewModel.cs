using System;

using System.Windows.Input;

namespace Mox.UI.Lobby
{
    public class PlayHubViewModel : MoxScreen
    {
        public PlayHubViewModel()
        {
            DisplayName = "Play";
        }

        #region Commands

        public ICommand CreateLocalLobbyCommand
        {
            get
            {
                return new RelayCommand(CreateLocalLobby);
            }
        }

        private void CreateLocalLobby()
        {
            CreateLobbyViewModel createLobbyPage = new CreateLobbyViewModel
            {
                DisplayName = "Create a local lobby",
            };
            createLobbyPage.Show(this);
        }

        #endregion
    }
}
