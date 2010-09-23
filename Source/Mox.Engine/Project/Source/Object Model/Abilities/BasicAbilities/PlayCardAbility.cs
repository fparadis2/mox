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

        /// <summary>
        /// Mana cost to play the card.
        /// </summary>
        public ManaCost ManaCost
        {
            get { return GetValue(ManaCostProperty); }
            set { SetValue(ManaCostProperty, value); }
        }

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
        public override sealed IEnumerable<ImmediateCost> Play(Spell spell)
        {
            if (!CanPlayImpl(spell))
            {
                yield return CannotPlay;
            }

            if (spell.Source.Is(Type.Land))
            {
                if (!OneLandPerTurn.CanPlayLand(Game))
                {
                    yield return CannotPlay;
                }

                spell.UseStack = false;
            }

            Spell innerSpell = spell.Resolve(spell.Game, true);
            IEnumerable<ImmediateCost> specificCosts = PlaySpecific(innerSpell);
            if (specificCosts != null)
            {
                foreach (ImmediateCost cost in specificCosts)
                {
                    yield return cost;
                }
            }

            innerSpell.DelayedCosts.ForEach(spell.DelayedCosts.Add);

            spell.DelayedCosts.Add(PayMana(ManaCost));

            spell.PreEffect = (s, c) =>
            {
                if (s.Source.Is(Type.Land))
                {
                    OneLandPerTurn.PlayOneLand(s.Game);
                }

                if (s.UseStack)
                {
                    s.Source.Zone = s.Source.Manager.Zones.Stack;
                }

                if (innerSpell.PreEffect != null)
                {
                    innerSpell.PreEffect(s, c);
                }
            };

            spell.Effect = (s, c) =>
            {
                if (innerSpell.Effect != null)
                {
                    innerSpell.Effect(s, c);
                }

                Zone zone = s.Source.Zone;
                if (zone == s.Game.Zones.Hand || zone == s.Game.Zones.Stack)
                {
                    s.Source.Zone = GetTargetZone(s);
                }
            };
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
        protected virtual IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
        {
            yield break;
        }

        private static Zone GetTargetZone(Spell spell)
        {
            if (spell.Source.IsPermanent())
            {
                return spell.Game.Zones.Battlefield;
            }
            else
            {
                return spell.Game.Zones.Graveyard;
            }
        }

        #endregion
    }
}
