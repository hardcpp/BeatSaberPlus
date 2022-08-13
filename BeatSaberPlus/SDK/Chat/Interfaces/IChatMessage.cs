using System;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatMessage
    {
        string Id { get; }
        bool IsSystemMessage { get; }
        bool IsActionMessage { get; }
        bool IsHighlighted { get; }
        bool IsPing { get; }
        string Message { get; }
        IChatUser Sender { get; }
        IChatChannel Channel { get; }
        IChatEmote[] Emotes { get; }
    }

    internal class IChatMessageImpl : IChatMessage
    {
        private IChatChannel m_IChatChannel;
        private CP_SDK.Chat.Interfaces.IChatMessage m_IChatMessage;

        internal IChatMessageImpl(CP_SDK.Chat.Interfaces.IChatMessage p_IChatMessage, IChatChannel p_IChatChannel)
        {
            m_IChatMessage = p_IChatMessage;
            m_IChatChannel = p_IChatChannel;
        }

        public string Id => m_IChatMessage.Id;
        public string Message => m_IChatMessage.Message;
        public IChatUser Sender => new IChatUserImpl(m_IChatMessage.Sender);
        public IChatChannel Channel => m_IChatChannel;
        public bool IsSystemMessage => m_IChatMessage.IsSystemMessage;
        public bool IsActionMessage => m_IChatMessage.IsActionMessage;
        public bool IsHighlighted => m_IChatMessage.IsHighlighted;
        public bool IsPing => m_IChatMessage.IsPing;
        public IChatEmote[] Emotes => Array.Empty<IChatEmote>();
    }
}
