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

namespace Mox
{
    public class TrackingEffectInstance : GlobalEffectInstance
    {
        #region Properties

        private readonly Zone.Id m_zone = Zone.Id.Battlefield;
        public static readonly Property<Zone.Id> ZoneProperty = Property<Zone.Id>.RegisterProperty<TrackingEffectInstance>("ZoneId", instance => instance.m_zone);

        private readonly Condition m_condition = null;
        public static readonly Property<Condition> ConditionProperty = Property<Condition>.RegisterProperty<TrackingEffectInstance>("Condition", instance => instance.m_condition);

        #endregion

        #region Properties

        public Zone.Id TrackedZone
        {
            get { return m_zone; }
        }

        public Condition Condition
        {
            get { return m_condition; }
        }

        private Game Game
        {
            get { return (Game) Manager; }
        }

        #endregion

        #region Methods

        protected override IEnumerable<Object> InitObjects()
        {
            return Update(Condition);
        }

        private IEnumerable<Object> Update(Condition condition)
        {
            Zone.Id trackedZone = TrackedZone;

            foreach (Card affectedCard in AffectedObjects.ToList())
            {
                if (affectedCard.GetValue(Card.ZoneIdProperty) != trackedZone || !condition.Matches(affectedCard))
                {
                    if (RemoveAffectedObject(affectedCard))
                    {
                        yield return affectedCard;
                    }
                }
            }

            foreach (Card card in Game.Zones[trackedZone].AllCards.Where(condition.Matches))
            {
                if (AddAffectedObject(card))
                {
                    yield return card;
                }
            }
        }

        protected override IEnumerable<Object> Invalidate(Object sender, PropertyBase property)
        {
            var condition = Condition;

            if (property == Card.ZoneIdProperty)
            {
                Card changedCard = (Card)sender;
                Zone.Id newValue = changedCard.GetValue(Card.ZoneIdProperty);

                if (newValue != TrackedZone)
                {
                    if (RemoveAffectedObject(changedCard))
                    {
                        yield return changedCard;
                    }
                }
                else if (condition.Matches(changedCard))
                {
                    if (AddAffectedObject(changedCard))
                    {
                        yield return changedCard;
                    }
                }
            }
            
            if (condition.Invalidate(property))
            {
                foreach (var obj in Update(condition))
                {
                    yield return obj;
                }
            }

            foreach (var obj in BaseInvalidate(sender, property))
            {
                yield return obj;
            }
        }

        private IEnumerable<Object> BaseInvalidate(Object sender, PropertyBase property)
        {
            return base.Invalidate(sender, property);
        }

        #endregion
    }
}
