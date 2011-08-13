using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyChatViewModel : PropertyChangedBase
    {
        #region Variables

        private readonly IDispatcher m_dispatcher;
        private readonly ILobby m_lobby;

        #endregion

        #region Constructor

        public LobbyChatViewModel(ILobby lobby, IDispatcher dispatcher)
        {
            Throw.IfNull(lobby, "lobby");
            Throw.IfNull(dispatcher, "dispatcher");

            m_lobby = lobby;
            m_dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        #endregion

        #region Event Handlers

        #endregion

        #region Inner Types

        #endregion
    }
}
