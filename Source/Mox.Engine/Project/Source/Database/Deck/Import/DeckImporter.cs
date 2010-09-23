using System;
using System.Collections.Generic;
using System.IO;

namespace Mox.Database
{
    public class DeckImporter
    {
        #region Variables

        private readonly CardDatabase m_database;
        private readonly TextReader m_reader;
        private readonly Deck m_deck = new Deck();

        private string m_error;

        private bool m_inHeader = true;
        private int m_lineNumber;

        #endregion

        #region Constructor

        private DeckImporter(CardDatabase database, TextReader reader)
        {
            Throw.IfNull(database, "database");
            Throw.IfNull(reader, "reader");

            m_database = database;
            m_reader = reader;
        }

        #endregion

        #region Properties

        public CardDatabase Database
        {
            get { return m_database; }
        }

        public string Error
        {
            get { return m_error ?? string.Empty; }
            set { m_error = value; }
        }

        public Deck Deck
        {
            get
            {
                if (!string.IsNullOrEmpty(Error))
                {
                    return null;
                }

                if (m_deck.Cards.Keys.Count == 0)
                {
                    return null;
                }

                return m_deck;
            }
        }

        #endregion

        #region Methods

        public static Deck Import(CardDatabase database, string text, out string error)
        {
            if (string.IsNullOrEmpty(text))
            {
                error = string.Empty;
                return null;
            }

            using (StringReader reader = new StringReader(text))
            {
                return ImportImpl(database, reader, out error);
            }
        }

        public static Deck Import(CardDatabase database, Stream stream, out string error)
        {
            Throw.IfNull(stream, "stream");

            using (StreamReader reader = new StreamReader(stream))
            {
                return ImportImpl(database, reader, out error);
            }
        }

        private static Deck ImportImpl(CardDatabase database, TextReader reader, out string error)
        {
            var importer = new DeckImporter(database, reader);
            importer.Import();
            error = importer.Error;
            return importer.Deck;
        }

        private void Import()
        {
            string line;

            while ((line = m_reader.ReadLine()) != null && string.IsNullOrEmpty(Error))
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                line = line.Trim();

                if (string.Equals(line, "Sideboard", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Error = ParseLine(line);
                m_lineNumber++;
            }
        }

        private string ParseLine(string line)
        {
            if (line.StartsWith("//"))
            {
                return ParseComment(line.Substring("//".Length).Trim());
            }

            if (line.StartsWith("SB:", StringComparison.OrdinalIgnoreCase))
            {
                // Ignore sideboard for now
                return null;
            }

            m_inHeader = false;

            QuantityInfo quantityInfo;
            string error = ParseQuantityLine(line, out quantityInfo);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            if (m_deck.Cards.ContainsKey(quantityInfo.CardIdentfier))
            {
                return string.Format("Card '{0}' appears more than once", FormatIdentifier(quantityInfo.CardIdentfier));
            }

            m_deck.Cards[quantityInfo.CardIdentfier] = quantityInfo.Quantity;
            return null;
        }

        private struct QuantityInfo
        {
            public int Quantity;
            public CardIdentifier CardIdentfier;
        }

        private string ParseQuantityLine(string line, out QuantityInfo quantityInfo)
        {
            quantityInfo = new QuantityInfo();

            int firstSpaceIndex = line.IndexOf(' ');
            if (firstSpaceIndex < 0)
            {
                return string.Format("'{0}' is an invalid line", line);
            }

            string quantityString = line.Substring(0, firstSpaceIndex).Trim();
            string cardName = line.Substring(firstSpaceIndex).Trim();

            if (string.IsNullOrEmpty(cardName))
            {
                return string.Format("'{0}' is an invalid line", line);
            }

            int quantity;
            if (!int.TryParse(quantityString, out quantity) || quantity < 1)
            {
                return string.Format("'{0}' is not a valid card quantity", quantityString);
            }

            if (!Database.Cards.ContainsKey(cardName))
            {
                return string.Format("'{0}' is not a known card", cardName);
            }

            quantityInfo.Quantity = quantity;
            quantityInfo.CardIdentfier = new CardIdentifier { Card = cardName };
            return null;
        }

        private string ParseComment(string comment)
        {
            if (m_inHeader)
            {
                KeyValuePair<string, string> tag = ParseTag(comment);

                string key = tag.Key ?? string.Empty;

                switch (key.ToLower())
                {
                    case "name":
                        m_deck.Name = tag.Value;
                        break;

                    case "creator":
                        m_deck.Author = tag.Value;
                        break;

                    default:
                        ParseNormalHeader(comment);
                        break;
                }
            }

            // Ignore line
            return null;
        }

        private void ParseNormalHeader(string headerLine)
        {
            if (m_lineNumber == 0) // First line is interpreted as name
            {
                m_deck.Name = headerLine;
            }
            else // otherwise it's a comment
            {
                if (!string.IsNullOrEmpty(m_deck.Description))
                {
                    m_deck.Description += Environment.NewLine;
                }

                m_deck.Description += headerLine;
            }
        }

        private static KeyValuePair<string, string> ParseTag(string comment)
        {
            int index = comment.IndexOf(':');
            if (index > 0 && index < comment.Length - 1)
            {
                string key = comment.Substring(0, index).Trim();
                string value = comment.Substring(index + 1).Trim();

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    return new KeyValuePair<string, string>(key, value);
                }
            }

            return new KeyValuePair<string, string>();
        }

        private static string FormatIdentifier(CardIdentifier identifier)
        {
            if (string.IsNullOrEmpty(identifier.Set))
            {
                return identifier.Card;
            }

            return string.Format("{0} ({1})", identifier.Card, identifier.Set);
        }

        #endregion
    }
}
