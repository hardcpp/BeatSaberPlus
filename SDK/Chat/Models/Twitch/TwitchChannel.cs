using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    public class TwitchChannel : IChatChannel
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public TwitchRoomstate Roomstate { get; internal set; }
    }
}
