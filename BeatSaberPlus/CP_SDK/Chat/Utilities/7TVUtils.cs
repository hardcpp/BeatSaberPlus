using CP_SDK.Animation;
using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Chat.SimpleJSON;

namespace CP_SDK.Chat.Utilities
{
    /// <summary>
    /// 7TV utilities
    /// </summary>
    public static class _7TVUtils
    {
        /// <summary>
        /// Parse a 7TV emote set
        /// </summary>
        /// <param name="p_EmoteSet">Emote set to parse</param>
        /// <param name="p_EmoteType">Emote type</param>
        /// <param name="p_InternalIDPrefix">Internal ID prefix</param>
        /// <returns></returns>
        public static int ParseEmoteSet(IChatResourceProvider<ChatResourceData> p_ChatResourceProvider, JSONObject p_EmoteSet, string p_EmoteType, string p_InternalIDPrefix)
        {
            var l_Count = 0;
            if (!p_EmoteSet.HasKey("emotes") || !p_EmoteSet["emotes"].IsArray)
                return l_Count;

            var l_Emotes = p_EmoteSet["emotes"].AsArray;
            foreach (JSONObject l_Object in l_Emotes.AsArray)
            {
                if (!l_Object.HasKey("data") || !l_Object["data"].IsObject)
                    continue;

                var l_Name  = l_Object["name"].Value;   ///< Renamed emotes
                var l_Data  = l_Object["data"];
                var l_ID    = l_Data["id"].Value;
                var l_Host  = l_Data["host"];

                if (l_ID == "000000000000000000000000" || !l_Host.IsObject)
                    continue;

                var l_BaseURL       = $"https:{l_Host["url"].Value}/2x.webp";
                var l_InternalID    = string.IsNullOrEmpty(p_InternalIDPrefix) ? l_Name : $"{p_InternalIDPrefix}_{l_Name}";

                if (p_ChatResourceProvider.Resources.ContainsKey(l_InternalID))
                    continue;

                p_ChatResourceProvider.Resources.TryAdd(l_InternalID, new ChatResourceData()
                {
                    Uri         = l_BaseURL,
                    Animation   = EAnimationType.AUTODETECT,
                    Category    = EChatResourceCategory.Emote,
                    Type        = p_EmoteType
                });

                l_Count++;
            }

            return l_Count;
        }
    }
}
