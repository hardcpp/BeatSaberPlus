using Newtonsoft.Json;
using UnityEngine;

namespace BeatSaberPlus_MenuMusic
{
    internal class MMConfig : BeatSaberPlus.SDK.Config.JsonConfig<MMConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        [JsonProperty] internal bool ShowPlayer = true;
        [JsonProperty] internal bool ShowPlayTime = true;

        [JsonProperty] internal Color BackgroundColor = new Color(0f, 0f, 0f, 0.5f);
        [JsonProperty] internal bool StartSongFromBeginning = false;
        [JsonProperty] internal bool StartANewMusicOnSceneChange = true;
        [JsonProperty] internal bool LoopCurrentMusic = false;
        [JsonProperty] internal bool UseOnlyCustomMenuSongsFolder = false;
        [JsonProperty] internal float PlaybackVolume = 0.5f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/MenuMusic/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {
            if (BeatSaberPlus.Config.MenuMusic.OldConfigMigrated)
            {
                Save();
                return;
            }

            Enabled = BeatSaberPlus.Config.MenuMusic.Enabled;

            ShowPlayer      = BeatSaberPlus.Config.MenuMusic.ShowPlayer;
            ShowPlayTime    = BeatSaberPlus.Config.MenuMusic.ShowPlayTime;

            BackgroundColor                 = BeatSaberPlus.Config.MenuMusic.BackgroundColor;
            StartSongFromBeginning          = BeatSaberPlus.Config.MenuMusic.StartSongFromBeginning;
            StartANewMusicOnSceneChange     = BeatSaberPlus.Config.MenuMusic.StartANewMusicOnSceneChange;
            LoopCurrentMusic                = BeatSaberPlus.Config.MenuMusic.LoopCurrentMusic;
            UseOnlyCustomMenuSongsFolder    = BeatSaberPlus.Config.MenuMusic.UseOnlyCustomMenuSongsFolder;
            PlaybackVolume                  = BeatSaberPlus.Config.MenuMusic.PlaybackVolume;

            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.Config.MenuMusic.OldConfigMigrated = true;
            Save();
        }
    }
}
