using Newtonsoft.Json;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker
{
    internal class GTConfig : BeatSaberPlus.SDK.Config.JsonConfig<GTConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        /// Gameplay
        [JsonProperty] internal bool RemoveDebris                 = false;
        [JsonProperty] internal bool RemoveAllCutParticles        = false;
        [JsonProperty] internal bool RemoveObstacleParticles      = false;
        [JsonProperty] internal bool RemoveSaberBurnMarks         = false;
        [JsonProperty] internal bool RemoveSaberBurnMarkSparkles  = false;
        [JsonProperty] internal bool RemoveSaberClashEffects      = false;
        [JsonProperty] internal bool RemoveWorldParticles         = false;
        [JsonProperty] internal bool RemoveMusicBandLogo          = false;
        [JsonProperty] internal bool RemoveFullComboLossAnimation = false;
        [JsonProperty] internal bool NoFake360HUD                 = false;

        /// Main menu
        [JsonProperty] internal bool DisableBeatMapEditorButtonOnMainMenu = true;
        [JsonProperty] internal bool RemoveNewContentPromotional          = true;

        /// Level selection
        [JsonProperty] internal bool RemoveBaseGameFilterButton       = true;
        [JsonProperty] internal bool ReorderPlayerSettings            = true;
        [JsonProperty] internal bool AddOverrideLightIntensityOption  = true;
        [JsonProperty] internal bool MergeLightPressetOptions         = true;
        [JsonProperty] internal float OverrideLightIntensity          = 1.0f;
        [JsonProperty] internal bool DeleteSongButton                 = true;
        [JsonProperty] internal bool DeleteSongBrowserTrashcan        = false;
        [JsonProperty] internal bool HighlightPlayedSong              = true;

        /// Logs
        [JsonProperty] internal bool RemoveOldLogs = true;
        [JsonProperty] internal int LogEntriesToKeep = 8;

        /// FPFC escape
        [JsonProperty] internal bool FPFCEscape = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/GameTweaker/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (BeatSaberPlus.Config.GameTweaker.OldConfigMigrated)
            {
                Save();
                return;
            }

            Enabled = BeatSaberPlus.Config.GameTweaker.Enabled;

            RemoveDebris                 = BeatSaberPlus.Config.GameTweaker.RemoveDebris;
            RemoveAllCutParticles        = BeatSaberPlus.Config.GameTweaker.RemoveAllCutParticles;
            RemoveObstacleParticles      = BeatSaberPlus.Config.GameTweaker.RemoveObstacleParticles;
            RemoveSaberBurnMarks         = BeatSaberPlus.Config.GameTweaker.RemoveSaberBurnMarks;
            RemoveSaberBurnMarkSparkles  = BeatSaberPlus.Config.GameTweaker.RemoveSaberBurnMarkSparkles;
            RemoveSaberClashEffects      = BeatSaberPlus.Config.GameTweaker.RemoveSaberClashEffects;
            RemoveWorldParticles         = BeatSaberPlus.Config.GameTweaker.RemoveWorldParticles;
            RemoveMusicBandLogo          = BeatSaberPlus.Config.GameTweaker.RemoveMusicBandLogo;
            RemoveFullComboLossAnimation = BeatSaberPlus.Config.GameTweaker.RemoveFullComboLossAnimation;
            NoFake360HUD                 = BeatSaberPlus.Config.GameTweaker.NoFake360HUD;

            DisableBeatMapEditorButtonOnMainMenu    = BeatSaberPlus.Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu;
            RemoveNewContentPromotional             = BeatSaberPlus.Config.GameTweaker.RemoveNewContentPromotional;

            RemoveBaseGameFilterButton       = BeatSaberPlus.Config.GameTweaker.RemoveBaseGameFilterButton;
            ReorderPlayerSettings            = BeatSaberPlus.Config.GameTweaker.ReorderPlayerSettings;
            AddOverrideLightIntensityOption  = BeatSaberPlus.Config.GameTweaker.AddOverrideLightIntensityOption;
            MergeLightPressetOptions         = BeatSaberPlus.Config.GameTweaker.MergeLightPressetOptions;
            OverrideLightIntensity           = BeatSaberPlus.Config.GameTweaker.OverrideLightIntensity;
            DeleteSongButton                 = BeatSaberPlus.Config.GameTweaker.DeleteSongButton;
            DeleteSongBrowserTrashcan        = BeatSaberPlus.Config.GameTweaker.DeleteSongBrowserTrashcan;
            HighlightPlayedSong              = BeatSaberPlus.Config.GameTweaker.HighlightPlayedSong;

            RemoveOldLogs       = BeatSaberPlus.Config.GameTweaker.RemoveOldLogs;
            LogEntriesToKeep    = BeatSaberPlus.Config.GameTweaker.LogEntriesToKeep;

            FPFCEscape = BeatSaberPlus.Config.GameTweaker.FPFCEscape;

            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.Config.GameTweaker.OldConfigMigrated = true;
            Save();
        }
    }
}
