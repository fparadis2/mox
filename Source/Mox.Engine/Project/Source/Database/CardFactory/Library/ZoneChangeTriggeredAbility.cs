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

using Mox.Events;
using Mox.Flow;

namespace Mox.Database.Library
{
    public abstract class ZoneChangeTriggeredAbility : TriggeredAbility, IEventHandler<ZoneChangeEvent>
    {
        #region Inner Types

        protected class ZoneChangeContext
        {
            public readonly Resolvable<Card> Card;

            public ZoneChangeContext(Card card)
            {
                Card = card;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the card can trigger the ability.
        /// </summary>
        /// <remarks>
        /// By default, checks if the card is the source.
        /// </remarks>
        /// <param name="card"></param>
        /// <returns></returns>
        protected virtual bool IsValidCard(Card card)
        {
            return card == Source;
        }

        /// <summary>
        /// Returns true if this is the zone from which the card should come from.
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        protected virtual bool IsTriggeringSourceZone(Zone zone)
        {
            return true;
        }

        private bool IsTriggeringTargetZone(Game game, ZoneChangeContext zoneChangeContext)
        {
            return zoneChangeContext.Card.Resolve(game).Zone.ZoneId == TriggerZone;
        }

        public override bool CanPushOnStack(Game game, object zoneChangeContext)
        {
            return IsTriggeringTargetZone(game, (ZoneChangeContext) zoneChangeContext);
        }

        public override sealed void Play(Spell spell)
        {
            ZoneChangeContext zoneChangeContext = (ZoneChangeContext)spell.Context;

            Play(spell, zoneChangeContext.Card);
        }

        protected internal override void ResolveSpellEffect(Part.Context context, Spell spell)
        {
            if (IsTriggeringTargetZone(context.Game, (ZoneChangeContext)spell.Context))
            {
                base.ResolveSpellEffect(context, spell);
            }
        }

        protected abstract void Play(Spell spell, Resolvable<Card> card);

        void IEventHandler<ZoneChangeEvent>.HandleEvent(Game game, ZoneChangeEvent e)
        {
            if (IsValidCard(e.Card) && IsTriggeringSourceZone(e.OldZone) && e.NewZone.ZoneId == TriggerZone)
            {
                Trigger(new ZoneChangeContext(e.Card));
            }
        }

        #endregion
    }
}
