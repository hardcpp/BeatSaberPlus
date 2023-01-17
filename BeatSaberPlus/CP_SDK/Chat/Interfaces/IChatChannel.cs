namespace CP_SDK.Chat.Interfaces
{
    public interface IChatChannel
    {
        string Name { get; }
        string Id { get; }
        bool IsTemp { get; }
        string Prefix { get; }
        bool CanSendMessages { get; }
        bool Live { get; }
        int ViewerCount { get; }
    }
}
