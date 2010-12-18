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
using Mox.Database;

namespace Mox
{
    public class CompoundCardFactory : ICardFactory, IMasterCardFactory
    {
        #region Variables

        private readonly Dictionary<string, ICardFactory> m_subFactories = new Dictionary<string, ICardFactory>();

        #endregion

        #region Properties

        public int Count
        {
            get { return m_subFactories.Count; }
        }

        public ICollection<string> SupportedCards
        {
            get { return m_subFactories.Keys; }
        }

        #endregion

        #region Methods

        public void InitializeCard(Card card)
        {
            string name = card.Name;

            ICardFactory subFactory;
            if (m_subFactories.TryGetValue(name, out subFactory))
            {
                subFactory.InitializeCard(card);
            }
            else
            {
                if (!MasterCardDatabase.Instance.Cards.ContainsKey(name))
                {
                    Trace.TraceError("Unknown card: {0}", name);
                }
                else
                {
                    Trace.TraceError("Card {0} is not implemented yet.", name);
                }
            }
        }

        public bool IsDefined(string cardName)
        {
            return m_subFactories.ContainsKey(cardName);
        }

        protected void Register(string cardName, ICardFactory cardFactory)
        {
            Throw.IfEmpty(cardName, "cardName");
            Throw.IfNull(cardFactory, "cardFactory");

            m_subFactories.Add(cardName, cardFactory);
        }

        #endregion
    }
}
