using BeatSaberMarkupLanguage;

namespace BeatSaberPlus.Plugins.Online
{
    /// <summary>
    /// Online instance
    /// </summary>
    class Online : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Online";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.Online.Enabled; set => Config.Online.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.OnStart;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static Online Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind singleton
            Instance = this;

            /// Apply beat saver patch
            Patches.PBeatSaverSharp_BeatSaver.SetUseBeatSaberPlusCustomMapsServer(Config.Online.UseBSPCustomMapsServer);
        }
        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnDisable()
        {
            /// Restore beat saver patch
            Patches.PBeatSaverSharp_BeatSaver.SetUseBeatSaberPlusCustomMapsServer(false);

            /// Unbind singleton
            Instance = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        protected override void ShowUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();

            /// Change main view
            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.ChangeMainViewController(m_SettingsView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.Settings m_SettingsView = null;
    }
}
