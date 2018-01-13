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
using System.Reflection;

namespace Mox.Database
{
    /// <summary>
    /// The card factory to rule them all.
    /// </summary>
    public class CardBuilder : ICardFactory
    {
        #region Variables

        private readonly ICardDatabase m_database;
        private readonly AssemblyCardFactory m_assemblyFactory = new AssemblyCardFactory(typeof(CardBuilder).Assembly);

        private readonly Dictionary<string, ICardFactory> m_factories = new Dictionary<string, ICardFactory>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructor

        public CardBuilder(ICardDatabase database)
        {
            m_database = database;
        }

        #endregion

        #region Methods

        public CardFactoryResult InitializeCard(Card card)
        {
            if (!m_factories.TryGetValue(card.Name, out ICardFactory factory))
            {
                CardInfo cardInfo = m_database.GetCard(card.Name);
                Throw.InvalidArgumentIf(cardInfo == null, "Unknown card: " + card.Name, "card");
                factory = CreateFactory(card.Name, cardInfo);
                m_factories.Add(card.Name, factory);
            }

            if (factory == null)
            {
                return CardFactoryResult.NotImplemented($"Card {card.Name} is not supported.");
            }

            return factory.InitializeCard(card);
        }

        private ICardFactory CreateFactory(string name, CardInfo cardInfo)
        {
            var factory = m_assemblyFactory.CreateFactory(name, cardInfo);
            if (factory  != null)
                return factory;

            return RuleParserCardFactory.Create(cardInfo);
        }

        #endregion
    }
}
