namespace BeatSaberPlus_ChatIntegrations
{
    internal class ModPresence
    {
        public static bool Camera2
        {
            get
            {
                if (!m_Camera2.HasValue)
                    m_Camera2 = IPA.Loader.PluginManager.GetPluginFromId("Camera2") != null;

                return m_Camera2.Value;
            }
        }
        public static bool Camera2Fixed
        {
            get
            {
                if (!Camera2)
                    return false;

                if (!m_Camera2Fixed.HasValue)
                    m_Camera2Fixed = IPA.Loader.PluginManager.GetPluginFromId("Camera2").HVersion >= new Hive.Versioning.Version(0, 6, 9);

                return m_Camera2Fixed.Value;
            }
        }
        private static bool? m_Camera2;
        private static bool? m_Camera2Fixed;
    }
}
