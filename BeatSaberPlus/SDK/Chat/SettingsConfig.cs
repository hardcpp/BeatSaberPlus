using System;
using System.IO;

namespace BeatSaberPlus.SDK.Chat
{
    /// <summary>
    /// Settings config
    /// </summary>
    internal class SettingsConfig
    {
        /// <summary>
        /// Config instance
        /// </summary>
        private static SDK.Config.INIConfig m_Config = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal class WebApp
        {
            private static string _s = "WebApp";

            internal static bool LaunchWebAppOnStartup {
                get { return m_Config.GetBool(_s, "LaunchWebAppOnStartup", true, true); }
                set {        m_Config.SetBool(_s, "LaunchWebAppOnStartup", value);      }
            }
            internal static int WebAppPort {
                get { return m_Config.GetInt(_s, "WebAppPort", 8339, true);             }
                set {        m_Config.SetInt(_s, "WebAppPort", value);                  }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        internal static void Init()
        {
            try
            {
                var l_Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".beatsaberpluschatcore");
                m_Config = new SDK.Config.INIConfig(System.IO.Path.Combine(l_Path, "settings.ini"), true);
            }
            catch
            {

            }
        }
    }
}
