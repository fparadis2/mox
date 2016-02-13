using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
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

        public static void GroupByColor(List<DeckCardGroupViewModel> groups, IEnumerable<DeckCardViewModel> cards)
        {
            ByColor byColor = new ByColor(groups);

            foreach (var card in cards)
            {
                byColor.AddCard(card);
            }

            Cleanup(groups);
        }

        private class ByColor
        {
            private readonly DeckCardGroupViewModel m_white;
            private readonly DeckCardGroupViewModel m_blue;
            private readonly DeckCardGroupViewModel m_black;
            private readonly DeckCardGroupViewModel m_red;
            private readonly DeckCardGroupViewModel m_green;
            private readonly DeckCardGroupViewModel m_multiColored;
            private readonly DeckCardGroupViewModel m_colorless;
            private readonly DeckCardGroupViewModel m_invalid;

            public ByColor(List<DeckCardGroupViewModel> groups)
            {
                m_white = AddGroup(groups, "White");
                m_blue = AddGroup(groups, "Blue");
                m_black = AddGroup(groups, "Black");
                m_red = AddGroup(groups, "Red");
                m_green = AddGroup(groups, "Green");
                m_multiColored = AddGroup(groups, "Multi colored");
                m_colorless = AddGroup(groups, "Colorless");
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

                var color = cardInstanceInfo.Card.Color;

                if (cardInstanceInfo.Card.Type.Is(Type.Land))
                {
                    string frameColors;
                    if (AdditionalData.TryGetColorStringForLand(cardInstanceInfo.Card.Name, out frameColors))
                    {
                        if (frameColors.Length > 1)
                            return m_multiColored;

                        if (frameColors.Length > 0)
                        {
                            var symbol = ManaSymbolHelper.Parse(frameColors[0].ToString(), ManaSymbolNotation.Compact);
                            color = ManaSymbolHelper.GetColor(symbol);
                        }
                    }
                }

                if (color.CountColors() > 1)
                    return m_multiColored;

                switch (color)
                {
                    case Color.White: return m_white;
                    case Color.Blue: return m_blue;
                    case Color.Black: return m_black;
                    case Color.Red: return m_red;
                    case Color.Green: return m_green;
                    case Color.None: return m_colorless;
                    default: return m_invalid;
                }
            }
        }

        public static void GroupByCost(List<DeckCardGroupViewModel> groups, IEnumerable<DeckCardViewModel> cards)
        {
            ByCost byCost = new ByCost(groups);

            foreach (var card in cards)
            {
                byCost.AddCard(card);
            }

            byCost.Finalize(groups);

            Cleanup(groups);
        }

        private class DeckCardGroupByCostViewModel : DeckCardGroupViewModel
        {
            private readonly int m_cost;

            public DeckCardGroupByCostViewModel(int cost)
            {
                m_cost = cost;
                Name = string.Format("{0} drop", cost);
            }

            public int Cost
            {
                get { return m_cost; }
            }
        }

        private class ByCost
        {
            private readonly Dictionary<int, DeckCardGroupByCostViewModel> m_costGroups = new Dictionary<int, DeckCardGroupByCostViewModel>();

            private readonly DeckCardGroupViewModel m_special;
            private readonly DeckCardGroupViewModel m_land;
            private readonly DeckCardGroupViewModel m_invalid;

            public ByCost(List<DeckCardGroupViewModel> groups)
            {
                m_land = AddGroup(groups, "Land");
                m_special = AddGroup(groups, "No cost");
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

                if (cardInstanceInfo.Card.Type.Is(Type.Land))
                {
                    return m_land;
                }

                if (string.IsNullOrEmpty(cardInstanceInfo.Card.ManaCost))
                {
                    return m_special;
                }

                ManaCost cost = ManaCost.Parse(cardInstanceInfo.Card.ManaCost);
                return GetOrCreateGroup(cost.ConvertedValue);
            }

            private DeckCardGroupViewModel GetOrCreateGroup(int convertedCost)
            {
                DeckCardGroupByCostViewModel group;
                if (!m_costGroups.TryGetValue(convertedCost, out group))
                {
                    group = new DeckCardGroupByCostViewModel(convertedCost);
                    m_costGroups.Add(convertedCost, group);
                }

                return group;
            }

            public void Finalize(List<DeckCardGroupViewModel> groups)
            {
                groups.InsertRange(0, m_costGroups.Values.OrderBy(g => g.Cost));
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
        Color,
        Cost,
        Rarity
    }
}