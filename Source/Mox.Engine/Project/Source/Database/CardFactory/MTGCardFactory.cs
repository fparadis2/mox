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
using System.Text;

using Mox.Database;

namespace Mox
{
    public abstract class MTGCardFactory : ICardFactory
    {
        #region Inner Classes

        public class InitializationContext
        {
            public readonly PlayCardAbility PlayCardAbility;

            public InitializationContext(PlayCardAbility playCardAbility)
            {
                Debug.Assert(playCardAbility != null);
                PlayCardAbility = playCardAbility;
            }
        }

        #endregion

        #region Methods

        public void InitializeCard(Card card)
        {
            InitializationContext context = new InitializationContext(CreatePlayCardAbility(card));

            InitializeFromDatabase(card, context);
            Initialize(card, context);
        }

        protected virtual void Initialize(Card card, InitializationContext context)
        {
        }

        protected virtual PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return card.Manager.CreateAbility<PlayCardAbility>(card);
        }

        private static void InitializeFromDatabase(Card card, InitializationContext context)
        {
            CardInfo cardInfo;
            if (!MasterCardDatabase.Instance.Cards.TryGetValue(card.Name, out cardInfo))
            {
                Throw.InvalidArgumentIf(cardInfo == null, "Unknown card: " + card.Name, "card");
            }

            card.Type = cardInfo.Type;
            card.SubTypes = new SubTypes(cardInfo.SubTypes);
            card.Power = cardInfo.Power;
            card.Toughness = cardInfo.Toughness;
            card.Color = cardInfo.Color;

            context.PlayCardAbility.ManaCost = ManaCost.Parse(cardInfo.ManaCost);
        }

        #region Helpers

        protected static TAbility CreateAbility<TAbility>(Card card)
            where TAbility : Ability, new()
        {
            return card.Manager.CreateAbility<TAbility>(card);
        }

        #endregion

        #endregion
    }
}
