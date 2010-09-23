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

namespace Mox
{
    public static class EventRepositoryAssertExtensions
    {
        #region Inner Types

        private class GenericHandler<TEventArgs> : IEventHandler<TEventArgs>
        {
            private readonly Game m_expectedGame;
            private readonly Action<TEventArgs> m_action;

            public GenericHandler(Game expectedGame, Action<TEventArgs> action)
            {
                Assert.IsNotNull(expectedGame);

                m_expectedGame = expectedGame;
                m_action = action;
            }

            public void HandleEvent(Game game, TEventArgs e)
            {
                Assert.AreEqual(m_expectedGame, game);
                if (m_action != null)
                {
                    m_action(e);
                }
                NumCalled++;
            }

            public int NumCalled
            {
                get;
                private set;
            }
        }

        #endregion

        #region Methods

        public static void AssertTriggers<TEventArgs>(this Game game, System.Action operation, Action<TEventArgs> validation)
        {
            AssertTriggers(game, operation, validation, 1);
        }

        public static void AssertDoesNotTrigger<TEventArgs>(this Game game, System.Action operation)
        {
            AssertTriggers<TEventArgs>(game, operation, e => { }, 0);
        }

        public static void AssertTriggers<TEventArgs>(this Game game, System.Action operation, Action<TEventArgs> validation, int times)
        {
            GenericHandler<TEventArgs> handler = new GenericHandler<TEventArgs>(game, validation);
            try
            {
                game.Events.Register(handler);
                operation();
            }
            finally
            {
                game.Events.Unregister(handler);
            }

            if (handler.NumCalled == 0 && times > 0)
            {
                if (times == 1)
                {
                    Assert.Fail("Expected event to be called once but has not been called.");
                }
                else
                {
                    Assert.Fail("Expected event to be called {0} times but has not been called.", times);
                }
            }
            else if (handler.NumCalled == 1)
            {
                if (times > 1)
                {
                    Assert.Fail("Expected event to be called {0} times but has been called once.", times);
                }
                else if (times == 0)
                {
                    Assert.Fail("Expected event to be called {0} times but has not been called.", times);
                }
            }
            else if (handler.NumCalled > 1)
            {
                if (times == 0)
                {
                    Assert.Fail("Expected event not to be called but has been called {0} times.", handler.NumCalled);
                }
                else if (times == 1)
                {
                    Assert.AreEqual(times == 1, "Sanity check");
                    Assert.Fail("Expected event to be called once but has been called {0} times.", handler.NumCalled);
                }
            }
        }

        #endregion
    }
}
