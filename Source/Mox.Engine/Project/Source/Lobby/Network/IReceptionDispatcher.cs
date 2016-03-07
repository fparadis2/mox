using System;

namespace Mox.Lobby.Network
{
    internal interface IReceptionDispatcher
    {
        bool ReceiveMessagesSynchronously { get; }

        void BeginInvoke(System.Action action);
        void Invoke(System.Action action);

        void OnAfterRequest();
    }
}
