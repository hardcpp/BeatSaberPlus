using System.Linq;

namespace ChatPlexMod_ChatIntegrations
{
    internal static class ModulePresence
    {
        private static bool? m_Chat;
        private static bool? m_ChatEmoteRain;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static bool Chat { get {
            if (!m_Chat.HasValue)
                m_Chat = CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Chat");

            return m_Chat.Value;
        } }

        public static bool ChatEmoteRain { get {
            if (!m_ChatEmoteRain.HasValue)
                m_ChatEmoteRain = CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Chat Emote Rain");

            return m_ChatEmoteRain.Value;
        } }
    }
}
