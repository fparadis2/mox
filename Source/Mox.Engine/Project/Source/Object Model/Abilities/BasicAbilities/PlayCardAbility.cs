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
using Mox.Flow;
using System.Collections.Generic;

namespace Mox.Abilities
{
    public abstract class SpellAbility : Ability
    {
        #region Methods

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (!base.CanPlay(evaluationContext))
                return false;

            Spell spell = new Spell(this, evaluationContext.Player, evaluationContext.AbilityContext);

            Play(spell);
            if (!CanExecute(spell.Costs, evaluationContext))
            {
                return false;
            }

            return true;
        }

        private bool CanExecute<TCost>(IEnumerable<TCost> costs, AbilityEvaluationContext evaluationContext)
            where TCost : Cost
        {
            if (costs != null)
            {
                foreach (TCost cost in costs)
                {
                    if (!cost.CanExecute(Manager, evaluationContext))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes the given spell and returns the "pre payment" costs associated with the spell (asks players for modal choices, {X} choices, etc...)
        /// </summary>
        /// <param name="spell"></param>
        public abstract void Play(Spell spell);

        /// <summary>
        /// Called when the spell is pushed
        /// </summary>
        public virtual void Push(Spell spell)
        {
        }

        /// <summary>
        /// Called when the spell resolves
        /// </summary>
        public virtual void Resolve(Part.Context context, Spell spell)
        {
            if (spell.EffectPart != null)
            {
                ISpellEffectPart spellEffectPart = spell.EffectPart as ISpellEffectPart;
                if (spellEffectPart != null)
                {
                    spellEffectPart.PushSpell(context, spell);
                }

                context.Schedule(spell.EffectPart);
            }
        }

        #endregion
    }

    public abstract class PlayAbilityTemp : SpellAbility
    {
        #region Variables

        private ManaCost m_manaCost;
        public static readonly Property<ManaCost> ManaCostProperty = Property<ManaCost>.RegisterProperty<PlayAbilityTemp>("ManaCost", a => a.m_manaCost, PropertyFlags.Private);

        #endregion

        #region Properties

        /// <summary>
        /// Mana cost to play the ability
        /// </summary>
        public ManaCost ManaCost
        {
            get { return m_manaCost; }
            set { SetValue(ManaCostProperty, value, ref m_manaCost); }
        }

        #endregion
    }

    /// <summary>
    /// "Ability" of a card to be played.
    /// </summary>
    /// <remarks>
    /// Not an ability in comp. rules but easier to program it this way.
    /// </remarks>
    public class PlayCardAbility : PlayAbilityTemp
    {
        #region Properties

        public override AbilitySpeed AbilitySpeed
        {
            get { return Source.Is(Type.Instant) || Source.HasAbility<FlashAbility>() ? AbilitySpeed.Instant : AbilitySpeed.Sorcery; }
        }

        public override string AbilityText
        {
            get { return null; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Plays the card.
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public override sealed void Play(Spell spell)
        {
            if (!CanPlayImpl(spell))
            {
                spell.AddCost(CannotPlay);
                return;
            }

            if (spell.Source.Is(Type.Land))
            {
                if (!Game.TurnData.CanPlayLand)
                {
                    spell.AddCost(CannotPlay);
                    return;
                }

                spell.UseStack = false;
            }

            Spell innerSpell = spell.Resolve(spell.Game, true);
            PlaySpecific(innerSpell);

            innerSpell.Costs.ForEach(spell.AddCost);
            spell.AddCost(PayMana(ManaCost));
            spell.EffectPart = innerSpell.EffectPart;
        }

        public override void Push(Spell spell)
        {
            if (spell.Source.Is(Type.Land))
            {
                spell.Game.TurnData.PlayOneLand();
            }

            if (spell.UseStack)
            {
                spell.Source.Zone = spell.Game.Zones.Stack;
            }

            base.Push(spell);
        }

        public override void Resolve(Part.Context context, Spell spell)
        {
            base.Resolve(context, spell);
            context.Schedule(new PutSpellSourceInTargetZone(spell));
        }

        private bool CanPlayImpl(Spell spell)
        {
            // Can only "play" cards from the hand.
            if (Source.Zone != Source.Manager.Zones.Hand)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Implements the specific part of the ability (usually for instants/sorceries)
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        protected virtual void PlaySpecific(Spell spell)
        {
        }

        #endregion

        #region Inner Types

        private class PutSpellSourceInTargetZone : Part
        {
            private readonly Spell m_spell;

            public PutSpellSourceInTargetZone(Spell spell)
            {
                m_spell = spell;
            }

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                var source = m_spell.Resolve(context.Game, false).Source;

                Zone zone = source.Zone;
                if (zone == source.Manager.Zones.Hand || zone == source.Manager.Zones.Stack)
                {
                    source.Zone = GetTargetZone(source);
                }
                return null;
            }

            private static Zone GetTargetZone(Card card)
            {
                if (card.IsPermanent())
                {
                    return card.Manager.Zones.Battlefield;
                }

                return card.Manager.Zones.Graveyard;
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Cast ability
    /// </summary>
    public class PlayCardAbility2 : SpellAbility2
    {
        #region Properties

        public override AbilitySpeed AbilitySpeed
        {
            get { return Source.Is(Type.Instant) || Source.HasAbility<FlashAbility>() ? AbilitySpeed.Instant : AbilitySpeed.Sorcery; }
        }

        public override bool UseStack
        {
            get
            {
                if (Source.Is(Type.Land))
                    return false;

                return base.UseStack;
            }
        }

        #endregion

        #region Methods

        public override bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            if (!base.CanPlay(evaluationContext))
                return false;

            // Can only "play" cards from the hand.
            if (Source.Zone != Source.Manager.Zones.Hand)
                return false;

            return true;
        }

        public override void Push(Part.Context context, Player controller)
        {
            if (Source.Is(Type.Land))
            {
                context.Game.TurnData.PlayOneLand();
            }

            if (UseStack)
            {
                Source.Zone = context.Game.Zones.Stack;
            }

            base.Push(context, controller);
        }

        public override void Resolve(Part.Context context, Player controller)
        {
            base.Resolve(context, controller);
            context.Schedule(new PutSourceInTargetZone(this));
        }

        #endregion

        #region Inner Types

        private class PutSourceInTargetZone : Part
        {
            private readonly Resolvable<Ability> m_ability;

            public PutSourceInTargetZone(Resolvable<Ability> ability)
            {
                m_ability = ability;
            }

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                var source = m_ability.Resolve(context.Game).Source;

                Zone zone = source.Zone;
                if (zone == source.Manager.Zones.Hand || zone == source.Manager.Zones.Stack)
                {
                    source.Zone = GetTargetZone(source);
                }
                return null;
            }

            private static Zone GetTargetZone(Card card)
            {
                if (card.IsPermanent())
                {
                    return card.Manager.Zones.Battlefield;
                }

                return card.Manager.Zones.Graveyard;
            }

            #endregion
        }

        #endregion
    }
}
