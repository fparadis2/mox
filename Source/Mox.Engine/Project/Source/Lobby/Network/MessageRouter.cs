using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network
{
    internal class MessageRouter<THost, TContext>
    {
        #region Variables

        private readonly Dictionary<System.Type, Router> m_routers = new Dictionary<System.Type, Router>();

        #endregion

        #region Methods

        public void Register<TMessage>(Func<THost, Action<TContext, TMessage>> route)
            where TMessage : Message
        {
            m_routers.Add(typeof(TMessage), new Router<TMessage>(route));
        }

        public void Register<TMessage>(Func<THost, Action<TMessage>> route)
            where TMessage : Message
        {
            m_routers.Add(typeof(TMessage), new RouterWithoutContext<TMessage>(route));
        }

        public void Register<TRequest, TResponse>(Func<THost, Func<TContext, TRequest, TResponse>> route)
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            m_routers.Add(typeof(TRequest), new RouterWithResponse<TRequest, TResponse>(route));
        }

        public void Register<TRequest, TResponse>(Func<THost, Func<TRequest, TResponse>> route)
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            m_routers.Add(typeof(TRequest), new RouterWithResponseWithoutContext<TRequest, TResponse>(route));
        }

        public Response Route(THost host, TContext context, Message message)
        {
            Router router;
            if (m_routers.TryGetValue(message.GetType(), out router))
            {
                return router.Route(host, context, message);
            }

            return null;
        }

        #endregion

        #region Inner Types

        private abstract class Router
        {
            public abstract Response Route(THost host, TContext context, Message message);
        }

        private class Router<TMessage> : Router
            where TMessage : Message
        {
            private readonly Func<THost, Action<TContext, TMessage>> m_route;

            public Router(Func<THost, Action<TContext, TMessage>> route)
            {
                m_route = route;
            }

            public override Response Route(THost host, TContext context, Message message)
            {
                m_route(host)(context, (TMessage)message);
                return null;
            }
        }

        private class RouterWithoutContext<TMessage> : Router
            where TMessage : Message
        {
            private readonly Func<THost, Action<TMessage>> m_route;

            public RouterWithoutContext(Func<THost, Action<TMessage>> route)
            {
                m_route = route;
            }

            public override Response Route(THost host, TContext context, Message message)
            {
                m_route(host)((TMessage)message);
                return null;
            }
        }

        private class RouterWithResponse<TRequest, TResponse> : Router
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            private readonly Func<THost, Func<TContext, TRequest, TResponse>> m_route;

            public RouterWithResponse(Func<THost, Func<TContext, TRequest, TResponse>> route)
            {
                m_route = route;
            }

            public override Response Route(THost host, TContext context, Message message)
            {
                var request = (TRequest)message;
                return m_route(host)(context, request);
            }
        }

        private class RouterWithResponseWithoutContext<TRequest, TResponse> : Router
            where TRequest : Request<TResponse>
            where TResponse : Response
        {
            private readonly Func<THost, Func<TRequest, TResponse>> m_route;

            public RouterWithResponseWithoutContext(Func<THost, Func<TRequest, TResponse>> route)
            {
                m_route = route;
            }

            public override Response Route(THost host, TContext source, Message message)
            {
                var request = (TRequest)message;
                return m_route(host)(request);
            }
        }

        #endregion
    }
}
