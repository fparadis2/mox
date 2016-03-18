using System;
using System.Collections.Generic;
using System.IO;

namespace Mox.Database
{
    public class DeckReader
    {
        private static readonly char[] ms_quantitySeparatorChars = { ' ', '\t' };

        private enum Phase
        {
            Header,
            Main,
            Sideboard
        }

        private readonly Deck m_deck;
        private readonly List<string> m_errors = new List<string>();
        private Phase m_phase;

        public DeckReader(string name)
        {
            m_deck = new Deck(name);
        }

        public bool HasErrors
        {
            get { return m_errors.Count > 0; }
        }

        public Deck Read(string contents)
        {
            if (string.IsNullOrEmpty(contents))
                return m_deck;

            m_deck.Contents = contents;

            using (StringReader reader = new StringReader(contents))
            {
                Read(reader);
            }

            if (m_errors.Count > 0)
            {
                m_deck.Error = m_errors[0];
            }

            return m_deck;
        }

        private void Read(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                ParseLine(line.Trim());
            }
        }

        private void ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                if (m_phase == Phase.Main)
                    m_phase = Phase.Sideboard;

                return;
            }

            if (line.StartsWith("//"))
            {
                if (m_phase == Phase.Header)
                    ParseHeaderComment(line.Substring(2).TrimStart());
                return;
            }

            if (line.StartsWith("SB:"))
            {
                line = line.Substring(3).TrimStart();
                m_phase = Phase.Sideboard;
            }

            QuantityInfo quantityInfo;
            if (!ParseQuantityLine(line, out quantityInfo))
            {
                return;
            }

            if (m_phase == Phase.Header)
                m_phase = Phase.Main;

            switch (m_phase)
            {
                case Phase.Main:
                    m_deck.Cards.Add(quantityInfo.CardIdentfier, quantityInfo.Quantity);
                    break;

                case Phase.Sideboard:
                    m_deck.Sideboard.Add(quantityInfo.CardIdentfier, quantityInfo.Quantity);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void ParseHeaderComment(string comment)
        {
            if (!string.IsNullOrEmpty(m_deck.Description))
            {
                m_deck.Description += Environment.NewLine;
            }

            m_deck.Description += comment;
        }

        private bool ParseQuantityLine(string line, out QuantityInfo quantityInfo)
        {
            quantityInfo = new QuantityInfo();

            int firstSpaceIndex = line.IndexOfAny(ms_quantitySeparatorChars);
            if (firstSpaceIndex < 0)
            {
                LogError(string.Format("'{0}' is an invalid line", line));
                return false;
            }

            string quantityString = line.Substring(0, firstSpaceIndex).Trim();
            string cardName = line.Substring(firstSpaceIndex).Trim();

            if (string.IsNullOrEmpty(cardName))
            {
                LogError(string.Format("'{0}' is an invalid line", line));
                return false;
            }

            int quantity;
            if (!int.TryParse(quantityString, out quantity) || quantity < 1)
            {
                LogError(string.Format("'{0}' is not a valid card quantity", quantityString));
                return false;
            }

            quantityInfo.Quantity = quantity;
            quantityInfo.CardIdentfier = new CardIdentifier { Card = cardName };
            return true;
        }

        private struct QuantityInfo
        {
            public int Quantity;
            public CardIdentifier CardIdentfier;
        }

        private void LogError(string error)
        {
            m_errors.Add(error);
        }
    }
}
