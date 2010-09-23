// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mox
{
    public class GameDebugWriter
    {
        #region Constants

        private const int TotalWidth = 80;
        private const int PlayerNameWidth = 16;
        private const int TotalPlayerNameWidth = PlayerNameWidth + 2; // To account for []
        private const int MaxCardLength = (TotalWidth - TotalPlayerNameWidth) / 2 - 1;

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly StringBuilder m_stringBuilder = new StringBuilder();

        #endregion

        #region Constructor

        public GameDebugWriter(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;

            WriteHeader();
            WritePlayers();
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return m_stringBuilder.ToString();
        }

        private void WriteHeader()
        {
            const string header = "--- Turn {0:##0} ";
            m_stringBuilder.AppendFormat(header.PadRight(TotalWidth, '-'), m_game.State.CurrentTurn);
            m_stringBuilder.AppendLine();
        }

        private void WritePlayers()
        {
            for (int i = 0; i < m_game.Players.Count; i++)
            {
                Player player = m_game.Players[i];
                WritePlayer(player);

                if (i < m_game.Players.Count - 1)
                {
                    m_stringBuilder.AppendLine();
                }
            }
        }

        private void WritePlayer(Player player)
        {
            WritePlayerName(player);
            WritePlayerZoneCounts(player);
            m_stringBuilder.AppendLine();
            WritePlayerZone("Hand", player.Hand);
            m_stringBuilder.AppendLine();
            WritePlayerZone("Play", player.Battlefield);
        }

        private void WritePlayerName(Player player)
        {
            string life = player.Life.ToString();
            int maxNameLength = PlayerNameWidth - life.Length - 1;
            string name = player.Name;
            if (maxNameLength < name.Length)
            {
                name = name.Substring(0, maxNameLength);
            }
            string padding = new string(' ', PlayerNameWidth - life.Length - name.Length);
            m_stringBuilder.AppendFormat("[{0}{1}{2}]", name, padding, life);
        }

        private void WritePlayerZoneCounts(Player player)
        {
            m_stringBuilder.AppendFormat("=[Library:{0,3}]=[Graveyard:{1,3}]", player.Library.Count, player.Graveyard.Count);
        }

        private void WritePlayerZone(string name, IEnumerable<Card> cards)
        {
            var sortedCards = cards.ToList();
            sortedCards.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));

            string zoneInfo = string.Format("[{0}:{1,3}]", name, sortedCards.Count).PadLeft(TotalPlayerNameWidth);
            m_stringBuilder.Append(zoneInfo);

            for (int i = 0; i < sortedCards.Count; i++)
            {
                bool firstColumn = i % 2 == 0;
                Card card = sortedCards[i];

                string cardString = WriteCard(card);
                Debug.Assert(cardString.Length < MaxCardLength);

                if (i < sortedCards.Count - 1 && firstColumn)
                {
                    cardString = cardString.PadRight(MaxCardLength);
                }

                m_stringBuilder.AppendFormat(" {0}", cardString);

                if (i < sortedCards.Count - 1 && !firstColumn)
                {
                    m_stringBuilder.AppendLine();
                    m_stringBuilder.Append(new string(' ', TotalPlayerNameWidth));
                }
            }
        }

        private static string WriteCard(Card card)
        {
            string pw = string.Empty;
            if (card.Is(Type.Creature) && card.Zone.ZoneId == Zone.Id.Battlefield)
            {
                pw = string.Format(" ({0}/{1})", card.Power, card.Toughness);
            }

            string name = card.Name;

            if (name.Length >= MaxCardLength - pw.Length)
            {
                name = name.Substring(0, MaxCardLength - pw.Length);
            }

            return string.Format("{0}{1}", name, pw);
        }

        #endregion
    }
}
