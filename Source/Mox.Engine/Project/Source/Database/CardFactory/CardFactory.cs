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

using Mox.Abilities;
using Mox.Database;

namespace Mox
{
    public abstract class BaseCardFactory
    {
        protected static void InitializeFromDatabase(Card card, CardInfo cardInfo)
        {
            card.Type = cardInfo.Type;
            card.SubTypes = new SubTypes(cardInfo.SubTypes);
            card.Power = cardInfo.Power;
            card.Toughness = cardInfo.Toughness;
            card.Color = cardInfo.Color;

            AddImplicitAbilities(card, cardInfo);
        }

        private static void AddImplicitAbilities(Card card, CardInfo cardInfo)
        {
            if (cardInfo.Type.HasFlag(Type.Land))
            {
                AddLandAbilities(card, cardInfo);
            }
        }

        private static void AddLandAbilities(Card card, CardInfo cardInfo)
        {
            foreach (var subType in cardInfo.SubTypes)
            {
                switch (subType)
                {
                    case SubType.Plains:
                        CreateAbility<TapForManaAbility>(card).Color = Color.White;
                        break;
                    case SubType.Island:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Blue;
                        break;
                    case SubType.Swamp:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Black;
                        break;
                    case SubType.Mountain:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Red;
                        break;
                    case SubType.Forest:
                        CreateAbility<TapForManaAbility>(card).Color = Color.Green;
                        break;

                    default:
                        break;
                }
            }
        }

        protected static TAbility CreateAbility<TAbility>(Card card)
            where TAbility : Ability, new()
        {
            return card.Manager.CreateAbility<TAbility>(card);
        }
    }

    public abstract class CardFactory : BaseCardFactory, ICardFactory
    {
        #region Methods

        public CardFactoryResult InitializeCard(Card card, CardInfo cardInfo)
        {
            InitializeFromDatabase(card, cardInfo);

            var playAbility = CreatePlayCardAbility(card);
            playAbility.ManaCost = ManaCost.Parse(cardInfo.ManaCost);

            Initialize(card);

            return CardFactoryResult.Success;
        }

        protected virtual void Initialize(Card card)
        {
        }

        protected virtual PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return card.Manager.CreateAbility<PlayCardAbility>(card);
        }

        #endregion
    }
}
