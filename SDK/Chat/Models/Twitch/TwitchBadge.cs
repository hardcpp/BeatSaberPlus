using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    public class TwitchBadge : IChatBadge
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Uri { get; internal set; }
    }
}
