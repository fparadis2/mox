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
using System.Linq;

namespace Mox.Flow.Phases
{
    public class UntapStep : Step
    {
        #region Constructor

        public UntapStep() 
            : base(Steps.Untap)
        {
        }

        #endregion

        #region Methods

        protected override NewPart SequenceImpl(NewPart.Context context, Player player)
        {
            UntapPermanents(context, player);
            // Don't call base step.. no player gets priority during untap step.
            return null;
        }

        private static void UntapPermanents(NewPart.Context context, Player player)
        {
            Debug.Assert(player != null);

            // TODO: Ask player what permanents to untap..

            using (context.Game.Controller.BeginCommandGroup())
            {
                foreach (Card controlledCard in player.Manager.Cards.Where(card => card.Controller == player))
                {
                    controlledCard.Tapped = false;
                    Rules.SummoningSickness.RemoveSickness(controlledCard);
                }
            }
        }

        #endregion
    }
}
