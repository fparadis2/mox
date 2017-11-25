using Mox.Lobby.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Lobby.Network.Protocol;
using System.Collections;

namespace Mox.Lobby.Server
{
    partial class LobbyBackend
    {
        private class Bots : IEnumerable<LobbyUser>
        {
            #region Static Data

            private static readonly string[] ms_botNames =
            {
                "Marvin",
                "Jarvis",
                "Hal",
            };

            #endregion

            #region Variables

            private readonly List<LobbyUser> m_bots = new List<LobbyUser>();

            #endregion

            #region Methods

            public LobbyUser CreateBot(LobbyBackend lobby)
            {
                User bot = new User(null, ms_botNames[m_bots.Count % ms_botNames.Length]);
                var botIdentity = new UserIdentity { Name = bot.Name };

                var user = new LobbyUser(bot, botIdentity, true);
                lobby.SendUserJoinedMessages(user);
                lobby.m_users.Add(user);

                m_bots.Add(user);
                return user;
            }

            public IEnumerator<LobbyUser> GetEnumerator()
            {
                return m_bots.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_bots.GetEnumerator();
            }

            #endregion
        }
    }
}
