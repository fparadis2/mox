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

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    public class RemoveSpell : Part
    {
        private readonly Resolvable<Spell2> m_spell;

        public RemoveSpell(Spell2 spell)
        {
            m_spell = spell;
        }

        public override Part Execute(Context context)
        {
            m_spell.Resolve(context.Game).Remove();
            return null;
        }
    }
}
