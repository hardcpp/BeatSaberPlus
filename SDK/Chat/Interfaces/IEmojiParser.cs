using System.Collections.Generic;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IEmojiParser
    {
        List<IChatEmote> FindEmojis(string str);
    }
}
