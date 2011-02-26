namespace Mox.Lobby
{
    public interface IChatClient
    {
        void OnMessageReceived(MessageReceivedEventArgs e);
    }
}