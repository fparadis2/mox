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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace Mox.Database.Xml
{
    public class XmlParser
    {
        #region Variables

        private static readonly HashSet<string> m_ignoredSets = new HashSet<string>();
        private static readonly HashSet<string> m_creatureSubTypes = new HashSet<string>();

        private readonly CardDatabase m_cardDatabase;

        private readonly Dictionary<string, CardInstances> m_instances = new Dictionary<string, CardInstances>();
        private readonly Dictionary<string, CardInstancesBySet> m_instancesBySet = new Dictionary<string, CardInstancesBySet>();

        #endregion

        #region Constructor

        static XmlParser()
        {
            m_ignoredSets.Add("ATH"); // Anthologies
            m_ignoredSets.Add("ARC"); // Archenemy
            m_ignoredSets.Add("BRB"); // Battle Royale Box Set
            m_ignoredSets.Add("BTD"); // Beatdown Box Set
            m_ignoredSets.Add("DKM"); // Deckmasters

            // Duel decks
            m_ignoredSets.Add("DVD");
            m_ignoredSets.Add("EVG");
            m_ignoredSets.Add("GVL");
            m_ignoredSets.Add("JVC");
            m_ignoredSets.Add("PVC");

            // Un*
            m_ignoredSets.Add("UGL");
            m_ignoredSets.Add("UNH");

            // Planechase
            m_ignoredSets.Add("TBA");
            m_ignoredSets.Add("HOP");

            // Vanguard
            m_ignoredSets.Add("VGO");
            m_ignoredSets.Add("VG");
            m_ignoredSets.Add("VG1");
            m_ignoredSets.Add("VG2");
            m_ignoredSets.Add("VG3");
            m_ignoredSets.Add("VG4");
        }

        public XmlParser(CardDatabase database)
        {
            Throw.IfNull(database, "database");
            m_cardDatabase = database;
        }

        #endregion

        #region Methods

        #region Ignore List

        private static bool IgnoreSet(string setIdentifier)
        {
            if (string.IsNullOrEmpty(setIdentifier))
            {
                return true;
            }

            return m_ignoredSets.Contains(setIdentifier);
        }

        #endregion

        #region Entry point

        public void Parse(Stream setsStream, Stream cardsStream, Stream instancesStream)
        {
            m_instances.Clear();
            m_instancesBySet.Clear();

            ParseSets(setsStream);
            ParseCardInstances(instancesStream);
            ParseCards(cardsStream);

            PrepareInstances();
        }

        #endregion

        #region Sets

        private void ParseSets(Stream setsStream)
        {
            XPathDocument document = new XPathDocument(setsStream);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XPathNavigator cardNavigator in navigator.Select("//set"))
            {
                ParseSet(cardNavigator);
            }
        }

        private void ParseSet(XPathNavigator navigator)
        {
            string identifier = navigator.GetElementValue("code");
            string name = navigator.GetElementValue("name");
            string block = navigator.GetElementValue("block");

            if (IgnoreSet(identifier))
            {
                return;
            }

            DateTime releaseDate = navigator.GetElementValue("release-date", DateTime.Parse);

            m_cardDatabase.AddSet(identifier, name, block, releaseDate);
        }

        #endregion

        #region Card Instances

        private void ParseCardInstances(Stream instancesStream)
        {
            XPathDocument document = new XPathDocument(instancesStream);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XPathNavigator cardNavigator in navigator.Select("//card"))
            {
                ParseCardInstance(cardNavigator);
            }
        }

        private void ParseCardInstance(XPathNavigator navigator)
        {
            string name = navigator.GetElementValue("name");

            foreach (XPathNavigator cardNavigator in navigator.Select("instance"))
            {
                ParseCardInstance(name, cardNavigator);
            }
        }

        private void ParseCardInstance(string name, XPathNavigator navigator)
        {
            string setIdentifier = navigator.GetElementValue("set");

            if (IgnoreSet(setIdentifier))
            {
                return;
            }

            Instance instance = new Instance(m_cardDatabase.Sets[setIdentifier], name)
            {
                Rarity = RarityHelper.FromSymbol(navigator.GetElementValue("rarity")),
                Index = navigator.GetElementValue("number", s => int.Parse(s)),
                Artist = navigator.GetElementValue("artist")
            };

            AddTo(m_instances, instance, instance.Name);
            AddTo(m_instancesBySet, instance, instance.Set.Identifier);
        }

        private static void AddTo<TKey, TInstances>(IDictionary<TKey, TInstances> store, Instance instance, TKey key)
            where TInstances : IList<Instance>, new()
        {
            TInstances instances;
            if (!store.TryGetValue(key, out instances))
            {
                instances = new TInstances();
                store.Add(key, instances);
            }
            instances.Add(instance);
        }

        private void PrepareInstances()
        {
            foreach (var set in m_instancesBySet.Values)
            {
                set.Prepare(m_cardDatabase);
            }
        }

        #endregion

        #region Cards

        private void ParseCards(Stream cardsStream)
        {
            XPathDocument document = new XPathDocument(cardsStream);
            XPathNavigator navigator = document.CreateNavigator();

            foreach (XPathNavigator cardNavigator in navigator.Select("//card"))
            {
                ParseCard(cardNavigator);
            }
        }

        private void ParseCard(XPathNavigator navigator)
        {
            string name = navigator.GetElementValue("name");

            // Check if card has instances
            CardInstances instances;
            if (!m_instances.TryGetValue(name, out instances))
            {
                return;
            }

            string manacost = navigator.GetElementValue("cost");

            Type type;
            SuperType superType;
            List<SubType> subTypes;
            ParseTypes(navigator.GetElementValues("typelist/type"), out superType, out type, out subTypes);

            string power = navigator.GetElementValue("pow");
            string toughness = navigator.GetElementValue("tgh");

            IEnumerable<string> abilities = navigator.GetElementValues("rulelist/rule");

            CardInfo card = m_cardDatabase.AddCard(name, manacost, superType, type, subTypes, power, toughness, abilities);

            foreach (Instance instance in instances)
            {
                Debug.Assert(instance.Card == null);
                instance.Card = card;
            }
        }

        private static void ParseTypes(IEnumerable<string> typeList, out SuperType superType, out Type type, out List<SubType> subTypes)
        {
            type = Type.None;
            superType = SuperType.None;
            subTypes = new List<SubType>();

            foreach (string typeToken in typeList)
            {
                string token = typeToken.Replace("-", string.Empty);
                token = token.Replace("’", string.Empty);

                SuperType theSuperType;
                Type theType;
                SubType theSubType;
                if (TryParseEnum(token, out theSuperType))
                {
                    superType |= theSuperType;
                }
                else if (TryParseEnum(token, out theType))
                {
                    type |= theType;
                }
                else if (TryParseEnum(token, out theSubType))
                {
                    subTypes.Add(theSubType);

                    if ((type & Type.Creature) == Type.Creature)
                    {
                        m_creatureSubTypes.Add(token);
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format("Unknown subtype: {0}", token));

                    if ((type & Type.Creature) == Type.Creature)
                    {
                        m_creatureSubTypes.Add(token);
                    }
                }
            }
        }

        private static bool TryParseEnum<T>(string text, out T result)
        {
            if (Enum.IsDefined(typeof(T), text))
            {
                result = (T)Enum.Parse(typeof(T), text);
                return true;
            }

            result = default(T);
            return false;
        }

        #endregion

        #region Utilities

        internal static string Print_SubTypes()
        {
            StringBuilder builder = new StringBuilder();

            foreach (string subType in m_creatureSubTypes.OrderBy(s => s))
            {
                builder.AppendLine(subType + ",");
            }

            return builder.ToString();
        }

        #endregion

        #endregion

        #region Inner Types

        private class CardInstances : List<Instance>
        {
        }

        private class CardInstancesBySet : List<Instance>
        {
            public void Prepare(CardDatabase database)
            {
                if (this.Any(i => i.Index == 0))
                {
                    Debug.Assert(this.All(i => i.Index == 0));
                    Debug.Assert(this.All(i => i.Card != null));

                    Sort(CompareInstances);

                    for (int i = 0; i < Count; i++)
                    {
                        this[i].Index = i + 1;
                    }
                }

                foreach (Instance instance in this)
                {
#warning [LOW] support "multi" cards
                    if (instance.Card != null)
                    {
                        database.AddCardInstance(instance.Card, instance.Set, instance.Rarity, instance.Index, instance.Artist);
                    }
                }
            }

            private static int CompareInstances(Instance a, Instance b)
            {
                int compare = GetColorOrder(a.Card.Color).CompareTo(GetColorOrder(b.Card.Color));
                if (compare != 0)
                {
                    return compare;
                }

                compare = GetColorOrder(a.Card.Type).CompareTo(GetColorOrder(b.Card.Type));
                if (compare != 0)
                {
                    return compare;
                }

                return a.Name.CompareTo(b.Name);
            }

            private static int GetColorOrder(Color color)
            {
                switch (color)
                {
                    case Color.Black:
                        return 0;
                    case Color.Blue:
                        return 1;
                    case Color.Green:
                        return 2;
                    case Color.Red:
                        return 3;
                    case Color.White:
                        return 4;
                    case Color.None:
                        return 5;
                    default:
                        return 6;
                }
            }

            private static int GetColorOrder(Type type)
            {
                if (type.Is(Type.Land))
                {
                    return 1;
                }

                return 0;
            }
        }

        private class Instance
        {
            private readonly SetInfo m_set;
            private readonly string m_name;

            public Instance(SetInfo set, string name)
            {
                Debug.Assert(set != null);
                Debug.Assert(!string.IsNullOrEmpty(name));
                m_set = set;
                m_name = name;
            }

            public SetInfo Set { get { return m_set; } }
            public string Name { get { return m_name; } }

            public CardInfo Card { get; set; }

            public Rarity Rarity { get; set; }
            public int Index { get; set; }
            public string Artist { get; set; }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, Set.Name);
            }
        }

        #endregion
    }
}