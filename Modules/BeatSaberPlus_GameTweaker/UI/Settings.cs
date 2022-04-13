using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.UI
{
    /// <summary>
    /// Saber Tweaker view controller
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIObject("TabSelector")]
        private GameObject m_TabSelector;
        private TextSegmentedControl m_TabSelector_TabSelectorControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Particles tab
        [UIObject("ParticlesTab")]                                  private GameObject m_ParticlesTab = null;
        [UIComponent("ParticlesTab_RemoveDebris")]                  private ToggleSetting m_ParticlesTab_RemoveDebris;
        [UIComponent("ParticlesTab_RemoveCutParticles")]            private ToggleSetting m_ParticlesTab_RemoveCutParticles;
        [UIComponent("ParticlesTab_RemoveObstacleParticles")]       private ToggleSetting m_ParticlesTab_RemoveObstacleParticles;
        [UIComponent("ParticlesTab_RemoveFloorBurnMarkParticles")]  private ToggleSetting m_ParticlesTab_RemoveFloorBurnMarkParticles;
        [UIComponent("ParticlesTab_RemoveFloorBurnMarkEffects")]    private ToggleSetting m_ParticlesTab_RemoveFloorBurnMarkEffects;
        [UIComponent("ParticlesTab_RemoveSaberClashEffects")]       private ToggleSetting m_ParticlesTab_RemoveSaberClashEffects;
        [UIComponent("ParticlesTab_RemoveWorldParticles")]          private ToggleSetting m_ParticlesTab_RemoveWorldParticles;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Environment Tab
        [UIObject("EnvironmentTab")]                                    private GameObject m_EnvironmentTab = null;
        [UIComponent("EnvironmentTab_RemoveMusicBandLogo")]             private ToggleSetting m_EnvironmentTab_RemoveMusicBandLogo;
        [UIComponent("EnvironmentTab_RemoveFullComboLossAnimation")]    private ToggleSetting m_EnvironmentTab_RemoveFullComboLossAnimation;
        [UIComponent("EnvironmentTab_NoFake360Maps")]                   private ToggleSetting m_EnvironmentTab_NoFake360Maps;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region LevelSelection Tab
        [UIObject("LevelSelectionTab")]                                 private GameObject m_LevelSelectionTab = null;
        [UIComponent("LevelSelectionTab_RemoveBaseGameFilterButton")]   private ToggleSetting m_LevelSelectionTab_RemoveBaseGameFilterButton;
        [UIComponent("LevelSelectionTab_DeleteSongButton")]             private ToggleSetting m_LevelSelectionTab_DeleteSongButton;
        [UIComponent("LevelSelectionTab_RemoveSongBrowserTrashcan")]    private ToggleSetting m_LevelSelectionTab_RemoveSongBrowserTrashcan;
        [UIComponent("LevelSelectionTab_HighlightPlayedSong")]          private ToggleSetting m_LevelSelectionTab_HighlightPlayedSong;
        [UIComponent("LevelSelectionTab_HighlightPlayedSongColor")]     private ColorSetting m_LevelSelectionTab_HighlightPlayedSongColor;
        [UIComponent("LevelSelectionTab_HighlightPlayedSongAllColor")]  private ColorSetting m_LevelSelectionTab_HighlightPlayedSongAllColor;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region PlayerOptions Tab
        [UIObject("PlayerOptionsTab")]                                      private GameObject m_PlayerOptionsTab = null;
        [UIComponent("PlayerOptionsTab_JumpDurationIncrement")]             private ListSetting m_PlayerOptionsTab_JumpDurationIncrement;
        [UIValue("PlayerOptionsTab_JumpDurationIncrementChoices")]          private List<object> m_PlayerOptionsTab_JumpDurationIncrementChoices = new List<object>() { "5ms", "10ms", "100ms" };
        [UIValue("PlayerOptionsTab_JumpDurationIncrementValue")]            private string m_PlayerOptionsTab_JumpDurationIncrementValue;
        [UIComponent("PlayerOptionsTab_ReorderPlayerSettings")]             private ToggleSetting m_PlayerOptionsTab_ReorderPlayerSettings;
        [UIComponent("PlayerOptionsTab_AddOverrideLightIntensityOption")]   private ToggleSetting m_PlayerOptionsTab_AddOverrideLightIntensityOption;
        [UIComponent("PlayerOptionsTab_MergeLightEffectFilterOptions")]     private ToggleSetting m_PlayerOptionsTab_MergeLightEffectFilterOptions;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region MainMenu Tab
        [UIObject("MainMenuTab")]                                   private GameObject m_MainMenuTab = null;
        [UIComponent("MainMenuTab_OverrideMenuEnvColors")]          private ToggleSetting m_MainMenuTab_OverrideMenuEnvColors;
        [UIComponent("MainMenuTab_BaseColor")]                      private ColorSetting m_MainMenuTab_BaseColor;
        [UIComponent("MainMenuTab_LevelClearedColor")]              private ColorSetting m_MainMenuTab_LevelClearedColor;
        [UIComponent("MainMenuTab_LevelFailedColor")]               private ColorSetting m_MainMenuTab_LevelFailedColor;
        [UIComponent("MainMenuTab_DisableEditorButton")]            private ToggleSetting m_MainMenuTab_DisableEditorButton;
        [UIComponent("MainMenuTab_RemoveNewContentPromotional")]    private ToggleSetting m_MainMenuTab_RemoveNewContentPromotional;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Tools Tab
        [UIObject("ToolsTab")]                      private GameObject m_ToolsTab = null;
        [UIComponent("ToolsTab_RemoveOldLogs")]     private ToggleSetting m_ToolsTab_RemoveOldLogs;
        [UIComponent("ToolsTab_LogEntriesToKeep")]  private IncrementSetting m_ToolsTab_LogEntriesToKeep;
        [UIComponent("ToolsTab_FPFCEscape")]        private ToggleSetting m_ToolsTab_FPFCEscape;
        #endregion
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        internal Settings()
        {
            int l_TypeIndex = GTConfig.Instance.PlayerOptions.JumpDurationIncrement % m_PlayerOptionsTab_JumpDurationIncrementChoices.Count;
            if (l_TypeIndex >= 0)
                m_PlayerOptionsTab_JumpDurationIncrementValue = m_PlayerOptionsTab_JumpDurationIncrementChoices[l_TypeIndex] as string;
            else
            {
                GTConfig.Instance.PlayerOptions.JumpDurationIncrement = 1;
                m_PlayerOptionsTab_JumpDurationIncrementValue = m_PlayerOptionsTab_JumpDurationIncrementChoices[0] as string;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_Event2 = new BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged2), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_Event3 = new BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged3), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_Event4 = new BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged4), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Create type selector
            m_TabSelector_TabSelectorControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_TabSelector.transform as RectTransform, false);
            m_TabSelector_TabSelectorControl.SetTexts(new string[] { "Particles", "Environment", "LevelSelection", "PlayerOptions", "MainMenu", "Tools" });
            m_TabSelector_TabSelectorControl.ReloadData();
            m_TabSelector_TabSelectorControl.didSelectCellEvent += OnTabSelected;

            ////////////////////////////////////////////////////////////////////////////
            /// Prepare tabs
            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ParticlesTab,      0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_EnvironmentTab,    0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_LevelSelectionTab, 0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_PlayerOptionsTab,  0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_MainMenuTab,       0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ToolsTab,          0.50f);

            #region Particles tab
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveDebris,                   l_Event,    GTConfig.Instance.RemoveDebris,                 true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveCutParticles,             l_Event,    GTConfig.Instance.RemoveAllCutParticles,        true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveObstacleParticles,        l_Event,    GTConfig.Instance.RemoveObstacleParticles,      true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveFloorBurnMarkParticles,   l_Event,    GTConfig.Instance.RemoveSaberBurnMarkSparkles,  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveFloorBurnMarkEffects,     l_Event,    GTConfig.Instance.RemoveSaberBurnMarks,         true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveSaberClashEffects,        l_Event,    GTConfig.Instance.RemoveSaberClashEffects,      true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ParticlesTab_RemoveWorldParticles,           l_Event,    GTConfig.Instance.RemoveWorldParticles,         true);
            #endregion

            #region Environment Tab
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_EnvironmentTab_RemoveMusicBandLogo,          l_Event,    GTConfig.Instance.Environment.RemoveMusicBandLogo,          true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_EnvironmentTab_RemoveFullComboLossAnimation, l_Event,    GTConfig.Instance.Environment.RemoveFullComboLossAnimation, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_EnvironmentTab_NoFake360Maps,                l_Event,    GTConfig.Instance.Environment.NoFake360HUD,                 true);
            #endregion

            #region LevelSelection Tab
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_LevelSelectionTab_RemoveBaseGameFilterButton,    l_Event, GTConfig.Instance.LevelSelection.RemoveBaseGameFilterButton,   true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_LevelSelectionTab_DeleteSongButton,              l_Event, GTConfig.Instance.LevelSelection.DeleteSongButton,             true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_LevelSelectionTab_RemoveSongBrowserTrashcan,     l_Event, GTConfig.Instance.LevelSelection.DeleteSongBrowserTrashcan,    true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_LevelSelectionTab_HighlightPlayedSong,           l_Event, GTConfig.Instance.LevelSelection.HighlightEnabled,             true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_LevelSelectionTab_HighlightPlayedSongColor,       l_Event, GTConfig.Instance.LevelSelection.HighlightPlayed,              true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_LevelSelectionTab_HighlightPlayedSongAllColor,    l_Event, GTConfig.Instance.LevelSelection.HighlightAllPlayed,           true);
            #endregion

            #region PlayerOptions Tab
            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_PlayerOptionsTab_JumpDurationIncrement,                l_Event,                                                                true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_PlayerOptionsTab_ReorderPlayerSettings,              l_Event, GTConfig.Instance.PlayerOptions.ReorderPlayerSettings,         true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_PlayerOptionsTab_AddOverrideLightIntensityOption,    l_Event, GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption,  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_PlayerOptionsTab_MergeLightEffectFilterOptions,      l_Event, GTConfig.Instance.PlayerOptions.MergeLightPressetOptions,      true);
            #endregion

            #region MainMenu Tab
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_MainMenuTab_OverrideMenuEnvColors,       l_Event2, GTConfig.Instance.MainMenu.OverrideMenuEnvColors,          true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_MainMenuTab_BaseColor,                    l_Event2, GTConfig.Instance.MainMenu.BaseColor,                      true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_MainMenuTab_LevelClearedColor,            l_Event3, GTConfig.Instance.MainMenu.LevelClearedColor,              true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_MainMenuTab_LevelFailedColor,             l_Event4, GTConfig.Instance.MainMenu.LevelFailedColor,               true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_MainMenuTab_DisableEditorButton,         l_Event,  GTConfig.Instance.MainMenu.DisableEditorButtonOnMainMenu,  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_MainMenuTab_RemoveNewContentPromotional, l_Event,  GTConfig.Instance.MainMenu.RemoveNewContentPromotional,    true);
            #endregion

            #region Tools Tab
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ToolsTab_RemoveOldLogs,          l_Event,        GTConfig.Instance.Tools.RemoveOldLogs,    true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ToolsTab_LogEntriesToKeep,    l_Event, null,  GTConfig.Instance.Tools.LogEntriesToKeep, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ToolsTab_FPFCEscape,             l_Event,        GTConfig.Instance.Tools.FPFCEscape,       true);
            #endregion

            /// Show first tab by default
            OnTabSelected(null, 0);
            /// Refresh UI
            OnSettingChanged(null);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            GTConfig.Instance.Save();
            Managers.CustomMenuLightManager.SwitchToBase();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a tab is selected
        /// </summary>
        /// <param name="p_SegmentControl">Tab control instance</param>
        /// <param name="p_TabIndex">Tab index</param>
        private void OnTabSelected(SegmentedControl p_SegmentControl, int p_TabIndex)
        {
            m_ParticlesTab.SetActive(p_TabIndex == 0);
            m_EnvironmentTab.SetActive(p_TabIndex == 1);
            m_LevelSelectionTab.SetActive(p_TabIndex == 2);
            m_PlayerOptionsTab.SetActive(p_TabIndex == 3);
            m_MainMenuTab.SetActive(p_TabIndex == 4);
            m_ToolsTab.SetActive(p_TabIndex == 5);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            #region Particles tab
            GTConfig.Instance.RemoveDebris                     = m_ParticlesTab_RemoveDebris.Value;
            GTConfig.Instance.RemoveAllCutParticles            = m_ParticlesTab_RemoveCutParticles.Value;
            GTConfig.Instance.RemoveObstacleParticles          = m_ParticlesTab_RemoveObstacleParticles.Value;
            GTConfig.Instance.RemoveSaberBurnMarkSparkles      = m_ParticlesTab_RemoveFloorBurnMarkParticles.Value;
            GTConfig.Instance.RemoveSaberBurnMarks             = m_ParticlesTab_RemoveFloorBurnMarkEffects.Value;
            GTConfig.Instance.RemoveSaberClashEffects          = m_ParticlesTab_RemoveSaberClashEffects.Value;
            GTConfig.Instance.RemoveWorldParticles             = m_ParticlesTab_RemoveWorldParticles.Value;
            #endregion

            #region Environment Tab
            GTConfig.Instance.Environment.RemoveMusicBandLogo           = m_EnvironmentTab_RemoveMusicBandLogo.Value;
            GTConfig.Instance.Environment.RemoveFullComboLossAnimation  = m_EnvironmentTab_RemoveFullComboLossAnimation.Value;
            GTConfig.Instance.Environment.NoFake360HUD                  = m_EnvironmentTab_NoFake360Maps.Value;
            #endregion

            #region LevelSelection Tab
            GTConfig.Instance.LevelSelection.RemoveBaseGameFilterButton = m_LevelSelectionTab_RemoveBaseGameFilterButton.Value;
            GTConfig.Instance.LevelSelection.DeleteSongButton           = m_LevelSelectionTab_DeleteSongButton.Value;
            GTConfig.Instance.LevelSelection.DeleteSongBrowserTrashcan  = m_LevelSelectionTab_RemoveSongBrowserTrashcan.Value;
            GTConfig.Instance.LevelSelection.HighlightEnabled           = m_LevelSelectionTab_HighlightPlayedSong.Value;
            GTConfig.Instance.LevelSelection.HighlightPlayed            = m_LevelSelectionTab_HighlightPlayedSongColor.CurrentColor;
            GTConfig.Instance.LevelSelection.HighlightAllPlayed         = m_LevelSelectionTab_HighlightPlayedSongAllColor.CurrentColor;
            #endregion

            #region PlayerOptions Tab
            GTConfig.Instance.PlayerOptions.JumpDurationIncrement           = m_PlayerOptionsTab_JumpDurationIncrementChoices.IndexOf(m_PlayerOptionsTab_JumpDurationIncrement.Value as string);
            GTConfig.Instance.PlayerOptions.ReorderPlayerSettings           = m_PlayerOptionsTab_ReorderPlayerSettings.Value;
            GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption    = m_PlayerOptionsTab_AddOverrideLightIntensityOption.Value;
            GTConfig.Instance.PlayerOptions.MergeLightPressetOptions        = m_PlayerOptionsTab_MergeLightEffectFilterOptions.Value;
            #endregion

            #region MainMenu Tab
            GTConfig.Instance.MainMenu.OverrideMenuEnvColors            = m_MainMenuTab_OverrideMenuEnvColors.Value;
            GTConfig.Instance.MainMenu.BaseColor                        = m_MainMenuTab_BaseColor.CurrentColor;
            GTConfig.Instance.MainMenu.LevelClearedColor                = m_MainMenuTab_LevelClearedColor.CurrentColor;
            GTConfig.Instance.MainMenu.LevelFailedColor                 = m_MainMenuTab_LevelFailedColor.CurrentColor;
            GTConfig.Instance.MainMenu.DisableEditorButtonOnMainMenu    = m_MainMenuTab_DisableEditorButton.Value;
            GTConfig.Instance.MainMenu.RemoveNewContentPromotional      = m_MainMenuTab_RemoveNewContentPromotional.Value;

            m_MainMenuTab_BaseColor.interactable            = GTConfig.Instance.MainMenu.OverrideMenuEnvColors;
            m_MainMenuTab_LevelClearedColor.interactable    = GTConfig.Instance.MainMenu.OverrideMenuEnvColors;
            m_MainMenuTab_LevelFailedColor.interactable     = GTConfig.Instance.MainMenu.OverrideMenuEnvColors;
            #endregion

            #region Tools Tab
            GTConfig.Instance.Tools.RemoveOldLogs     = m_ToolsTab_RemoveOldLogs.Value;
            GTConfig.Instance.Tools.LogEntriesToKeep  = (int)m_ToolsTab_LogEntriesToKeep.Value;
            GTConfig.Instance.Tools.FPFCEscape        = m_ToolsTab_FPFCEscape.Value;
            #endregion

            /// Update patches
            GameTweaker.Instance.UpdatePatches(false);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged2(object p_Value)
        {
            OnSettingChanged(p_Value);
            Managers.CustomMenuLightManager.UpdateFromConfig();
            Managers.CustomMenuLightManager.SwitchToBase();
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged3(object p_Value)
        {
            OnSettingChanged(p_Value);
            Managers.CustomMenuLightManager.UpdateFromConfig();
            Managers.CustomMenuLightManager.SwitchToLevelCleared();
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged4(object p_Value)
        {
            OnSettingChanged(p_Value);
            Managers.CustomMenuLightManager.UpdateFromConfig();
            Managers.CustomMenuLightManager.SwitchToLevelFailed();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            #region Particles tab
            m_ParticlesTab_RemoveDebris.Value                   = GTConfig.Instance.RemoveDebris;
            m_ParticlesTab_RemoveCutParticles.Value             = GTConfig.Instance.RemoveAllCutParticles;
            m_ParticlesTab_RemoveObstacleParticles.Value        = GTConfig.Instance.RemoveObstacleParticles;
            m_ParticlesTab_RemoveFloorBurnMarkParticles.Value   = GTConfig.Instance.RemoveSaberBurnMarkSparkles;
            m_ParticlesTab_RemoveFloorBurnMarkEffects.Value     = GTConfig.Instance.RemoveSaberBurnMarks;
            m_ParticlesTab_RemoveSaberClashEffects.Value        = GTConfig.Instance.RemoveSaberClashEffects;
            m_ParticlesTab_RemoveWorldParticles.Value           = GTConfig.Instance.RemoveWorldParticles;
            #endregion

            #region Environment Tab
            m_EnvironmentTab_RemoveMusicBandLogo.Value              = GTConfig.Instance.Environment.RemoveMusicBandLogo;
            m_EnvironmentTab_RemoveFullComboLossAnimation.Value     = GTConfig.Instance.Environment.RemoveFullComboLossAnimation;
            m_EnvironmentTab_NoFake360Maps.Value                    = GTConfig.Instance.Environment.NoFake360HUD;
            #endregion

            #region LevelSelection Tab
            m_LevelSelectionTab_RemoveBaseGameFilterButton.Value            = GTConfig.Instance.LevelSelection.RemoveBaseGameFilterButton;
            m_LevelSelectionTab_DeleteSongButton.Value                      = GTConfig.Instance.LevelSelection.DeleteSongButton;
            m_LevelSelectionTab_RemoveSongBrowserTrashcan.Value             = GTConfig.Instance.LevelSelection.DeleteSongBrowserTrashcan;
            m_LevelSelectionTab_HighlightPlayedSong.Value                   = GTConfig.Instance.LevelSelection.HighlightEnabled;
            m_LevelSelectionTab_HighlightPlayedSongColor.CurrentColor       = GTConfig.Instance.LevelSelection.HighlightPlayed;
            m_LevelSelectionTab_HighlightPlayedSongAllColor.CurrentColor    = GTConfig.Instance.LevelSelection.HighlightAllPlayed;
            #endregion

            #region PlayerOptions Tab
            m_PlayerOptionsTab_JumpDurationIncrement.Value              = m_PlayerOptionsTab_JumpDurationIncrementChoices[GTConfig.Instance.PlayerOptions.JumpDurationIncrement % m_PlayerOptionsTab_JumpDurationIncrementChoices.Count];
            m_PlayerOptionsTab_ReorderPlayerSettings.Value              = GTConfig.Instance.PlayerOptions.ReorderPlayerSettings;
            m_PlayerOptionsTab_AddOverrideLightIntensityOption.Value    = GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption;
            m_PlayerOptionsTab_MergeLightEffectFilterOptions.Value      = GTConfig.Instance.PlayerOptions.MergeLightPressetOptions;
            #endregion

            #region MainMenu Tab
            m_MainMenuTab_OverrideMenuEnvColors.Value       = GTConfig.Instance.MainMenu.OverrideMenuEnvColors;
            m_MainMenuTab_BaseColor.CurrentColor            = GTConfig.Instance.MainMenu.BaseColor;
            m_MainMenuTab_LevelClearedColor.CurrentColor    = GTConfig.Instance.MainMenu.LevelClearedColor;
            m_MainMenuTab_LevelFailedColor.CurrentColor     = GTConfig.Instance.MainMenu.LevelFailedColor;
            m_MainMenuTab_DisableEditorButton.Value         = GTConfig.Instance.MainMenu.DisableEditorButtonOnMainMenu;
            m_MainMenuTab_RemoveNewContentPromotional.Value = GTConfig.Instance.MainMenu.RemoveNewContentPromotional;
            #endregion

            #region Tools Tab
            m_ToolsTab_RemoveOldLogs.Value      = GTConfig.Instance.Tools.RemoveOldLogs;
            m_ToolsTab_LogEntriesToKeep.Value   = GTConfig.Instance.Tools.LogEntriesToKeep;
            m_ToolsTab_FPFCEscape.Value         = GTConfig.Instance.Tools.FPFCEscape;
            #endregion

            m_PreventChanges = false;

            /// Refresh UI
            OnSettingChanged(null);
        }
    }
}
