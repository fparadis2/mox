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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.AI
{
    using Part = Flow.Part<BaseMinMaxControllerTests.IMockController>;
    using Sequencer = Flow.Sequencer<BaseMinMaxControllerTests.IMockController>;

    public class BaseMinMaxControllerTests : BaseGameTests
    {
        #region Inner Types

        public enum ChoiceA
        {
            ResultX,
            ResultY
        }

        public enum ChoiceB
        {
            Result1,
            Result2
        }

        public interface IMockController
        {
            ChoiceA ChoiceA();
            ChoiceB ChoiceB();
        }

        public interface IChoiceVerifier
        {
            void ChoiceA(ChoiceA choice);
            void ChoiceB(ChoiceB choice);
        }

        private class SingleChoicePart : Flow.Part<IMockController>
        {
            #region Variables

            private readonly IChoiceVerifier m_choiceVerifier;

            #endregion

            public SingleChoicePart(IChoiceVerifier choiceVerifier)
            {
                Throw.IfNull(choiceVerifier, "choiceVerifier");
                m_choiceVerifier = choiceVerifier;
            }

            #region Overrides of Part<IMockController>

            public override Flow.Part<IMockController> Execute(Context context)
            {
                m_choiceVerifier.ChoiceA(context.Controller.ChoiceA());
                return null;
            }

            #endregion
        }

        #endregion

        #region Variables

        private IChoiceVerifier m_mockChoiceVerifier;
        private SingleChoicePart m_part;
        private Sequencer m_sequencer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockChoiceVerifier = m_mockery.StrictMock<IChoiceVerifier>();
            m_part = new SingleChoicePart(m_mockChoiceVerifier);
            m_sequencer = new Sequencer(m_part, m_game);
        }

        #endregion

        #region Utilities

        #region Expectations

        protected void Try_ChoiceA(ChoiceA choice)
        {
            m_mockChoiceVerifier.ChoiceA(choice);
        }

        #endregion

        #endregion
    }
}


