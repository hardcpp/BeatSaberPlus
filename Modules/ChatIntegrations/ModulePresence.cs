namespace BeatSaberPlus_ChatIntegrations
{
    internal class ModulePresence
    {
        public static bool ChatEmoteRain
        {
            get
            {
                if (!m_ChatEmoteRain.HasValue)
                    m_ChatEmoteRain = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlus_ChatEmoteRain") != null;

                return m_ChatEmoteRain.Value;
            }
        }
        private static bool? m_ChatEmoteRain;

        public static bool ChatRequest
        {
            get
            {
                if (!m_ChatRequest.HasValue)
                    m_ChatRequest = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlus_ChatRequest") != null;

                return m_ChatRequest.Value;
            }
        }
        private static bool? m_ChatRequest;

        public static bool GameTweaker { get
            {
                if (!m_GameTweaker.HasValue)
                    m_GameTweaker = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlus_GameTweaker") != null;

                return m_GameTweaker.Value;
            }
        }
        private static bool? m_GameTweaker;

        public static bool NoteTweaker
        {
            get
            {
                if (!m_NoteTweaker.HasValue)
                    m_NoteTweaker = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlus_NoteTweaker") != null;

                return m_NoteTweaker.Value;
            }
        }
        private static bool? m_NoteTweaker;
    }
}
