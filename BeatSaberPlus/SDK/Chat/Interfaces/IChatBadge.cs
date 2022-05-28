namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public enum EBadgeType
    {
        Image,
        Emoji
    }

    public interface IChatBadge
    {
        string Id { get; }
        string Name { get; }
        EBadgeType Type { get; }
        string Content { get; }
    }
}
