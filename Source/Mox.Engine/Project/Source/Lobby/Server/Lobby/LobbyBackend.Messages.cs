using System;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    partial class LobbyBackend
    {
        #region Utilities

        internal void Broadcast(Message message)
        {
            foreach (var client in m_users.AllUsers)
            {
                client.Channel.Send(message);
            }
        }

        internal void BroadcastExceptTo(User user, Message message)
        {
            foreach (var client in m_users.AllUsers)
            {
                if (client != user)
                {
                    client.Channel.Send(message);
                }
            }
        }

        #endregion
        
        #region Messages

        private void SendPlayerJoinMessages(User newUser, PlayerData data)
        {
            BroadcastExceptTo(newUser, new PlayersChangedMessage(PlayersChangedMessage.ChangeType.Joined, new[] { data }));
            BroadcastExceptTo(newUser, new Network.Protocol.ServerMessage { User = newUser.Id, Message = "joined the lobby" });
        }

        private void SendPlayerLeaveMessages(User user, PlayerData data, string reason)
        {
            Broadcast(new PlayersChangedMessage(PlayersChangedMessage.ChangeType.Left, new[] { data }));

            BroadcastServerMessage(user, $"left the lobby ({reason})");
        }

        private void SendPlayerSlotChangedMessages(int index, PlayerSlotData slot)
        {
            Broadcast(new PlayerSlotsChangedMessage(index, slot));
        }

        private void BroadcastServerMessage(User user, string message)
        {
            Broadcast(new Network.Protocol.ServerMessage { User = user.Id, Message = message });
        }

        #endregion
    }
}
