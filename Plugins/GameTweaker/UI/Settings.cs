using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.GameTweaker.UI
{
    /// <summary>
    /// Saber Tweaker view controller
    /// </summary>
    internal class Settings : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;
        [UIObject("GamePlayPanel")]
        private GameObject m_GamePlayPanel;
        [UIObject("MenuPanel")]
        private GameObject m_MenuPanel;
        [UIObject("DevToolsTestingPanel")]
        private GameObject m_DevToolsTestingPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("removedebris-toggle")]
        public ToggleSetting m_RemoveDebris;
        [UIComponent("removeallcutparticles-toggle")]
        public ToggleSetting m_RemoveAllCutParticles;
        [UIComponent("removeobstacleparticles-toggle")]
        public ToggleSetting m_RemoveAllObstacleParticles;
        [UIComponent("removefloorburnmarkparticles-toggle")]
        public ToggleSetting m_RemoveFloorBurnMarkParticles;
        [UIComponent("removefloorburnmarkeffects-toggle")]
        public ToggleSetting m_RemoveFloorBurnMarkEffects;
        [UIComponent("removesaberclasheffects-toggle")]
        public ToggleSetting m_RemoveSaberClashEffects;

        [UIComponent("removemusicbandlogo-toggle")]
        public ToggleSetting m_RemoveMusicBandLogo;
        [UIComponent("removefullcombolossanimation-toggle")]
        public ToggleSetting m_RemoveFullComboLossAnimation;
        [UIComponent("nofake360hud-toggle")]
        public ToggleSetting m_NoFake360HUD;
        [UIComponent("removetrail-toggle")]
        public ToggleSetting m_RemoveTrail;
        [UIComponent("intensity-increment")]
        public IncrementSetting m_Intensity;

        //[UIComponent("menuconfirmation-toggle")]
        //public ToggleSetting m_MenuConfirmation;
        //[UIComponent("restartconfirmation-toggle")]
        //public ToggleSetting m_RestartConfirmation;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("disablebeatmapeditorbuttononmainmenu-toggle")]
        public ToggleSetting m_DisableBeatMapEditorButtonInMainMenu;
        [UIComponent("showplayerstatisticsonmainmenu-toggle")]
        public ToggleSetting m_ShowPlayerStatisticsInMainMenu;
        [UIComponent("removenewcontentpromotional-toggle")]
        public ToggleSetting m_RemoveNewContentPromotional;
        [UIComponent("reorderplayersettings-toggle")]
        public ToggleSetting m_ReorderPlayerSettings;
        [UIComponent("addoverridelightintensityoption-toggle")]
        public ToggleSetting m_AddOverrideLightIntensityOption;
        [UIComponent("removebasegamefilterbutton-toggle")]
        public ToggleSetting m_RemoveBaseGameFilterButton;
        [UIComponent("deletesongbutton-toggle")]
        public ToggleSetting m_DeleteSongButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("fpfcescape-toggle")]
        public ToggleSetting m_FPFCEscape;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Type segment control
        /// </summary>
        private TextSegmentedControl m_TypeSegmentControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            if (p_FirstActivation)
            {
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Create type selector
                m_TypeSegmentControl = BeatSaberPlus.Utils.GameUI.CreateTextSegmentedControl(m_TypeSegmentPanel.transform as RectTransform, false);
                m_TypeSegmentControl.SetTexts(new string[] { "Gameplay", "Menu", "Dev / Testing" });
                m_TypeSegmentControl.ReloadData();
                m_TypeSegmentControl.didSelectCellEvent += OnTypeChanged;

                ////////////////////////////////////////////////////////////////////////////
                /// GamePlay
                ////////////////////////////////////////////////////////////////////////////

                /// Prepare sliders left
                Utils.GameUI.PrepareToggleSetting(m_RemoveDebris,                           l_Event,        Config.GameTweaker.RemoveDebris,                            true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveAllCutParticles,                  l_Event,        Config.GameTweaker.RemoveAllCutParticles,                   true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveAllObstacleParticles,             l_Event,        Config.GameTweaker.RemoveObstacleParticles,                 true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveFloorBurnMarkParticles,           l_Event,        Config.GameTweaker.RemoveSaberBurnMarkSparkles,             true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveFloorBurnMarkEffects,             l_Event,        Config.GameTweaker.RemoveSaberBurnMarks,                    true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveSaberClashEffects,                l_Event,        Config.GameTweaker.RemoveSaberClashEffects,                 true);

                /// Prepare sliders right
                Utils.GameUI.PrepareToggleSetting(m_RemoveMusicBandLogo,                    l_Event,        Config.GameTweaker.RemoveMusicBandLogo,                     true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveFullComboLossAnimation,           l_Event,        Config.GameTweaker.RemoveFullComboLossAnimation,            true);
                Utils.GameUI.PrepareToggleSetting(m_NoFake360HUD,                           l_Event,        Config.GameTweaker.NoFake360HUD,                            true);
                Utils.GameUI.PrepareToggleSetting(m_RemoveTrail,                            l_Event,        Config.GameTweaker.RemoveSaberSmoothingTrail,               true);
                Utils.GameUI.PrepareIncrementSetting(m_Intensity,                           l_Event, null,  Config.GameTweaker.SaberSmoothingTrailIntensity,            true);

                //m_MenuConfirmation.Value    = Config.GameTweaker.SongBackButtonConfirm;
                //m_RestartConfirmation.Value = Config.GameTweaker.SongRestartButtonConfirm;
                //m_MenuConfirmation.onChange     = l_Event;
                //m_RestartConfirmation.onChange  = l_Event;

                /// Update UI
                m_Intensity.gameObject.SetActive(false);
                m_Intensity.interactable = !Config.GameTweaker.RemoveSaberSmoothingTrail;
                m_Intensity.gameObject.SetActive(true);

                ////////////////////////////////////////////////////////////////////////////
                /// Menu
                ////////////////////////////////////////////////////////////////////////////

                /// Prepare sliders
                Utils.GameUI.PrepareToggleSetting(m_DisableBeatMapEditorButtonInMainMenu,   l_Event,        Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu,    false);
                Utils.GameUI.PrepareToggleSetting(m_ShowPlayerStatisticsInMainMenu,         l_Event,        Config.GameTweaker.ShowPlayerStatisticsOnMainMenu,          false);
                Utils.GameUI.PrepareToggleSetting(m_RemoveNewContentPromotional,            l_Event,        Config.GameTweaker.RemoveNewContentPromotional,             false);
                Utils.GameUI.PrepareToggleSetting(m_ReorderPlayerSettings,                  l_Event,        Config.GameTweaker.ReorderPlayerSettings,                   false);
                Utils.GameUI.PrepareToggleSetting(m_AddOverrideLightIntensityOption,        l_Event,        Config.GameTweaker.AddOverrideLightIntensityOption,         false);
                Utils.GameUI.PrepareToggleSetting(m_RemoveBaseGameFilterButton,             l_Event,        Config.GameTweaker.RemoveBaseGameFilterButton,              false);
                Utils.GameUI.PrepareToggleSetting(m_DeleteSongButton,                       l_Event,        Config.GameTweaker.DeleteSongButton,                        false);

                ////////////////////////////////////////////////////////////////////////////
                /// Dev / Testing
                ////////////////////////////////////////////////////////////////////////////

                /// Prepare sliders
                Utils.GameUI.PrepareToggleSetting(m_FPFCEscape,                             l_Event,        Config.GameTweaker.FPFCEscape,                              false);

                ////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////

                /// Force change to tab SubRain
                OnTypeChanged(null, 0);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the type is changed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Index">Tab index</param>
        private void OnTypeChanged(SegmentedControl p_Sender, int p_Index)
        {
            m_GamePlayPanel.SetActive(p_Index == 0);
            m_MenuPanel.SetActive(p_Index == 1);
            m_DevToolsTestingPanel.SetActive(p_Index == 2);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            ////////////////////////////////////////////////////////////////////////////
            /// GamePlay
            ////////////////////////////////////////////////////////////////////////////

            /// Update config
            Config.GameTweaker.RemoveDebris                     = m_RemoveDebris.Value;
            Config.GameTweaker.RemoveAllCutParticles            = m_RemoveAllCutParticles.Value;
            Config.GameTweaker.RemoveObstacleParticles          = m_RemoveAllObstacleParticles.Value;
            Config.GameTweaker.RemoveSaberBurnMarkSparkles      = m_RemoveFloorBurnMarkParticles.Value;
            Config.GameTweaker.RemoveSaberBurnMarks             = m_RemoveFloorBurnMarkEffects.Value;
            Config.GameTweaker.RemoveSaberClashEffects          = m_RemoveSaberClashEffects.Value;

            /// Update config
            Config.GameTweaker.RemoveMusicBandLogo              = m_RemoveMusicBandLogo.Value;
            Config.GameTweaker.RemoveFullComboLossAnimation     = m_RemoveFullComboLossAnimation.Value;
            Config.GameTweaker.NoFake360HUD                     = m_NoFake360HUD.Value;
            Config.GameTweaker.RemoveSaberSmoothingTrail        = m_RemoveTrail.Value;
            Config.GameTweaker.SaberSmoothingTrailIntensity     = m_Intensity.Value;

            //Config.GameTweaker.SongBackButtonConfirm        = m_MenuConfirmation.Value;
            //Config.GameTweaker.SongRestartButtonConfirm     = m_RestartConfirmation.Value;

            /// Update UI
            m_Intensity.interactable = !Config.GameTweaker.RemoveSaberSmoothingTrail;

            /// Apply cut particles
            try {
                Patches.PNoteCutCoreEffectsSpawner.SetRemoveCutParticles(Config.GameTweaker.RemoveAllCutParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PNoteCutCoreEffectsSpawner"); Logger.Instance.Error(p_PatchException); }
            /// Apply obstacle particles
            try {
                Patches.PObstacleSaberSparkleEffectManager.SetRemoveObstacleParticles(Config.GameTweaker.RemoveObstacleParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PObstacleSaberSparkleEffectManager"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark particles
            try {
                Patches.PSaberBurnMarkSparkles.SetRemoveSaberBurnMarkSparkles(Config.GameTweaker.RemoveSaberBurnMarkSparkles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberBurnMarkSparkles"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark effect
            try {
                Patches.PSaberBurnMarkArea.SetRemoveSaberBurnMarks(Config.GameTweaker.RemoveSaberBurnMarks);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberBurnMarkArea"); Logger.Instance.Error(p_PatchException); }
            /// Apply saber clash effects
            try {
                Patches.PSaberClashEffect.SetRemoveClashEffects(Config.GameTweaker.RemoveSaberClashEffects);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberClashEffect"); Logger.Instance.Error(p_PatchException); }
            /// Apply trail
            try {
                Patches.PSaberTrailRenderer.SetEnabled(Config.GameTweaker.RemoveSaberSmoothingTrail, Config.GameTweaker.SaberSmoothingTrailIntensity);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberTrailRenderer"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Update config
            Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu = m_DisableBeatMapEditorButtonInMainMenu.Value;
            Config.GameTweaker.ShowPlayerStatisticsOnMainMenu       = m_ShowPlayerStatisticsInMainMenu.Value;
            Config.GameTweaker.RemoveNewContentPromotional          = m_RemoveNewContentPromotional.Value;
            Config.GameTweaker.ReorderPlayerSettings                = m_ReorderPlayerSettings.Value;
            Config.GameTweaker.AddOverrideLightIntensityOption      = m_AddOverrideLightIntensityOption.Value;
            Config.GameTweaker.RemoveBaseGameFilterButton           = m_RemoveBaseGameFilterButton.Value;
            Config.GameTweaker.DeleteSongButton                     = m_DeleteSongButton.Value;

            /// Apply show player statistics in main menu
            try {
                Patches.PMainMenuViewController.SetShowPlayerStatistics(Config.GameTweaker.ShowPlayerStatisticsOnMainMenu);
                Patches.PMainMenuViewController.SetBeatMapEditorButtonDisabled(Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PMainMenuViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply new content promotional settings
            try {
                Patches.PPromoViewController.SetEnabled(Config.GameTweaker.RemoveNewContentPromotional);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PPromoViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply player settings
            try {
                Patches.PPlayerSettingsPanelController.SetReorderEnabled(Config.GameTweaker.ReorderPlayerSettings, Config.GameTweaker.AddOverrideLightIntensityOption);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PlayerSettingsPanelController"); Logger.Instance.Error(p_PatchException); }
            /// Apply remove base game filter button settings
            try {
                Patches.PLevelSearchViewController.SetRemoveBaseGameFilter(Config.GameTweaker.RemoveBaseGameFilterButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PLevelSearchViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply song delete button
            try {
                Patches.PStandardLevelDetailView.SetDeleteSongButtonEnabled(Config.GameTweaker.DeleteSongButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PStandardLevelDetailView"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Update config
            Config.GameTweaker.FPFCEscape = m_FPFCEscape.Value;

            /// Update patches
            GameTweaker.Instance.UpdateFPFCEscape();
        }
    }
}
