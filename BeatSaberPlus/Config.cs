using UnityEngine;

namespace BeatSaberPlus
{
    /// <summary>
    /// Config class helper
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Config instance
        /// </summary>
        private static SDK.Config.INIConfig m_Config = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static bool FirstRun {
            get { return m_Config.GetBool("Config", "FirstRun", true, true);          }
            set {        m_Config.SetBool("Config", "FirstRun", value);               }
        }
        public static bool FirstChatCoreRun {
            get { return m_Config.GetBool("Config", "FirstChatCoreRun", true, true);  }
            set {        m_Config.SetBool("Config", "FirstChatCoreRun", value);       }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        public static void Init() => m_Config = new SDK.Config.INIConfig("BeatSaberPlus");
    }
}
