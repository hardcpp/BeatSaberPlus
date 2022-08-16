using HarmonyLib;
using IPA;
using System.Reflection;

namespace BeatSaberPlus_GameTweaker
{
    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        /// <summary>
        /// Harmony ID for patches
        /// </summary>
        internal const string HarmonyID = "com.github.hardcpp.beatsaberplus_gametweaker";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Harmony patch holder
        /// </summary>
        private static Harmony m_Harmony;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        [Init]
        public Plugin(IPA.Logging.Logger p_Logger)
        {
            /// Setup logger
            Logger.Instance = new CP_SDK.Logging.IPALogger(p_Logger);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On  enable
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            try
            {
                Logger.Instance.Debug("Applying Harmony patches.");

                /// Setup harmony
                m_Harmony = new Harmony(HarmonyID);
                m_Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[Plugin.OnEnable] Error:");
                Logger.Instance.Error(p_Exception);
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
