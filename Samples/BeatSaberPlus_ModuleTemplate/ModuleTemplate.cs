namespace BeatSaberPlus_ModuleTemplate
{
    /// <summary>
    /// Online instance
    /// </summary>
    internal class ModuleTemplate : CP_SDK.ModuleBase<ModuleTemplate>
    {
        public override CP_SDK.EIModuleBaseType             Type            => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name            => "Module Template";
        public override string                              Description     => "Hello world!";
        public override bool                                UseChatFeatures => false;
        public override bool                                IsEnabled       { get => MTConfig.Instance.Enabled; set { MTConfig.Instance.Enabled = value; MTConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType  => CP_SDK.EIModuleBaseActivationType.OnStart;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsMainView m_SettingsMainView = null;

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
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsMainView == null) m_SettingsMainView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();

            return (m_SettingsMainView, null, null);
        }

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

            Logger.Instance.Warning($"Map {l_MapName} started on platform {l_PlatformName}");
        }
    }
}
