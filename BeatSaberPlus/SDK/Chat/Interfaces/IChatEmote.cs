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
    }
}
