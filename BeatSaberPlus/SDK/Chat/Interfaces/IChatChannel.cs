namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatChannel
    {
        string Name { get; }
        string Id { get; }
    }

    internal class IChatChannelImpl : IChatChannel
    {
        private CP_SDK.Chat.Interfaces.IChatChannel m_IChatChannel;
        internal IChatChannelImpl(CP_SDK.Chat.Interfaces.IChatChannel p_IChatChannel) => m_IChatChannel = p_IChatChannel;

        public string Name => m_IChatChannel.Name;
        public string Id => m_IChatChannel.Id;
    }
}
