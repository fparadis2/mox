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
using Mox.Transactions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class EventRepositoryTests : BaseGameTests
    {
        #region Inner Types

        public class MyEventArgs
        {
            public int Value;
        }

        public class MyOtherEventArgs
        {
            public int Value;
        }

        #endregion

        #region Variables

        private EventRepository m_repository;

        private IEventHandler<MyEventArgs> m_handler;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_repository = new EventRepository(m_game);
            m_handler = m_mockery.StrictMock<IEventHandler<MyEventArgs>>();
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
            m_repository.Register(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };

            m_handler.HandleEvent(m_game, e);

            m_mockery.Test(() => m_repository.Trigger(e));
        }

        [Test]
        public void Test_Can_register_with_a_type()
        {
            m_repository.Register(typeof(MyEventArgs), m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };

            m_handler.HandleEvent(m_game, e);

            m_mockery.Test(() => m_repository.Trigger(e));
        }

        [Test]
        public void Test_Can_unregister()
        {
            m_repository.Register(m_handler);
            m_repository.Unregister(m_handler);

            m_mockery.Test(() => m_repository.Trigger(new MyEventArgs()));
        }

        [Test]
        public void Test_Can_unregister_with_a_type()
        {
            m_repository.Register(m_handler);
            m_repository.Unregister(typeof(MyEventArgs), m_handler);

            m_mockery.Test(() => m_repository.Trigger(new MyEventArgs()));
        }

        [Test]
        public void Test_Unregister_does_nothing_if_not_already_registered()
        {
            m_repository.Unregister(m_handler);
        }

        [Test]
        public void Test_Can_trigger_more_than_once()
        {
            m_repository.Register(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            MyEventArgs e2 = new MyEventArgs { Value = 20 };

            using (m_mockery.Ordered())
            {
                m_handler.HandleEvent(m_game, e);
                m_handler.HandleEvent(m_game, e2);
            }

            m_mockery.Test(() =>
            {
                m_repository.Trigger(e);
                m_repository.Trigger(e2);
            });
        }

        [Test]
        public void Test_Trigger_does_nothing_in_synchronized_mode()
        {
            m_repository.Register(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };

            m_mockery.Test(() =>
            {
                using (m_game.ChangeControlMode(ReplicationControlMode.Slave))
                {
                    m_repository.Trigger(e);
                }
            });
        }

        private class MyCommand<TEventArgs> : Command
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
            m_repository.Register(m_handler);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            MyCommand<MyEventArgs> command = new MyCommand<MyEventArgs>(m_repository, e);

            // Received only once.
            m_handler.HandleEvent(m_game, e);

            m_mockery.Test(() =>
            {
                using (ITransaction transaction = m_game.TransactionStack.BeginTransaction(TransactionType.None))
                {
                    m_game.TransactionStack.PushAndExecute(command);

                    transaction.Rollback();
                }
            });
        }

        [Test]
        public void Test_Can_register_more_than_one_handler()
        {
            var handler1 = m_mockery.StrictMock<IEventHandler<MyEventArgs>>();
            var handler2 = m_mockery.StrictMock<IEventHandler<MyEventArgs>>();
            var handler3 = m_mockery.StrictMock<IEventHandler<MyEventArgs>>();

            m_repository.Register(handler1);
            m_repository.Register(handler2);
            m_repository.Register(handler3);

            MyEventArgs e = new MyEventArgs { Value = 10 };

            using (m_mockery.Unordered())
            {
                handler1.HandleEvent(m_game, e);
                handler2.HandleEvent(m_game, e);
                handler3.HandleEvent(m_game, e);
            }

            m_mockery.Test(() => m_repository.Trigger(e));
        }

        [Test]
        public void Test_Can_have_handlers_for_multiple_events()
        {
            var handler1 = m_mockery.StrictMock<IEventHandler<MyEventArgs>>();
            var handler2 = m_mockery.StrictMock<IEventHandler<MyOtherEventArgs>>();

            m_repository.Register(handler1);
            m_repository.Register(handler2);

            MyEventArgs e = new MyEventArgs { Value = 10 };
            handler1.HandleEvent(m_game, e);
            m_mockery.Test(() => m_repository.Trigger(e));

            MyOtherEventArgs e2 = new MyOtherEventArgs { Value = 10 };
            handler2.HandleEvent(m_game, e2);
            m_mockery.Test(() => m_repository.Trigger(e2));
        }

        [Test]
        public void Test_GetEventHandlerTypes_returns_the_event_handler_interfaces_a_type_implements()
        {
            var handler1 = m_mockery.StrictMock<IEventHandler<MyEventArgs>>();
            var handler2 = m_mockery.StrictMultiMock<IEventHandler<MyOtherEventArgs>>(typeof(IEventHandler<MyEventArgs>));

            Assert.Collections.AreEquivalent(new[] { typeof(MyEventArgs) }, EventRepository.GetEventHandlerTypes(handler1.GetType()));
            Assert.Collections.AreEquivalent(new[] { typeof(MyEventArgs), typeof(MyOtherEventArgs) }, EventRepository.GetEventHandlerTypes(handler2.GetType()));
        }

        #endregion
    }
}
