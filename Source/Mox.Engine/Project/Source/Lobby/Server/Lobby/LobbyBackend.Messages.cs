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
            foreach (var client in m_users.Channels)
            {
                client.Send(message);
            }
        }

        internal void BroadcastExceptTo(IChannel exceptClient, Message message)
        {
            foreach (var client in m_users.Channels)
            {
                if (client != exceptClient)
                {
                    client.Send(message);
                }
            }
        }

        #endregion
        
        #region Messages

        private void SendUserJoinMessages(IChannel newClient, User newUser)
        {
            BroadcastExceptTo(newClient, new UserChangedResponse(UserChange.Joined, new[] { newUser }));
            BroadcastExceptTo(newClient, new ServerMessage { User = newUser.Id, Message = string.Format("{0} joined the lobby", newUser.Name) });
        }

        private void SendUserLeaveMessages(UserInternalData userData, string reason)
        {
            Broadcast(new UserChangedResponse(UserChange.Left, new[] { userData.User }));
            Broadcast(new ServerMessage { User = userData.User.Id, Message = string.Format("{0} left the lobby ({1})", userData.User.Name, reason) });
        }

        private void SendPlayerSlotChangedMessages(int index, PlayerSlotNetworkDataChange change, PlayerSlot slot)
        {
            Broadcast(new PlayerSlotChangedMessage(index, change, slot));
        }

        #endregion
    }
}
