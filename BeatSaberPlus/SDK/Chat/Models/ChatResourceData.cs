using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models
{
    public class ChatResourceData : IChatResourceData
    {
        public string Uri { get; internal set; }
        public Animation.AnimationType Animation { get; internal set; }
        public EChatResourceCategory Category { get; internal set; }
        public string Type { get; internal set; }
    }
}
