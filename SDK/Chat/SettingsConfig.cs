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

        internal class Global
        {
            private static string _s = "Global";

            internal static bool ParseEmojis {
                get { return m_Config.GetBool(_s, "ParseEmojis", true, true);           }
                set {        m_Config.SetBool(_s, "ParseEmojis", value);                }
            }
        }

        internal class Twitch
        {
            private static string _s = "Twitch";

            internal static bool ParseBTTVEmotes {
                get { return m_Config.GetBool(_s, "ParseBTTVEmotes", true, true); }
                set {        m_Config.SetBool(_s, "ParseBTTVEmotes", value); }
            }
            internal static bool ParseFFZEmotes {
                get { return m_Config.GetBool(_s, "ParseFFZEmotes", true, true); }
                set {        m_Config.SetBool(_s, "ParseFFZEmotes", value); }
            }
            internal static bool ParseTwitchEmotes {
                get { return m_Config.GetBool(_s, "ParseTwitchEmotes", true, true); }
                set {        m_Config.SetBool(_s, "ParseTwitchEmotes", value); }
            }
            internal static bool ParseCheermotes {
                get { return m_Config.GetBool(_s, "ParseCheermotes", true, true); }
                set {        m_Config.SetBool(_s, "ParseCheermotes", value); }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        internal static void Init()
        {
            var l_Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".beatsaberpluschatcore");
            m_Config = new SDK.Config.INIConfig(System.IO.Path.Combine(l_Path, "settings.ini"), true);
        }
    }
}
