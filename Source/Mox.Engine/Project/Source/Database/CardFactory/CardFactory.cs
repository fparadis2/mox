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
        #region Properties

        public CardInfo CardInfo
        {
            get;
            internal set;
        }

        #endregion

        protected void InitializeFromDatabase(Card card)
        {
            card.Type = CardInfo.Type;
            card.SubTypes = new SubTypes(CardInfo.SubTypes);
            card.Power = CardInfo.Power;
            card.Toughness = CardInfo.Toughness;
            card.Color = CardInfo.Color;
        }

        protected static TAbility CreateAbility<TAbility>(Card card)
            where TAbility : Ability, new()
        {
            //return card.Manager.CreateAbility<TAbility>(card);
            throw new NotSupportedException();
        }

        protected static TAbility CreateAbility<TAbility>(Card card, SpellDefinition spell)
            where TAbility : Ability, new()
        {            
            return card.Manager.CreateAbility<TAbility>(card, spell);
        }
    }

    public abstract class CardFactory : BaseCardFactory, ICardFactory
    {
        #region Variables

        private int m_spellIndex;

        #endregion

        #region Methods

        protected SpellDefinition CreateSpell()
        {
            var identifier = new SpellDefinitionIdentifier
            {
                SourceName = CardInfo.Name,
                Id = m_spellIndex++
            };

            return new SpellDefinition(identifier);
        }

        public virtual void Build()
        { }

        public CardFactoryResult InitializeCard(Card card)
        {
            InitializeFromDatabase(card);

            var playSpell = CreateSpell();
            CreatePlayCardSpell(playSpell);
            CreateAbility<PlayCardAbility>(card, playSpell);

            Initialize(card);

            return CardFactoryResult.Success;
        }

        protected virtual void Initialize(Card card)
        {
        }

        protected virtual PlayCardAbility CreatePlayCardAbility(Card card)
        {
#warning todo spell_v2
            throw new NotImplementedException();
            //return card.Manager.CreateAbility<PlayCardAbility>(card);
        }

        protected virtual void CreatePlayCardSpell(SpellDefinition spell)
        {
            spell.AddCost(new PayManaCost(ManaCost.Parse(CardInfo.ManaCost)));
        }

        #endregion

#warning todo spell_v2 hopefully temporary

        #region Basic Costs

        /// <summary>
        /// A cost that requires that the object be tapped.
        /// </summary>
        protected static Cost Tap(Card card)
        {
            return Cost.Tap(card);
        }

        /// <summary>
        /// A cost that requires the source of the spell to be tapped.
        /// </summary>
        protected static Cost TapSelf()
        {
            return new TapCost(ObjectResolver.SpellSource, true);
        }

        /// <summary>
        /// A cost that requires the controller to pay the given <paramref name="manaCost"/>.
        /// </summary>
        /// <param name="manaCost"></param>
        /// <returns></returns>
        protected static Cost PayMana(ManaCost manaCost)
        {
            return new PayManaCost(manaCost);
        }

        /// <summary>
        /// A cost that requires the controller to pay the given <paramref name="manaCost"/>.
        /// </summary>
        /// <param name="manaCost"></param>
        /// <returns></returns>
        protected static Cost PayMana(string manaCost)
        {
            return PayMana(ManaCost.Parse(manaCost));
        }

        #endregion
    }
}
