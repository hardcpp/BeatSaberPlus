using BeatSaberMarkupLanguage;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace BeatSaberPlus_NoteTweaker
{
    /// <summary>
    /// NoteTweaker Module
    /// </summary>
    public class NoteTweaker : BeatSaberPlus.SDK.ModuleBase<NoteTweaker>
    {
        internal const string IMPORT_FOLDER = "UserData/BeatSaberPlus/NoteTweaker/Import/";
        internal const string EXPORT_FOLDER = "UserData/BeatSaberPlus/NoteTweaker/Export/";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Module type
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseType Type => BeatSaberPlus.SDK.IModuleBaseType.Integrated;
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
        public override bool IsEnabled { get => NTConfig.Instance.Enabled; set { NTConfig.Instance.Enabled = value; NTConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseActivationType ActivationType => BeatSaberPlus.SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

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
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange += OnSceneChange;

            /// Trigger a Set config
            OnSceneChange(BeatSaberPlus.SDK.Game.Logic.SceneType.Menu);

            try
            {
                if (!Directory.Exists(IMPORT_FOLDER))
                    Directory.CreateDirectory(IMPORT_FOLDER);
                if (!Directory.Exists(EXPORT_FOLDER))
                    Directory.CreateDirectory(EXPORT_FOLDER);
            }
            catch (System.Exception)
            {

            }
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange -= OnSceneChange;

            /// Restore config
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PGameNoteController.SetTemp(false, 1f);
            Patches.PBurstSliderGameNoteController.SetFromConfig(true);
            Patches.PBurstSliderGameNoteController.SetTemp(false, 1f);
            Patches.PBombController.SetFromConfig(true);
            Patches.PBombController.SetTemp(false, 1f);
            Patches.PSliderController.SetFromConfig(true);
            Patches.PSliderHapticFeedbackInteractionEffect.SetFromConfig(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// List available profiles
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableProfiles()
            => NTConfig.Instance.Profiles.Select(x => x.Name).ToList();
        /// <summary>
        /// Switch to profile
        /// </summary>
        /// <param name="p_Index">Profile index</param>
        public void SwitchToProfile(int p_Index)
        {
            p_Index = Mathf.Clamp(p_Index, 0, NTConfig.Instance.Profiles.Count);
            NTConfig.Instance.ActiveProfile = p_Index;

            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PBurstSliderGameNoteController.SetFromConfig(true);
            Patches.PBombController.SetFromConfig(true);
            Patches.PSliderController.SetFromConfig(true);
            Patches.PSliderHapticFeedbackInteractionEffect.SetFromConfig(true);
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
        private void OnSceneChange(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene)
        {
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PGameNoteController.SetTemp(false, 1f);
            Patches.PBurstSliderGameNoteController.SetFromConfig(true);
            Patches.PBurstSliderGameNoteController.SetTemp(false, 1f);
            Patches.PBombController.SetFromConfig(true);
            Patches.PBombController.SetTemp(false, 1f);
            Patches.PSliderController.SetFromConfig(true);
            Patches.PSliderHapticFeedbackInteractionEffect.SetFromConfig(true);
        }
    }
}
