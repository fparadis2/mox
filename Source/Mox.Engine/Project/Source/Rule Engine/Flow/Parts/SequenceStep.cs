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

using Mox.Flow.Phases;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Plays a step
    /// </summary>
    public class SequenceStep : PlayerPart
    {
        #region Variables

        private readonly Step m_step;

        #endregion

        #region Constructor

        public SequenceStep(Player player, Step step)
            : base(player)
        {
            Throw.IfNull(step, "step");

            m_step = step;
        }

        #endregion

        #region Properties

        public Step Step
        {
            get { return m_step; }
        }

        #endregion

        #region Overrides of Part<IGameController>

        public override NewPart Execute(Context context)
        {
            context.Game.TargetData.Clear();
            return m_step.Sequence(context, GetPlayer(context));
        }

        #endregion
    }
}
