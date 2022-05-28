using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models
{
    public class ChatResourceData : IChatResourceData
    {
        public string Uri { get; set; }
        public Animation.AnimationType Animation { get; set; }
        public EChatResourceCategory Category { get; set; }
        public string Type { get; set; }
    }
}
