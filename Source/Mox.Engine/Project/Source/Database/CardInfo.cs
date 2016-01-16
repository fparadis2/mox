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
using System.Linq;
using System.Text;

namespace Mox.Database
{
    /// <summary>
    /// Contains information about a card.
    /// </summary>
    public class CardInfo
    {
        #region Variables

        private readonly CardDatabase m_database;

        private readonly string m_name;
        private readonly string m_manaCost;

        private readonly SuperType m_superType;
        private readonly Type m_type;
        private readonly List<SubType> m_subTypes = new List<SubType>();

        private readonly string m_power;
        private readonly string m_toughness;

        private readonly string m_text;

        #endregion

        #region Constructor

        internal CardInfo(CardDatabase database, string name, string manaCost, SuperType superType, Type type, IEnumerable<SubType> subTypes, string power, string toughness, string text)
        {
            Throw.IfNull(database, "database");
            Throw.InvalidArgumentIf(type == Type.None, "A card must have a type", "type");

            m_database = database;
            m_name = name;
            m_manaCost = manaCost;

            m_superType = superType;
            m_type = type;

            if (subTypes != null)
            {
                m_subTypes.AddRange(subTypes);
            }

            m_power = power;
            m_toughness = toughness;
            m_text = text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Database from which the card is.
        /// </summary>
        public CardDatabase Database
        {
            get { return m_database; }
        }

        /// <summary>
        /// Editions of this card.
        /// </summary>
        public IEnumerable<CardInstanceInfo> Instances
        {
            get { return m_database.GetCardInstances(this); }
        }

        /// <summary>
        /// Name of the card.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Mana Cost, as indicated on the card.
        /// </summary>
        public string ManaCost
        {
            get { return m_manaCost; }
        }

        /// <summary>
        /// Color of the card.
        /// </summary>
        public Color Color
        {
            get
            {
                Color color = Color.None;

                foreach (ManaSymbol symbol in Mox.ManaCost.Parse(ManaCost).Symbols)
                {
                    color |= ManaSymbolHelper.GetColor(symbol);
                }

                return color;
            }
        }

        /// <summary>
        /// Supertype of the card (basic, legendary), if any.
        /// </summary>
        public SuperType SuperType
        {
            get { return m_superType; }
        }

        /// <summary>
        /// Type of the card.
        /// </summary>
        public Type Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// SubTypes of the card.
        /// </summary>
        public IEnumerable<SubType> SubTypes
        {
            get { return m_subTypes.AsReadOnly(); }
        }

        /// <summary>
        /// Base power of the card.
        /// </summary>
        public string PowerString
        {
            get { return m_power; }
        }

        /// <summary>
        /// Base toughness of the card.
        /// </summary>
        public string ToughnessString
        {
            get { return m_toughness; }
        }

        /// <summary>
        /// Base power of the card.
        /// </summary>
        public int Power
        {
            get { return ParsePT(m_power); }
        }

        /// <summary>
        /// Base toughness of the card.
        /// </summary>
        public int Toughness
        {
            get { return ParsePT(m_toughness); }
        }

        /// <summary>
        /// Abilities on the card.
        /// </summary>
        public string Text
        {
            get { return m_text; }
        }

        public string TypeLine
        {
            get
            {
                string superType = SuperType == SuperType.None ? string.Empty : FormatSuperType(SuperType) + " ";
                string types = FormatType(Type);
                string subTypes = SubTypes.Any() ? " — " + SubTypes.Join(" ") : string.Empty;
                return superType + types + subTypes;
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        public string ToOracleString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(Name);

            if (!string.IsNullOrEmpty(ManaCost))
            {
                builder.AppendLine(ManaCost);
            }

            string typeLine = FormatSuperType(SuperType);
            if (!string.IsNullOrEmpty(typeLine))
            {
                typeLine += " ";
            }
            typeLine += FormatType(Type);
            if (SubTypes.Any())
            {
                typeLine += " - " + SubTypes.Join(" ");
            }
            builder.AppendLine(typeLine);

            if (Type == Type.Creature)
            {
                builder.AppendFormat("{0}/{1}{2}", PowerString, ToughnessString, Environment.NewLine);
            }

            builder.Append(Text);

            return builder.ToString();
        }

        public static string NormalizeCardName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                name = name.Replace(' ', '_');
                name = name.Replace("\'", "");
                name = name.Replace(",", "");
            }
            return name;
        }

        private static int ParsePT(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int result;
            if (!int.TryParse(value, out result))
            {
                result = -1;
            }
            return result;
        }

        private static string FormatType(Type tested)
        {
            return Enum.GetValues(typeof(Type)).Cast<Type>().Where(type => (tested & type) != Type.None).Join(" ");
        }

        private static string FormatSuperType(SuperType tested)
        {
            return Enum.GetValues(typeof(SuperType)).Cast<SuperType>().Where(type => (tested & type) != SuperType.None).Join(" ");
        }

        #endregion
    }
}
