using System;
using System.Collections;
using System.Collections.Generic;

using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Client
{
    internal class ClientPlayerCollection : IPlayerCollection
    {
        #region Variables

        private readonly Dictionary<Guid, PlayerData> m_players = new Dictionary<Guid, PlayerData>();

        #endregion

        #region Properties

        public int Count
        {
            get { return m_players.Count; }
        }

        #endregion

        #region Methods

        internal void HandleChangedMessage(PlayersChangedMessage message)
        {
            PlayersChangedEventArgs.ChangeType change;

            switch (message.Change)
            {
                case PlayersChangedMessage.ChangeType.Joined:
                    message.Players.ForEach(Add);
                    change = PlayersChangedEventArgs.ChangeType.Joined;
                    break;

                case PlayersChangedMessage.ChangeType.Left:
                    message.Players.ForEach(Remove);
                    change = PlayersChangedEventArgs.ChangeType.Left;
                    break;

                case PlayersChangedMessage.ChangeType.Changed:
                    message.Players.ForEach(Refresh);
                    change = PlayersChangedEventArgs.ChangeType.Changed;
                    break;

                default:
                    throw new NotImplementedException();
            }

            PlayersChangedEventArgs e = new PlayersChangedEventArgs { Change = change };
            e.Players.AddRange(message.Players);
            Changed.Raise(this, e);
        }

        private void Add(PlayerData player)
        {
            m_players.Add(player.Id, player);
        }

        private void Remove(PlayerData player)
        {
            m_players.Remove(player.Id);
        }

        private void Refresh(PlayerData player)
        {
            m_players.Remove(player.Id);
            m_players.Add(player.Id, player);
        }

        public bool TryGet(Guid id, out PlayerData player)
        {
            return m_players.TryGetValue(id, out player);
        }

        public IEnumerator<PlayerData> GetEnumerator()
        {
            return m_players.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Events

        public event EventHandler<PlayersChangedEventArgs> Changed;

        #endregion
    }
}
