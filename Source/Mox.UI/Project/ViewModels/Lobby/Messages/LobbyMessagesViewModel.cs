﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Mox.Lobby;
using System.Windows.Input;

namespace Mox.UI.Lobby
{
    public class LobbyMessagesViewModel : PropertyChangedBase, IDisposable
    {
        #region Variables

        private readonly LobbyMessageListViewModel m_messageList = new LobbyMessageListViewModel();

        private LobbyViewModel m_lobby;
        private IMessageService m_messageService;

        private string m_input;

        #endregion

        #region Constructor

        public void Dispose()
        {
            Bind(null, null);
        }

        #endregion

        #region Properties

        public LobbyMessageListViewModel MessageList { get { return m_messageList; } }

        public string Input
        {
            get
            {
                return m_input;
            }
            set
            {
                if (m_input != value)
                {
                    m_input = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public ICommand SendMessageCommand
        {
            get { return new RelayCommand(SendMessage, CanSendMessage); }
        }

        #endregion

        #region Methods

        internal void Bind(LobbyViewModel lobby, IMessageService messageService)
        {
            if (m_messageService != null)
            {
                m_messageService.ChatMessageReceived -= WhenChatMessageReceived;
                m_messageService.ServerMessageReceived -= WhenServerMessageReceived;
                m_messageService.GameMessageReceived -= WhenGameMessageReceived;
            }

            m_lobby = lobby;
            m_messageService = messageService;
            m_messageList.Clear();

            if (m_messageService != null)
            {
                m_messageService.ChatMessageReceived += WhenChatMessageReceived;
                m_messageService.ServerMessageReceived += WhenServerMessageReceived;
                m_messageService.GameMessageReceived += WhenGameMessageReceived;
            }
        }

        private void WhenChatMessageReceived(object sender, ChatMessage e)
        {
            if (m_lobby.TryGetUser(e.SpeakerUserId, out LobbyUserViewModel user))
            {
                m_messageList.Add(LobbyMessageType.Chat, user, e.Text);
            }
        }

        private void WhenServerMessageReceived(object sender, ServerMessage e)
        {
            if (m_lobby.TryGetUser(e.UserId, out LobbyUserViewModel user))
            {
                m_messageList.Add(LobbyMessageType.Server, user, e.Text);
            }
        }

        private void WhenGameMessageReceived(object sender, GameMessage e)
        {
            if (m_lobby.TryGetUser(e.SourceUserId, out LobbyUserViewModel user))
            {
                m_messageList.Add(LobbyMessageType.Game, user, e.Text);
            }
        }

        public bool CanSendMessage()
        {
            return !string.IsNullOrEmpty(Input);
        }

        public void SendMessage()
        {
            if (CanSendMessage())
            {
                m_messageService.SendMessage(Input);
                Input = string.Empty;
            }
        }

        #endregion
    }

    public class LobbyMessagesViewModel_DesignTime : LobbyMessagesViewModel
    {
        public LobbyMessagesViewModel_DesignTime()
        {
            LobbyMessageListViewModel_DesignTime.AddMessages(MessageList);
        }
    }
}
