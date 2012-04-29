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

namespace Mox.UI.Game
{
    public class InteractionControllerTester : BaseGameTests
    {
        #region Inner Types

        public interface IMockInteraction
        {
            void End(int result);
        }

        protected class MockInteractionController : InteractionController
        {
            private class MockInteraction : Interaction, IMockInteraction
            {
                #region Implementation of IMockInteraction

                void IMockInteraction.End(int result)
                {
                    End(result);
                }

                #endregion
            }

            public MockInteractionController(GameViewModel model)
                : base(model)
            {
            }

            public IMockInteraction BeginMockInteraction()
            {
                MockInteraction interaction = new MockInteraction();
                BeginInteraction(interaction, null);
                return interaction;
            }

            public void BeginInteraction(Choice choice)
            {
                BeginInteraction(choice, null);
            }

            protected override void SendResult(Mox.Lobby.InteractionRequestedEventArgs request, object result)
            {
                Result = result;
            }

            public object Result
            {
                get;
                private set;
            }
        }

        #endregion

        #region Variables

        protected static readonly Resolvable<Player> EmptyPlayer = Resolvable<Player>.Empty;

        protected GameViewModel Model;
        protected GameViewModelSynchronizer m_synchronizer;
        protected MockInteractionController InteractionController;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            Model = new GameViewModel();
            m_synchronizer = new GameViewModelSynchronizer(Model, m_game, m_playerA, null);
            InteractionController = new MockInteractionController(Model);
        }

        public override void Teardown()
        {
            base.Teardown();

            m_synchronizer.Dispose();
        }

        #endregion
    }
}
