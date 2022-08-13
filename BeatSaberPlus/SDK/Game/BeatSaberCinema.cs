using IPA.Loader;
using System;
using System.Reflection;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// BeatSaberCinema utils
    /// </summary>
    public static class BeatSaberCinema
    {
        /// <summary>
        /// Is initialized
        /// </summary>
        private static bool m_Init;

        /// <summary>
        /// Is present
        /// </summary>
        private static bool m_IsPresent;
        /// <summary>
        /// SetSelectedLevel
        /// </summary>
        private static MethodBase m_Events_SetSelectedLevel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is mod installed
        /// </summary>
        public static bool IsPresent { get { Init(); return m_IsPresent; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set selected level for BeatSaberCinema mod
        /// </summary>
        public static void SetSelectedLevel(IPreviewBeatmapLevel p_PreviewBeatMapLevel)
        {
            Init();

            if (!m_IsPresent || m_Events_SetSelectedLevel == null)
                return;

            try { m_Events_SetSelectedLevel.Invoke(null, new object[] { p_PreviewBeatMapLevel }); } catch { }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        private static void Init()
        {
            if (m_Init)
                return;

            var l_MetaData = PluginManager.GetPluginFromId("BeatSaberCinema");
            if (l_MetaData != null)
            {
                try {
                    m_Events_SetSelectedLevel = l_MetaData.Assembly.GetType("BeatSaberCinema.Events")?
                                                .GetMethod("SetSelectedLevel", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(IPreviewBeatmapLevel) }, null);
                }
                catch(Exception) { }

                m_IsPresent = m_Events_SetSelectedLevel != null;
            }
            else
                m_IsPresent = false;

            m_Init = true;
        }
    }
}
