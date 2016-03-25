﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public struct LobbyParametersNetworkData
    {
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

        public static LobbyParametersNetworkData FromParameters(LobbyParameters parameters)
        {
            return new LobbyParametersNetworkData
            {
                GameFormat = parameters.GameFormat == null ? null : parameters.GameFormat.Name,
                DeckFormat = parameters.DeckFormat == null ? null : parameters.DeckFormat.Name
            };
        }

        public LobbyParameters ToParameters(out string error)
        {
            LobbyParameters parameters = new LobbyParameters();

            if (!GameFormats.TryGetFormat(GameFormat, out parameters.GameFormat))
            {
                error = string.Format("'{0}' is not a supported game format.", GameFormat);
                return parameters;
            }

            if (!DeckFormats.TryGetFormat(DeckFormat, out parameters.DeckFormat))
            {
                error = string.Format("'{0}' is not a supported deck format.", DeckFormat);
                return parameters;
            }

            error = null;
            return parameters;
        }
    }

    [Serializable]
    public class CreateLobbyRequest : Request<JoinLobbyResponse>
    {
        public IPlayerIdentity Identity
        {
            get;
            set;
        }

        public LobbyParametersNetworkData Parameters
        {
            get; 
            set; 
        }
    }
}
