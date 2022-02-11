using System.Collections.Concurrent;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatMessageParser
    {
        bool ParseRawMessage(string rawMessage, ConcurrentDictionary<string, IChatChannel> channelInfo, IChatUser loggedInUser, out IChatMessage[] parsedMessage);
    }
}
