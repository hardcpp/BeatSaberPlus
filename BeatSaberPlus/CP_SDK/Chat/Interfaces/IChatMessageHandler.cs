namespace CP_SDK.Chat.Interfaces
{
    public interface IChatMessageHandler
    {
        void OnMessageReceived(IChatMessage messasge);
    }
}
