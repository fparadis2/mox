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

namespace Mox.Database.Library
{
    public class EquipAbility : InPlayAbility
    {
        /// <summary>
        /// Mana cost to equip the card.
        /// </summary>
        public ManaCost ManaCost
        {
            get { return GetValue(ManaCostProperty); }
            set { SetValue(ManaCostProperty, value); }
        }

        public override sealed void Play(Spell spell)
        {
            TargetCost target = Target.Creature();
            spell.Costs.Add(target);

            spell.Costs.Add(PayMana(ManaCost));

            spell.Effect = s =>
            {
                s.Source.Attach((Card)s.Resolve(target));
            };
        }
    }
}
