using BeatSaberMarkupLanguage;
using UnityEngine;

namespace BeatSaberPlus.Modules.NoteTweaker
{
    /// <summary>
    /// NoteTweaker Module
    /// </summary>
    internal class NoteTweaker : SDK.ModuleBase<NoteTweaker>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Note Tweaker";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Customize base notes!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.NoteTweaker.Enabled; set => Config.NoteTweaker.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// NoteTweaker view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// NoteTweaker left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// NoteTweaker right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            SDK.Game.Logic.OnSceneChange += OnSceneChange;

            /// Trigger a Set config
            OnSceneChange(SDK.Game.Logic.SceneType.Menu);
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            SDK.Game.Logic.OnSceneChange -= OnSceneChange;

            /// Restore config
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PBombController.SetFromConfig(true);
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
            /// Create view if needed
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            /// Create view if needed
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            /// Change main view
            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">New active scene</param>
        private void OnSceneChange(SDK.Game.Logic.SceneType p_Scene)
        {
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PBombController.SetFromConfig(true);
        }
    }
}
