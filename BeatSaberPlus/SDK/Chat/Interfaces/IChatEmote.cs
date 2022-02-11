using BeatSaberPlus.SDK.Chat.Models;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatEmote
    {
        string Id { get; }
        string Name { get; }
        string Uri { get; }
        int StartIndex { get; }
        int EndIndex { get; }
        Animation.AnimationType Animation { get; }
        /// <summary>
        /// The type of resource associated with this chat emote
        /// </summary>
        EmoteType Type { get; }
        /// <summary>
        /// The UV coordinates of this emote, only used if <see cref="Type"/> is <see cref="EmoteType.SpriteSheet"/>
        /// <para>X, Y = X/Y Position</para>
        /// <para>Z, W = Width/Height</para>
        /// </summary>
        ImageRect UVs { get; }
    }
}
