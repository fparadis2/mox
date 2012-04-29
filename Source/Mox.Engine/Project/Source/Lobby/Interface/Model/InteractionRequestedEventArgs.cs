using System;
using Mox.Flow;

namespace Mox.Lobby
{
    public class InteractionRequestedEventArgs : EventArgs
    {
        private readonly IChannel m_channel;
        private readonly ChoiceDecisionRequest m_request;

        public InteractionRequestedEventArgs(IChannel channel, ChoiceDecisionRequest request)
        {
            m_channel = channel;
            m_request = request;
        }

        public Choice Choice
        {
            get { return m_request.Choice; }
        }

        public void SendResult(object result)
        {
            m_channel.Respond(m_request, new ChoiceDecisionResponse { Result = result });
        }
    }
}