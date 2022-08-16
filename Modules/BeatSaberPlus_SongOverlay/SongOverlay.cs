using BeatSaberMarkupLanguage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_SongOverlay
{
    /// <summary>
    /// Online instance
    /// </summary>
    internal class SongOverlay : BeatSaberPlus.SDK.BSPModuleBase<SongOverlay>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override CP_SDK.EIModuleBaseType Type => CP_SDK.EIModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Song Overlay";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Song overlay server for your stream!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => SOConfig.Instance.Enabled; set { SOConfig.Instance.Enabled = value; SOConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override CP_SDK.EIModuleBaseActivationType ActivationType => CP_SDK.EIModuleBaseActivationType.OnStart;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Settings view
        /// </summary>
        private UI.Settings m_SettingsView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnEnable()
        {
            Network.OverlayServer.Start();

            CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_CheckCompatibility());
        }
        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnDisable()
        {
            Network.OverlayServer.Stop();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();

            /// Change main view
            return (m_SettingsView, null, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check compatibility coroutine
        /// </summary>
        /// <returns></returns>
        private static IEnumerator Coroutine_CheckCompatibility()
        {
            yield return new WaitForSeconds(10f);

            var l_OverlaysMods = new List<string>()
            {
                "BeatSaberPlus_SongOverlay"
            };

            if (IPA.Loader.PluginManager.GetPluginFromId("BeatSaberHTTPStatus") != null)
                l_OverlaysMods.Add("BeatSaberHTTPStatus");

            if (IPA.Loader.PluginManager.GetPluginFromId("DataPuller") != null)
                l_OverlaysMods.Add("DataPuller");

            if (l_OverlaysMods.Count > 1)
            {
                CP_SDK.Chat.Service.Multiplexer.InternalBroadcastSystemMessage(
                    "<color=red>Warning you are running multiple song overlay mods (<color=yellow>" + string.Join(", ", l_OverlaysMods) + "</color>). "
                    + "It's recommended to only use 1 of these mods for performance reasons");
            }
        }

    }
}
