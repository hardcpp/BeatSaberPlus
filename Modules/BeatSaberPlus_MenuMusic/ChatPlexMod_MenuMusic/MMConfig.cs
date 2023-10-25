using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace ChatPlexMod_MenuMusic
{
    internal class MMConfig : CP_SDK.Config.JsonConfig<MMConfig>
    {
        [JsonProperty] internal bool Enabled = false;

        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        internal Data.MusicProviderType.E MusicProvider = Data.MusicProviderType.E.GameMusic;

        [JsonProperty] internal bool ShowPlayer = true;

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
            => $"{CP_SDK.ChatPlexSDK.ProductName}Plus/MenuMusic/Config";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On config init
        /// </summary>
        /// <param name="p_OnCreation">On creation</param>
        protected override void OnInit(bool p_OnCreation)
        {

        }
    }
}
