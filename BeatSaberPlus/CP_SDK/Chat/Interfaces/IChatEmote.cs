using CP_SDK.Chat.Models;

namespace CP_SDK.Chat.Interfaces
{
    public interface IChatEmote
    {
        string Id { get; }
        string Name { get; }
        string Uri { get; }
        int StartIndex { get; }
        int EndIndex { get; }
        Animation.EAnimationType Animation { get; }
    }
}
