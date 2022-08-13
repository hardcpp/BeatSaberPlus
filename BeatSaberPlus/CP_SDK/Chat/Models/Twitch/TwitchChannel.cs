using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    public class TwitchChannel : IChatChannel
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public TwitchRoomstate Roomstate { get; internal set; }
        public bool CanSendMessages { get; internal set; } = true;
    }
}
