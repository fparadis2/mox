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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mox.Flow;

namespace Mox.Flow.Phases
{
    public class CleanupStep : Step
    {
        #region Inner Types

        private class Discard : MTGPart
        {
            #region Constructor

            public Discard(Player player)
                : base(player)
            {
            }

            #endregion

            #region Overrides of Part<IGameController>

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Single;
                }
            }

            public override Part<IGameController> Execute(Context context)
            {
                Player player = GetPlayer(context);
                if (player.Hand.Count <= player.MaximumHandSize)
                {
                    return null;
                }

                int[] targets = player.Hand.Select(card => card.Identifier).ToArray();
                TargetContext targetInfo = new TargetContext(false, targets, TargetContextType.Discard);
                int selectedTarget = context.Controller.Target(context, player, targetInfo);
                if (targets.Contains(selectedTarget))
                {
                    Card card = Resolvable<Card>.Resolve(context.Game, selectedTarget);
                    player.Discard(card);

                    return new Discard(player);
                }
                else
                {
                    // retry
                    return this;
                }
            }

            #endregion
        }

        private class RemoveDamageOnPermanents : Part<IGameController>
        {
            public override Part<IGameController> Execute(Context context)
            {
                foreach (Card card in context.Game.Cards)
                {
                    card.ResetValue(Card.DamageProperty);
                }

                return null;
            }
        }

        private class TriggerEndOfTurn : MTGPart
        {
            #region Constructor

            public TriggerEndOfTurn(Player player)
                : base(player)
            {
            }

            #endregion

            #region Methods

            public override Part<IGameController> Execute(Context context)
            {
                context.Game.Events.Trigger(new Events.EndOfTurnEvent(GetPlayer(context)));
                return null;
            }

            #endregion
        }

        #endregion

        #region Constructor

        public CleanupStep() 
            : base(Steps.Cleanup)
        {
        }

        #endregion

        #region Methods

        protected override MTGPart SequenceImpl(MTGPart.Context context, Player player)
        {
            context.Schedule(new Discard(player));
            context.Schedule(new RemoveDamageOnPermanents());
            context.Schedule(new TriggerEndOfTurn(player));

            // Not quite sure I understand the rules about giving priority during clean up step :)
            // return base.Sequence(sequencer, player);
            return null;
        }

        #endregion
    }
}
