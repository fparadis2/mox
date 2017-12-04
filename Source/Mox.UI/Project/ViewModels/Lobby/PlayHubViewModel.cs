using Caliburn.Micro;

namespace Mox.UI.Lobby
{
    public class PlayHubViewModel : Conductor<Screen>.Collection.OneActive
    {
        public PlayHubViewModel()
        {
            DisplayName = "Play";

            ActivateItem(new CreateLobbyPageViewModel());
        }
    }
}
