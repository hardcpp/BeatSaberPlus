using System.Collections.Generic;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IEmojiParser
    {
        void FindEmojis(string p_Message, List<IChatEmote> p_Emotes);
    }
}
