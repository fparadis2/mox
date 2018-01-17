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

namespace Mox.Abilities
{
    public class EnchantAbility : PlayCardAbility
    {
#warning todo spell_v2
        /*protected override sealed void PlaySpecific(Spell spell)
        {
            TargetCost target = CreateTargetCost();
            spell.AddCost(target);

            spell.Effect = s =>
            {
                s.Source.Attach((Card)s.Resolve(target));
            };
        }*/

        protected virtual TargetCost CreateTargetCost()
        {
            return Target.Creature();
        }
    }
}
