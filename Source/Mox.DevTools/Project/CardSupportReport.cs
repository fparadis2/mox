using Mox.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public class CardSupportReport
    {
        private readonly CardDatabase m_database = MasterCardDatabase.Instance;
        private readonly Dictionary<string, RuleParser> m_parsers = new Dictionary<string, RuleParser>();

        private readonly Dictionary<string, CardInfo> m_cardsWithCustomCardFactory = new Dictionary<string, CardInfo>();
        private readonly Dictionary<string, CardInfo> m_cardsThatParseCorrectly = new Dictionary<string, CardInfo>();

        private readonly Dictionary<string, CardInfo> m_implementedCards = new Dictionary<string, CardInfo>();

        public CardSupportReport()
        {
            foreach (var card in m_database.Cards)
            {
                RuleParserCardFactory parserFactory = new RuleParserCardFactory();
                var parser = parserFactory.CreateRuleParser(card);
                if (parser != null)
                {
                    if (parser.UnknownFragments.Count == 0)
                    {
                        m_cardsThatParseCorrectly.Add(card.Name, card);
                        m_implementedCards[card.Name] = card;
                    }
                    m_parsers.Add(card.Name, parser);
                }

                if (MasterCardFactory.Instance.HasCustomFactory(card.Name))
                {
                    m_cardsWithCustomCardFactory.Add(card.Name, card);
                    m_implementedCards[card.Name] = card;
                }
            }
        }

        public void Write()
        {
            if (Debugger.IsAttached)
            {
                Write(new DebugWriter());
            }
            else
            {
                Write(Console.Out);
            }
        }

        public void Write(TextWriter writer)
        {
            var allCards = m_database.Cards;
            var notImplemented = allCards.Except(m_implementedCards.Values);

            IEnumerable<CardInfo> cardsWithFactory = m_cardsWithCustomCardFactory.Values;
            IEnumerable<CardInfo> cardsThatParseCorrectly = m_cardsThatParseCorrectly.Values;
            IEnumerable<CardInfo> cardsWithFactoryThatDontNeedIt = cardsWithFactory.Intersect(cardsThatParseCorrectly);
            cardsWithFactory = cardsWithFactory.Except(cardsWithFactoryThatDontNeedIt);

            writer.WriteLine();
            writer.WriteLine("======== CARD REPORT ========");
            WriteCardsWithBreakdown(writer, "Implemented Cards", m_implementedCards.Values, allCards);
            WriteCardsWithBreakdown(writer, "Implemented Cards With Parsing", cardsThatParseCorrectly, allCards);
            WriteCardsWithBreakdown(writer, "Not Implemented Cards", notImplemented, allCards);

            writer.WriteLine();
            writer.WriteLine("======== SETS ========");
            WriteSets(writer);

            writer.WriteLine();
            writer.WriteLine("======== CARDS ========");
            WriteCardList(writer, "Cards with custom factory that don't need it", cardsWithFactoryThatDontNeedIt, allCards);
            WriteCardList(writer, "Cards with custom factory", cardsWithFactory, allCards);
            WriteCardList(writer, "Cards that parse correctly", cardsThatParseCorrectly, allCards);
            WriteCardList(writer, "Not Implemented Cards", notImplemented, allCards);

            writer.WriteLine();
            writer.WriteLine("======== MOST SEEN FRAGMENTS ========");
            WriteFragments(writer);

            writer.Flush();
        }

        private static void WriteCardsWithBreakdown(TextWriter writer, string header, IEnumerable<CardInfo> cards, IReadOnlyCollection<CardInfo> allCards)
        {
            int count = cards.Count();

            writer.WriteLine();
            WriteHeader(writer, header, count, allCards.Count);

            writer.WriteLine("By color:");
            WriteBreakdown(writer, cards, allCards, c => c.Color);

            writer.WriteLine();
            writer.WriteLine("By type:");
            WriteBreakdown(writer, cards, allCards, c => GetMasterType(c.Type));
        }

        private static void WriteBreakdown<TKey>(TextWriter writer, IEnumerable<CardInfo> source, IReadOnlyCollection<CardInfo> all, Func<CardInfo, TKey> selector)
        {
            foreach (var grouping in source.GroupBy(selector))
            {
                int count = grouping.Count();
                int totalCount = all.Count(c => Equals(selector(c), grouping.Key));
                float pct = (float)count / totalCount * 100;
                writer.WriteLine($"{grouping.Key}: {count}/{totalCount} ({pct:0.#}%)");
            }
        }

        private static void WriteCardList(TextWriter writer, string header, IEnumerable<CardInfo> cards, IReadOnlyCollection<CardInfo> allCards)
        {
            var count = cards.Count();
            if (count == 0)
                return;

            writer.WriteLine();
            WriteHeader(writer, header, count, allCards.Count);

            foreach (var card in cards.OrderBy(c => c.Name))
            {
                var oracleText = card.ToOracleString()
                    .Replace(Environment.NewLine, " | ")
                    .Replace("\n", " | ");
                writer.WriteLine(oracleText);
            }
        }

        private void WriteSets(TextWriter writer)
        {
            writer.WriteLine();

            foreach (var set in m_database.Sets.OrderBy(s => s.ReleaseDate))
            {
                var setCards = set.CardInstances.Select(c => c.Card).Distinct(new NameEqualityComparer());
                var implemented = setCards.Where(c => m_implementedCards.ContainsKey(c.Name));

                int totalCount = setCards.Count();
                int count = implemented.Count();

                float pct = (float)count / totalCount * 100;
                writer.WriteLine($"[{set.Identifier}] {set.Name}: {count}/{totalCount} ({pct:0.#}%)");
            }
        }

        private void WriteFragments(TextWriter writer)
        {
            Dictionary<string, int> fragmentDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var parser in m_parsers.Values)
            {
                foreach (var fragment in parser.UnknownFragments)
                {
                    if (fragmentDictionary.TryGetValue(fragment, out int count))
                    {
                        fragmentDictionary[fragment] = count + 1;
                    }
                    else
                    {
                        fragmentDictionary.Add(fragment, 1);
                    }
                }
            }

            var fragments = fragmentDictionary.Select(pair => new { Text = pair.Key, Count = pair.Value });
            var totalCount = fragments.Count();

            foreach (var fragment in fragments.OrderByDescending(f => f.Count))
            {
                float pct = (float)fragment.Count / totalCount * 100;
                writer.WriteLine($"{fragment.Count}/{totalCount} ({pct:0.#}%): {fragment.Text}");
            }
        }

        private static void WriteHeader(TextWriter writer, string header, int count, int totalCount)
        {
            float pct = (float)count / totalCount * 100;
            writer.WriteLine($"--- {header} ({count}/{totalCount} = {pct:0.#}%)");
            writer.WriteLine();
        }

        private static Type GetMasterType(Type tested)
        {
            Type[] types = new[] { Type.Land, Type.Creature, Type.Artifact, Type.Enchantment, Type.Sorcery, Type.Instant };

            foreach (Type type in types)
            {
                if (tested.Is(type))
                {
                    return type;
                }
            }

            return tested;
        }

        private class DebugWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.Default;

            public override void Flush()
            {
                Debug.Flush();
                base.Flush();
            }

            public override void Write(bool value)
            {
                Debug.Write(value);
            }

            public override void Write(char value)
            {
                Debug.Write(value);
            }

            public override void Write(char[] buffer)
            {
                Debug.Write(buffer);
            }

            public override void Write(decimal value)
            {
                Debug.Write(value);
            }

            public override void Write(double value)
            {
                Debug.Write(value);
            }

            public override void Write(float value)
            {
                Debug.Write(value);
            }

            public override void Write(int value)
            {
                Debug.Write(value);
            }

            public override void Write(long value)
            {
                Debug.Write(value);
            }

            public override void Write(object value)
            {
                Debug.Write(value);
            }

            public override void Write(string value)
            {
                Debug.Write(value);
            }

            public override void Write(uint value)
            {
                Debug.Write(value);
            }

            public override void Write(ulong value)
            {
                Debug.Write(value);
            }

            public override void Write(string format, object arg0)
            {
                Debug.Write(string.Format(format, arg0));
            }

            public override void Write(string format, params object[] arg)
            {
                Debug.Write(string.Format(format, arg));
            }

            public override void Write(char[] buffer, int index, int count)
            {
                string x = new string(buffer, index, count);
                Debug.Write(x);
            }

            public override void Write(string format, object arg0, object arg1)
            {
                Debug.Write(string.Format(format, arg0, arg1));
            }

            public override void Write(string format, object arg0, object arg1, object arg2)
            {
                Debug.Write(string.Format(format, arg0, arg1, arg2));
            }

            public override void WriteLine()
            {
                Debug.WriteLine(string.Empty);
            }

            public override void WriteLine(bool value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(char value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(char[] buffer)
            {
                Debug.WriteLine(buffer);
            }

            public override void WriteLine(decimal value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(double value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(float value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(int value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(long value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(object value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(string value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(uint value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(ulong value)
            {
                Debug.WriteLine(value);
            }

            public override void WriteLine(string format, object arg0)
            {
                Debug.WriteLine(string.Format(format, arg0));
            }

            public override void WriteLine(string format, params object[] arg)
            {
                Debug.WriteLine(string.Format(format, arg));
            }

            public override void WriteLine(char[] buffer, int index, int count)
            {
                string x = new string(buffer, index, count);
                Debug.WriteLine(x);
            }

            public override void WriteLine(string format, object arg0, object arg1)
            {
                Debug.WriteLine(string.Format(format, arg0, arg1));
            }

            public override void WriteLine(string format, object arg0, object arg1, object arg2)
            {
                Debug.WriteLine(string.Format(format, arg0, arg1, arg2));
            }
        }

        private class NameEqualityComparer : IEqualityComparer<CardInfo>
        {
            public bool Equals(CardInfo x, CardInfo y)
            {
                return Equals(x.Name, y.Name);
            }

            public int GetHashCode(CardInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}
