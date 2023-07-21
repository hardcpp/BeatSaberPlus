namespace BeatSaberPlus_ChatIntegrations.BeatSaber
{
    internal static class ModPresence
    {
        private static bool? m_Camera2;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static bool Camera2 { get {
            if (!m_Camera2.HasValue)
                m_Camera2 = IPA.Loader.PluginManager.GetPluginFromId("Camera2") != null;

            return m_Camera2.Value;
        } }
    }
}
