namespace CP_SDK.Chat.Interfaces
{
    public interface IChatBadge
    {
        string      Id      { get; }
        string      Name    { get; }
        EBadgeType  Type    { get; }
        string      Content { get; }
    }
}
