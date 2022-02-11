using BeatSaberMarkupLanguage;
using UnityEngine;

namespace BeatSaberPlus_ModuleTemplate
{
    /// <summary>
    /// Online instance
    /// </summary>
    internal class ModuleTemplate : BeatSaberPlus.SDK.ModuleBase<ModuleTemplate>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseType Type => BeatSaberPlus.SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Module Template";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Hello world!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => MTConfig.Instance.Enabled; set { MTConfig.Instance.Enabled = value; MTConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseActivationType ActivationType => BeatSaberPlus.SDK.IModuleBaseActivationType.OnStart;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            BeatSaberPlus.SDK.Game.Logic.OnLevelStarted += Game_OnLevelStarted;
        }
        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Unbind event
            BeatSaberPlus.SDK.Game.Logic.OnLevelStarted -= Game_OnLevelStarted;
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
        /// Settings view
        /// </summary>
        private UI.Settings m_SettingsView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On level started
        /// </summary>
        /// <param name="p_LevelData">Level data</param>
        private void Game_OnLevelStarted(BeatSaberPlus.SDK.Game.LevelData p_LevelData)
        {
            var l_MapName = p_LevelData?.Data?.previewBeatmapLevel?.songName ?? "?";
            var l_PlatformName = p_LevelData?.Data?.environmentInfo?.serializedName ?? "?";

            Logger.Instance.Warn($"Map {l_MapName} started on platform {l_PlatformName}");
        }
    }
}
