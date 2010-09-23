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
using Mox.Flow;
using Mox.Flow.Parts;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Mox.Flow.Phases
{
    public class BaseStepTests<TStep> : BaseGameTests
        where TStep : Step
    {
        #region Variables

        protected TStep m_step;
        protected SequencerTester m_sequencerTester;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencerTester = new SequencerTester(m_mockery, m_game);
        }

        #endregion

        #region Utilities

        protected MTGPart SequenceStep(Player player)
        {
            MTGPart result = null;
            m_mockery.Test(() => result = m_step.Sequence(m_sequencerTester.Context, player));
            return result;
        }

        protected void RunStep(Player player)
        {
            m_game.State.ActivePlayer = player;
            m_sequencerTester.Run(new SequenceStep(player, m_step));
        }

        protected TPart GetScheduledPart<TPart>()
            where TPart : MTGPart
        {
            foreach (MTGPart part in m_sequencerTester.Context.ScheduledParts)
            {
                if (part is TPart)
                {
                    return (TPart)part;
                }
            }

            return null;
        }

        #endregion
    }
}
