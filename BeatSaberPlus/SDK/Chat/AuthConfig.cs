using System;
using System.IO;

namespace BeatSaberPlus.SDK.Chat
{
    /// <summary>
    /// Auth config
    /// </summary>
    internal class AuthConfig
    {
        /// <summary>
        /// Config instance
        /// </summary>
        private static SDK.Config.INIConfig m_Config = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal class Twitch
        {
            private static string _s = "Twitch";

            internal static string OAuthToken {
                get { return m_Config.GetString(_s, "Twitch.OAuthToken", "", true);     }
                set {        m_Config.SetString(_s, "Twitch.OAuthToken", value);        }
            }
            internal static string OAuthAPIToken {
                get { return m_Config.GetString(_s, "Twitch.OAuthAPIToken", "", true);  }
                set {        m_Config.SetString(_s, "Twitch.OAuthAPIToken", value);     }
            }
            internal static string Channels {
                get { return m_Config.GetString(_s, "Twitch.Channels", "", true);       }
                set {        m_Config.SetString(_s, "Twitch.Channels", value);          }
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
                m_Config = new SDK.Config.INIConfig(System.IO.Path.Combine(l_Path, "auth.ini"), true);
            }
            catch
            {

            }
        }
    }
}
