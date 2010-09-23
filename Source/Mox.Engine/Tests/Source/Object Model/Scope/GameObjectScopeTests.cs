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
using Mox.Effects;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class GameObjectScopeTests : BaseGameTests
    {
        #region Inner Types

        private class Patate
        { }

        private class MyScope : GameObjectScope, IEventHandler<Patate>
        {
            public void HandleEvent(Game game, Patate e)
            {
                Handled.Raise(this, EventArgs.Empty);
            }

            public event EventHandler Handled;
        }

        private class EmptyEffect : Effect<PowerAndToughness>
        {
            public EmptyEffect()
                : base(Card.PowerAndToughnessProperty)
            {
            }

            public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
            {
                return value;
            }
        }

        #endregion

        #region Variables

        private LocalEffectInstance m_instance;
        private MyScope m_scope;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_instance = m_game.CreateScopedLocalEffect<MyScope>(m_card, new EmptyEffect());
            m_scope = (MyScope)m_instance.Scope;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(m_scope);
            Assert.AreSame(m_instance, m_scope.AffectedObject);
        }

        [Test]
        public void Test_Effect_scope_is_automatically_registered_as_an_event_handler()
        {
            EventSink<EventArgs> sink = new EventSink<EventArgs>();
            m_scope.Handled += sink;

            Assert.EventCalledOnce(sink, () => m_game.Events.Trigger(new Patate()));
        }

        #endregion
    }
}
