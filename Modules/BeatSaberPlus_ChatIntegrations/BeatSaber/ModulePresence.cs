using System.Linq;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber
{
    internal static class ModulePresence
    {
        private static bool? m_ChatRequest;
        private static bool? m_GameTweaker;
        private static bool? m_NoteTweaker;
        private static bool? m_SongChartVisualizer;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static bool ChatRequest { get {
            if (!m_ChatRequest.HasValue)
                m_ChatRequest = CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Chat Request");

            return m_ChatRequest.Value;
        } }
        internal static bool GameTweaker { get {
            if (!m_GameTweaker.HasValue)
                m_GameTweaker = CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Game Tweaker");

            return m_GameTweaker.Value;
        } }
        internal static bool NoteTweaker { get {
            if (!m_NoteTweaker.HasValue)
                m_NoteTweaker = CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Note Tweaker");

            return m_NoteTweaker.Value;
        } }
        internal static bool SongChartVisualizer { get {
            if (!m_SongChartVisualizer.HasValue)
                m_SongChartVisualizer = CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Song Chart Visualizer");

            return m_SongChartVisualizer.Value;
        } }
    }
}
