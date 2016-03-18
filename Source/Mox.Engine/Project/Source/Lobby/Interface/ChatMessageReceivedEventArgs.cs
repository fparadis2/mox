using System;

namespace Mox.Lobby
{
    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public ChatMessageReceivedEventArgs(PlayerData speaker, string message)
        {
            SpeakerId = speaker.Id;
            SpeakerName = speaker.Name;
            Message = message;
        }

        /// <summary>
        /// The id of the speaker.
        /// </summary>
        public Guid SpeakerId { get; set; }

        /// <summary>
        /// The name of the speaker.
        /// </summary>
        public string SpeakerName { get; set; }

        /// <summary>
        /// The player message.
        /// </summary>
        public string Message { get; set; }

        public string ToChatMessage()
        {
            string userName = string.IsNullOrEmpty(SpeakerName)  ? "[[Unknown]]" : SpeakerName;
            return string.Format("{0}: {1}", userName, Message);
        }
    }
}