namespace CP_SDK.Chat.Interfaces
{
    public interface IChatChannelPointEvent
    {
        string RewardID { get; }
        string TransactionID { get; }
        string Title { get; }
        string Prompt { get; }
        string UserInput { get; }
        int Cost { get; }
        string Image { get; }
        string BackgroundColor { get; }
    }
}