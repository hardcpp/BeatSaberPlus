using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models
{
    public class Emoji : IChatEmote
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Uri { get; internal set; }
        public int StartIndex { get; internal set; }
        public int EndIndex { get; internal set; }
        public bool IsAnimated { get; internal set; }
        public EmoteType Type { get; internal set; }
        public ImageRect UVs { get; internal set; }
    }
}
