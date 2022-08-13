using System;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatUser
    {
        string Id { get; }
        string UserName { get; }
        string DisplayName { get; }
        string PaintedName { get; }
        string Color { get; }
        bool IsBroadcaster { get; }
        bool IsModerator { get; }
        bool IsSubscriber { get; }
        bool IsVip { get; }
        IChatBadge[] Badges { get; }
    }

    internal class IChatUserImpl : IChatUser
    {
        private CP_SDK.Chat.Interfaces.IChatUser m_IChatUser;
        internal IChatUserImpl(CP_SDK.Chat.Interfaces.IChatUser p_IChatUser) => m_IChatUser = p_IChatUser;

        public string Id => m_IChatUser.Id;
        public string UserName => m_IChatUser.UserName;
        public string DisplayName => m_IChatUser.DisplayName;
        public string PaintedName => m_IChatUser.PaintedName;
        public string Color => m_IChatUser.Color;
        public bool IsBroadcaster => m_IChatUser.IsBroadcaster;
        public bool IsModerator => m_IChatUser.IsModerator;
        public bool IsSubscriber => m_IChatUser.IsSubscriber;
        public bool IsVip => m_IChatUser.IsVip;
        public IChatBadge[] Badges => Array.Empty<IChatBadge>();
    }
}
