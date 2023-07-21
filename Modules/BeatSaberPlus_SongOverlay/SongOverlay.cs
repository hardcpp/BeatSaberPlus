using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_SongOverlay
{
    /// <summary>
    /// Song Overlay instance
    /// </summary>
    public class SongOverlay : CP_SDK.ModuleBase<SongOverlay>
    {
        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Song Overlay";
        public override string                              Description         => "Song overlay server for your stream!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#song-overlay";
        public override bool                                UseChatFeatures     => false;
        public override bool                                IsEnabled           { get => SOConfig.Instance.Enabled; set { SOConfig.Instance.Enabled = value; SOConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnStart;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsLeftView m_SettingsLeftView = null;
        private UI.SettingsMainView m_SettingsMainView = null;

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
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);

            Network.OverlayServer.Stop();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsMainView == null) m_SettingsMainView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsLeftView == null) m_SettingsLeftView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();

            return (m_SettingsMainView, m_SettingsLeftView, null);
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
