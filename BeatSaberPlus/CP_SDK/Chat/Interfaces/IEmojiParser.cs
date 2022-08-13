using System.Collections.Generic;

namespace CP_SDK.Chat.Interfaces
{
    public interface IEmojiParser
    {
        void FindEmojis(string p_Message, List<IChatEmote> p_Emotes);
    }
}
