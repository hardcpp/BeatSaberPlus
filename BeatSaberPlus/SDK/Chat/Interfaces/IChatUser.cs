namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatUser
    {
        string Id { get; }
        string UserName { get; }
        string DisplayName { get; }
        string PaintedName { get; }
        string Color { get; }
        bool IsBroadcaster { get; }
        bool IsModerator { get; }
        IChatBadge[] Badges { get; }
    }
}
