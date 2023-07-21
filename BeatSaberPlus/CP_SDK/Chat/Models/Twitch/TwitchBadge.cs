using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    public struct TwitchBadge : IChatBadge
    {
        public string       Id      { get; internal set; }
        public string       Name    { get; internal set; }
        public EBadgeType   Type    { get; internal set; }
        public string       Content { get; internal set; }
    }
}
