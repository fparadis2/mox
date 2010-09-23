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

namespace Mox
{
    public interface IEventHandler<TEventArgs>
    {
        void HandleEvent(Game game, TEventArgs e);
    }

    /// <summary>
    /// Manages/Triggers gameplay events.
    /// </summary>
    public class EventRepository
    {
        #region Inner Types

        private interface IHandlerCollection
        {
            void Trigger<TEventArgs>(Game game, TEventArgs e);
            void Register(object handler);
            void Unregister(object handler);
        }

        private class HandlerCollection : IHandlerCollection
        {
            #region Variables

            private readonly List<object> m_handlers = new List<object>();

            #endregion

            #region Methods

            public void Trigger<TEventArgs>(Game game, TEventArgs e)
            {
                List<object> handlers = new List<object>(m_handlers);
                handlers.ForEach(handler => ((IEventHandler<TEventArgs>)handler).HandleEvent(game, e));
            }

            public void Register(object handler)
            {
                m_handlers.Add(handler);
            }

            public void Unregister(object handler)
            {
                m_handlers.Remove(handler);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly Dictionary<System.Type, IHandlerCollection> m_handlers = new Dictionary<System.Type, IHandlerCollection>();

        #endregion

        #region Constructor

        public EventRepository(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;
        }

        #endregion

        #region Methods

        public void Trigger<TEventArgs>(TEventArgs e)
        {
            if (m_game.IsMaster)
            {
                GetHandlerCollection(typeof (TEventArgs)).Trigger(m_game, e);
            }
        }

        public void Register<TEventArgs>(IEventHandler<TEventArgs> handler)
        {
            Register(typeof(TEventArgs), handler);
        }

        public void Unregister<TEventArgs>(IEventHandler<TEventArgs> handler)
        {
            Unregister(typeof(TEventArgs), handler);
        }

        public void Register(System.Type eventType, object handler)
        {
            Debug.Assert(eventType != null);
            AssertIsCorrectEventHandlerInstance(eventType, handler);
            GetHandlerCollection(eventType).Register(handler);
        }

        public void Unregister(System.Type eventType, object handler)
        {
            Debug.Assert(eventType != null);
            AssertIsCorrectEventHandlerInstance(eventType, handler);
            GetHandlerCollection(eventType).Unregister(handler);
        }

        private IHandlerCollection GetHandlerCollection(System.Type type)
        {
            IHandlerCollection collection;
            if (!m_handlers.TryGetValue(type, out collection))
            {
                collection = new HandlerCollection();
                m_handlers.Add(type, collection);
            }
            Debug.Assert(collection != null);
            return collection;
        }

        [Conditional("DEBUG")]
        private static void AssertIsCorrectEventHandlerInstance(System.Type eventType, object handler)
        {
            Throw.IfNull(handler, "handler");

            foreach (System.Type interfaceType in handler.GetType().GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEventHandler<>) && 
                    interfaceType.GetGenericArguments()[0] == eventType)
                {
                    return;
                }
            }

            throw new ArgumentException("Handler does not correspond to event type", "handler");
        }

        public static IEnumerable<System.Type> GetEventHandlerTypes(System.Type type)
        {
            // Could possibly be cached if performance sensitive.
            foreach (System.Type iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                {
                    yield return iface.GetGenericArguments()[0];
                }
            }
        }

        #endregion
    }

    public static class EventRepositoryExtensions
    {
        public static void RegisterAllHandlerTypes(this EventRepository repository, object handler)
        {
            EventRepository.GetEventHandlerTypes(handler.GetType()).ForEach(type => repository.Register(type, handler));
        }

        public static void UnregisterAllHandlerTypes(this EventRepository repository, object handler)
        {
            EventRepository.GetEventHandlerTypes(handler.GetType()).ForEach(type => repository.Unregister(type, handler));
        }
    }
}
