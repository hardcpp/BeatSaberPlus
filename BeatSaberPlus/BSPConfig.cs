using Newtonsoft.Json;

namespace BeatSaberPlus
{
    /// <summary>
    /// BeatSaberPlus SDK config
    /// </summary>
    internal class BSPConfig : SDK.Config.JsonConfig<BSPConfig>
    {
        internal class _OBS
        {
            [JsonProperty] internal bool Enabled    = false;
            [JsonProperty] internal string Server   = "127.0.0.1:4444";
            [JsonProperty] internal string Pssword  = "";
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _OBS OBS = new _OBS();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal class _Twitch
        {
            [JsonProperty] internal bool ParseBTTVEmotes    = true;
            [JsonProperty] internal bool ParseFFZEmotes     = true;
            [JsonProperty] internal bool Parse7TVEmotes     = true;
            [JsonProperty] internal bool ParseTwitchEmotes  = true;
            [JsonProperty] internal bool ParseCheermotes    = true;
            [JsonProperty] internal bool ParseEmojis        = true;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal _Twitch Twitch = new _Twitch();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get relative config path
        /// </summary>
        /// <returns></returns>
        public override string GetRelativePath()
            => "BeatSaberPlus/Config";

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