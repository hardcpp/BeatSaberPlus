using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    public class TwitchEmote : IChatEmote
    {
        public string Id { get; internal set; }
        public string Name { get; internal set; }
        public string Uri { get; internal set; }
        public int StartIndex { get; internal set; }
        public int EndIndex { get; internal set; }
        public bool IsAnimated { get; internal set; }
        public EmoteType Type { get; internal set; } = EmoteType.SingleImage;
        public ImageRect UVs { get; internal set; }
        /// <summary>
        /// The number of bits associated with this emote (probably a cheermote)
        /// </summary>
        public int Bits { get; internal set; }
        /// <summary>
        /// If there are bits associated with this emote, this is the color the bits text should be.
        /// </summary>
        public string Color { get; internal set; }

        public TwitchEmote() { }
    }
}
