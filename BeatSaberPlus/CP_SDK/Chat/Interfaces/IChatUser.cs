namespace CP_SDK.Chat.Interfaces
{
    public interface IChatUser
    {
        string          Id              { get; }
        string          UserName        { get; }
        string          DisplayName     { get; }
        string          PaintedName     { get; }
        string          Color           { get; }
        bool            IsBroadcaster   { get; }
        bool            IsModerator     { get; }
        bool            IsSubscriber    { get; }
        bool            IsVip           { get; }
        IChatBadge[]    Badges          { get; }
    }
}
