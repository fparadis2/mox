using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby
{
    [Serializable]
    public class CreateLobbyRequest : Request<JoinLobbyResponse>
    {
        public string Username
        {
            get;
            set;
        }

        public string GameFormat
        {
            get; 
            set; 
        }

        public string DeckFormat
        {
            get;
            set;
        }
    }
}
