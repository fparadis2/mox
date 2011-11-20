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
using System.Linq;

namespace Mox.Flow.Phases
{
    public class CleanupStep : Step
    {
        #region Inner Types

        private class Discard : ChoicePart<TargetResult>
        {
            #region Constructor

            public Discard(Player player)
                : base(player)
            {
            }

            #endregion

            #region Overrides of ChoicePart

            public override Choice GetChoice(Sequencer sequencer)
            {
                Player player = GetPlayer(sequencer.Game);
                int[] targets = player.Hand.Select(card => card.Identifier).ToArray();
                TargetContext targetInfo = new TargetContext(false, targets, TargetContextType.Discard);
                return new TargetChoice(player, targetInfo);
            }

            public override Part Execute(Context context, TargetResult targetToDiscard)
            {
                Player player = GetPlayer(context);

                if (targetToDiscard.IsValid)
                {
                    Card card = targetToDiscard.Resolve<Card>(context.Game);
                    if (player.Hand.Contains(card))
                    {
                        player.Discard(card);

                        if (NeedsToDiscard(player))
                        {
                            return new Discard(player);
                        }

                        return null;
                    }
                }

                // retry
                return this;
            }

            public static bool NeedsToDiscard(Player player)
            {
                return player.Hand.Count > player.MaximumHandSize;
            }

            #endregion
        }

        private class RemoveDamageOnPermanents : Part
        {
            public override Part Execute(Context context)
            {
                foreach (Card card in context.Game.Cards)
                {
                    card.ResetValue(Card.DamageProperty);
                }

                return null;
            }
        }

        private class TriggerEndOfTurn : PlayerPart
        {
            #region Constructor

            public TriggerEndOfTurn(Player player)
                : base(player)
            {
            }

            #endregion

            #region Methods

            public override Part Execute(Context context)
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

        protected override Part SequenceImpl(Part.Context context, Player player)
        {
            if (Discard.NeedsToDiscard(player))
            {
                context.Schedule(new Discard(player));
            }
            
            context.Schedule(new RemoveDamageOnPermanents());
            context.Schedule(new TriggerEndOfTurn(player));

            // Not quite sure I understand the rules about giving priority during clean up step :)
            // return base.Sequence(sequencer, player);
            return null;
        }

        #endregion
    }
}
