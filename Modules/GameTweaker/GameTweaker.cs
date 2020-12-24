﻿using BeatSaberMarkupLanguage;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Modules.GameTweaker
{
    /// <summary>
    /// Game Tweaker instance
    /// </summary>
    internal class GameTweaker : SDK.ModuleBase<GameTweaker>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Game Tweaker";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Customize your game play & menu experience!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.GameTweaker.Enabled; set => Config.GameTweaker.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Settings view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Settings left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Settings right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// FPFC escape object
        /// </summary>
        private Components.FPFCEscape m_FPFCEscape = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            SDK.Game.Logic.OnSceneChange += Game_OnSceneChange;

            /// Update patches
            UpdatePatches(false);
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Update patches
            UpdatePatches(true);

            /// Unbind event
            SDK.Game.Logic.OnSceneChange -= Game_OnSceneChange;
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
        /// Update patches
        /// </summary>
        /// <param name="p_ForceDisable">Force disable</param>
        internal void UpdatePatches(bool p_ForceDisable)
        {
            ////////////////////////////////////////////////////////////////////////////
            /// GamePlay
            ////////////////////////////////////////////////////////////////////////////

            /// Apply cut particles
            try {
                Patches.PNoteCutCoreEffectsSpawner.SetRemoveCutParticles(!p_ForceDisable && Config.GameTweaker.RemoveAllCutParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PNoteCutCoreEffectsSpawner"); Logger.Instance.Error(p_PatchException); }
            /// Apply obstacle particles
            try {
                Patches.PObstacleSaberSparkleEffectManager.SetRemoveObstacleParticles(!p_ForceDisable && Config.GameTweaker.RemoveObstacleParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PObstacleSaberSparkleEffectManager"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark particles
            try {
                Patches.PSaberBurnMarkSparkles.SetRemoveSaberBurnMarkSparkles(!p_ForceDisable && Config.GameTweaker.RemoveSaberBurnMarkSparkles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberBurnMarkSparkles"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark effect
            try {
                Patches.PSaberBurnMarkArea.SetRemoveSaberBurnMarks(!p_ForceDisable && Config.GameTweaker.RemoveSaberBurnMarks);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberBurnMarkArea"); Logger.Instance.Error(p_PatchException); }
            /// Apply saber clash effects
            try {
                Patches.PSaberClashEffect.SetRemoveClashEffects(!p_ForceDisable && Config.GameTweaker.RemoveSaberClashEffects);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberClashEffect"); Logger.Instance.Error(p_PatchException); }
            /// World particles
            try {
                UpdateWorldParticles(p_ForceDisable);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating UpdateWorldParticles"); Logger.Instance.Error(p_PatchException); }
            /// Apply trail
            try {
                Patches.PSaberTrailRenderer.SetEnabled(!p_ForceDisable && Config.GameTweaker.RemoveSaberSmoothingTrail, p_ForceDisable ? 1.0f : Config.GameTweaker.SaberSmoothingTrailIntensity);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberTrailRenderer"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Apply show player statistics in main menu
            try {
                Patches.PMainMenuViewController.SetShowPlayerStatistics(!p_ForceDisable && Config.GameTweaker.ShowPlayerStatisticsOnMainMenu);
                Patches.PMainMenuViewController.SetBeatMapEditorButtonDisabled(!p_ForceDisable && Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PMainMenuViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply new content promotional settings
            try {
                Patches.PPromoViewController.SetEnabled(!p_ForceDisable && Config.GameTweaker.RemoveNewContentPromotional);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PPromoViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply player settings
            try {
                Patches.PPlayerSettingsPanelController.SetReorderEnabled(!p_ForceDisable && Config.GameTweaker.ReorderPlayerSettings, !p_ForceDisable && Config.GameTweaker.AddOverrideLightIntensityOption);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PlayerSettingsPanelController"); Logger.Instance.Error(p_PatchException); }
            /// Apply remove base game filter button settings
            try {
                Patches.PLevelSearchViewController.SetRemoveBaseGameFilter(!p_ForceDisable && Config.GameTweaker.RemoveBaseGameFilterButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PLevelSearchViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply song delete button
            try {
                Patches.PStandardLevelDetailView.SetDeleteSongButtonEnabled(!p_ForceDisable && Config.GameTweaker.DeleteSongButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PStandardLevelDetailView"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            try {
                UpdateFPFCEscape(p_ForceDisable);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating UpdateFPFCEscape"); Logger.Instance.Error(p_PatchException); }
        }
        /// <summary>
        /// Update FPFC escape
        /// </summary>
        /// <param name="p_ForceDisable">Force disable</param>
        internal void UpdateFPFCEscape(bool p_ForceDisable = false)
        {
            if (!p_ForceDisable && Config.GameTweaker.FPFCEscape && m_FPFCEscape == null)
            {
                m_FPFCEscape = new GameObject("BeatSaberPlus_FPFCEscape").AddComponent<Components.FPFCEscape>();
                GameObject.DontDestroyOnLoad(m_FPFCEscape.gameObject);
            }
            else if ((p_ForceDisable || !Config.GameTweaker.FPFCEscape) && m_FPFCEscape != null)
            {
                GameObject.DestroyImmediate(m_FPFCEscape.gameObject);
                m_FPFCEscape = null;
            }
        }
        /// <summary>
        /// Update world particles
        /// </summary>
        /// <param name="p_ForceDisable">Force disable</param>
        internal void UpdateWorldParticles(bool p_ForceDisable = false)
        {
            var l_Objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name == "DustPS");
            foreach (var l_Current in l_Objects)
                l_Current.SetActive(p_ForceDisable ? true : !Config.GameTweaker.RemoveWorldParticles);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On game scene change
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void Game_OnSceneChange(SDK.Game.Logic.SceneType p_Scene)
        {
            if (Config.GameTweaker.RemoveMusicBandLogo && p_Scene == SDK.Game.Logic.SceneType.Playing)
                new GameObject("BeatSaberPlus_MusicBandLogoRemover").AddComponent<Components.MusicBandLogoRemover>();

            UpdateWorldParticles();
        }
    }
}
