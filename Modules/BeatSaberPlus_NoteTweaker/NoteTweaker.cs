using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker
{
    /// <summary>
    /// NoteTweaker Module
    /// </summary>
    public class NoteTweaker : CP_SDK.ModuleBase<NoteTweaker>
    {
        internal const string IMPORT_FOLDER = "UserData/BeatSaberPlus/NoteTweaker/Import/";
        internal const string EXPORT_FOLDER = "UserData/BeatSaberPlus/NoteTweaker/Export/";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Note Tweaker";
        public override string                              Description         => "Customize base notes!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#note-tweaker";
        public override bool                                UseChatFeatures     => false;
        public override bool                                IsEnabled           { get => NTConfig.Instance.Enabled; set { NTConfig.Instance.Enabled = value; NTConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsLeftView     m_SettingsLeftView  = null;
        private UI.SettingsMainView     m_SettingsMainView  = null;
        private UI.SettingsRightView    m_SettingsRightView = null;

        private int? m_BackupProfileIndex = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            CP_SDK_BS.Game.Logic.OnSceneChange += OnSceneChange;

            /// Trigger a Set config
            OnSceneChange(CP_SDK_BS.Game.Logic.ESceneType.Menu);

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
            CP_SDK_BS.Game.Logic.OnSceneChange -= OnSceneChange;

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsRightView);

            /// Restore config
            Patches.PBombNoteController.SetFromConfig(true);
            Patches.PBombNoteController.SetTemp(false, 1f);
            Patches.PBurstSliderGameNoteController.SetFromConfig(true);
            Patches.PBurstSliderGameNoteController.SetTemp(false, 1f);
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PGameNoteController.SetTemp(false, 1f);
            Patches.PSliderController.SetFromConfig(true);
            Patches.PSliderHapticFeedbackInteractionEffect.SetFromConfig(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsLeftView == null)     m_SettingsLeftView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();
            if (m_SettingsMainView == null)     m_SettingsMainView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsRightView == null)    m_SettingsRightView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsRightView>();

            /// Change main view
            return (m_SettingsMainView, m_SettingsLeftView, m_SettingsRightView);
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
        /// <param name="p_Temporary">Is a temporary change?</param>
        public void SwitchToProfile(int p_Index, bool p_Temporary)
        {
            if (p_Temporary)
                m_BackupProfileIndex = NTConfig.Instance.ActiveProfile;
            else
                m_BackupProfileIndex = null;

            NTConfig.Instance.ActiveProfile = Mathf.Clamp(p_Index, 0, NTConfig.Instance.Profiles.Count);

            Patches.PBombNoteController.SetFromConfig(true);
            Patches.PBurstSliderGameNoteController.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PSliderController.SetFromConfig(true);
            Patches.PSliderHapticFeedbackInteractionEffect.SetFromConfig(true);

            if (!p_Temporary)
                NTConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">New active scene</param>
        private void OnSceneChange(CP_SDK_BS.Game.Logic.ESceneType p_Scene)
        {
            if (p_Scene == CP_SDK_BS.Game.Logic.ESceneType.Playing && m_BackupProfileIndex.HasValue)
                NTConfig.Instance.ActiveProfile = Mathf.Clamp(m_BackupProfileIndex.Value, 0, NTConfig.Instance.Profiles.Count);

            Patches.PBombNoteController.SetFromConfig(true);
            Patches.PBombNoteController.SetTemp(false, 1f);
            Patches.PBurstSliderGameNoteController.SetFromConfig(true);
            Patches.PBurstSliderGameNoteController.SetTemp(false, 1f);
            Patches.PColorNoteVisuals.SetFromConfig(true);
            Patches.PColorNoteVisuals.SetBlockColorOverride(false, Color.black, Color.black);
            Patches.PGameNoteController.SetFromConfig(true);
            Patches.PGameNoteController.SetTemp(false, 1f);
            Patches.PSliderController.SetFromConfig(true);
            Patches.PSliderHapticFeedbackInteractionEffect.SetFromConfig(true);
        }
    }
}
