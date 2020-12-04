using System;
using System.Net.Http;
using System.Reflection;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// BeatSaver extensions
    /// </summary>
    public static class BeatSaver_Extensions
    {
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static PropertyInfo m_BeatSaver_HttpInstance = null;
        /// <summary>
        /// Reflection cache
        /// </summary>
        private static PropertyInfo m_Http_Client = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Make usage of beat saber plus custom maps server
        /// </summary>
        /// <param name="p_Instance"></param>
        /// <param name="p_Enabled"></param>
        /// <returns></returns>
        public static bool UseBeatSaberPlusCustomMapsServer(this BeatSaverSharp.BeatSaver p_Instance, bool p_Enabled)
        {
            if (p_Instance == null)
                return false;

            try
            {
                if (p_Enabled && (!BeatSaberPlus.Config.Online.Enabled || !BeatSaberPlus.Config.Online.UseBSPCustomMapsServer))
                    return false;

                if (m_BeatSaver_HttpInstance == null)
                    m_BeatSaver_HttpInstance = typeof(BeatSaverSharp.BeatSaver).GetProperty("HttpInstance", BindingFlags.Instance | BindingFlags.NonPublic);

                var l_BSSHttp = m_BeatSaver_HttpInstance.GetValue(p_Instance);
                if (l_BSSHttp != null)
                {
                    if (m_Http_Client == null)
                        m_Http_Client = l_BSSHttp.GetType().GetProperty("Client", BindingFlags.Instance | BindingFlags.NonPublic);

                    var l_HTTPClient = m_Http_Client.GetValue(l_BSSHttp) as HttpClient;
                    if (l_HTTPClient != null)
                    {
                        l_HTTPClient.BaseAddress = new Uri(p_Enabled ? "https://maps.beatsaberplus.com/api/" : $"{BeatSaverSharp.BeatSaver.BaseURL}/api/");
                        return true;
                    }
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("UseBeatSaberPlusCustomMapsServer failed,");
                Logger.Instance.Error(p_Exception);
            }

            return false;
        }
    }
}
