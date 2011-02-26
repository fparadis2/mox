namespace Mox.Lobby.Backend
{
    public interface IChatClient
    {
        void OnMessageReceived(MessageReceivedEventArgs e);
    }
}