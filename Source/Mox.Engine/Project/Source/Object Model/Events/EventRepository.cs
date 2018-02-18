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
    public abstract class Event
    {
    }

    public interface IEventHandler
    {
        void HandleEvent(Game game, Event e);
    }

    /// <summary>
    /// Manages/Triggers gameplay events.
    /// </summary>
    public class EventRepository
    {
        #region Inner Types

        private class HandlerCollection
        {
            #region Variables

            private readonly List<IEventHandler> m_handlers = new List<IEventHandler>();

            #endregion

            #region Methods

            public void Handle(Game game, Event e)
            {
                List<IEventHandler> handlers = new List<IEventHandler>(m_handlers);
                handlers.ForEach(handler => handler.HandleEvent(game, e));
            }

            public void Register(IEventHandler handler)
            {
                m_handlers.Add(handler);
            }

            public void Unregister(IEventHandler handler)
            {
                m_handlers.Remove(handler);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly Dictionary<System.Type, HandlerCollection> m_handlers = new Dictionary<System.Type, HandlerCollection>();

        #endregion

        #region Constructor

        public EventRepository(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;
        }

        #endregion

        #region Methods

        public void Trigger<TEvent>(TEvent e)
            where TEvent : Event
        {
            if (m_game.IsMaster)
            {
                GetHandlerCollection(typeof (TEvent)).Handle(m_game, e);
            }
        }

        public void Register<TEventArgs>(IEventHandler handler)
        {
            Register(typeof(TEventArgs), handler);
        }

        public void Unregister<TEventArgs>(IEventHandler handler)
        {
            Unregister(typeof(TEventArgs), handler);
        }

        public void Register(System.Type eventType, IEventHandler handler)
        {
            Debug.Assert(eventType != null);
            GetHandlerCollection(eventType).Register(handler);
        }

        public void Unregister(System.Type eventType, IEventHandler handler)
        {
            Debug.Assert(eventType != null);
            GetHandlerCollection(eventType).Unregister(handler);
        }

        private HandlerCollection GetHandlerCollection(System.Type type)
        {
            HandlerCollection collection;
            if (!m_handlers.TryGetValue(type, out collection))
            {
                collection = new HandlerCollection();
                m_handlers.Add(type, collection);
            }
            Debug.Assert(collection != null);
            return collection;
        }

        #endregion
    }
}
