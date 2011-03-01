namespace Mox.Lobby.Network
{
    public interface IServerAdapter
    {
        string SessionId { get; }

        TCallback GetCallback<TCallback>();
        void Disconnect(object callback);
    }
}