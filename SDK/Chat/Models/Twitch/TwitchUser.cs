using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    public class TwitchUser : IChatUser
    {
        public string Id { get; internal set; }
        public string UserName { get; internal set; }
        public string DisplayName { get; internal set; }
        public string Color { get; internal set; }
        public bool IsModerator { get; internal set; }
        public bool IsBroadcaster { get; internal set; }
        public bool IsSubscriber { get; internal set; }
        public bool IsTurbo { get; internal set; }
        public bool IsVip { get; internal set; }
        public IChatBadge[] Badges { get; internal set; }
    }
}
