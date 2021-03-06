﻿using System;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    partial class LobbyBackend
    {
        #region Utilities

        internal void Broadcast(Message message)
        {
            m_users.Broadcast(message);
        }

        internal void BroadcastExceptTo(User user, Message message)
        {
            m_users.BroadcastExceptTo(user, message);
        }

        #endregion
        
        #region Messages

        private void SendUserJoinedMessages(LobbyUser newUser)
        {
            Broadcast(new UserJoinedMessage { UserId = newUser.Id, Data = newUser.Data });
            Broadcast(new Network.Protocol.ServerMessage { User = newUser.Id, Message = "joined the lobby" });
        }

        private void SendUserLeftMessages(User user, string reason)
        {
            Broadcast(new UserLeftMessage { UserId = user.Id });
            BroadcastServerMessage(user, $"left the lobby ({reason})");
        }

        private void SendPlayerSlotChangedMessages(int index, PlayerSlotData slot)
        {
            Broadcast(new PlayerSlotsChangedMessage(index, slot));
        }

        private void BroadcastServerMessage(LobbyUser user, string message)
        {
            BroadcastServerMessage(user.User, message);
        }

        private void BroadcastServerMessage(User user, string message)
        {
            Broadcast(new Network.Protocol.ServerMessage { User = user.Id, Message = message });
        }

        #endregion
    }
}
