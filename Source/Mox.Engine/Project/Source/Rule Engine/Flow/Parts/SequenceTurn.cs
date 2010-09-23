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

using Mox.Flow.Phases;

namespace Mox.Flow.Parts
{
    /// <summary>
    /// Sequences a single turn, for one player.
    /// </summary>
    public class SequenceTurn : MTGPart
    {
        #region Variables

        private readonly ITurnFactory m_turnFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public SequenceTurn(Player player)
            : this(player, new DefaultTurnFactory())
        {
        }

        public SequenceTurn(Player player, ITurnFactory turnFactory)
            : base(player)
        {
            Throw.IfNull(turnFactory, "turnFactory");
            m_turnFactory = turnFactory;
        }

        #endregion

        #region Properties

        internal ITurnFactory TurnFactory
        {
            get { return m_turnFactory; }
        }

        #endregion

        #region Methods

        public override Part<IGameController> Execute(Context context)
        {
            Player player = GetPlayer(context);

            context.Game.TurnData.ResetAllValues();
            context.Game.State.CurrentTurn += 1;
            context.Game.State.ActivePlayer = player;

            Turn turn = m_turnFactory.CreateTurn();

            foreach (Phase phase in turn.Phases)
            {
                context.Schedule(new SequencePhase(player, phase));
            }

            if (Configuration.Debug_Turns != TurnDebuggingLevel.None)
#pragma warning disable 162
            {
                OutputDebugTurn(Configuration.Debug_Turns, context.Game);
            }
#pragma warning restore 162

            return null;
        }

        private static void OutputDebugTurn(TurnDebuggingLevel level, Game game)
        {
            if (!game.TransactionStack.IsInMasterTransaction)
            {
                switch (level)
                {
                    case TurnDebuggingLevel.None:
                        break;
                    case TurnDebuggingLevel.Light:
                        OutputDebugLight(game);
                        break;

                    case TurnDebuggingLevel.Verbose:
                        OutputDebugVerbose(game);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static void OutputDebugLight(Game game)
        {
            Trace.WriteLine(string.Format("Turn {0}: {1}", game.State.CurrentTurn, game.Players.Select(p => string.Format("{0} ({1} life)", p.Name, p.Life)).Join(", ")));
        }

        private static void OutputDebugVerbose(Game game)
        {
            Trace.WriteLine(new GameDebugWriter(game).ToString());
        }

        #endregion
    }
}
