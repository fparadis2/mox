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
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class MasterCardDatabaseTests
    {
        #region Tests

        [Test]
        public void Test_Can_access_static_instance()
        {
            Assert.IsNotNull(MasterCardDatabase.Instance);
        }

        [Test]
        public void Test_Can_access_sets()
        {
            Assert.IsNotNull(MasterCardDatabase.Instance.Sets["10E"]);
        }

        [Test]
        public void Test_Every_card_has_at_least_an_instance()
        {
            var database = MasterCardDatabase.Instance;

            foreach (CardInfo card in database.Cards)
            {
                Assert.That(card.Instances.Any(), "Card {0} has no instances!", card.Name);
            }
        }

        #region Stats

        [Test, Ignore]
        public void Test_Supported_cards_statistics_for_10E()
        {
            var database = MasterCardDatabase.Instance;
            var cards = database.Sets["10E"].CardInstances.Select(ci => ci.Card);

            var allCardNames = cards.OrderBy(c => c.Name).Distinct(new NameEqualityComparer());
            int cardsCount = allCardNames.Count();

            var defined = allCardNames.ToLookup(card => MasterCardFactory.Instance.IsDefined(card.Name));

            WriteImplemented(defined[true], cardsCount);

            Trace.WriteLine(string.Empty);
            Trace.WriteLine(string.Empty);

            WriteNonImplemented(defined[false], cardsCount);

            Trace.WriteLine(string.Empty);
            Trace.WriteLine("## List of implemented cards ##");
            foreach (var card in defined[true])
            {
                Trace.WriteLine(card.Name);
            }
        }

        private static void WriteImplemented(IEnumerable<CardInfo> cards, int totalCount)
        {
            int count = cards.Count();

            WriteStatsHeader("Implemented", count, totalCount);

            Trace.WriteLine(string.Empty);
            Trace.WriteLine("By color:");
            WriteBreakdown(cards, c => c.Color);

            Trace.WriteLine(string.Empty);
            Trace.WriteLine("By type:");
            WriteBreakdown(cards, c => GetMasterType(c.Type));
        }

        private static Type GetMasterType(Type tested)
        {
            Type[] types = new[] {Type.Land, Type.Creature, Type.Artifact};

            foreach (Type type in types)
            {
                if (tested.Is(type))
                {
                    return type;
                }
            }

            return tested;
        }

        private static void WriteBreakdown<TKey>(IEnumerable<CardInfo> source, Func<CardInfo, TKey> selector)
        {
            int totalCount = source.Count();

            foreach (var grouping in source.GroupBy(selector))
            {
                int count = grouping.Count();
                float pct = (float)count / totalCount * 100;
                Trace.WriteLine(string.Format("{0}: {1} ({2:0.#}%)", grouping.Key, count, pct));
            }
        }

        private static void WriteNonImplemented(IEnumerable<CardInfo> cards, int totalCount)
        {
            int count = cards.Count();

            WriteStatsHeader("Non implemented", count, totalCount);

            foreach (var card in cards)
            {
                Trace.WriteLine(string.Empty);
                Trace.WriteLine(card.ToOracleString());
            }
        }

        private static void WriteStatsHeader(string category, int count, int totalCount)
        {
            float pct = (float)count / totalCount * 100;
            Trace.WriteLine(string.Format(" --- {0} cards ({1}/{2} - {3:0.#}%) --- ", category, count, totalCount, pct));
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

        #endregion

        #endregion
    }
}
