using BeatSaberMarkupLanguage;

namespace BeatSaberPlus.Modules.Online
{
    /// <summary>
    /// Online instance
    /// </summary>
    internal class Online : SDK.ModuleBase<Online>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Online";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Customize your online experience!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.Online.Enabled; set => Config.Online.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnStart;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Apply beat saver patch
            Patches.PBeatSaverSharp_BeatSaver.SetUseBeatSaberPlusCustomMapsServer(Config.Online.UseBSPCustomMapsServer);
        }
        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Restore beat saver patch
            Patches.PBeatSaverSharp_BeatSaver.SetUseBeatSaberPlusCustomMapsServer(false);
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
        /// Saber tweaker view
        /// </summary>
        private UI.Settings m_SettingsView = null;
    }
}
