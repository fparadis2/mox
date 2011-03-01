namespace Mox.Lobby.Backend
{
    public interface IChatClient
    {
        void OnMessageReceived(User user, string message);
    }
}