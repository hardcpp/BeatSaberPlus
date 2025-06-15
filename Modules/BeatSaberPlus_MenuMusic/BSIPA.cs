using HarmonyLib;
using IPA;
using System.Reflection;

namespace BeatSaberPlus_MenuMusic
{
    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class BSIPA
    {
        internal const string HarmonyID = "com.github.hardcpp.beatsaberplus_menumusic";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static Harmony m_Harmony;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        [Init]
        public BSIPA(IPA.Logging.Logger p_Logger)
        {
            /// Setup logger
            ChatPlexMod_MenuMusic.Logger.Instance = new CP_SDK.Logging.IPALogger(p_Logger);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On BeatSaberPlus enable
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            try
            {
                ChatPlexMod_MenuMusic.Logger.Instance.Debug("Applying Harmony patches.");

                /// Setup harmony
                m_Harmony = new Harmony(HarmonyID);
                m_Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (System.Exception p_Exception)
            {
                ChatPlexMod_MenuMusic.Logger.Instance.Error("[Plugin.OnEnabled] Error:");
                ChatPlexMod_MenuMusic.Logger.Instance.Error(p_Exception);
            }
        }
        /// <summary>
        /// On BeatSaberPlus disable
        /// </summary>
        [OnDisable]
        public void OnDisable()
        {

        }
    }
}
