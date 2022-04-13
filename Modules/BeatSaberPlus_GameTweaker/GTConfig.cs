using Newtonsoft.Json;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker
{
    internal class GTConfig : BeatSaberPlus.SDK.Config.JsonConfig<GTConfig>
    {
        internal class _Environment
        {
            [JsonProperty] internal bool RemoveMusicBandLogo = false;
            [JsonProperty] internal bool RemoveFullComboLossAnimation = false;
            [JsonProperty] internal bool NoFake360HUD = true;
        }
        internal class _LevelSelection
        {
            [JsonProperty] internal bool RemoveBaseGameFilterButton = false;
            [JsonProperty] internal bool DeleteSongButton = true;
            [JsonProperty] internal bool DeleteSongBrowserTrashcan = true;
            [JsonProperty] internal bool HighlightEnabled = true;
            [JsonProperty] internal Color32 HighlightPlayed = new Color32(248,230,0,255);
            [JsonProperty] internal Color32 HighlightAllPlayed = new Color32(82,247,0,255);
        }
        internal class _PlayerOptions
        {
            [JsonProperty] internal int JumpDurationIncrement = 1;
            [JsonProperty] internal bool ReorderPlayerSettings = true;
            [JsonProperty] internal bool MergeLightPressetOptions = true;
            [JsonProperty] internal bool OverrideLightIntensityOption = true;
            [JsonProperty] internal float OverrideLightIntensity = 1.0f;
        }
        internal class _MainMenu
        {
            [JsonProperty] internal bool OverrideMenuEnvColors = false;
            [JsonProperty] internal Color BaseColor = new Color(0.421376616f, 0.201642916f, 0.6745098f, 1f);
            [JsonProperty] internal Color LevelClearedColor = new Color(0.203647852f, 0.479708f, 0.07326582f, 1f);
            [JsonProperty] internal Color LevelFailedColor = new Color(0.796078444f, 0.137425855f, 0.0f, 1f);
            [JsonProperty] internal bool DisableEditorButtonOnMainMenu = true;
            [JsonProperty] internal bool RemoveNewContentPromotional = true;
        }
        internal class _Tools
        {
            [JsonProperty] internal bool RemoveOldLogs = true;
            [JsonProperty] internal int LogEntriesToKeep = 8;
            [JsonProperty] internal bool FPFCEscape = false;
        }

        [JsonProperty] internal bool Enabled = false;

        /// Gameplay
        [JsonProperty] internal bool RemoveDebris = false;
        [JsonProperty] internal bool RemoveAllCutParticles = false;
        [JsonProperty] internal bool RemoveObstacleParticles = false;
        [JsonProperty] internal bool RemoveSaberBurnMarks = false;
        [JsonProperty] internal bool RemoveSaberBurnMarkSparkles = false;
        [JsonProperty] internal bool RemoveSaberClashEffects = false;
        [JsonProperty] internal bool RemoveWorldParticles = false;


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Environment Environment = new _Environment();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _LevelSelection LevelSelection = new _LevelSelection();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _PlayerOptions PlayerOptions = new _PlayerOptions();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _MainMenu MainMenu = new _MainMenu();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Tools Tools = new _Tools();

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
            Save();
        }
    }
}