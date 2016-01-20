using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Mox.Database.Internal
{
    public class JsonParser
    {
        #region Variables

        private readonly CardDatabase m_cardDatabase;

        #endregion

        #region

        public JsonParser(CardDatabase cardDatabase)
        {
            m_cardDatabase = cardDatabase;
        }

        #endregion

        #region Methods

        public void Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var setsDictionary = JsonConvert.DeserializeObject<Dictionary<string, Set>>(reader.ReadToEnd());

                var sets = setsDictionary.Values.ToList();

                sets.Sort((a, b) => b.releaseDate.CompareTo(a.releaseDate));

                foreach (var set in sets)
                    ParseSet(set);
            }
        }

        private class Set
        {
            public string name { get; set; }
            public string code { get; set; }
            public DateTime releaseDate { get; set; }
            public string block { get; set; }
            public string type { get; set; }

            public string magicCardsInfoCode { get; set; }

            public List<Card> cards { get; set; }
        }

        private class Card
        {
            public string layout { get; set; }

            public string name { get; set; }
            public string manaCost { get; set; }
            public List<Color> colors { get; set; }
            public List<string> colorIdentity { get; set; }

            public List<string> supertypes { get; set; }
            public List<string> types { get; set; }
            public List<string> subtypes { get; set; }

            public string rarity { get; set; }
            public string text { get; set; }
            public string flavor { get; set; }
            public string artist { get; set; }
            public string number { get; set; }
            public string power { get; set; }
            public string toughness { get; set; }
            public int loyalty { get; set; }

            public int multiverseid { get; set; }
        }

        private void ParseSet(Set set)
        {
            switch (set.type)
            {
                case "core":
                case "expansion":
                    break;

                default:
                    return;
            }

            var setInfo = m_cardDatabase.AddSet(set.code, set.name, set.block, set.releaseDate);
            setInfo.MagicCardsInfoIdentifier = set.magicCardsInfoCode;

            foreach (var card in set.cards)
            {
                ParseCard(setInfo, card);
            }
        }

        private void ParseCard(SetInfo setInfo, Card card)
        {
#warning "TODO [Low]: Support split & flip cards
            if (card.layout != "normal")
                return;

            CardInfo cardInfo;
            if (!m_cardDatabase.Cards.TryGetValue(card.name, out cardInfo))
            {
                SuperType supertype = ParseSuperType(card.supertypes);
                Type type = ParseType(card.types);
                var subtypes = ParseSubTypes(card.subtypes);
                var color = ComputeColor(type, card);

                cardInfo = m_cardDatabase.AddCard(card.name, card.manaCost, color, supertype, type, subtypes, card.power, card.toughness, card.text);
            }

            var index = ParseIndex(card.number);
            var rarity = ParseRarity(card.rarity);
            m_cardDatabase.AddCardInstance(cardInfo, setInfo, index, rarity, card.multiverseid, card.artist, card.flavor);
        }

        private static SuperType ParseSuperType(IEnumerable<string> values)
        {
            SuperType result = SuperType.None;

            if (values != null)
            {
                foreach (var value in values)
                {
                    result |= (SuperType) Enum.Parse(typeof (SuperType), value);
                }
            }

            return result;
        }

        private static Type ParseType(IEnumerable<string> values)
        {
            Type result = Type.None;

            if (values != null)
            {
                foreach (var value in values)
                {
                    result |= (Type) Enum.Parse(typeof (Type), value);
                }
            }

            return result;
        }

        private static IEnumerable<SubType> ParseSubTypes(IEnumerable<string> values)
        {
            if (values == null)
                return Enumerable.Empty<SubType>();

            return values.Select(value =>
            {
                value = value.Replace("-", string.Empty);
                value = value.Replace("’", string.Empty);

                return (SubType) Enum.Parse(typeof (SubType), value);
            });
        }

        private static Rarity ParseRarity(string value)
        {
            switch (value)
            {
                case "Land":
                case "Basic Land": 
                    return Rarity.Land;
                case "Common":
                    return Rarity.Common;
                case "Uncommon":
                    return Rarity.Uncommon;
                case "Rare":
                    return Rarity.Rare;
                case "Mythic Rare":
                    return Rarity.MythicRare;
                case "Special":
                    return Rarity.Special;
                
                default:
                    throw new NotImplementedException();
            }
        }

        private static int ParseIndex(string number)
        {
            if (string.IsNullOrEmpty(number)) // collector number is not a good index.. older sets don't have collector numbers
                return 0;

            int end = number.Length;

            while (end-- > 0)
            {
                if (char.IsDigit(number[end]))
                    break;
            }

            if (end != number.Length - 1)
            {
                number = number.Substring(0, end + 1);
            }

            int result;
            int.TryParse(number, out result);
            return result;
        }

        private static Color ComputeColor(Type type, Card card)
        {
            Color result = Color.None;

            if (type.HasFlag(Type.Land) && card.colors == null && card.colorIdentity != null)
            {
                foreach (var color in card.colorIdentity)
                {
                    Debug.Assert(color.Length == 1);
                    result |= ColorExtensions.ParseSingleColor(color[0]);
                }

                return result;
            }

            if (card.colors != null)
            {
                foreach (var color in card.colors)
                {
                    result |= color;
                }
            }

            return result;
        }

        #endregion
    }
}
