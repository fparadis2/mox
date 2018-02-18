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
using Mox.Transactions;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class EventRepositoryTests : BaseGameTests
    {
        #region Inner Types

        public class MyEventArgs : Event
        {
            public int Value;
        }

        public class MyOtherEventArgs : Event
        {
            public int Value;
        }

        private class MockHandler : IEventHandler
        {
            public MockHandler(Game game)
            {
                Game = game;
            }

            public Game Game { get; }
            public List<Event> Events { get; } = new List<Event>();

            public void HandleEvent(Game game, Event e)
            {
                Assert.AreEqual(Game, game);
                Events.Add(e);
            }
        }

        #endregion

        #region Variables

        private EventRepository m_repository;

        private MockHandler m_handler;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_repository = new EventRepository(m_game);
            m_handler = new MockHandler(m_game);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new EventRepository(null));
        }

        [Test]
        public void Test_Can_register_to_be_notified()
        {
            m_repository.Register<MyEventArgs>(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            m_repository.Trigger(e);

            Assert.AreEqual(1, m_handler.Events.Count);
            Assert.AreEqual(e, m_handler.Events[0]);
        }

        [Test]
        public void Test_Can_register_with_a_type()
        {
            m_repository.Register(typeof(MyEventArgs), m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            m_repository.Trigger(e);

            Assert.AreEqual(1, m_handler.Events.Count);
            Assert.AreEqual(e, m_handler.Events[0]);
        }

        [Test]
        public void Test_Can_unregister()
        {
            m_repository.Register<MyEventArgs>(m_handler);
            m_repository.Unregister<MyEventArgs>(m_handler);

            m_repository.Trigger(new MyEventArgs());
            Assert.AreEqual(0, m_handler.Events.Count);
        }

        [Test]
        public void Test_Can_unregister_with_a_type()
        {
            m_repository.Register<MyEventArgs>(m_handler);
            m_repository.Unregister(typeof(MyEventArgs), m_handler);

            m_repository.Trigger(new MyEventArgs());
            Assert.AreEqual(0, m_handler.Events.Count);
        }

        [Test]
        public void Test_Unregister_does_nothing_if_not_already_registered()
        {
            m_repository.Unregister<MyEventArgs>(m_handler);
        }

        [Test]
        public void Test_Can_trigger_more_than_once()
        {
            m_repository.Register<MyEventArgs>(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            MyEventArgs e2 = new MyEventArgs { Value = 20 };

            m_repository.Trigger(e);
            m_repository.Trigger(e2);

            Assert.Collections.AreEqual(new[] { e, e2 }, m_handler.Events);
        }

        [Test]
        public void Test_Trigger_does_nothing_in_synchronized_mode()
        {
            m_repository.Register<MyEventArgs>(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };

            using (m_game.UpgradeController(new MockObjectController()))
            {
                m_repository.Trigger(e);
            }

            Assert.AreEqual(0, m_handler.Events.Count);
        }

        private class MyCommand<TEventArgs> : Command
            where TEventArgs : Event
        {
            private readonly EventRepository m_repository;
            private readonly TEventArgs m_eventArgs;

            public MyCommand(EventRepository repository, TEventArgs e)
            {
                m_repository = repository;
                m_eventArgs = e;
            }

            public override void Execute(ObjectManager manager)
            {
                Trigger();
            }

            public override void Unexecute(ObjectManager manager)
            {
                Trigger();
            }

            private void Trigger()
            {
                m_repository.Trigger(m_eventArgs);
            }
        }

        [Test]
        public void Test_Trigger_does_nothing_while_undoing()
        {
            m_repository.Register<MyEventArgs>(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            MyCommand<MyEventArgs> command = new MyCommand<MyEventArgs>(m_repository, e);

            object token = new object();

            m_game.Controller.BeginTransaction(token);
            {
                m_game.Controller.Execute(command);
            }
            m_game.Controller.EndTransaction(true, token);

            Assert.AreEqual(1, m_handler.Events.Count);
        }

        [Test]
        public void Test_Can_register_more_than_one_handler()
        {
            var handler1 = new MockHandler(m_game);
            var handler2 = new MockHandler(m_game);
            var handler3 = new MockHandler(m_game);

            m_repository.Register<MyEventArgs>(handler1);
            m_repository.Register<MyEventArgs>(handler2);
            m_repository.Register<MyEventArgs>(handler3);

            MyEventArgs e = new MyEventArgs { Value = 10 };

            m_repository.Trigger(e);

            Assert.Collections.AreEqual(new[] { e }, handler1.Events);
            Assert.Collections.AreEqual(new[] { e }, handler2.Events);
            Assert.Collections.AreEqual(new[] { e }, handler3.Events);
        }

        [Test]
        public void Test_Can_have_handlers_for_multiple_events()
        {
            var handler1 = new MockHandler(m_game);
            var handler2 = new MockHandler(m_game);

            m_repository.Register<MyEventArgs>(handler1);
            m_repository.Register<MyOtherEventArgs>(handler2);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            m_repository.Trigger(e);
            Assert.Collections.AreEqual(new[] { e }, handler1.Events);
            Assert.Collections.IsEmpty(handler2.Events);

            MyOtherEventArgs e2 = new MyOtherEventArgs { Value = 10 };
            m_repository.Trigger(e2);
            Assert.Collections.AreEqual(new[] { e }, handler1.Events);
            Assert.Collections.AreEqual(new[] { e2 }, handler2.Events);
        }

        [Test]
        public void Test_Can_register_the_same_handler_for_multiple_events()
        {
            m_repository.Register<MyEventArgs>(m_handler);
            m_repository.Register<MyOtherEventArgs>(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            m_repository.Trigger(e);
            Assert.Collections.AreEqual(new[] { e }, m_handler.Events);

            MyOtherEventArgs e2 = new MyOtherEventArgs { Value = 10 };
            m_repository.Trigger(e2);
            Assert.Collections.AreEqual(new Event[] { e, e2 }, m_handler.Events);
        }

        #endregion

        #region Mock Types

        private class MockObjectController : IObjectController
        {
            #region Implementation of IObjectController

            public bool IsInTransaction
            {
                get { throw new NotImplementedException(); }
            }

            public void BeginTransaction(object token)
            {
                throw new NotImplementedException();
            }

            public void EndTransaction(bool rollback, object token)
            {
                throw new NotImplementedException();
            }

            public IDisposable BeginCommandGroup()
            {
                throw new NotImplementedException();
            }

            public void Execute(ICommand command)
            {
                throw new NotImplementedException();
            }

            public event EventHandler<CommandEventArgs> CommandExecuted { add { } remove { } }

            public ICommand CreateInitialSynchronizationCommand()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }
}
