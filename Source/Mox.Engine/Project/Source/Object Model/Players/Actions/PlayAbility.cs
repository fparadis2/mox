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

using Mox.Flow;

namespace Mox
{
    /// <summary>
    /// Plays an ability.
    /// </summary>
    [Serializable]
    public class PlayAbility : PlayerAction
    {
        #region Variables

        private readonly Resolvable<Ability> m_ability;

#if DEBUG
        private readonly string m_name;
#endif

        #endregion

        #region Constructor

        public PlayAbility(Ability ability)
        {
            Throw.IfNull(ability, "ability");
            m_ability = ability;

#if DEBUG
            m_name = ability.GetType().Name + " on " + ability.Source.Name;
#endif
        }

        #endregion

        #region Properties

        public Resolvable<Ability> Ability
        {
            get { return m_ability; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override bool CanExecute(ExecutionEvaluationContext evaluationContext)
        {
            return m_ability.Resolve(evaluationContext.Player.Manager).CanPlay(evaluationContext);
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        public override void Execute(Part.Context context, Player player)
        {
            Debug.Assert(player.Manager == context.Game, "Cross-game operation");
            context.Schedule(new Flow.Parts.PlayAbility(player, m_ability, null));
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("[Play ability: {0}]", m_name);
        }
#endif

        #endregion
    }
}
