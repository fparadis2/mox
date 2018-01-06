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
using System.Diagnostics;

namespace Mox
{
#warning todo: spell_v2 remove or merge elsewhere..
    public static class ITargetableExtensions
    {
        #region Methods

        public static void DealDamage(this ITargetable target, int damage)
        {
            // This is probably a bit simplistic :)
            if (target is Player)
            {
                ((Player)target).LoseLife(damage);
            }
            else if (target is Card)
            {
                Card card = (Card)target;
                Debug.Assert(card.Is(Type.Creature), "Damage not supported or implemented on anything else than creatures");
                card.Damage += damage;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}