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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// A part that resolves a spell.
    /// </summary>
    public class ResolveSpell : MTGPart
    {
        #region Variables

        private readonly Spell m_spell;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public ResolveSpell(Spell spell)
            : base(spell == null ? null : spell.Controller)
        {
            Throw.IfNull(spell, "spell");
            m_spell = spell;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Spell being resolved.
        /// </summary>
        public Spell Spell
        {
            get { return m_spell; }
        }

        #endregion

        #region Overrides of Part

        public override ControllerAccess ControllerAccess
        {
            get
            {
                return ControllerAccess.Multiple;
            }
        }

        public override Part<IGameController> Execute(Context context)
        {
            Spell spell = m_spell.Resolve(context.Game, false);
            if (spell.Effect != null)
            {
                spell.Effect(spell, context);
            }

            return null;
        }

        #endregion
    }
}
