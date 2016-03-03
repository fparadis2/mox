using System;
using System.Collections.Generic;

namespace Mox.Lobby
{
    internal class MessageRouter<THost>
    {
        #region Variables

        private readonly Dictionary<System.Type, Router> m_routers = new Dictionary<System.Type, Router>();

        #endregion

        #region Methods

        public void Register<TMessage>(Func<THost, Action<IChannel, TMessage>> route)
            where TMessage : Message
        {
            m_routers.Add(typeof(TMessage), new Router<TMessage>(route));
        }

        public void Register<TMessage>(Func<THost, Action<TMessage>> route)
            where TMessage : Message
        {
            m_routers.Add(typeof(TMessage), new RouterWithoutChannel<TMessage>(route));
        }

        public void Register<TRequest, TResponse>(Func<THost, Func<IChannel, TRequest, TResponse>> route)
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            m_routers.Add(typeof(TRequest), new RouterWithResponse<TRequest, TResponse>(route));
        }

        public void Register<TRequest, TResponse>(Func<THost, Func<TRequest, TResponse>> route)
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            m_routers.Add(typeof(TRequest), new RouterWithResponseWithoutChannel<TRequest, TResponse>(route));
        }

        public void Route(THost host, IChannel source, Message message)
        {
            Router router;
            if (m_routers.TryGetValue(message.GetType(), out router))
            {
                router.Route(host, source, message);
            }
        }

        #endregion

        #region Inner Types

        private abstract class Router
        {
            public abstract void Route(THost host, IChannel source, Message message);
        }

        private class Router<TMessage> : Router
            where TMessage : Message
        {
            private readonly Func<THost, Action<IChannel, TMessage>> m_route;

            public Router(Func<THost, Action<IChannel, TMessage>> route)
            {
                m_route = route;
            }

            public override void Route(THost host, IChannel source, Message message)
            {
                m_route(host)(source, (TMessage)message);
            }
        }

        private class RouterWithoutChannel<TMessage> : Router
            where TMessage : Message
        {
            private readonly Func<THost, Action<TMessage>> m_route;

            public RouterWithoutChannel(Func<THost, Action<TMessage>> route)
            {
                m_route = route;
            }

            public override void Route(THost host, IChannel source, Message message)
            {
                m_route(host)((TMessage)message);
            }
        }

        private class RouterWithResponse<TRequest, TResponse> : Router
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            private readonly Func<THost, Func<IChannel, TRequest, TResponse>> m_route;

            public RouterWithResponse(Func<THost, Func<IChannel, TRequest, TResponse>> route)
            {
                m_route = route;
            }

            public override void Route(THost host, IChannel source, Message message)
            {
                var request = (TRequest)message;
                var response = m_route(host)(source, request);
                source.Respond(request, response);
            }
        }

        private class RouterWithResponseWithoutChannel<TRequest, TResponse> : Router
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            private readonly Func<THost, Func<TRequest, TResponse>> m_route;

            public RouterWithResponseWithoutChannel(Func<THost, Func<TRequest, TResponse>> route)
            {
                m_route = route;
            }

            public override void Route(THost host, IChannel source, Message message)
            {
                var request = (TRequest)message;
                var response = m_route(host)(request);
                source.Respond(request, response);
            }
        }

        #endregion
    }
}
