using Newtonsoft.Json;
using UnityEngine;

namespace BeatSaberPlus_MenuMusic
{
    internal class MMConfig : CP_SDK.Config.JsonConfig<MMConfig>
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
            => $"{CP_SDK.ChatPlexSDK.ProductName}/MenuMusic/Config";

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
