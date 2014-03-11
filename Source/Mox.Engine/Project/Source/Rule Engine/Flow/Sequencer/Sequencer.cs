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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.Flow
{
    public enum SequencerResult
    {
        Stop,
        Continue,
        Retry
    }

    /// <summary>
    /// A sequencer sequences <see cref="Part"/>s together.
    /// </summary>
    public partial class Sequencer
    {
        #region Variables

        private readonly Game m_game;
        private ImmutableStack<Part> m_parts = new ImmutableStack<Part>();

        #endregion

        #region Constructor

        public Sequencer(Game game, Part initialPart)
        {
            Throw.IfNull(game, "game");
            Throw.IfNull(initialPart, "initialPart");

            m_game = game;

            Push(initialPart);
        }

        /// <summary>
        /// Clone ctor
        /// </summary>
        private Sequencer(Sequencer other, Game game)
        {
            m_game = game;
            m_parts = other.m_parts;
            m_argumentStack = other.m_argumentStack;
        }

        #endregion

        #region Properties

        public Game Game
        {
            get { return m_game; }
        }

        public bool IsEmpty
        {
            get { return m_parts.IsEmpty; }
        }

        public Part NextPart
        {
            get { return m_parts.Peek(); }
        }

#warning TEST
        public IEnumerable<Part> Parts
        {
            get { return m_parts; }
        }

        #endregion

        #region Methods

        #region Cloning

        /// <summary>
        /// Clones the current sequencer.
        /// </summary>
        /// <returns></returns>
        public Sequencer Clone()
        {
            return Clone(Game);
        }

        /// <summary>
        /// Clones the current sequencer.
        /// </summary>
        /// <returns></returns>
        public Sequencer Clone(Game game)
        {
            return new Sequencer(this, game);
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs all the parts.
        /// </summary>
        /// <remarks>
        /// Returns true if finished 'naturally'.
        /// </remarks>
        public bool Run(IChoiceDecisionMaker decisionMaker)
        {
            while (!IsEmpty && RunOnce(decisionMaker) != SequencerResult.Stop)
            {
            }

            return IsEmpty;
        }

        /// <summary>
        /// Runs the "next" scheduled part.
        /// </summary>
        /// <returns></returns>
        public SequencerResult RunOnce(IChoiceDecisionMaker decisionMaker)
        {
            Debug.Assert(!ReferenceEquals(decisionMaker, null), "Invalid decision maker");

            Part partToExecute = m_parts.Peek();

            var context = new Part.Context(this);
            ResolveChoice(decisionMaker, context, partToExecute);
            Part nextPart = partToExecute.Execute(context);

            if (Game.State.HasEnded)
            {
                m_parts = new ImmutableStack<Part>();
                return SequencerResult.Stop;
            }

            PrepareNextPart(nextPart, context);
            return Equals(nextPart, partToExecute) ? SequencerResult.Retry : SequencerResult.Continue;
        }

        private void ResolveChoice(IChoiceDecisionMaker decisionMaker, Part.Context context, Part part)
        {
            IChoicePart choicePart = part as IChoicePart;
            if (choicePart != null)
            {
                var choice = choicePart.GetChoice(this);
                Debug.Assert(choice != null);
                var result = decisionMaker.MakeChoiceDecision(this, choice);

#warning [MEDIUM] should we validate if the result is valid?
                choicePart.PushChoiceResult(context, result);
            }
        }

        private void PrepareNextPart(Part nextPart, Part.Context lastContext)
        {
            Pop();

            if (nextPart != null)
            {
                Push(nextPart);
            }

            lastContext.ScheduledParts.ForEach(Push);
        }

        protected void Push(Part part)
        {
            m_parts = m_parts.Push(part);
        }

        protected void Pop()
        {
            m_parts = m_parts.Pop();
        }

        public void Skip()
        {
            Pop();
        }

        #endregion

        #endregion
    }
}
