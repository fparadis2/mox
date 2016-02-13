using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mox.Database;

namespace Mox.UI.Library
{
    public class DeckCardGroupViewModel
    {
        private readonly List<DeckCardViewModel> m_cards = new List<DeckCardViewModel>();

        public List<DeckCardViewModel> Cards
        {
            get { return m_cards; }
        }

        public string Name { get; set; }

        public string Title
        {
            get { return string.Format("{0} ({1})", Name, CardCount); }
        }

        public int CardCount
        {
            get { return m_cards.Sum(c => c.Quantity); }
        }

        public static void GroupByType(List<DeckCardGroupViewModel> groups, IEnumerable<DeckCardViewModel> cards)
        {
            ByType byType = new ByType(groups);

            foreach (var card in cards)
            {
                byType.AddCard(card);
            }

            Cleanup(groups);
        }

        private class ByType
        {
            private readonly DeckCardGroupViewModel m_planeswalkers;
            private readonly DeckCardGroupViewModel m_creatures;
            private readonly DeckCardGroupViewModel m_sorceries;
            private readonly DeckCardGroupViewModel m_instants;
            private readonly DeckCardGroupViewModel m_artifacts;
            private readonly DeckCardGroupViewModel m_enchantments;
            private readonly DeckCardGroupViewModel m_lands;
            private readonly DeckCardGroupViewModel m_invalid;

            public ByType(List<DeckCardGroupViewModel> groups)
            {
                m_planeswalkers = AddGroup(groups, "Planeswalker");
                m_creatures = AddGroup(groups, "Creature");
                m_sorceries = AddGroup(groups, "Sorcery");
                m_instants = AddGroup(groups, "Instant");
                m_artifacts = AddGroup(groups, "Artifact");
                m_enchantments = AddGroup(groups, "Enchantment");
                m_lands = AddGroup(groups, "Land");
                m_invalid = AddGroup(groups, "Invalid");
            }

            public void AddCard(DeckCardViewModel card)
            {
                GetGroup(card).Cards.Add(card);
            }

            private DeckCardGroupViewModel GetGroup(DeckCardViewModel card)
            {
                var cardInstanceInfo = card.CardInstanceInfo;

                if (cardInstanceInfo == null)
                {
                    return m_invalid;
                }

                var cardInfo = cardInstanceInfo.Card;

                if (cardInfo.Type.Is(Type.Planeswalker))
                {
                    return m_planeswalkers;
                }

                if (cardInfo.Type.Is(Type.Creature))
                {
                    return m_creatures;
                }

                if (cardInfo.Type.Is(Type.Sorcery))
                {
                    return m_sorceries;
                }

                if (cardInfo.Type.Is(Type.Instant))
                {
                    return m_instants;
                }

                if (cardInfo.Type.Is(Type.Artifact))
                {
                    return m_artifacts;
                }

                if (cardInfo.Type.Is(Type.Enchantment))
                {
                    return m_enchantments;
                }

                if (cardInfo.Type.Is(Type.Land))
                {
                    return m_lands;
                }

                return m_invalid;
            }
        }

        public static void GroupByRarity(List<DeckCardGroupViewModel> groups, IEnumerable<DeckCardViewModel> cards)
        {
            ByRarity byRarity = new ByRarity(groups);

            foreach (var card in cards)
            {
                byRarity.AddCard(card);
            }

            Cleanup(groups);
        }

        private class ByRarity
        {
            private readonly DeckCardGroupViewModel[] m_rarityGroups;
            private readonly DeckCardGroupViewModel m_invalid;

            public ByRarity(List<DeckCardGroupViewModel> groups)
            {
                var rarities = Enum.GetValues(typeof(Rarity)).Cast<Rarity>().ToArray();

                m_rarityGroups = new DeckCardGroupViewModel[(int)rarities.Last() + 1];

                foreach (var rarity in rarities.Reverse())
                {
                    m_rarityGroups[(int)rarity] = AddGroup(groups, rarity.ToPrettyString());
                }

                m_invalid = AddGroup(groups, "Invalid");
            }

            public void AddCard(DeckCardViewModel card)
            {
                GetGroup(card).Cards.Add(card);
            }

            private DeckCardGroupViewModel GetGroup(DeckCardViewModel card)
            {
                var cardInstanceInfo = card.CardInstanceInfo;

                if (cardInstanceInfo == null)
                {
                    return m_invalid;
                }

                return m_rarityGroups[(int) cardInstanceInfo.Rarity];
            }
        }

        private static DeckCardGroupViewModel AddGroup(ICollection<DeckCardGroupViewModel> groups, string name)
        {
            var group = new DeckCardGroupViewModel { Name = name };
            groups.Add(group);
            return group;
        }

        private static void Cleanup(List<DeckCardGroupViewModel> groups)
        {
            groups.RemoveAll(g => g.Cards.Count == 0);

            foreach (var group in groups)
            {
                group.Cards.Sort((a, b) => string.CompareOrdinal(a.Card.Card, b.Card.Card));
            }
        }
    }

    public enum DeckCardGrouping
    {
        Overview,
        Rarity
    }
}