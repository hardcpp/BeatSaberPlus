using BeatSaberMarkupLanguage;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Plugins.GameTweaker
{
    /// <summary>
    /// Game Tweaker instance
    /// </summary>
    class GameTweaker : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Game Tweaker";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.GameTweaker.Enabled; set => Config.GameTweaker.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static GameTweaker Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// FPFC escape object
        /// </summary>
        private Components.FPFCEscape m_FPFCEscape = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind singleton
            Instance = this;

            /// Bind event
            Utils.Game.OnSceneChange += Game_OnSceneChange;

            ////////////////////////////////////////////////////////////////////////////
            /// GamePlay
            ////////////////////////////////////////////////////////////////////////////

            /// Apply cut particles
            try {
                Patches.PNoteCutCoreEffectsSpawner.SetRemoveCutParticles(Config.GameTweaker.RemoveAllCutParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PNoteCutCoreEffectsSpawner"); Logger.Instance.Error(p_PatchException); }
            /// Apply obstacle particles
            try {
                Patches.PObstacleSaberSparkleEffectManager.SetRemoveObstacleParticles(Config.GameTweaker.RemoveObstacleParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PObstacleSaberSparkleEffectManager"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark particles
            try {
                Patches.PSaberBurnMarkSparkles.SetRemoveSaberBurnMarkSparkles(Config.GameTweaker.RemoveSaberBurnMarkSparkles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PSaberBurnMarkSparkles"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark effect
            try {
                Patches.PSaberBurnMarkArea.SetRemoveSaberBurnMarks(Config.GameTweaker.RemoveSaberBurnMarks);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PSaberBurnMarkArea"); Logger.Instance.Error(p_PatchException); }
            /// Apply saber clash effects
            try {
                Patches.PSaberClashEffect.SetRemoveClashEffects(Config.GameTweaker.RemoveSaberClashEffects);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PSaberClashEffect"); Logger.Instance.Error(p_PatchException); }
            /// Apply trail
            try {
                Patches.PSaberTrailRenderer.SetEnabled(Config.GameTweaker.RemoveSaberSmoothingTrail, Config.GameTweaker.SaberSmoothingTrailIntensity);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PSaberTrailRenderer"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Apply show player statistics in main menu
            try {
                Patches.PMainMenuViewController.SetShowPlayerStatistics(Config.GameTweaker.ShowPlayerStatisticsOnMainMenu);
                Patches.PMainMenuViewController.SetBeatMapEditorButtonDisabled(Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PMainMenuViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply new content promotional settings
            try {
                Patches.PPromoViewController.SetEnabled(Config.GameTweaker.RemoveNewContentPromotional);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PPromoViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply player settings
            try {
                Patches.PPlayerSettingsPanelController.SetReorderEnabled(Config.GameTweaker.ReorderPlayerSettings, Config.GameTweaker.AddOverrideLightIntensityOption);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PlayerSettingsPanelController"); Logger.Instance.Error(p_PatchException); }
            /// Apply remove base game filter button settings
            try {
                Patches.PLevelSearchViewController.SetRemoveBaseGameFilter(Config.GameTweaker.RemoveBaseGameFilterButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PLevelSearchViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply song delete button
            try {
                Patches.PStandardLevelDetailView.SetDeleteSongButtonEnabled(Config.GameTweaker.DeleteSongButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling PStandardLevelDetailView"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            try {
                UpdateFPFCEscape();
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on enabling UpdateFPFCEscape"); Logger.Instance.Error(p_PatchException); }
        }
        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnDisable()
        {
            ////////////////////////////////////////////////////////////////////////////
            /// GamePlay
            ////////////////////////////////////////////////////////////////////////////

            /// Restore cut particles
            try {
                Patches.PNoteCutCoreEffectsSpawner.SetRemoveCutParticles(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PNoteCutCoreEffectsSpawner"); Logger.Instance.Error(p_PatchException); }
            /// Restore obstacle particles
            try {
                Patches.PObstacleSaberSparkleEffectManager.SetRemoveObstacleParticles(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PObstacleSaberSparkleEffectManager"); Logger.Instance.Error(p_PatchException); }
            /// Restore burn mark particles
            try {
                Patches.PSaberBurnMarkSparkles.SetRemoveSaberBurnMarkSparkles(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PSaberBurnMarkSparkles"); Logger.Instance.Error(p_PatchException); }
            /// Restore burn mark effect
            try {
                Patches.PSaberBurnMarkArea.SetRemoveSaberBurnMarks(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PSaberBurnMarkArea"); Logger.Instance.Error(p_PatchException); }
            /// Restore saber clash effects
            try {
                Patches.PSaberClashEffect.SetRemoveClashEffects(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PSaberClashEffect"); Logger.Instance.Error(p_PatchException); }
            /// Restore trail
            try {
                Patches.PSaberTrailRenderer.SetEnabled(false, 1f);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PSaberTrailRenderer"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Restore show player statistics in main menu
            try {
                Patches.PMainMenuViewController.SetShowPlayerStatistics(false);
                Patches.PMainMenuViewController.SetBeatMapEditorButtonDisabled(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PMainMenuViewController"); Logger.Instance.Error(p_PatchException); }
            /// Restore new content promotional settings
            try {
                Patches.PPromoViewController.SetEnabled(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PPromoViewController"); Logger.Instance.Error(p_PatchException); }
            /// Restore player settings
            try {
                Patches.PPlayerSettingsPanelController.SetReorderEnabled(false, false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PlayerSettingsPanelController"); Logger.Instance.Error(p_PatchException); }
            /// Apply remove base game filter button settings
            try {
                Patches.PLevelSearchViewController.SetRemoveBaseGameFilter(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PLevelSearchViewController"); Logger.Instance.Error(p_PatchException); }
            /// Restore song delete button
            try {
                Patches.PStandardLevelDetailView.SetDeleteSongButtonEnabled(false);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling PStandardLevelDetailView"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            try {
                UpdateFPFCEscape(true);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling UpdateFPFCEscape"); Logger.Instance.Error(p_PatchException); }


            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////////////////////
            /// GamePlay setup
            ////////////////////////////////////////////////////////////////////////////

            try {
                BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.RemoveTab("BeatSaberPlus");
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on disabling GameplaySetup"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// Unbind event
            Utils.Game.OnSceneChange -= Game_OnSceneChange;

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
        /// Update FPFC escape
        /// </summary>
        /// <param name="p_OnDisable">On plugin disable</param>
        internal void UpdateFPFCEscape(bool p_OnDisable = false)
        {
            if (!p_OnDisable && Config.GameTweaker.FPFCEscape && m_FPFCEscape == null)
            {
                m_FPFCEscape = new GameObject("BeatSaberPlus_FPFCEscape").AddComponent<Components.FPFCEscape>();
                GameObject.DontDestroyOnLoad(m_FPFCEscape.gameObject);
            }
            else if ((p_OnDisable || !Config.GameTweaker.FPFCEscape) && m_FPFCEscape != null)
            {
                GameObject.DestroyImmediate(m_FPFCEscape.gameObject);
                m_FPFCEscape = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On game scene change
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void Game_OnSceneChange(Utils.Game.SceneType p_Scene)
        {
            if (Config.GameTweaker.RemoveMusicBandLogo && p_Scene == Utils.Game.SceneType.Playing)
                new GameObject("BeatSaberPlus_MusicBandLogoRemover").AddComponent<Components.MusicBandLogoRemover>();
        }
    }
}
