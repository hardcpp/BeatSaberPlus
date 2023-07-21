using CP_SDK.Chat.Interfaces;
using System.Collections.Generic;

namespace CP_SDK.Chat.Models.Twitch
{
    public class TwitchCheermoteData
    {
        public string                       Prefix;
        public List<TwitchCheermoteTier>    Tiers = new List<TwitchCheermoteTier>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public TwitchCheermoteTier GetTier(int p_BitsAmount)
        {
            for (int l_I = 1; l_I < Tiers.Count; l_I++)
            {
                if (p_BitsAmount < Tiers[l_I].MinBits)
                    return Tiers[l_I - 1];
            }

            return Tiers.Count > 0 ? Tiers[0] : null;
        }
    }
}
