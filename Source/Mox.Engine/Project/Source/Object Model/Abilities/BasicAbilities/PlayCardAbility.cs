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
using Mox.Rules;

namespace Mox
{
    /// <summary>
    /// "Ability" of a card to be played.
    /// </summary>
    /// <remarks>
    /// Not an ability in comp. rules but easier to program it this way.
    /// </remarks>
    public class PlayCardAbility : Ability
    {
        #region Properties

        public override AbilitySpeed AbilitySpeed
        {
            get { return Source.Is(Type.Instant) || Source.HasAbility<FlashAbility>() ? AbilitySpeed.Instant : AbilitySpeed.Sorcery; }
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
                spell.Costs.Add(CannotPlay);
                return;
            }

            if (spell.Source.Is(Type.Land))
            {
                if (!Game.TurnData.CanPlayLand)
                {
                    spell.Costs.Add(CannotPlay);
                    return;
                }

                spell.UseStack = false;
            }

            Spell innerSpell = spell.Resolve(spell.Game, true);
            PlaySpecific(innerSpell);

            innerSpell.Costs.ForEach(spell.Costs.Add);
            spell.Costs.Add(PayMana(ManaCost));
            spell.EffectPart = innerSpell.EffectPart;

            spell.PushEffect = s =>
            {
                if (s.Source.Is(Type.Land))
                {
                    s.Game.TurnData.PlayOneLand();
                }

                if (s.UseStack)
                {
                    s.Source.Zone = s.Source.Manager.Zones.Stack;
                }

                if (innerSpell.PushEffect != null)
                {
                    innerSpell.PushEffect(s);
                }
            };
        }

        protected internal override void ResolveSpellEffect(Part.Context context, Spell spell)
        {
            base.ResolveSpellEffect(context, spell);
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
}
