using System;
using System.Collections.Generic;

namespace Mox.Lobby.Backend
{
    partial class LobbyBackend
    {
        #region Utilities

        private void Broadcast(Message message)
        {
            foreach (var client in m_users.Channels)
            {
                client.Send(message);
            }
        }

        private void BroadcastExceptTo(IChannel exceptClient, Message message)
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
            newClient.Send(new UserChangedResponse(UserChange.Joined, Users));
            newClient.Send(new PlayerChangedResponse(PlayerChange.Joined, Players));
        }

        private void SendUserLeaveMessages(UserInternalData userData)
        {
            Broadcast(new UserChangedResponse(UserChange.Left, new[] { userData.User }));
        }

        private void SendPlayerChangedMessages(Player player)
        {
            Broadcast(new PlayerChangedResponse(PlayerChange.Changed, new[] { player }));
        }

        #endregion
    }
}
