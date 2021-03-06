﻿// Copyright (c) François Paradis
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

namespace Mox.Flow.Phases
{
    public class EndOfCombatStep : Step
    {
        #region Inner Types

        private class ResetCombatData : Part
        {
            public override Part Execute(Context context)
            {
                context.Game.CombatData.ResetAllValues();
                return null;
            }
        }

        #endregion

        #region Constructor

        public EndOfCombatStep() 
            : base(Steps.EndOfCombat)
        {
        }

        #endregion

        #region Methods

        protected override Part SequenceImpl(Part.Context context, Player player)
        {
            Part part = base.SequenceImpl(context, player);
            context.Schedule(new ResetCombatData());
            return part;
        }

        #endregion
    }
}
