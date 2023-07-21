using CP_SDK.XUI;

namespace BeatSaberPlus_GameTweaker.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUITabControl   m_TabControl;

        private XUIToggle       m_ParticlesTab_RemoveDebris;
        private XUIToggle       m_ParticlesTab_RemoveWorldParticles;
        private XUIToggle       m_ParticlesTab_RemoveCutParticles;
        private XUIToggle       m_ParticlesTab_RemoveObstacleParticles;
        private XUIToggle       m_ParticlesTab_RemoveFloorBurnMarkParticles;
        private XUIToggle       m_ParticlesTab_RemoveFloorBurnMarkEffects;
        private XUIToggle       m_ParticlesTab_RemoveSaberClashEffects;

        private XUIToggle       m_EnvironmentTab_RemoveMusicBandLogo;
        private XUIToggle       m_EnvironmentTab_RemoveFullComboLossAnimation;
        private XUIToggle       m_EnvironmentTab_NoFake360Maps;

        private XUIColorInput   m_LevelSelectionTab_HighlightPlayedSongColor;
        private XUIToggle       m_LevelSelectionTab_HighlightPlayedSong;
        private XUIColorInput   m_LevelSelectionTab_HighlightPlayedSongAllColor;
        private XUIToggle       m_LevelSelectionTab_RemoveBaseGameFilterButton;
        private XUIToggle       m_LevelSelectionTab_DeleteSongButton;
        private XUIToggle       m_LevelSelectionTab_RemoveSongBrowserTrashcan;

        private XUIToggle       m_PlayerOptionsTab_ReorderPlayerSettings;
        private XUIToggle       m_PlayerOptionsTab_AddOverrideLightIntensityOption;
        private XUIToggle       m_PlayerOptionsTab_MergeLightEffectFilterOptions;

        private XUIToggle       m_MainMenuTab_OverrideMenuEnvColors;
        private XUIColorInput   m_MainMenuTab_BaseColor;
        private XUIColorInput   m_MainMenuTab_LevelClearedColor;
        private XUIColorInput   m_MainMenuTab_LevelFailedColor;
        private XUIToggle       m_MainMenuTab_DisableEditorButton;
        private XUIToggle       m_MainMenuTab_RemoveNewContentPromotional;
        private XUIToggle       m_MainMenuTab_DisableFireworks;

        private XUIToggle       m_ToolsTab_RemoveOldLogs;
        private XUISlider       m_ToolsTab_LogEntriesToKeep;
        private XUIToggle       m_ToolsTab_FPFCEscape;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Game Tweaker | Settings"),

                XUITabControl.Make(
                    ("Particles",       BuildParticlesTab()),
                    ("Environment",     BuildEnvironmentTab()),
                    ("LevelSelection",  BuildLevelSelectionTab()),
                    ("PlayerOptions",   BuildPlayerOptionsTab()),
                    ("MainMenu",        BuildMainMenuTab()),
                    ("Tools",           BuildToolsTab())
                )
                .Bind(ref m_TabControl)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            /// Refresh UI
            OnSettingChanged();
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
        /// Build particles tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildParticlesTab()
        {
            var l_Config = GTConfig.Instance;
            return XUIVLayout.Make(
                XUIText.Make("Remove all debris spawn"),
                XUIToggle.Make().SetValue(l_Config.RemoveDebris).Bind(ref m_ParticlesTab_RemoveDebris).OnValueChanged((_) => OnSettingChanged()),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Remove world particles"),
                        XUIToggle.Make().SetValue(l_Config.RemoveWorldParticles).Bind(ref m_ParticlesTab_RemoveWorldParticles),
                        XUIText.Make("Remove floor burn particles"),
                        XUIToggle.Make().SetValue(l_Config.RemoveSaberBurnMarkSparkles).Bind(ref m_ParticlesTab_RemoveFloorBurnMarkParticles)
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Remove cut particles"),
                        XUIToggle.Make().SetValue(l_Config.RemoveAllCutParticles).Bind(ref m_ParticlesTab_RemoveCutParticles),
                        XUIText.Make("Remove floor burn marks"),
                        XUIToggle.Make().SetValue(l_Config.RemoveSaberBurnMarks).Bind(ref m_ParticlesTab_RemoveFloorBurnMarkEffects)
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Remove obstacle particles"),
                        XUIToggle.Make().SetValue(l_Config.RemoveObstacleParticles).Bind(ref m_ParticlesTab_RemoveObstacleParticles),
                        XUIText.Make("Remove saber clash effect"),
                        XUIToggle.Make().SetValue(l_Config.RemoveSaberClashEffects).Bind(ref m_ParticlesTab_RemoveSaberClashEffects)
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()))
                )
                .SetPadding(0)
            );
        }
        /// <summary>
        /// Build environment tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildEnvironmentTab()
        {
            var l_Config = GTConfig.Instance.Environment;
            return XUIVLayout.Make(
                XUIText.Make("Remove music group logos (BTS, LinkinPark...) in environments"),
                XUIToggle.Make().SetValue(l_Config.RemoveMusicBandLogo).Bind(ref m_EnvironmentTab_RemoveMusicBandLogo),

                XUIText.Make("Remove full combo loss animation"),
                XUIToggle.Make().SetValue(l_Config.RemoveFullComboLossAnimation).Bind(ref m_EnvironmentTab_RemoveFullComboLossAnimation),

                XUIText.Make("No fake 360 maps"),
                XUIToggle.Make().SetValue(l_Config.NoFake360HUD).Bind(ref m_EnvironmentTab_NoFake360Maps)
            )
            .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()));
        }
        /// <summary>
        /// Build level selection tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildLevelSelectionTab()
        {
            var l_Config = GTConfig.Instance.LevelSelection;
            return XUIVLayout.Make(
                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Played color"),
                        XUIColorInput.Make().SetValue(l_Config.HighlightPlayed).Bind(ref m_LevelSelectionTab_HighlightPlayedSongColor)
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Highlight played song"),
                        XUIToggle.Make().SetValue(l_Config.HighlightEnabled).Bind(ref m_LevelSelectionTab_HighlightPlayedSong)
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("All played color"),
                        XUIColorInput.Make().SetValue(l_Config.HighlightAllPlayed).Bind(ref m_LevelSelectionTab_HighlightPlayedSongAllColor)
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChanged()))
                )
                .SetPadding(0),

                XUIText.Make("Remove base game filter"),
                XUIToggle.Make().SetValue(l_Config.RemoveBaseGameFilterButton).Bind(ref m_LevelSelectionTab_RemoveBaseGameFilterButton),

                XUIText.Make("Song delete button"),
                XUIToggle.Make().SetValue(l_Config.DeleteSongButton).Bind(ref m_LevelSelectionTab_DeleteSongButton),

                XUIText.Make("Remove SongBrowser trashcan"),
                XUIToggle.Make().SetValue(l_Config.DeleteSongButton).Bind(ref m_LevelSelectionTab_RemoveSongBrowserTrashcan)
            )
            .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()))
            .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChanged()));
        }
        /// <summary>
        /// Build player options tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildPlayerOptionsTab()
        {
            var l_Config = GTConfig.Instance.PlayerOptions;
            return XUIVLayout.Make(
                XUIText.Make("Better player options menu"),
                XUIToggle.Make().SetValue(l_Config.ReorderPlayerSettings).Bind(ref m_PlayerOptionsTab_ReorderPlayerSettings),

                XUIText.Make("Add override lights intensity option"),
                XUIToggle.Make().SetValue(l_Config.OverrideLightIntensityOption).Bind(ref m_PlayerOptionsTab_AddOverrideLightIntensityOption),

                XUIText.Make("Merge light effect filter options"),
                XUIToggle.Make().SetValue(l_Config.MergeLightPressetOptions).Bind(ref m_PlayerOptionsTab_MergeLightEffectFilterOptions)
            )
            .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()));
        }
        /// <summary>
        /// Build main menu tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildMainMenuTab()
        {
            var l_Config = GTConfig.Instance.MainMenu;
            return XUIVLayout.Make(
                XUIText.Make("Override menu environment colors"),
                XUIToggle.Make().SetValue(l_Config.OverrideMenuEnvColors).Bind(ref m_MainMenuTab_OverrideMenuEnvColors).OnValueChanged((_) => OnSettingChangedOverrideMenuColor()),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Base color"),
                        XUIColorInput.Make().SetValue(l_Config.BaseColor).Bind(ref m_MainMenuTab_BaseColor),

                        XUIText.Make("Disable Editor button"),
                        XUIToggle.Make().SetValue(l_Config.DisableEditorButtonOnMainMenu).Bind(ref m_MainMenuTab_DisableEditorButton).OnValueChanged((_) => OnSettingChanged())
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChangedOverrideMenuColor())),

                    XUIVLayout.Make(
                        XUIText.Make("Level cleared color"),
                        XUIColorInput.Make().SetValue(l_Config.LevelClearedColor).Bind(ref m_MainMenuTab_LevelClearedColor),

                        XUIText.Make("Remove new content promotional"),
                        XUIToggle.Make().SetValue(l_Config.RemoveNewContentPromotional).Bind(ref m_MainMenuTab_RemoveNewContentPromotional).OnValueChanged((_) => OnSettingChanged())
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChangedLevelClearedColor())),

                    XUIVLayout.Make(
                        XUIText.Make("Level failed color"),
                        XUIColorInput.Make().SetValue(l_Config.LevelFailedColor).Bind(ref m_MainMenuTab_LevelFailedColor),

                        XUIText.Make("Disable fireworks"),
                        XUIToggle.Make().SetValue(l_Config.DisableFireworks).Bind(ref m_MainMenuTab_DisableFireworks).OnValueChanged((_) => OnSettingChanged())
                    )
                    .SetPadding(0)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChangedLevelFailedColor()))
                )
                .SetPadding(0)
            );
        }
        /// <summary>
        /// Build tools tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildToolsTab()
        {
            var l_Config = GTConfig.Instance.Tools;
            return XUIVLayout.Make(
                XUIText.Make("Remove old logs"),
                XUIToggle.Make().SetValue(l_Config.RemoveOldLogs).Bind(ref m_ToolsTab_RemoveOldLogs),

                XUIText.Make("Amount of logs to keep"),
                XUISlider.Make().SetMinValue(4.0f).SetMaxValue(20.0f).SetIncrements(1.0f).SetInteger(true).SetValue(l_Config.LogEntriesToKeep).Bind(ref m_ToolsTab_LogEntriesToKeep),

                XUIText.Make("FPFC Escape"),
                XUIToggle.Make().SetValue(l_Config.FPFCEscape).Bind(ref m_ToolsTab_FPFCEscape)
            )
            .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged()))
            .ForEachDirect<XUISlider>(x => x.OnValueChanged((_) => OnSettingChanged()));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On setting changed
        /// </summary>
        private void OnSettingChanged()
        {
            if (m_PreventChanges)
                return;

            #region Particles tab
            var l_ParticlesConfig = GTConfig.Instance;
            l_ParticlesConfig.RemoveDebris                     = m_ParticlesTab_RemoveDebris.Element.GetValue();
            l_ParticlesConfig.RemoveAllCutParticles            = m_ParticlesTab_RemoveCutParticles.Element.GetValue();
            l_ParticlesConfig.RemoveObstacleParticles          = m_ParticlesTab_RemoveObstacleParticles.Element.GetValue();
            l_ParticlesConfig.RemoveSaberBurnMarkSparkles      = m_ParticlesTab_RemoveFloorBurnMarkParticles.Element.GetValue();
            l_ParticlesConfig.RemoveSaberBurnMarks             = m_ParticlesTab_RemoveFloorBurnMarkEffects.Element.GetValue();
            l_ParticlesConfig.RemoveSaberClashEffects          = m_ParticlesTab_RemoveSaberClashEffects.Element.GetValue();
            l_ParticlesConfig.RemoveWorldParticles             = m_ParticlesTab_RemoveWorldParticles.Element.GetValue();
            #endregion

            #region Environment Tab
            var l_EnvironmentConfig = GTConfig.Instance.Environment;
            l_EnvironmentConfig.RemoveMusicBandLogo           = m_EnvironmentTab_RemoveMusicBandLogo.Element.GetValue();
            l_EnvironmentConfig.RemoveFullComboLossAnimation  = m_EnvironmentTab_RemoveFullComboLossAnimation.Element.GetValue();
            l_EnvironmentConfig.NoFake360HUD                  = m_EnvironmentTab_NoFake360Maps.Element.GetValue();
            #endregion

            #region LevelSelection Tab
            var l_LevelSelectionConfig = GTConfig.Instance.LevelSelection;
            l_LevelSelectionConfig.RemoveBaseGameFilterButton = m_LevelSelectionTab_RemoveBaseGameFilterButton.Element.GetValue();
            l_LevelSelectionConfig.DeleteSongButton           = m_LevelSelectionTab_DeleteSongButton.Element.GetValue();
            l_LevelSelectionConfig.DeleteSongBrowserTrashcan  = m_LevelSelectionTab_RemoveSongBrowserTrashcan.Element.GetValue();
            l_LevelSelectionConfig.HighlightEnabled           = m_LevelSelectionTab_HighlightPlayedSong.Element.GetValue();
            l_LevelSelectionConfig.HighlightPlayed            = m_LevelSelectionTab_HighlightPlayedSongColor.Element.GetValue();
            l_LevelSelectionConfig.HighlightAllPlayed         = m_LevelSelectionTab_HighlightPlayedSongAllColor.Element.GetValue();
            #endregion

            #region PlayerOptions Tab
            var l_PlayerOptionConfig = GTConfig.Instance.PlayerOptions;
            l_PlayerOptionConfig.ReorderPlayerSettings        = m_PlayerOptionsTab_ReorderPlayerSettings.Element.GetValue();
            l_PlayerOptionConfig.OverrideLightIntensityOption = m_PlayerOptionsTab_AddOverrideLightIntensityOption.Element.GetValue();
            l_PlayerOptionConfig.MergeLightPressetOptions     = m_PlayerOptionsTab_MergeLightEffectFilterOptions.Element.GetValue();
            #endregion

            #region MainMenu Tab
            var l_MainMenuConfig = GTConfig.Instance.MainMenu;
            l_MainMenuConfig.OverrideMenuEnvColors            = m_MainMenuTab_OverrideMenuEnvColors.Element.GetValue();
            l_MainMenuConfig.BaseColor                        = m_MainMenuTab_BaseColor.Element.GetValue();
            l_MainMenuConfig.LevelClearedColor                = m_MainMenuTab_LevelClearedColor.Element.GetValue();
            l_MainMenuConfig.LevelFailedColor                 = m_MainMenuTab_LevelFailedColor.Element.GetValue();
            l_MainMenuConfig.DisableEditorButtonOnMainMenu    = m_MainMenuTab_DisableEditorButton.Element.GetValue();
            l_MainMenuConfig.RemoveNewContentPromotional      = m_MainMenuTab_RemoveNewContentPromotional.Element.GetValue();
            l_MainMenuConfig.DisableFireworks                 = m_MainMenuTab_DisableFireworks.Element.GetValue();

            m_MainMenuTab_BaseColor         .SetInteractable(l_MainMenuConfig.OverrideMenuEnvColors);
            m_MainMenuTab_LevelClearedColor .SetInteractable(l_MainMenuConfig.OverrideMenuEnvColors);
            m_MainMenuTab_LevelFailedColor  .SetInteractable(l_MainMenuConfig.OverrideMenuEnvColors);
            #endregion

            #region Tools Tab
            var l_ToolsConfig = GTConfig.Instance.Tools;

            l_ToolsConfig.RemoveOldLogs     = m_ToolsTab_RemoveOldLogs.Element.GetValue();
            l_ToolsConfig.LogEntriesToKeep  = (int)m_ToolsTab_LogEntriesToKeep.Element.GetValue();
            l_ToolsConfig.FPFCEscape        = m_ToolsTab_FPFCEscape.Element.GetValue();
            #endregion

            /// Update patches
            GameTweaker.Instance.UpdatePatches(false);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        private void OnSettingChangedOverrideMenuColor()
        {
            OnSettingChanged();
            Managers.CustomMenuLightManager.UpdateFromConfig();
            Managers.CustomMenuLightManager.SwitchToBase();
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        private void OnSettingChangedLevelClearedColor()
        {
            OnSettingChanged();
            Managers.CustomMenuLightManager.UpdateFromConfig();
            Managers.CustomMenuLightManager.SwitchToLevelCleared();
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        private void OnSettingChangedLevelFailedColor()
        {
            OnSettingChanged();
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
            m_ParticlesTab_RemoveDebris                 .SetValue(GTConfig.Instance.RemoveDebris);
            m_ParticlesTab_RemoveCutParticles           .SetValue(GTConfig.Instance.RemoveAllCutParticles);
            m_ParticlesTab_RemoveObstacleParticles      .SetValue(GTConfig.Instance.RemoveObstacleParticles);
            m_ParticlesTab_RemoveFloorBurnMarkParticles .SetValue(GTConfig.Instance.RemoveSaberBurnMarkSparkles);
            m_ParticlesTab_RemoveFloorBurnMarkEffects   .SetValue(GTConfig.Instance.RemoveSaberBurnMarks);
            m_ParticlesTab_RemoveSaberClashEffects      .SetValue(GTConfig.Instance.RemoveSaberClashEffects);
            m_ParticlesTab_RemoveWorldParticles         .SetValue(GTConfig.Instance.RemoveWorldParticles);
            #endregion

            #region Environment Tab
            m_EnvironmentTab_RemoveMusicBandLogo            .SetValue(GTConfig.Instance.Environment.RemoveMusicBandLogo);
            m_EnvironmentTab_RemoveFullComboLossAnimation   .SetValue(GTConfig.Instance.Environment.RemoveFullComboLossAnimation);
            m_EnvironmentTab_NoFake360Maps                  .SetValue(GTConfig.Instance.Environment.NoFake360HUD);
            #endregion

            #region LevelSelection Tab
            m_LevelSelectionTab_RemoveBaseGameFilterButton  .SetValue(GTConfig.Instance.LevelSelection.RemoveBaseGameFilterButton);
            m_LevelSelectionTab_DeleteSongButton            .SetValue(GTConfig.Instance.LevelSelection.DeleteSongButton);
            m_LevelSelectionTab_RemoveSongBrowserTrashcan   .SetValue(GTConfig.Instance.LevelSelection.DeleteSongBrowserTrashcan);
            m_LevelSelectionTab_HighlightPlayedSong         .SetValue(GTConfig.Instance.LevelSelection.HighlightEnabled);
            m_LevelSelectionTab_HighlightPlayedSongColor    .SetValue(GTConfig.Instance.LevelSelection.HighlightPlayed);
            m_LevelSelectionTab_HighlightPlayedSongAllColor .SetValue(GTConfig.Instance.LevelSelection.HighlightAllPlayed);
            #endregion

            #region PlayerOptions Tab
            m_PlayerOptionsTab_ReorderPlayerSettings            .SetValue(GTConfig.Instance.PlayerOptions.ReorderPlayerSettings);
            m_PlayerOptionsTab_AddOverrideLightIntensityOption  .SetValue(GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption);
            m_PlayerOptionsTab_MergeLightEffectFilterOptions    .SetValue(GTConfig.Instance.PlayerOptions.MergeLightPressetOptions);
            #endregion

            #region MainMenu Tab
            m_MainMenuTab_OverrideMenuEnvColors         .SetValue(GTConfig.Instance.MainMenu.OverrideMenuEnvColors);
            m_MainMenuTab_BaseColor                     .SetValue(GTConfig.Instance.MainMenu.BaseColor);
            m_MainMenuTab_LevelClearedColor             .SetValue(GTConfig.Instance.MainMenu.LevelClearedColor);
            m_MainMenuTab_LevelFailedColor              .SetValue(GTConfig.Instance.MainMenu.LevelFailedColor);
            m_MainMenuTab_DisableEditorButton           .SetValue(GTConfig.Instance.MainMenu.DisableEditorButtonOnMainMenu);
            m_MainMenuTab_RemoveNewContentPromotional   .SetValue(GTConfig.Instance.MainMenu.RemoveNewContentPromotional);
            m_MainMenuTab_DisableFireworks              .SetValue(GTConfig.Instance.MainMenu.DisableFireworks);
            #endregion

            #region Tools Tab
            m_ToolsTab_RemoveOldLogs    .SetValue(GTConfig.Instance.Tools.RemoveOldLogs);
            m_ToolsTab_LogEntriesToKeep .SetValue(GTConfig.Instance.Tools.LogEntriesToKeep);
            m_ToolsTab_FPFCEscape       .SetValue(GTConfig.Instance.Tools.FPFCEscape);
            #endregion

            m_PreventChanges = false;

            /// Refresh UI
            OnSettingChanged();
        }
    }
}
