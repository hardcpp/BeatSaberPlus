using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker
{
    /// <summary>
    /// Game Tweaker instance
    /// </summary>
    internal class GameTweaker : CP_SDK.ModuleBase<GameTweaker>
    {
        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Game Tweaker";
        public override string                              Description         => "Customize your game play & menu experience!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#game-tweaker";
        public override bool                                UseChatFeatures     => false;
        public override bool                                IsEnabled           { get => GTConfig.Instance.Enabled; set { GTConfig.Instance.Enabled = value; GTConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsMainView m_SettingsMainView = null;
        private UI.SettingsLeftView m_SettingsLeftView = null;

        private Components.FPFCEscape m_FPFCEscape = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            CP_SDK_BS.Game.Logic.OnSceneChange  += Game_OnSceneChange;
            CP_SDK_BS.Game.Logic.OnLevelStarted += Game_OnLevelStarted;

            Managers.CustomMenuLightManager.Init();

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

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);

            /// Unbind event
            CP_SDK_BS.Game.Logic.OnLevelStarted -= Game_OnLevelStarted;
            CP_SDK_BS.Game.Logic.OnSceneChange  -= Game_OnSceneChange;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsMainView == null) m_SettingsMainView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsLeftView == null) m_SettingsLeftView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();

            /// Change main view
            return (m_SettingsMainView, m_SettingsLeftView, null);
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
                Patches.PNoteCutCoreEffectsSpawner.SetRemoveCutParticles(!p_ForceDisable && GTConfig.Instance.RemoveAllCutParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PNoteCutCoreEffectsSpawner"); Logger.Instance.Error(p_PatchException); }
            /// Apply obstacle particles
            try {
                Patches.PObstacleSaberSparkleEffectManager.SetRemoveObstacleParticles(!p_ForceDisable && GTConfig.Instance.RemoveObstacleParticles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PObstacleSaberSparkleEffectManager"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark particles
            try {
                Patches.PSaberBurnMarkSparkles.SetRemoveSaberBurnMarkSparkles(!p_ForceDisable && GTConfig.Instance.RemoveSaberBurnMarkSparkles);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberBurnMarkSparkles"); Logger.Instance.Error(p_PatchException); }
            /// Apply burn mark effect
            try {
                Patches.PSaberBurnMarkArea.SetRemoveSaberBurnMarks(!p_ForceDisable && GTConfig.Instance.RemoveSaberBurnMarks);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberBurnMarkArea"); Logger.Instance.Error(p_PatchException); }
            /// Apply saber clash effects
            try {
                Patches.PSaberClashEffect.SetRemoveClashEffects(!p_ForceDisable && GTConfig.Instance.RemoveSaberClashEffects);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PSaberClashEffect"); Logger.Instance.Error(p_PatchException); }
            /// World particles
            try {
                UpdateWorldParticles(p_ForceDisable);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating UpdateWorldParticles"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Apply show player statistics in main menu
            try {
                Patches.PMainMenuViewController.SetBeatMapEditorButtonDisabled(!p_ForceDisable && GTConfig.Instance.MainMenu.DisableEditorButtonOnMainMenu);
                Patches.PMainMenuViewController.SetRemovePackMusicPromoBanner(!p_ForceDisable && GTConfig.Instance.MainMenu.RemoveNewContentPromotional);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PMainMenuViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply new content promotional settings
            try {
#if BEATSABER_1_35_0_OR_NEWER
                Patches.PMusicPackPromoBanner.SetEnabled(!p_ForceDisable && GTConfig.Instance.MainMenu.RemoveNewContentPromotional);
#else
                Patches.PPromoViewController.SetEnabled(!p_ForceDisable && GTConfig.Instance.MainMenu.RemoveNewContentPromotional);
#endif
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PPromoViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply player settings
            try {
                Patches.PPlayerSettingsPanelController.SetReorderEnabled(!p_ForceDisable && GTConfig.Instance.PlayerOptions.ReorderPlayerSettings, !p_ForceDisable && GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption);
                Patches.PPlayerSettingsPanelController.SetLightsOptionMerging(!p_ForceDisable && GTConfig.Instance.PlayerOptions.MergeLightPressetOptions);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PlayerSettingsPanelController"); Logger.Instance.Error(p_PatchException); }
            /// Apply remove base game filter button settings
            try {
                Patches.PLevelSearchViewController.SetRemoveBaseGameFilter(!p_ForceDisable && GTConfig.Instance.LevelSelection.RemoveBaseGameFilterButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PLevelSearchViewController"); Logger.Instance.Error(p_PatchException); }
            /// Apply song delete button
            try {
                Patches.PStandardLevelDetailView.SetDeleteSongButtonEnabled(!p_ForceDisable && GTConfig.Instance.LevelSelection.DeleteSongButton);
            } catch (System.Exception p_PatchException) { Logger.Instance.Error("[GameTweaker] Error on updating PStandardLevelDetailView"); Logger.Instance.Error(p_PatchException); }

            ////////////////////////////////////////////////////////////////////////////
            /// Tools / Dev
            ////////////////////////////////////////////////////////////////////////////

            /// Clean logs
            CleanLogs(GTConfig.Instance.Tools.RemoveOldLogs, GTConfig.Instance.Tools.LogEntriesToKeep);

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
            var l_FPFCCamera = Resources.FindObjectsOfTypeAll<FirstPersonFlyingController>().FirstOrDefault();
            if (l_FPFCCamera == null || !l_FPFCCamera.enabled)
                return;

            if (!p_ForceDisable && GTConfig.Instance.Tools.FPFCEscape && m_FPFCEscape == null)
            {
                m_FPFCEscape = new GameObject("BeatSaberPlus_FPFCEscape").AddComponent<Components.FPFCEscape>();
                GameObject.DontDestroyOnLoad(m_FPFCEscape.gameObject);
            }
            else if ((p_ForceDisable || !GTConfig.Instance.Tools.FPFCEscape) && m_FPFCEscape != null)
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
                l_Current.SetActive(p_ForceDisable ? true : !GTConfig.Instance.RemoveWorldParticles);
        }
        /// <summary>
        /// Clean logs
        /// </summary>
        /// <param name="p_ShouldClean">Should clean logs</param>
        /// <param name="p_EntriesToKeep">Number of old entry to keep</param>
        internal void CleanLogs(bool p_ShouldClean, int p_EntriesToKeep)
        {
            if (!p_ShouldClean)
                return;

            var l_DeleteCount = CleanLogsInFolder("Logs", p_EntriesToKeep);
            Logger.Instance.Warning("[GameTweaker] CleanLogs, " + l_DeleteCount + " old logs entry deleted!");
        }
        /// <summary>
        /// Clean logs in folder
        /// </summary>
        /// <param name="p_Directory">Directory to clean</param>
        /// <param name="p_EntriesToKeep">Number of old entry to keep</param>
        /// <returns>Deleted log count</returns>
        private int CleanLogsInFolder(string p_Directory, int p_EntriesToKeep)
        {
            int l_Deleted = 0;
            List<string> l_Files = new List<String>();

            try
            {
                l_Files.AddRange(Directory.GetFiles(p_Directory, "*.log.gz", SearchOption.TopDirectoryOnly)) ;
                l_Files.Sort();
                l_Files.Reverse();

                if (l_Files.Count > p_EntriesToKeep)
                {
                    for (int l_I = p_EntriesToKeep; l_I < l_Files.Count; ++l_I, ++l_Deleted)
                        File.Delete(l_Files[l_I]);
                }

                foreach (string l_Directory in Directory.GetDirectories(p_Directory))
                    l_Deleted += CleanLogsInFolder(l_Directory, p_EntriesToKeep);
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("[GameTweaker] CleanLogsInFolder");
                Logger.Instance.Error(p_Exception);
            }

            return l_Deleted;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On game scene change
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void Game_OnSceneChange(CP_SDK_BS.Game.Logic.ESceneType p_Scene)
        {
            Patches.Lights.PLightsPatches.SetIsValidScene(p_Scene == CP_SDK_BS.Game.Logic.ESceneType.Playing);
            UpdateWorldParticles();
        }
        /// <summary>
        /// On level started
        /// </summary>
        /// <param name="p_LevelData">Level data</param>
        private void Game_OnLevelStarted(CP_SDK_BS.Game.LevelData p_LevelData)
        {
            Patches.Lights.PLightsPatches.SetFromConfig();
            Patches.PNoteDebrisSpawner.SetFromConfig();

#if BEATSABER_1_38_0_OR_NEWER
            if (GTConfig.Instance.Environment.RemoveMusicBandLogo && p_LevelData?.Data?.targetEnvironmentInfo != null)
#else
            if (GTConfig.Instance.Environment.RemoveMusicBandLogo && p_LevelData?.Data?.environmentInfo != null)
#endif
            {
#if BEATSABER_1_38_0_OR_NEWER
                switch (p_LevelData.Data.targetEnvironmentInfo.serializedName)
#else
                switch (p_LevelData.Data.environmentInfo.serializedName)
#endif
                {
                    case "BTSEnvironment":
                    case "LinkinParkEnvironment":
                        new GameObject("BeatSaberPlus_MusicBandLogoRemover").AddComponent<Components.MusicBandLogoRemover>();
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
