// Copyright (c) Fran�ois Paradis
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
using Rhino.Mocks;

using Mox.Abilities;

namespace Mox.Flow
{
    public abstract class PartTestBase : BaseGameTests
    {
        #region Variables

        protected NewSequencerTester m_sequencerTester;
        protected Part.Context m_lastContext;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencerTester = new NewSequencerTester(m_mockery, m_game);
            m_sequencerTester.MockAllPlayersChoices();
        }

        public override void Teardown()
        {
            m_sequencerTester.VerifyExpectations();

            base.Teardown();
        }

        #endregion

        #region Utilities

        protected Part Execute(Part part)
        {
            m_lastContext = m_sequencerTester.CreateContext();
            using (m_mockery.Test())
            {
                return part.Execute(m_lastContext);
            }
        }

        protected Part ExecuteWithChoice<TPart>(TPart part, object choiceResult)
            where TPart : Part, IChoicePart
        {
            m_lastContext = m_sequencerTester.CreateContext();
            part.PushChoiceResult(m_lastContext, choiceResult);
            using (m_mockery.Test())
            {
                return part.Execute(m_lastContext);
            }
        }

        #endregion

        #region Inner Types

        public interface ISpellEffect
        {
            void Do();
        }

        #endregion
    }
}