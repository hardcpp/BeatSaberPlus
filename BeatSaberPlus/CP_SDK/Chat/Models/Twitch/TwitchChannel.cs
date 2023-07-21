using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    public class TwitchChannel : IChatChannel
    {
        public string           Id              { get; internal set; }
        public string           Name            { get; internal set; }
        public bool             IsTemp          { get; internal set; } = false;
        public string           Prefix          { get; internal set; }
        public bool             CanSendMessages { get; internal set; } = true;
        public bool             Live            { get; internal set; } = false;
        public int              ViewerCount     { get; internal set; } = 0;
        public TwitchRoomState  Roomstate       { get; internal set; }
    }
}
