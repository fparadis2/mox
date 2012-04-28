using System;

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
            BroadcastExceptTo(newClient, new ServerMessage { User = newUser, Message = string.Format("{0} joined the lobby", newUser.Name) });
            newClient.Send(new UserChangedResponse(UserChange.Joined, Users));
            newClient.Send(new PlayerChangedResponse(PlayerChange.Joined, Players));
        }

        private void SendUserLeaveMessages(UserInternalData userData, string reason)
        {
            Broadcast(new UserChangedResponse(UserChange.Left, new[] { userData.User }));
            Broadcast(new ServerMessage { User = userData.User, Message = string.Format("{0} left the lobby ({1})", userData.User.Name, reason) });
        }

        private void SendPlayerChangedMessages(Player player)
        {
            Broadcast(new PlayerChangedResponse(PlayerChange.Changed, new[] { player }));
        }

        #endregion
    }
}
