using IPA;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations
{
    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        /// <summary>
        /// Plugin instance
        /// </summary>
        internal static Plugin Instance { get; private set; }
        /// <summary>
        /// Custom logo texture
        /// </summary>
        internal static Texture2D CustomLogoTexture = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        [Init]
        public Plugin(IPA.Logging.Logger p_Logger)
        {
            /// Set instance
            Instance = this;

            /// Setup logger
            Logger.Instance = p_Logger;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On BeatSaberPlus_Online enable
        /// </summary>
        [OnStart]
        public void OnApplicationStart()
        {
            try
            {

            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Critical(p_Exception);
            }
        }
        /// <summary>
        /// On BeatSaberPlus_Online disable
        /// </summary>
        [OnExit]
        public void OnApplicationQuit()
        {

        }
    }
}
