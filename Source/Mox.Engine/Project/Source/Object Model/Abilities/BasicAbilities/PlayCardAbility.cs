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
    /// <summary>
    /// Cast ability
    /// </summary>
    public class PlayCardAbility : SpellAbility
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

            if (Source.Is(Type.Land))
            {
                if (!Game.TurnData.CanPlayLand)
                    return false;
            }

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
